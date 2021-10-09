/*

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;
using Diluvion;

[RequireComponent(typeof(Rigidbody))]
public class Drive : MonoBehaviour 
{
	public Vector3 targetLoc;

	public bool driving = false;

	[Range(0, 5)] public float armingRotationSpeed 	= .2f;
    [Range(0, 5)] public float drivingRotationSpeed = .3f;

	float trueRotationSpeed;

	[Space]
	public float maxMovementSpeed;
	public float acceleration;
	public float strafeDragMagnitude;

	[Space]
	public List<Transform> propellers = new List<Transform>();
    public bool alternateProps = false;

	//float initAngularDrag;
	float currentSpeed;
	Vector3 localVelocity;
	Vector3 strafeDrag;
	Rigidbody rb;
	bool stopped = false;
    Vector3 targetDelta;
    float trueLerp = 0;

    void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}


	public void OnSpawned()
	{
		stopped = false;
		driving = false;
		currentSpeed = 0;
		localVelocity = Vector3.zero;
		strafeDrag = Vector3.zero;
		trueRotationSpeed = 0;
	}
		

	void Update() {

		if (stopped) return;

		// Determine the rotation speed
		float newRotSpeed = armingRotationSpeed;
		if (FullyArmed()) newRotSpeed = drivingRotationSpeed;

		trueRotationSpeed = Mathf.Lerp(trueRotationSpeed, newRotSpeed, Time.deltaTime * 8);

		if (driving) {

            rb.angularDrag = 3;
			
            int altRotate = 1;
            foreach (Transform t in propellers) {
				if (t == null) continue;
				t.Rotate (new Vector3(0, 0, 1000* altRotate) * Time.deltaTime, Space.Self);
                if (alternateProps)
                    altRotate *= -1;
            }
		}
	}

    public float CurrentSpeed() { return currentSpeed; }

	void FixedUpdate()
	{
		if (stopped) return;
        if ( !driving ) return;
        /*
		 targetDelta = Vector3.MoveTowards(targetDelta, targetLoc - transform.position, 8 * Time.deltaTime);
		
		//get the angle between transform.forward and target delta
		float angleDiff = Vector3.Angle(transform.forward, targetDelta);
		
		// get its cross product, which is the axis of rotation to get from one vector to the other
		Vector3 cross = Vector3.Cross(transform.forward, targetDelta);
		
		// apply torque along that axis according to the magnitude of the angle.
		rb.AddTorque(cross * angleDiff * trueRotationSpeed * rb.mass);
        *

        // Get rotation to move towards
        Quaternion newRot = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(targetLoc - transform.position), trueRotationSpeed * Time.deltaTime);
        rb.MoveRotation(newRot);

		if(!driving) return;

		localVelocity = transform.InverseTransformDirection(rb.velocity);

		//the drag from the ship moving sideways / up and down
		strafeDrag = new Vector3( localVelocity.x, localVelocity.y, 0);	
		rb.AddRelativeForce(-strafeDrag * strafeDragMagnitude);
		currentSpeed = localVelocity.z;

		//apply speed
		if(currentSpeed < maxMovementSpeed) 
			rb.AddForce(transform.forward * acceleration * rb.mass);
	}

	bool FullyArmed() {
		Targeting targeting = GetComponent<Targeting>();
		if (!targeting) return false;

		if (targeting.armed) return true;
		return false;
	}


	public void Break() {
		stopped = true;
	}
}
*/