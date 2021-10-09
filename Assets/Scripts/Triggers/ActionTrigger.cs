using UnityEngine;
using System.Collections.Generic;
using Diluvion;
using Diluvion.Ships;

public class ActionTrigger : Trigger {

    public List<Action> actionsToTrigger = new List<Action>();

    public override void TriggerAction(Bridge otherBridge)
    {
        base.TriggerAction(otherBridge);
        foreach (Action a in actionsToTrigger) a.DoAction(otherBridge);
    }
}
