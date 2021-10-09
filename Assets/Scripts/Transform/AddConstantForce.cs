using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Diluvion;
using Diluvion.Ships;

public class AddConstantForce : MonoBehaviour {

	[InfoBox("Adds force on every fixed update to the list of rigidbodies.")]
	public Vector3 force;
	
	[ToggleLeft]
	public bool local;
	
	[Space]

	[ToggleLeft]
    public bool useTrigger = true;
	[ToggleLeft]
	public bool playerOnly;

	public List<Rigidbody> bodies = new List<Rigidbody>();

	void OnDrawGizmosSelected() {

		Gizmos.color = Color.cyan;

		Gizmos.DrawRay(transform.position, FinalForce());
	}

	// Use this for initialization
	void Start () {
	
		//gameObject.layer = LayerMask.NameToLayer("Tools");
	}

	Vector3 FinalForce() {
		Vector3 finalForce = force;
		if (local) finalForce = transform.TransformDirection(finalForce);
		return finalForce;
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		foreach (Rigidbody rb in bodies) rb.AddForce(FinalForce());
	}

	void OnTriggerEnter(Collider other) {

        if ( !useTrigger ) return;

		Rigidbody otherRB = other.GetComponent<Rigidbody>();
		if (otherRB == null) return;

		if (playerOnly) {
			Bridge otherBridge = otherRB.GetComponent<Bridge>();
			if (otherBridge != PlayerManager.pBridge) return;
		}

		if (!bodies.Contains(otherRB)) bodies.Add(otherRB);
	}

	void OnTriggerExit(Collider other) {

        if ( !useTrigger ) return;

		Rigidbody otherRB = other.GetComponent<Rigidbody>();
		if (otherRB == null) return;

		if (bodies.Contains(otherRB)) bodies.Remove(otherRB);
	}
}
