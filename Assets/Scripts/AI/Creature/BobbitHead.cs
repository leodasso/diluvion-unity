using UnityEngine;
using System.Collections;
using Diluvion.Ships;

public class BobbitHead : MonoBehaviour
{

    public delegate void InRange(Transform thing);
    public InRange inRange;

    public Rigidbody grabBase;

    FixedJoint grabJoint;

    Transform currentInRange;
    void SetInrange(Transform t)
    {
        string tName = "null";
        if (t != null)
            tName = t.name;
        Debug.Log("Setting Inrange to " + tName);
        currentInRange = t;
        if (inRange != null)
        {
            Debug.Log("Attempting to set " + tName + " inrange");
            inRange(currentInRange);
        }
    }

    public void Grab(Rigidbody targetR)
    {
        if (targetR == null) return;
        grabJoint = grabBase.gameObject.AddComponent<FixedJoint>();
        grabJoint.connectedBody = targetR;

    }

    public void LetGo()
    {
        if (grabJoint == null) return;
        grabJoint.connectedBody = null;
        Destroy(grabJoint);
    }

    public void OnTriggerEnter(Collider col)
    {
        if (col.transform == transform.parent) return;
        if (col.GetComponent<Bridge>())           
                SetInrange(col.transform);

    }


    public void OnTriggerExit(Collider col)
    {
        if (col.transform == transform.parent) return;
        if (col.transform==currentInRange)           
                SetInrange(null);
    }

}




