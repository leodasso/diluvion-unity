using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleQuality : MonoBehaviour {

	[Tooltip("the max particles for each quality level.  0 = cheapest")]
	public int[] maxParticles;

	int qualityLevel = 0;

	// Use this for initialization
	void Start () {
	
		qualityLevel = QualitySettings.GetQualityLevel();

		SetQuality(qualityLevel);
	}

	void SetQuality(int qLevel) {

		GetComponent<ParticleSystem>().maxParticles = maxParticles[qLevel];
	}
}