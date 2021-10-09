using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Diluvion.Roll;
using Loot;

namespace Diluvion
{

    /// <summary>
    /// Loot placers interface with room placer and explorable to create randomly generated
    /// loot inventories.
    /// </summary>
    [RequireComponent(typeof(Inventory))]
    [RequireComponent(typeof(InventoryClicker))]
    public class LootPlacer : MonoBehaviour, IRewardable
    {

        [ReadOnly]
        public Inventory inventory;

        [ReadOnly]
        public bool markedAsEmpty;


        // Use this for initialization
        void Awake()
        {
            inventory = GetComponent<Inventory>();
        }

        /// <summary>
        /// Create an inventory that makes a random roll with the given value
        /// </summary>
        public float MakeReward(float goldValue)
        {
            //Debug.Log("Making reward for " + gameObject.name, gameObject);

            inventory = GetComponent<Inventory>();
            if (inventory == null)
            {
                Debug.LogError("No inventory component found.", gameObject);
                return 0;
            }

            // Find the table to roll for an inv generator
            Table invGenTable = TableHolder.FindTableForInterior<InvGenerator>(transform);
            
           
            if (!invGenTable)
            {
                Debug.LogError("No table found containing invGenerator entries.", gameObject);
                return 0;
            }
            //Debug.Log("Table with inventory generators: " + invGenTable, invGenTable);
            
            InvGenerator inv = invGenTable.Roll<InvGenerator>() as InvGenerator;
            
            //Debug.Log("Found invGenerator " + inv,gameObject);
            // Apply it to the inventory
            inventory.invGenerator = inv;
            inventory.PopulateItems(goldValue);
            
            // Return the extra gold (if any)
            return goldValue - inventory.SumItemValue();
        }

        public float PopulatePriority()
        {
            return 5;
        }

        public void DisableIfEmpty()
        {
            markedAsEmpty = true;
            
            if (inventory)
            {
                if (inventory.invGenerator == null) Disable();
                if (inventory.SumItemValue() < 1) Disable();
            }
        }

        public void Disable()
        {
            if (!Application.isPlaying) DestroyImmediate(gameObject);
            else Destroy(gameObject);
        }
    }
}