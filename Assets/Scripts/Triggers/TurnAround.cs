using UnityEngine;
using System;
using HeavyDutyInspector;
using Diluvion.Ships;

[Serializable]
public class ControlOverride {

	public float overrideTime = 5;
	public Vector3 point;
	public Throttle throttle;
}

public class TurnAround : Trigger {

	[Comment("What should the camera be looking at when the turnaround is happening?")]
	public GameObject lookAt;

	[Comment("Which way will it send the player?")]
	public Vector3 turnaroundDir;

	[Comment("When the player enters the hit box, how long to you want to override their controls," +
		"and at what throttle? Point will automatically be set based on turnaroundDir")]
	public ControlOverride controlOverride;
	public bool showPopup = false;
	[HideConditional(true, "showPopup", true)]
    public PopupObject popup;

	public void OnDrawGizmosSelected() {

		Gizmos.color = Color.red;
		Vector3 dir = turnaroundDir;
		Gizmos.DrawRay(transform.position, dir);
		Gizmos.DrawWireSphere(transform.position + dir, .3f);
	}

    public override void TriggerAction(Bridge otherBridge)
    {
        base.TriggerAction(otherBridge);
        ShipControls otherControls = otherBridge.GetComponent<ShipControls>();
        if (otherControls == null) return;

        if (showPopup) popup.CreateUI();
        controlOverride.point = otherBridge.transform.position + turnaroundDir;
        otherControls.NewOverride(controlOverride);

    }
}
