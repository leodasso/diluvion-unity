using UnityEngine;
using System.Collections;
using Diluvion;

public class SimpleGun : MonoBehaviour {

	public Rigidbody ammo;
	public Vector3 fireVector;
	public float fireVelocity;

	Vector3 normFireVector;
	Hull localHull;

	void OnDrawGizmosSelected() {

		normFireVector = transform.TransformDirection(fireVector).normalized;

		Gizmos.color = Color.green;
		Gizmos.DrawRay(transform.position, normFireVector * 3);
	}

	// Use this for initialization
	void Start () {
	
		localHull = GetComponent<Hull>();
		if (localHull == null)
			localHull = GetComponentInParent<Hull>();
	}
	
	// Update is called once per frame
	public GameObject Fire() {

		// Get the normalized direction of fire
		normFireVector = transform.TransformDirection(fireVector).normalized;

		Rigidbody newAmmo = Instantiate(ammo, transform.position, transform.rotation) as Rigidbody;

		// Set velocity of the ammo
		newAmmo.velocity = normFireVector * fireVelocity;

		// Check for hull
		if (localHull != null) {

			SimpleAmmo simpleAmmo = newAmmo.GetComponent<SimpleAmmo>();
			if (simpleAmmo != null) {
				simpleAmmo.friendHull = localHull;
				simpleAmmo.Init();
			}
		}

		return newAmmo.gameObject;
	}

	/// <summary>
	/// Fire and pass the target to appropriate components of the ammo
	/// </summary>
	public GameObject Fire(Transform target) {

		GameObject newAmmo = Fire();

		MagneticMovement magMove = newAmmo.GetComponent<MagneticMovement>();
		if (magMove) magMove.target = target;

		return newAmmo;
	}
}
