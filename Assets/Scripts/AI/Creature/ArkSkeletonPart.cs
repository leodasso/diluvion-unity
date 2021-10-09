using UnityEngine;
using System.Collections;

public class ArkSkeletonPart : MonoBehaviour {

	public GameObject destroyedModel;

	SplineObject splineObject;

	// Use this for initialization
	void Start () {
		splineObject = GetComponent<SplineObject>();
	}

	public void Destruct() {
		StartCoroutine(TimedDestruct());
	}

	IEnumerator TimedDestruct() {

		yield return new WaitForSeconds(.1f);

		while (splineObject.sine < .95f) yield return null;
		Instantiate(destroyedModel, transform.position, transform.rotation);
		Destroy(gameObject);
		yield break;
	}
}
