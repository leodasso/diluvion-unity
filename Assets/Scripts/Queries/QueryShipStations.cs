using UnityEngine;
using Diluvion;
using Diluvion.Ships;

namespace Queries
{
    [CreateAssetMenu(fileName = "ship stations query", menuName = "Diluvion/queries/ship stations")]
    public class QueryShipStations : Query
    {
        public ShipModule module;

        public override bool IsTrue(Object o)
        {
            if (PlayerManager.PlayerShip() == null) return false;
            if (PlayerManager.PlayerShip().GetComponent<Bridge>().HasModule(module)) return true;
            return false;
        }

        protected override void Test()
        {
            Debug.Log("True if " + ToString() + ": " + IsTrue(null));
        }

        public override string ToString()
        {
            return "Player ship has a " + module.ToString() + " station.";
        }
    }
}
