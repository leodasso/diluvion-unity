using System.Collections;
using System.Collections.Generic;
using Diluvion;
using UnityEngine;
using Sirenix.OdinInspector;
using SpiderWeb;

[RequireComponent(typeof(Rigidbody))]
public class ChomperMine : MonoBehaviour, IAlignable
{

	public enum ChomperMode
	{
		Idle,
		Anxious,
		Attacking
	}

	public ChomperMode chomperMode = ChomperMode.Idle;

	[MinValue(1), Tooltip("The radius at which this mine becomes indirectly aware of a target. puts it into 'anxious' mode.")]
	[OnValueChanged("AdjustDetectionRadius")]
	public float detectionRadius = 20;

	[MinValue(.5f)]
	[Tooltip("The radius of the 'vision' spherecast. When an object is detected by this, it will become the attack target")]
	public float spherecastRadius = 4;

	public float damage = 5;
	public float explosionRadius = 25;

	public GameObject visionCone;
	
	public Explosion explosionPrefab;

	[Tooltip("Adjust this so that the blue line is coming straight out of my face")]
	public Vector3 forward;

	public LayerMask spherecastLayerMask;

	[Space]
	[Tooltip("The force of the 'bump' when moving from idle to anxious states")]
	public float bumpForce = 20;

	[Tooltip("The speed at which I rotate around the Y axis when anxious")]
	public float anxiousRotSpeed = 2;
	
	[Tooltip("How many seconds does it take once all targets have left my radius for me to become idle"), MinValue(0)]
	public float anxiousCooldown = 5;

	[Tooltip("How long I'll try to attack an object once I detect it")]
	public float attackTime = 5;

	[Tooltip("How much force do i move towards my attack target")]
	public float attackForce = 200;
	
	[Space]
	[Tooltip("This should be a sphere collider child of this object.")]
	public SphereCollider trigger;

	float _cooldown;

	[Space] 
	[Tooltip("All the unwelcome objects that have entered my detection radius"), ReadOnly]
	public List<GameObject> indirectTargets = new List<GameObject>();
	
	[Tooltip("The single target that I'm going after."), ReadOnly]
	public GameObject target;

	Animator _animator;
	Rigidbody _rigidbody;

	void OnDrawGizmosSelected()
	{
		Vector3 totalDir = transform.rotation * forward;

		if (visionCone)
		{
			visionCone.transform.localPosition = Vector3.zero;
			visionCone.transform.localRotation = Quaternion.LookRotation(forward);
		}
		
		Gizmos.color = Color.blue;
		
		Gizmos.DrawRay(transform.position, totalDir.normalized * 10);
	}

	void AdjustDetectionRadius()
	{
		if (!trigger)
		{
			Debug.LogError("A sphere collider child of " + name + " is required for it to work.", gameObject);
			return;
		}

		trigger.gameObject.layer = LayerMask.NameToLayer("Tools");
		float scale = transform.localScale.x;
		trigger.isTrigger = true;
		trigger.radius = detectionRadius / scale;
	}

	// Use this for initialization
	void Start ()
	{
		AdjustDetectionRadius();

		_rigidbody = GetComponent<Rigidbody>();
		_animator = GetComponent<Animator>();
		if (!_animator)
		{
			Debug.LogError("An animator is required for " + name + " to work properly.", gameObject);
		}
	}

	public void Bump()
	{
		_rigidbody.AddForce(RndBump(), RndBump(), RndBump());
	}

	float RndBump()
	{
		return Random.Range(-bumpForce, bumpForce);
	}
	
	
	// Update is called once per frame
	void Update ()
	{
		if (indirectTargets.Count > 0) _cooldown = anxiousCooldown;
		else if (_cooldown > 0)  _cooldown -= Time.deltaTime;
		
		// return to idle mode after the cooldown.
		else if (chomperMode == ChomperMode.Anxious)
		{
			_animator.SetBool("anxious", false);
			chomperMode = ChomperMode.Idle;
			SpiderSound.MakeSound("Stop_Large_Chomp_Mine", gameObject);
		}
	}

	/// <summary>
	/// Sets the target that I will attack
	/// </summary>
	void SetTarget(GameObject newTarget)
	{
		// If I've detected one of my indirect targets, set it as attack target
		target = newTarget;
		chomperMode = ChomperMode.Attacking;
		_animator.SetBool("attacking", true);
		StartCoroutine(AttackCooldown());

	}

	RaycastHit[] _hits;
	void FixedUpdate()
	{
		// if anxious, slowly rotate around the y axis
		if (chomperMode == ChomperMode.Anxious)
		{
			_rigidbody.AddRelativeTorque(0, anxiousRotSpeed * Time.fixedDeltaTime, 0);
			
			// Spherecast forward to 'see' any targets
			_hits = Physics.SphereCastAll(transform.position, spherecastRadius, transform.rotation * forward, detectionRadius,
				spherecastLayerMask);

			// Check through each target
			foreach (var hit in _hits)
			{
				// I only care about hits that are part of my indirect targets list
				if (!indirectTargets.Contains(hit.collider.gameObject)) continue;
				
				// If I've detected one of my indirect targets, set it as attack target
				SetTarget(hit.collider.gameObject);
				break;
			}
		}
		
		// attacking mode
		if (chomperMode == ChomperMode.Attacking)
		{
			Vector3 dir = target.transform.position - transform.position;
			_rigidbody.AddForce(dir.normalized * attackForce * Time.fixedDeltaTime);
		}
	}

	IEnumerator AttackCooldown()
	{
		yield return new WaitForSeconds(attackTime);
		target = null;
		_animator.SetBool("attacking", false);
		chomperMode = ChomperMode.Anxious;
	}

	public void ChompOpenSound()
	{
		SpiderSound.MakeSound("Play_Large_Chomp_Mine_Open", gameObject);
	}

	public void ChompCloseSound()
	{
		SpiderSound.MakeSound("Play_Large_Chomp_Mine_Chomp", gameObject);
	}


	#region triggers

	void OnCollisionEnter(Collision other)
	{
		Explosion newExplosion = Instantiate(explosionPrefab, transform.position, transform.rotation);
		newExplosion.baseDamage = damage;
		newExplosion.SetRadius(explosionRadius);
		Destroy(newExplosion, 10);
	}


	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == gameObject) return;
		if (other.isTrigger) return;
		if (other.GetComponent<MiniMine>()) return;
		if (other.GetComponent<ChomperMine>()) return;

		if (other.GetComponent<IDamageable>() != null)
		{
			if (indirectTargets.Contains(other.gameObject)) return;
			indirectTargets.Add(other.gameObject);
			
			// If i was idle before, become anxious now that a target has entered
			if (chomperMode == ChomperMode.Idle)
			{
				_animator.SetBool("anxious", true);
				chomperMode = ChomperMode.Anxious;
				SpiderSound.MakeSound("Play_Large_Chomp_Mine_Detect_Player", gameObject);
			}
		}
	}

	void OnTriggerExit(Collider other)
	{
		indirectTargets.Remove(other.gameObject);
	}
	#endregion


	public AlignmentToPlayer getAlignment()
	{
		return AlignmentToPlayer.Hostile;
	}

	public float SafeDistance()
	{
		return 15;
	}
}
