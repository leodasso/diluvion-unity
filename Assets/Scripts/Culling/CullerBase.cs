using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Diluvion;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CullerBase : MonoBehaviour
{
	[ValidateInput("ValidateUseTrigger", "If using trigger, must have a collider marked 'trigger' attached.")]
	[OnValueChanged("SetToolLayer")]
	public bool useTrigger;

	void SetToolLayer()
	{
		if (useTrigger) gameObject.layer = LayerMask.NameToLayer("Tools");
	}

	bool ValidateUseTrigger(bool isUsingTrigger)
	{
		if (isUsingTrigger)
		{
			Collider col = GetComponent<Collider>();
			if (col == null) return false;
			return col.isTrigger;
		}
		return true;
	}
	
	
	static float cullDistance = 100;
	public static List<CullerBase> all = new List<CullerBase>();
	static CullerManager _cullerManager;

	public bool startInactive;
	
	[HideIf("useTrigger")]
	public bool useCustomPivot;
	[ShowIf("useCustomPivot"), HideIf("useTrigger")]
	public Vector3 pivot;
	
	[HideIf("useTrigger")]
	public bool useCustomRange;
	[ShowIf("useCustomRange")]
	[HideIf("useTrigger")]
	public float customRange = 100;
	bool _activeState = true;

	#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
		if (useTrigger) return;
		//Prevent all children of a parent object from showing their gizmos
		if (Selection.activeGameObject != gameObject) return;
		
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(CullPosition(), CullDistance());
		Gizmos.color = new Color(.5f, .5f, .3f, .05f);
		Gizmos.DrawSphere(CullPosition(), cullDistance);
	}
	#endif

	void Awake()
	{
		if (startInactive || useTrigger) SetState(false);
	}

	protected float CullDistance()
	{
		if (useCustomRange) return customRange;
		return cullDistance;
	}

	/// <summary>
	/// The position used as the center of the culling sphere. By default this is the transform position, but can be changed
	/// with a custom pivot.
	/// </summary>
	protected Vector3 CullPosition()
	{
		if (useCustomPivot) return pivot + transform.position;
		return transform.position;
	}

	
	/// <summary>
	/// Returns the culler manager instance. Creates one if none exist.
	/// </summary>
	static CullerManager GetManager()
	{
		if (_cullerManager) return _cullerManager;
		
		GameObject newManager = new GameObject("behaviorCuller manager");
		_cullerManager = newManager.AddComponent<CullerManager>();
		return _cullerManager;
	}


	void OnTriggerEnter(Collider other)
	{
		if (TriggerValid(other))
			SetState(true);
	}

	void OnTriggerExit(Collider other)
	{
		if (TriggerValid(other))
			SetState(false);
	}

	bool TriggerValid(Collider other)
	{
		if (!useTrigger) return false;
		if (PlayerManager.PlayerShip() == null) return false;
		if (other.gameObject != PlayerManager.PlayerShip()) return false;
		return true;
	}

	float _dist;
	GameObject _playerShip;
	
	/// <summary>
	/// Updates the cullers status. This is called by the culler manager
	/// </summary>
	public void LocalUpdate()
	{
		if (useTrigger) return;
		
		_playerShip = PlayerManager.PlayerShip();
		if (!_playerShip)
		{
			//SetState(false);
			return;
		}

		// get distance from the player ship to me.
		_dist = Vector3.Distance(_playerShip.transform.position, CullPosition());

		// Set state based on distance
		SetState(_dist < CullDistance());
	}

	protected virtual bool SetState(bool enabled)
	{
		if (enabled == _activeState) return false;

		_activeState = enabled;
		return true;
	}
	
	/// <summary>
	/// On enable, add this instance to the global list.
	/// </summary>
	void OnEnable()
	{
		GetManager().cullersInScene.Add(this);
	}
	
	/// <summary>
	/// On Disable, remove this instance from the global list & clean the list.
	/// </summary>
	void OnDisable()
	{
		GetManager().cullersInScene.Remove(this);
	}
}
