using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Diluvion
{
    using Ships;

    /// <summary>
    /// Used for easy grouping of weapon parts. When turrets and mounts are children of an object with this component,
    /// it will override their weapon module.
    /// <seealso cref="TurretRotator"/> <seealso cref="Mount"/>
    /// </summary>
    public class MountGroup : MonoBehaviour
    {
        public WeaponModule weaponModule;

        void Awake()
        {
            ApplyModule();
        }

        
        /// <summary>
        /// Applies my weapon module to all weapon parts in my transform.
        /// </summary>
        [Button]
        public void ApplyModule()
        {
            if (weaponModule == null) return;
            
            foreach (WeaponPart p in GetComponentsInChildren<WeaponPart>())
                p.weaponModule = weaponModule;
        }
    }
}