using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Diluvion
{
    [CreateAssetMenu(fileName = "gold steal", menuName = "Diluvion/actions/steal gold")]
    public class StealGold : HazardAction
    {
        [Title("Gold Theft"), OnValueChanged("Refresh")]
        public int min = 10;

        [OnValueChanged("Refresh")]
        public int max = 50;

        [MinMaxSlider(0, 9999), ReadOnly]
        public Vector2 range = new Vector2(10, 50);


        public override bool DoAction (Object o)
        {
            Inventory inv = PlayerManager.PlayerInventory();
            if (!inv) return false;

            int stolenGold = Mathf.RoundToInt(Random.Range(range.x, range.y));

            inv.StealGold(stolenGold);
            
            // TODO battle  log of theft
            return true;
        }

        void Refresh()
        {
            min = Mathf.Clamp(min, 1, 9999);
            max = Mathf.Clamp(max, min, 9999);
            range = new Vector2(min, max);
        }

        public override string ToString ()
        {
            return "Steals a random amount of gold from player between " + range.x + " and " + range.y;
        }
    }
}