using UnityEngine;
using System.Collections;
using Diluvion.Ships;

[RequireComponent(typeof(Collider))]
public class EndDemoTrigger : Trigger
{

	public override void TriggerAction (Bridge otherBridge)
	{
		base.TriggerAction (otherBridge);
        // TODO
        //StageManager.Get().EndGameCredits();
	}
}
