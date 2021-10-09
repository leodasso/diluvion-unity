using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion.Ships;

namespace Diluvion
{
    /// <summary>
    /// Any part of the system of weapon mounts and turrets.
    /// </summary>
    public class WeaponPart : MonoBehaviour
    {
        public WeaponModule weaponModule;


        public virtual bool FireReady()
        {
            return true;
        }
    }
}