using UnityEngine;
using System.Collections;
using HeavyDutyInspector;
using Diluvion.Ships;

public class EndlessCorridorTrigger : Trigger {

	[Comment("If true, will set the endless corridor to active. Otherwise, sets it inactive.")]
	public bool setsActive;

	public override void TriggerAction (Bridge otherBridge)
	{
		base.TriggerAction (otherBridge);
		EndlessCorridor.Get().SetCorridorActive(setsActive);
	}
}
