using UnityEngine;
using System.Collections.Generic;
using HeavyDutyInspector;

public class TriggerAnimationFromDock : MonoBehaviour {

	[Comment("Place on any object with DockControl component, and it will" +
		"call animation triggers when this object is docked to / undocked from.")]

	public Animator animatorToTrigger;
    public List<Animator> animatorsToTrigger = new List<Animator>();

	public bool onDock;
	[HideConditional(true, "onDock", true)]
	public string triggerOnDock;

	public bool onUndock;
	[HideConditional(true, "onUndock", true)]
	public string triggerOnUndock;

	// Use this for initialization
	void Start () 
	{
		if (animatorToTrigger == null) enabled = false;
	}
	
	public void OnDock() {
		if (!onDock) return;

		Debug.Log("Triggered on dock " + triggerOnDock);
		animatorToTrigger.SetTrigger(triggerOnDock);
        foreach ( Animator anim in animatorsToTrigger ) anim.SetTrigger(triggerOnDock);
	}

	public void OnUndock() {
		if (!onUndock) return;

		Debug.Log("Triggered on undock " + triggerOnUndock);
		animatorToTrigger.SetTrigger(triggerOnUndock);
        foreach ( Animator anim in animatorsToTrigger ) anim.SetTrigger(triggerOnUndock);
	}
}
