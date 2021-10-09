using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Diluvion.Ships;


public class AnimationTrigger : Trigger
{

    public Animator animator;
    
    [Tooltip("The name of the animator trigger to call")]
    public string trigger;

    public override void TriggerAction(Bridge otherBridge)
    {
        if (!animator)
        {
            Debug.LogError("No animator is defined! Can't trigger animation.", gameObject);
            return;
        }
        
        animator.SetTrigger(trigger);
    }
}
