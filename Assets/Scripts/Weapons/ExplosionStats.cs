using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion
{
	[CreateAssetMenu(menuName = "Diluvion/Explosion stats")]
	public class ExplosionStats : ScriptableObject
	{

		[Tooltip("This should be a normalized curve, like all values between 0 and 1.")]
		public AnimationCurve damageCurve;

		[Tooltip("This should be a normalized curve, like all values between 0 and 1.")]
		public AnimationCurve forceCurve;

		[Tooltip("Effect that spawns when the explosion touches a damageable thing")]
		public GameObject explosionContactEffect;
	}
}