using UnityEngine;
using Diluvion.SaveLoad;
using Diluvion.Ships;

namespace Queries
{
    [CreateAssetMenu(fileName = "ship level query", menuName = "Diluvion/queries/ship level")]
    public class QueryShipLevel : Query
    {
        public Comparer comparison = Comparer.Equal;
        public int shipLevel;

        public override bool IsTrue(Object o)
        {
            if (DSave.current == null) return false;
            SubChassisData currentSub = DSave.current.playerShips[0];
            if (currentSub == null)
            {
                Debug.Log("no current ship data exists.");
                return false;
            }
            int lvl = currentSub.ChassisObject().shipLevel;
            //Debug.Log("Current ship, " + currentSub.ChassisObject().name + ", level is " + lvl);

            if (comparison == Comparer.Equal && lvl == shipLevel) return true;
            if (comparison == Comparer.GreaterThan && lvl > shipLevel) return true;
            if (comparison == Comparer.LessThan && lvl < shipLevel) return true;
            return false;
        }

        protected override void Test()
        {
            Debug.Log("True if " + ToString() + ": " + IsTrue(null));
        }

        public override string ToString()
        {
            return "players ship level is " + comparison.ToString() + " " + shipLevel;
        }
    }
}