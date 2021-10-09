using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Diluvion.Ships;
using Diluvion;

public class EndBossTrigger : Trigger {

	public override void TriggerAction (Bridge otherBridge)
	{
		base.TriggerAction (otherBridge);
        // TODO fix CombatZone.Get().EndCombatNow();
        FadeOverlay.FadeInThenOut(delayTime, Color.white, SwapObjects);
	}

	void SwapObjects() 
	{
		ObjectActivator.TriggerChanges();
	}
}
