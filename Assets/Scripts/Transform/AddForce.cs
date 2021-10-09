using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Collider))]
public class AddForce : MonoBehaviour {
	
	[InfoBox("Adds the given force to any rigidbody that enters the trigger.")]

	public Vector3 forceToAdd;
	public bool oneTimeOnly = false;
	public bool oneObjectOnly = false;
	[ShowIf("oneObjectOnly")]
	public GameObject specificObject;

	int timesAdded = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	void OnTriggerEnter(Collider other) {

		if (other.GetComponent<Rigidbody>()) {
			AddMyForce(other.GetComponent<Rigidbody>());
		}
	}

	void AddMyForce(Rigidbody rb) {

		if (oneObjectOnly) {
			if (!rb.gameObject.name.Contains(specificObject.name)) return;
		}
		if (oneTimeOnly && timesAdded > 0) return;

		rb.AddForce(forceToAdd);

		timesAdded ++;
	}
}
