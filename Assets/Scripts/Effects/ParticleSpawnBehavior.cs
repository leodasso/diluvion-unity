using UnityEngine;
using System.Collections;

public class ParticleSpawnBehavior : MonoBehaviour {

	ParticleSystem pSystem;

	// Use this for initialization
	void Start () {

        pSystem = GetComponent<ParticleSystem>();

	}
	
	void OnSpawned() {
		if (pSystem != null) {
            pSystem.Clear ();
		}
	}
}
