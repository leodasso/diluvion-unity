using UnityEngine;
using System.Collections;
using Diluvion;
using Diluvion.Ships;

public class GroundCreatureSpeedTrigger : MonoBehaviour {

	public GroundCreatureController target;
	public float moveSpeed;

	float initMoveSpeed;

	void Start() {

		initMoveSpeed = target.moveSpeed;
	}

	void OnTriggerEnter(Collider other) {

		Bridge otherBridge = other.GetComponent<Bridge>();
		if (otherBridge == null) return;

		if (otherBridge != PlayerManager.pBridge) return;

		target.moveSpeed = moveSpeed;
	}

	void OnTriggerExit(Collider other) 
	{
		Bridge otherBridge = other.GetComponent<Bridge>();
		if (otherBridge == null) return;

		if (otherBridge != PlayerManager.pBridge) return;

		target.moveSpeed = initMoveSpeed;
	}
}
