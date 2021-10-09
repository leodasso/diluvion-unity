using UnityEngine;
using System.Collections;

public class BeaconSafeZone : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	void OnTriggerEnter(Collider other) {

		MagneticMovement otherMagnet = other.GetComponent<MagneticMovement>();

		if (otherMagnet) otherMagnet.Repel();
	}
}
