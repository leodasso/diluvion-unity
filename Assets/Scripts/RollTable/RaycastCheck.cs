using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpiderWeb;

namespace Diluvion.Roll
{ 
    [CreateAssetMenu(fileName = "new raycastCheck", menuName = "Diluvion/SpotFinder/raycastCheck")]
    public class RaycastCheck : SpawnCheck {

        [Tooltip("The Direction we cast the ray")]
        public Vector3 castDirection = -Vector3.up;

        [Tooltip("The range of the ray, returns false if nothing is hit")]
        public float range = 40;

        [Tooltip("Use the normal up")]
        public bool useNormalUp = false;

        [Tooltip("The size of the rotation variance along the x axis")]
        public float droopWithGravity = 0;


        [Tooltip("Mask we cast against")]
        public LayerMask mask;


        private void OnEnable()
        {
            if (mask.value != 0) return;
            mask = Calc.IncludeLayer("Terrain");
        }
        /// <summary>
        ///  Raycast Function for finding a position on the input
        /// </summary>
        /// <param name="checkStart"> Start Point of the Raycast</param>     
        public override bool ValidCheck(Vector3 checkStart, float radius,  ref Vector3 position, ref Quaternion rotation)
        {
  
            Ray plantRay = new Ray(checkStart, castDirection);
           // Debug.DrawRay(plantRay.origin, plantRay.direction * 15, Color.red, 15);
            RaycastHit hit;

            //Cast a ray downwards and find a nice spot to check for other spawnables
            if (Physics.Raycast(plantRay, out hit, range, mask))
            {
                Vector3 hitNormal = hit.normal;
        
                Vector3 rayUp = hit.point - checkStart;
                
                //Rotation Around the normal
                Vector3 temp = Vector3.Cross(hitNormal, rayUp);
                if (temp == Vector3.zero)
                    temp = Vector3.right;
                
                Vector3 lookDirection = Vector3.Cross(hitNormal, temp);
                lookDirection = Quaternion.AngleAxis(Random.Range(0, 360), hitNormal) * lookDirection;
                
                Debug.DrawRay(hit.point, temp.normalized*5, Color.red,1);
                Debug.DrawRay(hit.point, lookDirection.normalized*5, Color.blue,1);
              
                //Roof flipping behaviour
                float upsideDown = 1;
                if (hitNormal.y < 0&&!useNormalUp) //stop the inversion of the normal on ceilings
                    upsideDown = -1;
                
                Vector3 varianceNormal = (hitNormal * upsideDown + (Vector3.up * droopWithGravity)).normalized;
                
                Debug.DrawRay(hit.point, varianceNormal*3, Color.green,1);
                
               
                rotation = Quaternion.LookRotation(lookDirection, varianceNormal);

                position = hit.point; // + offset;
                return true;
            }
            
            Debug.Log("Found no raycast target with " + name + " raycaster.", this);
            rotation = Quaternion.identity;
            position = checkStart;

            return false;
        }


        public override string ToString()
        {
            return "   We <color=green>do</color> raycasthit a layer within <b>" + range.ToString() + "</b> ";
        }
    }    
}
