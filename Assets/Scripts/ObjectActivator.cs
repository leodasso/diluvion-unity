using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectActivator : MonoBehaviour {

	public static List<ObjectActivator> objectsToTrigger = new List<ObjectActivator>();

	public bool willBeActive;
	public bool activeOnStart = true;

	// Use this for initialization
	void Start () {

		if (!objectsToTrigger.Contains(this)) objectsToTrigger.Add(this);

		gameObject.SetActive(activeOnStart);

	}

	/// <summary>
	/// Triggers all objects with activator component to activate / deactivate
	/// </summary>
	public static void TriggerChanges() {
		foreach (ObjectActivator activator in objectsToTrigger) {
			if (activator == null) continue;
			activator.gameObject.SetActive(activator.willBeActive);
		}
	}

	public static void Reset() {

		foreach (ObjectActivator activator in objectsToTrigger) {
			if (activator == null) continue;
			activator.gameObject.SetActive(activator.activeOnStart);
		}
	}
		
}
