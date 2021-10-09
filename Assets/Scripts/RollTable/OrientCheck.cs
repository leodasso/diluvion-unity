
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion.Roll
{
    /// <summary>
    /// OrientCheck just rotates the object to follow either global or local up
    /// </summary>
    [CreateAssetMenu(fileName = "new orientCheck", menuName = "Diluvion/SpotFinder/orientCheck")]
    public class OrientCheck : SpawnCheck
    {
        public enum OrientType { Global, SpawnersLocal };
        public OrientType orientation = OrientType.Global;
        public bool randomizeRot = false;

       
        /// <summary>
        /// Rotate the input rotation to fit global or local up, randomize again if needed
        /// </summary>
        public override bool ValidCheck(Vector3 checkStart, float radius, ref Vector3 position, ref Quaternion rotation)
        {
            Vector3 inputUp = rotation * Vector3.up;
            Vector3 lookDirection = rotation * Vector3.forward;           
         
            Vector3 targetUp;
            if (orientation == OrientType.Global)
                targetUp = Vector3.up;
            else
                targetUp = inputUp;

            if (randomizeRot)            
                lookDirection = Quaternion.AngleAxis(Random.Range(0, 360), targetUp) * lookDirection;            

            rotation = Quaternion.LookRotation(lookDirection, targetUp);
            return true;
        }

        public override string ToString()
        {
            return " and, rotates up to follow " + orientation.ToString();
        }
    }
}