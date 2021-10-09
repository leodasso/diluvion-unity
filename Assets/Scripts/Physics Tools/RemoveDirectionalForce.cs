using UnityEngine;
using System.Collections;

public class RemoveDirectionalForce : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	void OnTriggerEnter(Collider other) {

		AddDirectionalForce otherDirForce = other.GetComponent<AddDirectionalForce>();

		if (otherDirForce != null) {
			Destroy(otherDirForce);
		}
	}
}
