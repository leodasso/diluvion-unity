using UnityEngine;

namespace Diluvion.Ships
{

    [CreateAssetMenu(fileName = "overdrive mod", menuName = "Diluvion/subs/mods/overdrive")]
    public class OverdriveMod : ShipModifier
    {

        public override void Modify(Bridge bridge, float value)
        {
            ShipMover s = bridge.GetComponent<ShipMover>();
            if (s == null) return;
            s.overdriveMultiplier = value;
        }

        protected override string Test()
        {
            string s = base.Test();
            s += "This means the ship's overdrive cost would be " +
                ShipMover.OverdriveCost(TestingValue()) + " air tanks per second.";
            Debug.Log(s);
            return s;
        }
    }
}