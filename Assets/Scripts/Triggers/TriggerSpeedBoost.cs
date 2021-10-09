using UnityEngine;
using Diluvion.Ships;

public class TriggerSpeedBoost : Trigger {

    public float power = 1.7f;
    public GameObject propellerSpeedEffect;

    public GameObject parentObject;

    public override void TriggerAction(Bridge otherBridge)
    {
        ApplySpeedBoost(otherBridge);
    }

    //Applies the speed boost effect
    public void ApplySpeedBoost(Bridge b)
    {
        if (GetComponent<SphereCollider>())
            GetComponent<SphereCollider>().enabled = false;
    
        ShipAnimator anim = b.GetComponent<ShipAnimator>();
        ShipMover sm = b.GetComponent <ShipMover>();
        /*
        if (anim != null)
        {
            foreach (Transform t in anim.propellers)
            {
                GameObject ti = (GameObject)Instantiate(propellerSpeedEffect, t.position, t.rotation);
                ti.transform.SetParent(t);
            }
        }*/
        if(sm!= null)
        {
            sm.SetExtraEnginePower(power);
        }
        if(parentObject!=null)
            Destroy(parentObject);
    }

   
}
