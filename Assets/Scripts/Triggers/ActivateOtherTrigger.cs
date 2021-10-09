using UnityEngine;
using System.Collections;
using HeavyDutyInspector;
using Diluvion.Ships;

public class ActivateOtherTrigger : Trigger {

    public GameObject toActivate;
    public bool triggerOnEnter = true;
    [HideConditional(true,"triggerOnEnter", true)]
    public bool activeOnEnter = true;
    public bool triggerOnExit = false;
    [HideConditional(true, "triggerOnExit", true)]
    public bool activeOnExit = false;

    public override void TriggerAction(Bridge other)
    {
        if(triggerOnEnter)
            toActivate.SetActive(activeOnEnter);
    }

    public override void TriggerExitAction(Bridge other)
    {
        if (triggerOnExit)
            toActivate.SetActive(activeOnExit);
    }
}
