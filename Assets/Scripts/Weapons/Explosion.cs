using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using SpiderWeb;

namespace Diluvion
{

	[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(SphereCollider))]
	public class Explosion : MonoBehaviour
	{
		[ReadOnly, Tooltip("The radius of the explosion is determined by the scale and the sphere collider radius")]
		public float radius;

		[AssetsOnly]
		public ExplosionStats explosionStats;

		public float critChance = 1;
		public float baseDamage = 10;
		public float baseForce = 100;
		List<IDamageable> _damageablesTouched = new List<IDamageable>();
		List<Rigidbody> _rigidbodiesTouched = new List<Rigidbody>();
		SphereCollider _sphereCollider;

		void Awake()
		{
			_sphereCollider = GetComponent<SphereCollider>();
			_sphereCollider.enabled = false;
		}

		public void SetRadius(float newRadius)
		{
			transform.localScale = Vector3.one * 2 * newRadius;
		}


		void OnTriggerEnter(Collider other)
		{
			if (!enabled) return;
			
			
			// Damage damageable
			var newDamageable = GO.ComponentInParentOrSelf<IDamageable>(other.gameObject);
			if (newDamageable != null)
			{
				if (!_damageablesTouched.Contains(newDamageable))
				{
					_damageablesTouched.Add(newDamageable);
					DamageObject(newDamageable, NormalizedDistanceFromCenter(other.transform.position));
					Destroy(Instantiate(explosionStats.explosionContactEffect, other.transform.position, Quaternion.identity), 5);
				}
			}

			// Apply force to rigidbody
			var newRigidbody = GO.ComponentInParentOrSelf<Rigidbody>(other.gameObject);
			if (newRigidbody != null)
			{
				if (!_rigidbodiesTouched.Contains(newRigidbody))
				{
					_rigidbodiesTouched.Add(newRigidbody);
					ApplyForce(newRigidbody, NormalizedDistanceFromCenter(other.transform.position));
				}
			}
		}

		float NormalizedDistanceFromCenter(Vector3 position)
		{
			float dist = Vector3.Distance(transform.position, position);
			return dist / Radius();
		}

		void DamageObject(IDamageable damageable, float normalizedDist)
		{
			float damage = baseDamage * explosionStats.damageCurve.Evaluate(normalizedDist);
			damageable.Damage(damage, critChance, gameObject);
		}

		void ApplyForce(Rigidbody rb, float normalizedDist)
		{
			float forceMagnitude = baseForce * explosionStats.forceCurve.Evaluate(normalizedDist);
			
			Debug.Log("Explosion " + name + " applying " + forceMagnitude + " force to " + rb.name);
			
			Vector3 randomPos = Random.onUnitSphere + rb.transform.position;
			Vector3 force = (rb.transform.position - transform.position ).normalized * forceMagnitude;
			rb.AddForceAtPosition(force, randomPos);
		}

		[Button]
		void UpdateRadius()
		{
			radius = Radius();
		}

		float Radius()
		{
			if (!_sphereCollider)
				_sphereCollider = GetComponent<SphereCollider>();
			if (!_sphereCollider)
			{
				Debug.Log("Explosion requires a sphere collider.");
				return 0;
			}

			return transform.localScale.x * _sphereCollider.radius;
		}
	}
}