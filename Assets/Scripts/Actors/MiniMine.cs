using System;
using System.Collections;
using UnityEngine;
using Diluvion;
using Sirenix.OdinInspector;
using SpiderWeb;

/// <summary>
/// Mini mine explodes if something bumps into it! Requires a and an explosive to work.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class MiniMine : MonoBehaviour, IDamageable, IAlignable
{

	public LayerMask triggeringLayers;

	public GameObject explosionPrefab;
	
	[MinValue(1)]
	public float hp = 1;

	public float explosionRadius = 2;
	public float explosionDamage = 5;

	float _maxHp;
	public bool blowPlayerOnly;

	public float impactForce = 50;
	public float explodeDelay;

	[ReadOnly]
	public Rigidbody rb;
	
	public event Action<GameObject> onKilled;

	bool _consumed;

	[Button]
	void GetComponents()
	{
		if (!rb)
			rb = GetComponent<Rigidbody>();
		
		#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(gameObject);
		#endif
	}
	
	void Awake()
	{
		GetComponents();
		_maxHp = hp;
	}

	/// <summary>
	/// When impacted, force is added to the rigidbody
	/// </summary>
	public void Impact(Vector3 vector, Vector3 pos)
	{
		rb.AddForceAtPosition(vector.normalized * impactForce, pos);
	}

	

	/// <summary>
	/// If HP reaches zero, it triggers the explosive component.
	/// </summary>
	public void Damage(float damage, float critRate, GameObject source)
	{
		hp -= damage;
		if (hp <= 0) Explode();
		
		SpiderSound.MakeSound("Play_WEA_Bolts_Impact_Destructable", gameObject);
	}

	public float NormalizedHp()
	{
		return  hp / _maxHp;
	}

	[Button]
	void Explode()
	{
		if (_consumed) return;
		StartCoroutine(ExplodeSequence());
		_consumed = true;
	}

	IEnumerator ExplodeSequence()
	{
		yield return new WaitForSeconds(explodeDelay);
		onKilled?.Invoke(null);
		GameObject newExplosion = Instantiate(explosionPrefab, transform.position, transform.rotation);

		Explosion explosion = newExplosion.GetComponent<Explosion>();
		if (explosion)
		{
			explosion.baseDamage = explosionDamage;
			explosion.SetRadius(explosionRadius);
		}
		else
		{
			Debug.Log("Mine " + name + " spawned an explosion prefab that doesn't have an explosion component. Can't set damage and radius!");
		}
		
		
		Destroy(gameObject);
	}


    void OnCollisionEnter(Collision other)
    {

	    // Check if the object colliding into this is one of my triggering layers
	    if (triggeringLayers != (triggeringLayers | (1 << other.gameObject.layer))) return;
	    
	    // Make sure its not another mini mine triggering this
		MiniMine otherMine = other.gameObject.GetComponent<MiniMine>();
		if (otherMine) return;

	    Explode();
    }

	public AlignmentToPlayer getAlignment()
	{
		return AlignmentToPlayer.Hostile;
	}

	public float SafeDistance()
	{
		return 10;
	}
}
