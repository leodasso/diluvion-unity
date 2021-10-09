using UnityEngine;
using System.Collections.Generic;
using Diluvion;

namespace Queries
{
    [CreateAssetMenu(fileName = "inv query", menuName = "Diluvion/queries/inv query")]
    public class QueryInventory : Query
    {
        public List<StackedItem> requiredItems = new List<StackedItem>();

        public override bool IsTrue(Object o)
        {
            if (PlayerManager.PlayerShip() == null) return false;
            Inventory inv = PlayerManager.PlayerShip().GetComponent<Inventory>();
            if (inv == null) return false;

            foreach (StackedItem i in requiredItems)
            {
                if (inv.RemainingItems(i.item) < i.qty) return false;
            }
            return true;
        }

        public override List<StackedItem> ReferencedItems ()
        {
            return requiredItems;
        }

        protected override void Test()
        {
            Debug.Log("True if " + ToString() + ": " + IsTrue(null));
        }

        public override string ToString()
        {
            string s = "player has ";
            foreach (StackedItem i in requiredItems)
            {
                s += i.qty + " of " + i.item.name + " ";
            }
            return s;
        }
    }
}