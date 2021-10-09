using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;

public class ThornChaser : MonoBehaviour {

	public float gunCooldown = 1;
	public float fireDistance = 20;

	public float startDelay = 2;

	int gunIndex = 0;
	List<SimpleGun> guns = new List<SimpleGun>();
	Transform target;
	MagneticMovement magneticMovement;
	bool inSafeZone;

	// Use this for initialization
	IEnumerator Start () {
	
		guns.AddRange(GetComponentsInChildren<SimpleGun>());
		magneticMovement = GetComponent<MagneticMovement>();

		yield return new WaitForSeconds(.1f);
		target = magneticMovement.target;

		yield return new WaitForSeconds(startDelay);

		StartCoroutine(FireLoop());
		yield break;
	}

	IEnumerator FireLoop() {

		// don't fire if no target
		while (target == null) {
			target = magneticMovement.target;
			yield return new WaitForSeconds(.1f);
		}

		// Don't fire if not in distance
		while (!Calc.WithinDistance(fireDistance, transform, target))
			yield return new WaitForSeconds(.1f);


		Fire();
		yield return new WaitForSeconds(gunCooldown);

		StartCoroutine(FireLoop());
		yield break;
	}

	/*
	void OnTriggerEnter(Collider other) {

		if (other.gameObject.tag == "Safe"){

			magneticMovement.Repel();
			inSafeZone = true;
			StartCoroutine(SafeZoneCooldown());
		}
	}
	*/

	IEnumerator SafeZoneCooldown() {
		yield return new WaitForSeconds(5);
		inSafeZone = false;
	}


	void Fire() {

		if (target == null) return;

		SimpleGun currentGun = guns[gunIndex];
		currentGun.Fire(target);
         if (GetComponent<AKTriggerCallback>())
         GetComponent<AKTriggerCallback>().Callback();
        gunIndex++;
		if (gunIndex >= guns.Count)
			gunIndex = 0;
	}
}
