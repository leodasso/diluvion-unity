using UnityEngine;
using Diluvion;

namespace Loot
{
    [CreateAssetMenu(fileName = "new repair item", menuName = "Diluvion/items/repair item")]
    public class DItemRepair : DItem
    {
        public float HPPerUse = 5;

        /// <summary>
        /// Adds the given HP to the player ship
        /// </summary>
        public override void Use()
        {
            base.Use();
            Hull playerHull = PlayerManager.pBridge.hull;

            if (playerHull == null) return;

            if (playerHull.Repair(HPPerUse))
                RemoveFromPlayerInventory();
        }
    }
}