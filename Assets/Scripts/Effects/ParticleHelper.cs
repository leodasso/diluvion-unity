using UnityEngine;
using System.Collections;
using HeavyDutyInspector;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleHelper : MonoBehaviour {

	ParticleSystem particles;

	[Comment("If true, each particle will match it's starting rotation to the system's world rotation. " +
		"This will only have an effect when simulating in world space.")]
	public bool particleGlobalRot;
    public float rotationSpeed = 3;
	[HideConditional(true, "particleGlobalRot", true)]
	public Vector3 offsetEulers;

	
	// Update is called once per frame
	void Update () {

		if (particles == null) particles = GetComponent<ParticleSystem>();

		if (particleGlobalRot) {

			Vector3 finalAngles = transform.eulerAngles + offsetEulers;
			Vector3 radians = finalAngles * Mathf.Deg2Rad;
            particles.startRotation3D = Vector3.MoveTowards(particles.startRotation3D, radians,  rotationSpeed*Time.deltaTime);
        }
	}
	
	public static void SetParticleForce(Vector3 forceDirection, ParticleSystem particleSystem)
	{
		var forceModule = particleSystem.forceOverLifetime;

		forceModule.enabled = true;
		forceModule.x = new ParticleSystem.MinMaxCurve(forceDirection.x);
		forceModule.y = new ParticleSystem.MinMaxCurve(forceDirection.y);
		forceModule.z = new ParticleSystem.MinMaxCurve(forceDirection.z);
	}

}
