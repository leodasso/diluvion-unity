using UnityEngine;
using System.Collections;
using Diluvion.Ships;
using Diluvion;

public class TriggerBeacon : Trigger
{


    public GameObject triggerSphere;

    public ArkBeacon beacon;
    ArkBeacon MyBeacon()
    {
        if (beacon != null) return beacon;
        beacon = GetComponent<ArkBeacon>();
        if (beacon == null) GetComponentInParent<ArkBeacon>();
        return beacon;
    }

    public void StartBossFight()
    {
        if (MyBeacon().GetComponent<ActivateBoss>())
        {
            MyBeacon().GetComponent<ActivateBoss>().BeaconEnabled();
        }
    }

    public override void TriggerAction(Bridge otherBridge)
    {
        if (GetComponent<SphereCollider>())
            GetComponent<SphereCollider>().enabled = false;
        triggerSphere.GetComponent<Animator>().SetTrigger("BeaconGrab");
    }

    public void StartGun()
    {
        MyBeacon().enabled = true;
        OrbitCam.ShakeCam(0.1f, transform.position);
        if (GetComponent<AKTriggerPositive>())
            GetComponent<AKTriggerPositive>().TriggerPos();
    }

    public void StartGlow()
    {
        Debug.Log("trying to start glow");
        if (!GetComponent<TriggerRemoveCage>()) return;
        GetComponent<TriggerRemoveCage>().Glow();
        if (GetComponent<AKTriggerCallback>())
            GetComponent<AKTriggerCallback>().Callback();
    }

    public void StartBlow()
    {    
        if (!GetComponent<TriggerRemoveCage>()) return;
        GetComponent<TriggerRemoveCage>().ExplodeFromThis();
    }


}
