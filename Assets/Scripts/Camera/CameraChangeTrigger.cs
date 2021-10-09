using UnityEngine;
using System.Collections;
using Diluvion;
using Diluvion.Ships;

public class CameraChangeTrigger : Trigger {

    public float newCamDist = 25;
    public float transitionTimeIn = 5;
    public float transitionTimeOut = 5;

    bool activeCamEffect = false;


    public override void TriggerAction(Bridge otherBridge)
    {
        base.TriggerAction(otherBridge);
        // move the camera into position
        activeCamEffect = true;
        if ( OrbitCam.Get() ) OrbitCam.Get().SetAddlDist(newCamDist, transitionTimeIn);
    }

    public override void TriggerExitAction(Bridge otherBridge)
    {
        base.TriggerExitAction(otherBridge);

        // move the camera out of position
        if ( OrbitCam.Get() ) OrbitCam.Get().RemoveAddlDist(transitionTimeOut);
        activeCamEffect = false;
    }

    void OnDisable()
    {
        if (activeCamEffect)
            if ( OrbitCam.Get() ) OrbitCam.Get().RemoveAddlDist(transitionTimeOut);

        activeCamEffect = false;
    }
}
