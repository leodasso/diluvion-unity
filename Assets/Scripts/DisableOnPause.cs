using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DisableOnPause : MonoBehaviour {

	List<Rigidbody> dynamicRigidbodies;

	// Use this for initialization
	void Start () {

		dynamicRigidbodies = new List<Rigidbody>();
	
		//add all dynamic rigidbodies to a list
		foreach (Transform t in transform) {

			if (t.GetComponent<Rigidbody>()) {
				if (!t.GetComponent<Rigidbody>().isKinematic) {
					dynamicRigidbodies.Add(t.GetComponent<Rigidbody>());
				}
			}
		}
	}

	void OnPause() {

		foreach (Rigidbody rb in dynamicRigidbodies) {
			rb.isKinematic = true;
		}
	}

	void OnResume() {
		foreach (Rigidbody rb in dynamicRigidbodies) {
			rb.isKinematic = false;
		}
	}
}
