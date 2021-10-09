using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion.Roll
{
    /// <summary>
    /// Performs a  OverlapSphere check at the location, returning false if we intersect with something of the input mask
    /// </summary>
    [CreateAssetMenu(fileName = "new sphereCheck", menuName = "Diluvion/SpotFinder/sphereCheck")]
    public class SphereCheck : SpawnCheck
    {
        [Tooltip("The buffer adds to the input Radius of the check")]
        public float buffer = 0;
        public LayerMask mask;

        public override bool ValidCheck(Vector3 checkStart, float radius, ref Vector3 position, ref Quaternion rotation)
        {
            Collider[] results = new Collider[15];
            if (Physics.OverlapSphereNonAlloc(checkStart, radius + buffer, results, mask) > 0)
            {
                //Debug.Log("Found something in the way on " + name + " sphereCastCheck", this);

                return false;
            }
            else return true;
        }

        public override string ToString()
        {
            return "   We <color=red>dont</color> collide against the input layer(s) within the input <b> radius+" + buffer + "</b>";
        }
    }
}
