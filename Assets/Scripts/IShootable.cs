using System;
using UnityEngine;

namespace Diluvion
{

    /// <summary>
    /// Interface for things that can be impacted. Used to generate effects.
    /// </summary>
    public interface Impactable 
    {
        /// <summary>
        /// Another object impacted this!
        /// </summary>
        /// <param name="impactVector">Direction of the impact</param>
        /// <param name="point">World space position of the impact</param>
        void Impact(Vector3 impactVector, Vector3 point);
    }

    /// <summary>
    /// Interface for things that can be damaged. 
    /// </summary>
    public interface IDamageable : Impactable
    {
        event Action<GameObject> onKilled;
        
        /// <summary>
        /// Do damage to the object!
        /// </summary>
        /// <param name="damage">Amount of damage</param>
        /// <param name="critChance">Chance of critical hit.this value is from 0 to n, and crit chance is a product of this number & defense</param>
        /// <param name="source">the object doing the damage</param>
        void Damage(float damage, float critChance, GameObject source);

        float NormalizedHp();
    }
}