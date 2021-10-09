using Diluvion.Ships;
using UnityEngine.Events;
using UnityEngine;
using Sirenix.OdinInspector;

public class TriggerUnityEvent : Trigger
{
    [DrawWithUnity]
    public UnityEvent onValidTriggerEnter;

    [DrawWithUnity]
    public UnityEvent onValidTriggerExit;
    
    public override void TriggerExitAction(Bridge otherBridge)
    {
        base.TriggerExitAction(otherBridge);
        onValidTriggerExit.Invoke();
        
    }

    public override void TriggerAction(Bridge otherBridge)
    {
        base.TriggerAction(otherBridge);
        onValidTriggerEnter.Invoke();
    }
}
