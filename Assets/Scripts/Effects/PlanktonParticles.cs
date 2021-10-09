using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When added to a particle system, allows it to have a specific force added to all particles
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class PlanktonParticles : MonoBehaviour
{

	public Vector3 force;
	public float forceMultiplier = .2f;
	
	[Tooltip("How quickly the force will lerp back to zero")]
	public float normalization = 2;
	ParticleSystem _particleSystem;

	static List<PlanktonParticles> _all = new List<PlanktonParticles>();

	/// <summary>
	/// Sets the force over lifetime applied to all plankton particles.
	/// </summary>
	/// <param name="newForce">A world space force direction</param>
	public static void SetPlanktonForce(Vector3 newForce)
	{
		foreach (PlanktonParticles p in _all) p.force = newForce;
	}

	// Use this for initialization
	void Start ()
	{
		_particleSystem = GetComponent<ParticleSystem>();
		if (!_particleSystem) enabled = false;
		
		// add this instance to the global list
		if (_all == null) _all = new List<PlanktonParticles>();
		_all.Add(this);
	}


	
	// Update is called once per frame
	void Update ()
	{
		force = Vector3.Lerp(force, Vector3.zero, Time.deltaTime * normalization);
		
		if (force.magnitude > .05f)
			ParticleHelper.SetParticleForce(force * forceMultiplier, _particleSystem);
	}
}
