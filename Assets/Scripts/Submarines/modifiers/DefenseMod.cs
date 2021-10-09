using System.Collections.Generic;
using UnityEngine;

namespace Diluvion.Ships
{

    [CreateAssetMenu(fileName = "defense mod", menuName = "Diluvion/subs/mods/defense")]
    public class DefenseMod : ShipModifier
    {

        public override void Modify(Bridge bridge, float value)
        {
            Hull h = bridge.GetComponent<Hull>();
            if (h == null) return;

            h.defense = 1 + value;
        }

        protected override string Test()
        {
            string s = base.Test();
            s += "This would set a ship's defense to " + TestingValue();
            Debug.Log(s);
            return s;
        }
    }

}