using UnityEngine;
using System.Collections;
using Diluvion.Ships;

/// <summary>
/// Calls the given trigger when the player enters the trigger collider component on this.
/// </summary>
public class AnimationModifier : MonoBehaviour {

	public string trigger;
	public Animator animator;

	// Use this for initialization
	void Start () {
	
	}

	void OnTriggerEnter(Collider other) {

		ShipControls otherShipControls = other.GetComponent<ShipControls> ();

		if (otherShipControls) {
			animator.SetTrigger(trigger);
		}
	}
}
