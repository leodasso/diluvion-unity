using UnityEngine;
using System.Collections;
using SpiderWeb;

/*
public class Shaker : MonoBehaviour 
{
	public static Shaker instance;
	public float shakesPerSecond = 30;
	public float stability = 5;
	public float shakeSpeed = 60;
	public float maxDistForShake = 20;		// Objects more than this distance from cam won't initiate a shake
	public float shakeIntensity = .1f;
	float shakeMagnitude = 0;
	Vector3 offsetPos = new Vector3(5, 5, 5);
	float newPosTimer = 0;
	float sin;

	//Vector3 originalpos;
	Quaternion originalRot;

	bool shaking = false;

	void Awake()
	{
		if(!instance) instance = this;
		originalRot = transform.localRotation;
	}


	public void Shake(float time, float newMagnitude, Vector3 shakeLoc)
	{
		if (TimeControl.timeScale == 0) return;

		float dist = Vector3.Distance(transform.position, shakeLoc);
		if (dist > maxDistForShake) return;

		// Get a shake intensity between 0 and 1 based on distance from the shake source
		float normalizedDist = Mathf.Clamp01((maxDistForShake - dist) / maxDistForShake);

		// Get the total magnitude based on magnitude of shake and distance from the shake source
		float totalMagnitude = normalizedDist * newMagnitude;

		// if shake magnitude is lower than the new magnitude, set it to new magnitude
		shakeMagnitude = Mathf.Clamp(shakeMagnitude, totalMagnitude, 999);
	}


	void Update() 
	{
		// create a sin wave
		sin = Mathf.Sin(Time.time * shakeSpeed);

		shakeMagnitude = Mathf.Lerp(shakeMagnitude, 0, Time.deltaTime * stability);

		Vector3 finalOffset = offsetPos * sin * shakeMagnitude;

		transform.localPosition = finalOffset;

		if (OrbitCam.Get().cameraMode == CameraMode.Normal) transform.localEulerAngles = finalOffset;

	}
}
*/