using UnityEngine;
using Diluvion.SaveLoad;
using DUI;

namespace Loot
{
    [CreateAssetMenu(fileName = "new capt time item", menuName = "Diluvion/items/capt time item")]
    public class DItemCaptainTime : DItem
    {
        public int buffs = 1;

        public override void Use()
        {
            base.Use();

            if (DSave.current != null) DSave.current.captainTimeUpgrades += buffs;

            // Removes the item from this inventory
            RemoveFromPlayerInventory();

            // call UI to show buffs
            //DUICaptainTime.Get().ShowUpgrade();
        }
    }
}