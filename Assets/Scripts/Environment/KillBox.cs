using UnityEngine;
using System.Collections;
using Diluvion;

/// <summary>
/// Does damage to any hull it collides with. 
/// </summary>
public class KillBox : MonoBehaviour {

	public float damagePerCollision = 50;

	
	void OnCollisionEnter(Collision other) {

		Hull otherHull = other.collider.GetComponent<Hull>();
		DamageHull(otherHull);
	}

	void OnTriggerEnter(Collider other) {
		Hull otherHull = other.GetComponent<Hull>();
		DamageHull(otherHull);
	}

	void DamageHull(Hull hull) 
	{
		if (hull == null) 				return;
		if (damagePerCollision <= 0) 	return;

		hull.Damage(damagePerCollision,  1, gameObject);
	}
}
