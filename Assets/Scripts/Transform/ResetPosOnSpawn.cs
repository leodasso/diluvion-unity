using UnityEngine;
using System.Collections;
using PathologicalGames;

[RequireComponent(typeof(Rigidbody))]
public class ResetPosOnSpawn : MonoBehaviour {


    Vector3 localPosition;
   

	// Use this for initialization
	void Awake ()
    {
        RecordStartPos();
    }

	
    public void OnEnabled()
    {       
        SetStartPos();
    }

    void RecordStartPos()
    {
        localPosition = transform.localPosition;
    }

    public void SetStartPos()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        GetComponent<Rigidbody>().isKinematic = true;     
        transform.localPosition = localPosition;
        GetComponent<Rigidbody>().isKinematic = false;
    }
}
