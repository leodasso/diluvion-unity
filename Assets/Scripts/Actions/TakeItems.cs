using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Quests;
using DUI;

namespace Diluvion
{

    /// <summary>
    /// Takes items from player inventory, and shows the GUI window 'x has been removed from your inventory'
    /// </summary>
    [CreateAssetMenu(fileName = "take items action", menuName = "Diluvion/actions/take items")]
    public class TakeItems : Action
    {
        public List<StackedItem> itemsToTake = new List<StackedItem>();

        public bool useDefaultTitle = true;
        
        [Tooltip("The title to display when taking items."), HideIf("useDefaultTitle")]
        public string title;


        void AddLocToLibrary()
        {
            // TODO
        }

        public bool useQuestItems;

        [ShowIf("useQuestItems"), OnValueChanged("CheckQuestForItems"), AssetList(Path = "Prefabs/Quests/")]
        public DQuest quest;


        void CheckQuestForItems ()
        {
            if (quest == null) return;

            itemsToTake.Clear();

            foreach (ObjectiveContainer c in quest.objectiveCs)
            {
                if (!c.objective) continue;
                if (c.objective.ReferencedItems() != null) itemsToTake.AddRange(c.objective.ReferencedItems());
            }
        }

        public override bool DoAction (UnityEngine.Object o)
        {
            // Find player inventory
            Inventory inv = PlayerManager.PlayerInventory();
            if (inv == null)
            {
                Debug.LogError("Player inventory couldn't be found! Can't take items.", this);
                return false;
            }
            
            // Open panel to show items have been taken
            string newTitle = title;
            if (useDefaultTitle) newTitle = "Items Taken";
            ItemExchangePanel.ShowItemExchange(itemsToTake, newTitle, false);

            // remove the items
            foreach (StackedItem items in itemsToTake)
            {
                if (!inv.RemoveItem(items)) return false;
            }

            return true;
        }
        

        public override string ToString ()
        {
            string s =  "On invoke, takes ";
            foreach (StackedItem i in itemsToTake)
            {
                s += i.item.LocalizedName() + " X " + i.qty + ", ";
            }

            s += "from player inventory.";
            return s;
        }

        protected override void Test ()
        {

            DoAction(null);
        }
    }
}