using UnityEngine;
using System.Collections;

public class Bobber : MonoBehaviour {

	public float bobbingHeight = 1.0f;
	public float bobbingSpeedModifier = 1.0f;

	private float sineMod = 0;
	private float sineTimer = 0;
	private float originalY = 0;


	void Awake () {
		originalY = gameObject.transform.position.y;

		//Random start offset
		sineTimer += Random.Range (10.0f, 20.0f);
	}
	
	// Update is called once per frame
	void Update () {

		sineTimer += (Time.deltaTime*bobbingSpeedModifier);
		sineMod = bobbingHeight* Mathf.Sin (sineTimer);
		gameObject.transform.position = new Vector3 (gameObject.transform.position.x, originalY + sineMod, gameObject.transform.position.z);
	
	}
}
