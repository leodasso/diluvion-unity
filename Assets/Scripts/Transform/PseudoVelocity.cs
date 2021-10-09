using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PseudoVelocity : MonoBehaviour {

    [Range(1, 10)]
    public int velocitySamples = 5;
    public Vector3 velocity;

    List<Vector3> velocities = new List<Vector3>();
    Vector3 compoundVelocity = Vector3.zero;
    Vector3 previousPos = Vector3.zero;

    //Intitializes the sample array
    public void Start()
    {
        OnEnabled();
    }

    public void OnEnabled()
    {
        velocity = Vector3.zero;
        compoundVelocity = Vector3.zero;
        previousPos = transform.position;
    }

    public void Update()
    {
        velocities.Add(PVelocity());
        if (velocities.Count > velocitySamples) velocities.RemoveAt(0);

        compoundVelocity = Vector3.zero;
        for (int i = 0; i < velocities.Count; i++)
            compoundVelocity += velocities[i];

        velocity = compoundVelocity / velocities.Count;
    }

    Vector3 PVelocity()
    {
        if (Time.deltaTime == 0) return Vector3.zero;
        Vector3 returnVel;
        returnVel = (transform.position - previousPos) / Time.deltaTime;
        returnVel = Vector3.ClampMagnitude(returnVel, 9999);
        previousPos = transform.position;
        return returnVel;
    }
}