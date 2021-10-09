using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FillingTube : MonoBehaviour
{
    public List<Rigidbody> allFillingRigidbodies = new List<Rigidbody>();
   
    public List<ResetPosOnSpawn> allResets = new List<ResetPosOnSpawn>();

    bool d;
   
    void Awake()
    {
        SetupLists(false);
    }

    public void SetupLists(bool debug)
    {
        allFillingRigidbodies = new List<Rigidbody>();
        allResets = new List<ResetPosOnSpawn>();
        d = debug;
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            if (debug)
            {
                if (!rb.GetComponent<ResetPosOnSpawn>())
                    allResets.Add(rb.gameObject.AddComponent<ResetPosOnSpawn>());
                else
                    allResets.Add(rb.GetComponent<ResetPosOnSpawn>());
            }

            rb.isKinematic = true;
            allFillingRigidbodies.Add(rb);

        }
    }

    public void Reset()
    {
        foreach (ResetPosOnSpawn rpos in allResets)
            rpos.SetStartPos();
    }

    //Adds explosive force at target byf orce to all my children and then fuck you kills them after 10 seconds
    public void AddExplosiveForceFrom(Transform target, float force)
    {

        foreach(Rigidbody rb in allFillingRigidbodies)
        {
            if (rb == null) continue;

            rb.isKinematic = false;

            rb.AddExplosionForce(force, target.position, 6000);
            if(!d)
               Debug.Log("DESPAWNING FROM:" + this.name , this); Destroy(rb.gameObject, 10);
        }

        
    }

}
