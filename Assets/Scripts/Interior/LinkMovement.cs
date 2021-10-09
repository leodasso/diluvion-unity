using UnityEngine;
using System.Collections;

public class LinkMovement : MonoBehaviour {

	public bool linkX = false;
	public bool linkY = false;
	public Vector2 translateRatios;

	public bool linkRotation = false;
	public float rotateRatio = 1;

	public Transform linkedTransform;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if (linkX || linkY) {
			LinkTranslate();
		}

		if (linkRotation) {
			LinkRotate();
		}
	
	}

	void LinkTranslate() {

	}

	void LinkRotate() {

		float newRot = linkedTransform.localEulerAngles.z * rotateRatio;
		transform.localEulerAngles = new Vector3 (0, 0, newRot);
	}
}
