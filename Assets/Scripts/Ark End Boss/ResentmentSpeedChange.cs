using UnityEngine;
using System.Collections;


/// <summary>
/// Changes the speed of the 'Ark Resentment' boss when the head enters this trigger.
/// </summary>
public class ResentmentSpeedChange : MonoBehaviour {

	public float newSpeed = 100;

	void Start() {
		MeshRenderer mr = GetComponent<MeshRenderer>();
		if (mr != null) mr.enabled = false;
	}

	// Use this for initialization
	void OnTriggerEnter(Collider other) {

		ArkHead arkHead = other.GetComponent<ArkHead>();

		if (arkHead == null) return;

		Debug.Log(name + " hit by ark head.");

		arkHead.ChangeSpeed(newSpeed);
	}
}
