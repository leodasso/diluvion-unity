using System.Collections;
using System.Collections.Generic;
using Diluvion;
using UnityEngine;
using Sirenix.OdinInspector;
using SpiderWeb;

/// <summary>
/// Handles all tracking of combatants and combat music changes. Adds/removes combat track to AKMusic. This component acts
/// as a singleton, and should be added on the same object as AKMusic.
/// </summary>
public class CombatMusic : MonoBehaviour 
{
	
	[ OnValueChanged("PushAllSwitches")]
	public Combat_Zone_Type combatZoneTypeSwitch;
    
	[OnValueChanged("PushAllSwitches")]
	public Combat_State combatStateSwitch;
    
	[OnValueChanged("PushAllSwitches")]
	public Combat_Fight_State combatFightSwitch;
    
	[OnValueChanged("PushAllSwitches")]
	public Combat_Health healthSwitch;
    
	[OnValueChanged("PushAllSwitches")]
	public Combat_Result battleResultSwitch;
	
	public MusicSettingsObject musicSettings;
	
	[Space]
	[OnValueChanged("UpdateCombat"), Range(0, 1), Space, FoldoutGroup("Combat")]
	public float playerHealth = 1;
    
	[OnValueChanged("UpdateCombat"), FoldoutGroup("Combat"), Tooltip("Have combatants started firing weapons yet?")]
	public bool startedFiring;

	[OnValueChanged("UpdateCombat"), FoldoutGroup("Combat")]
	public bool interiorView;

	[OnValueChanged("UpdateCombat"), FoldoutGroup("Combat"), Range(0, 300)] 
	public float nearestHostileDistance = 50;
	
	public List<GameObject> combatants = new List<GameObject>();

	[ReadOnly]
	public float creatureTimer;
	
	AKStateCall combatState;
	AKStateCall combatEnd;

	static CombatMusic _instance;
	public static CombatMusic Get()
	{
		if (_instance) return _instance;
		_instance = AKMusic.Get().GetComponent<CombatMusic>();
		return _instance;
	}

	/// <summary>
	/// Switches the combat type from ship to creature. This is on a cooldown, and will eventually return to ship.
	/// </summary>
	public static void CreatureCombat()
	{
		Get().creatureTimer = Get().musicSettings.creatureCooldown;
		Get().combatZoneTypeSwitch = Combat_Zone_Type.Creature;
		Get().PushAllSwitches();
	}

	
	public static void AddCombatant(GameObject newCombatant)
	{
		// Cull add attempts if the combatant is too far from the player
		if (DistFromPlayer(newCombatant) > Get().musicSettings.combatMaxRange) return;
		
		if (Get().combatants.Contains(newCombatant)) return;
		Get().combatants.Add(newCombatant);
	}

	public static void RemoveCombatant(GameObject combatant)
	{
		if (!AKMusic.Exists()) return;
		Get().combatants.Remove(combatant);
	}
	
	public static void ShotsFired(Vector3 position)
	{
		Get().startedFiring = true;

		if (PlayerManager.PlayerShip() == null) return;

		// If this shot was fired nearer than the nearest hostile distance, override the nearest hostile distance
		float dist = Vector3.Distance(PlayerManager.PlayerShip().transform.position, position);
		if (dist < Get().nearestHostileDistance)
			Get().nearestHostileDistance = dist;
	}

	static float DistFromPlayer(GameObject obj)
	{
		if (PlayerManager.PlayerShip() == null) return 9999;
		return Vector3.Distance(obj.transform.position, PlayerManager.PlayerShip().transform.position);
	}


	// Use this for initialization
	void Start () 
	{
		combatState = new AKStateCall("Ingame_Music", "Combat", musicSettings.combatPriority);
		combatEnd = new AKStateCall("Ingame_Music", "Combat_End", musicSettings.combatEndPriority);
		PushAllSwitches();
	}
	
	// Update is called once per frame
	void Update ()
	{
		interiorView = OrbitCam.CamMode() == CameraMode.Interior;

		playerHealth = PlayerHealth();

		// Creature cooldown - if any creatures are nearby, creature music takes priority. After the cooldown, returns to ship music.
		if (creatureTimer > 0)
			creatureTimer -= Time.deltaTime;
		else 
			combatZoneTypeSwitch = Combat_Zone_Type.Ship;

		if (nearestHostileDistance < 300) nearestHostileDistance += musicSettings.combatCooldownSpeed * Time.deltaTime;
		
		UpdateCombat();
	}
	
	public void PushAllSwitches()
	{
		SpiderSound.SetSwitch("Combat_Zone_Type", combatZoneTypeSwitch.ToString(), gameObject);
		SpiderSound.SetSwitch("Combat_State", combatStateSwitch.ToString(), gameObject);
		SpiderSound.SetSwitch("Combat_Health", healthSwitch.ToString(), gameObject);
		SpiderSound.SetSwitch("Combat_Fight_State", combatFightSwitch.ToString(), gameObject);
		SpiderSound.SetSwitch("Combat_Result", battleResultSwitch.ToString(), gameObject);
	}
	
	
	/// <summary>
	/// Sets the relevant combat state
	/// </summary>
	void UpdateCombat()
	{
		// Check if the range of the nearest hostile is fine to start combat music
		if (nearestHostileDistance > musicSettings.combatMaxRange)
		{
			AKMusic.Get().RemoveState(combatState, this);
			return;
		}
        
		// Set switch for player health
		healthSwitch = playerHealth < .25f ? Combat_Health.Low_Health_Loop : Combat_Health.Normal;
        
		// If combattants havent started firing, play pre-battle music
		combatStateSwitch = !startedFiring ? Combat_State.Prebattle : Combat_State.Fight;
        
		// overwrite combat state if in side view
		if (interiorView) combatStateSwitch = Combat_State.SideView;
        
		// Set distance from nearest hostile switch
		combatFightSwitch = nearestHostileDistance > musicSettings.intenseCombatRange ? 
			Combat_Fight_State.Out_Range : Combat_Fight_State.In_Range;
        
        
		PushAllSwitches();
		AKMusic.Get().AddState(combatState, this);
	}

	//Sets the exit combat music and times the return to normal music
	public void EndCombat(Combat_Result result)
	{
		battleResultSwitch = result;
		PushAllSwitches();
		AKMusic.Get().RemoveState(combatState, this, true);
	}

	


	float PlayerHealth()
	{
		if (PlayerManager.PlayerHull()) return PlayerManager.PlayerHull().NormalizedHp();
		return 0;
	}
}

#region enums
public enum Combat_State
{
	Prebattle,
	Captain_Time,
	Fight,
	SideView
}

public enum Combat_Result
{
	Neutral,
	Victory
}

public enum Combat_Fight_State
{
	In_Range,
	Out_Range
}

public enum Combat_Health
{
	Low_Health_Loop,
	Low_Health_Trans,
	Normal
}

public enum Combat_Zone_Type
{
	Ship,
	Creature
}

#endregion