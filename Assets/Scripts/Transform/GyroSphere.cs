using UnityEngine;
using System.Collections;

public class GyroSphere : MonoBehaviour
{
    Quaternion lookRotation;

    public void Update()
    {
        Gyro();
    }

    public void Gyro()
    {
        if ( transform.parent == null ) return;

        lookRotation = Quaternion.LookRotation(transform.parent.right, Vector3.up);

        transform.rotation = lookRotation * Quaternion.Euler(0, 90, 0); 

    }
}
