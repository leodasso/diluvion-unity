using UnityEngine;
using System.Collections;

namespace Diluvion.Ships
{

    [RequireComponent(typeof(AddDirectionalForce))]
    public class SurfaceSub : MonoBehaviour
    {
        public GameObject surfaceEffectPrefab;
        bool surfaced = false;
        ShipControls controls;

        GameObject surfaceEffectInstance;
        AddDirectionalForce force;
        CrewManager cManager;
        Bridge bridge;

        float gravity = 3000;

        public void Awake()
        {
            //Surface();
        }

        Bridge ShipBridge()
        {
            if (bridge != null) return bridge;
            bridge = GetComponent<Bridge>();
            return bridge;
        }

        CrewManager ShipCrew()
        {
            if (cManager != null) return cManager;
            if (ShipBridge() == null) return null;
            cManager = ShipBridge().crewManager;
            return cManager;
        }

        ShipControls ShipControl()
        {
            if (controls != null) return controls;
            controls = GetComponent<ShipControls>();
            return controls;
        }

        AddDirectionalForce Force()
        {
            if (force != null) return force;
            force = GetComponent<AddDirectionalForce>();
            return force;
        }

        public void Surface()
        {
            if (surfaced) return;
            surfaced = true;
            Force().force = Vector3.up * -gravity;
            ShipControl().surfaced = true;
            ShipControl().canSideView = false;
            ShipControl().GetComponent<Rigidbody>().velocity = Vector3.zero;
            if (surfaceEffectInstance == null)
                surfaceEffectInstance = (GameObject)Instantiate(surfaceEffectPrefab, transform.position, transform.rotation);
            surfaceEffectInstance.transform.SetParent(transform);
        }

        /// <summary>
        /// Removes suimono buoyancy objects
        /// </summary>
        public void RemoveBuoyancy()
        {
            if (surfaceEffectInstance != null)
                Destroy(surfaceEffectInstance);
        }

        public void LeaveSurface()
        {
            if (!surfaced) return;
            surfaced = false;
            ShipControl().canSideView = true;
            ShipControl().surfaced = false;
            Force().force = Vector3.zero;
            Destroy(surfaceEffectInstance);
        }
        
        
        
        
    }
}