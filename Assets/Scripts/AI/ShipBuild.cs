using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion.Roll;
using Diluvion.Ships;
using Sirenix.OdinInspector;
using Diluvion.Sonar;




namespace Diluvion.AI
{   

//TODO Copy and add description from Captain settings, add the added signatures
    /// <summary>
    /// Class that Composes a fully ready ship, with AI, weapons and loot 
    /// </summary>
    [CreateAssetMenu(fileName = "new ship build", menuName = "Diluvion/subs/shipBuild")]
    public class ShipBuild : SpawnableEntry
    {
        [TabGroup("ShipBuild")]
        [OnValueChanged("SetSpawnablePrefab"), ValidateInput("NullChassis", "Chassis cannot Be Null", InfoMessageType.Error)]
        [AssetsOnly]
        public SubChassis chassis;

        [TabGroup("ShipBuild")]
        [SerializeField]     
        public ShipBuildSettings shipBuildSettings;

        void SetSpawnablePrefab()
        {
            Prefab();
        }

        //TODO AutoSetup of Entry Values based on the prefab        
        public override GameObject Prefab()
        {
            if (chassis) return chassis.shipPrefab;
            if (prefab != null) return base.Prefab();
            return prefab;
            //if (chassis == null) return prefab = null;
            //return prefab = chassis.Prefab();          
        }

      
        /// <summary>
        /// Only for creating a default sub, no parameters
        /// </summary> 
        public override GameObject Create(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (chassis == null) return null;

            GameObject sub = chassis.InstantiateChassis(null, shipBuildSettings, false,parent);
            if(sub.GetComponent<Spawnable>())
                sub.GetComponent<Spawnable>().SetDanger(Danger());
            sub.transform.position = position;
            sub.transform.rotation = rotation;        
            return sub;
        }

        bool NullChassis(SubChassis chassis)
        {
            if (chassis == null) return false;
            return true;
        }

        public override int Danger()
        {
            return resourceCost.danger;
        }
    }
}