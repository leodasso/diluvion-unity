using UnityEngine;
using System.Collections.Generic;
using Loot;
using Sirenix.OdinInspector;
using Diluvion.Roll;
using Diluvion.Ships;

namespace Diluvion
{

    [CreateAssetMenu(fileName = "Sub loadout", menuName = "Diluvion/subs/loadout")]
    public class SubLoadout : Entry
    {
      
        
        [OnValueChanged("ResetDanger")]
        public List<DItemWeapon> weapons = null;

        [OnValueChanged("ResetDanger")]
        public List<ShipModule> modules = new List<ShipModule>();

        [OnValueChanged("ResetDanger")]
        public List<Forging> bonusChunks = new List<Forging>();
        public DItemDecal emblem;
        public InvGenerator inventory;
        public int danger;
        
        [FoldoutGroup("Debug")]
        public SubChassis testingChassis;

        public void AddToChassis (SubChassisData data)
        {
            // Add modules
//            Debug.Log(modules.Count + " in modules on " + this.name,this );
            data.appliedModules.Clear();
           
            foreach (ShipModule m in modules)
            {
                if (m == null) continue;
                data.appliedModules.Add(m.GetSave(null));
            }

            // add bonus chunks
            foreach (Forging b in bonusChunks)
            {
                if (b == null) continue;
                data.appliedSlots.Add(b.name);
            }

            if (emblem) data.decalName = emblem.name;

            if (inventory) data.invName = inventory.name;
        }

        /// <summary>
        /// Represents the danger of the loadout 
        /// </summary>
        public override int Danger ()
        {
            if (danger != 0) return danger;
            
            
            foreach (DItemWeapon weapon in weapons)
            {
                if (!weapon) continue;
                danger += weapon.Danger();
            }

            foreach (Forging bonus in bonusChunks)
            {
                if (!bonus) continue;
                danger += (bonus.Danger());
            }

            foreach (ShipModule m in modules)
                danger += (int)m.dangerValue * GameManager.Mode().moduleDangerMultiplier;

            return danger;
        }

        void ResetDanger()
        {
            danger = 0;
            Danger();
        }

        [Button()]
        void Test ()
        {
            SubChassisData testingData = new SubChassisData(testingChassis);
            AddToChassis(testingData);
            Debug.Log(testingData.ToString());

            // Apply bonus slots
            foreach (DItem i in ItemsGlobal.GetItems(testingData.appliedSlots))
            {
                Forging chunk = i as Forging;
                if (!chunk) Debug.LogError("Chunk " + i.name + " not casting to chunk.");
                Debug.Log(chunk.name, chunk);
            }
            Danger();
        }
        
        
   
        public override string ToString()
        {
            fullString = "<color=yellow><b>" + this.name + "</b></color> can be rolled if ";
          
            fullString += " the table has more than <color=red>" + Danger() + "</color> Danger \n";
            fullString += " the table has more than <color=green>" + TechCost() + "</color> Tech \n";

            foreach (Tag tag in tags)
            {
                fullString += "and, <color = yellow><b>" + tag.ToString() + "</b></color> \n";
            }
            return fullString;
        }
    }
}