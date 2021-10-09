using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the movement of children of the beacon parent. Shows how much energy is left and recoil
/// by the position. Adjust speed and recoil on different pieces differently for awesome effect!
/// </summary>
public class BeaconCannonPart : MonoBehaviour {

	public float speed = 10;
	public float recoilAmount = .5f;
	public float recoilRotation = 10;

	ArkBeacon parentBeacon;
	float initY;
	float gotoY;
	float recoil = 0;

	float rotSpeed = 0;

	// Use this for initialization
	void Start () {

		gotoY = initY = transform.localPosition.y;
		parentBeacon = GetComponentInParent<ArkBeacon>();

		// Add recoil to the fire delegate
		parentBeacon.onFire += Recoil;
	}
	
	// Update is called once per frame
	void Update () {

		// Check recoil
		recoil = Mathf.Lerp(recoil, 0, Time.deltaTime * speed) * parentBeacon.energyValue;

		// Set the new local Y value
		gotoY = (parentBeacon.energyValue + recoil) * initY;

		Vector3 pos = transform.localPosition;
		Vector3 newPos = new Vector3(pos.x, gotoY, pos.z);

		transform.localPosition = Vector3.Lerp(transform.localPosition, newPos, Time.deltaTime * speed);

		// Spin rotation
		transform.Rotate( new Vector3(0, recoilRotation * rotSpeed * Time.deltaTime, 0), Space.Self);
		rotSpeed = Mathf.Lerp(rotSpeed, 0, Time.deltaTime);
	}

	void Recoil() {
		recoil += recoilAmount;
		rotSpeed += recoilAmount;
	}
}
