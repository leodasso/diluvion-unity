using UnityEngine;
using System.Collections;
using Diluvion;
using Diluvion.Ships;

public class CamClampsTrigger : Trigger {

    [Range(-90, 25)]
    public float newLowerClamp = -85;

    [Range(-25, 90)]
    public float newUpperClamp = 85;


    /// <summary>
    /// When a collider enters my trigger, apply my clamp angles to the camera.
    /// </summary>
    /// <param name="otherBridge"></param>
    public override void TriggerAction(Bridge otherBridge)
    {
        base.TriggerAction(otherBridge);
        if ( OrbitCam.Get() == null ) return;
        OrbitCam.Get().SetClampAngles(newLowerClamp, newUpperClamp);
    }

    public override void TriggerExitAction(Bridge otherBridge)
    {
        base.TriggerExitAction(otherBridge);
        if ( OrbitCam.Get() == null ) return;
        OrbitCam.Get().DefaultClampAngles();
    }

}
