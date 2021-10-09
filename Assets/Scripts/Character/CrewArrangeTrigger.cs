using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Diluvion.Ships;
using Sirenix.OdinInspector;
using CharacterInfo = Diluvion.CharacterInfo;


[System.Serializable]
public class CrewArrange
{
	[Tooltip("Moves this character to the station of this type."), AssetsOnly] 
	public CharacterInfo character;
	[AssetsOnly]
	public ShipModule module;
}

public class CrewArrangeTrigger : Trigger {

	public List<CrewArrange> crewMovements;
	public bool clearGuests;

	public override void TriggerAction (Bridge otherBridge)
	{
		base.TriggerAction (otherBridge);

		CrewManager crewManager = otherBridge.crewManager;
		if (crewManager == null) return;

		foreach (CrewArrange crewArrange in crewMovements) 
			crewManager.MoveCrew(crewArrange.character, crewArrange.module);

		if (clearGuests) crewManager.RemoveAllGuests();
	}
}
