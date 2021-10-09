using UnityEngine;
using System.Collections;


/// <summary>
/// Magnetically moves towards the target.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class MagneticMovement : MonoBehaviour {

	public Transform target;
	public float magneticPower = 5;
	public float maxMagnitude = 100;
	public bool inverse;
	public float minDistToTarget = 10;

	float sqMinDist;
	float sqMagnitude;
	float sqMaxMag;
	Rigidbody rb;
	bool canForce = true;
	float repelCooldownTime = 3;

	void Start() {
		rb = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void FixedUpdate() {

		if (target == null) return;
		if (!canForce) return;

		sqMinDist = minDistToTarget * minDistToTarget;
		sqMaxMag = maxMagnitude * maxMagnitude;

		Vector3 direction = target.position - transform.position;
		sqMagnitude = Mathf.Clamp(Vector3.SqrMagnitude(direction), 0, sqMaxMag);

		float totalPower = magneticPower / sqMagnitude;
		if (inverse) totalPower = magneticPower * sqMagnitude;

		if (sqMagnitude > sqMinDist)
			rb.AddForce(direction.normalized * totalPower);
	}

	IEnumerator RepelCooldown() {

		yield return new WaitForSeconds(repelCooldownTime);

		canForce = true;
	}

	/// <summary>
	/// Inverses the velocity
	/// </summary>
	public void Repel() {
		
		canForce = false;
		rb.velocity = -rb.velocity;

		StartCoroutine(RepelCooldown());
	}
}
