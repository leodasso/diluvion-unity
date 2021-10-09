using UnityEngine;
using Diluvion.Ships;
using Sirenix.OdinInspector;

namespace Diluvion
{

    [CreateAssetMenu(fileName = "bonus inv", menuName = "Diluvion/subs/bonus/inv slots")]
    public class BonusInv : Forging
    {

        [TabGroup("forge")]
        public int additionalSlots = 1;

        public override bool ApplyToShip (Bridge ship, SubChassis chassis)
        {
            if (!base.ApplyToShip(ship, chassis)) return false;

            //Debug.Log("Applying inventory bonus to " + ship.name);
            Inventory inv = ship.GetComponent<Inventory>();

            if (inv == null)
            {
                Debug.LogError("No inventory on target!", ship);
                return false;
            }

            inv.extraSlots += Mathf.RoundToInt(additionalSlots * Multiplier(chassis));
            return true;
        }

        public override bool RemoveFromShip(Bridge b, SubChassis sc)
        {
            if (!base.RemoveFromShip(b, sc)) return false;
         
            Debug.Log("Removing inventory bonus to " + b.name);
            Inventory inv = b.GetComponent<Inventory>();

            if (inv == null)
            {
                Debug.LogError("No inventory on target!", b);
                return false;
            }

            inv.extraSlots -= Mathf.RoundToInt(additionalSlots * Multiplier(sc));
            return true;
        }

        public override string LocalizedBody()
        {
            string body = base.LocalizedBody();
            return body.Replace("[s]", additionalSlots.ToString());
        }
    }
}