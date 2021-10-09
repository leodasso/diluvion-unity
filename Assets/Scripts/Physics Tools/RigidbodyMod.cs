using UnityEngine;
using System.Collections;

public class RigidbodyMod : MonoBehaviour
{
    public Vector3 torque;

    Rigidbody myRB;
	
	void Start () {
        ModRigidbody();

    }


    void ModRigidbody()
    {
        myRB = GetComponent<Rigidbody>();
        if (myRB) return;
        if(torque != Vector3.zero)
        {
            myRB.AddRelativeTorque(torque, ForceMode.Impulse);
        }

    }
	
}
