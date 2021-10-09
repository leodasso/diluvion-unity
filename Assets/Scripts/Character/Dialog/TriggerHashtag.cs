using UnityEngine;
using System.Collections;
using HeavyDutyInspector;
using Diluvion;
using Diluvion.Ships;

public class TriggerHashtag : Trigger {

	public string hashtag = "enterHashtagHere";
	[Button("Name object", "NameObject", true)] public bool hidden;

	public void NameObject() 
	{
		if (string.IsNullOrEmpty(hashtag)) return;
		gameObject.name = hashtag;
	}
		
	public override void TriggerAction (Bridge otherBridge)
	{
		base.TriggerAction (otherBridge);

		CrewManager otherCM = otherBridge.crewManager;

		if (otherCM == null) 						return;
		if (otherCM != PlayerManager.PlayerCrew()) 	return;

        Debug.Log("Triggered, sending tag " + hashtag);
		otherCM.BroadcastHashtag(hashtag);
	}
}