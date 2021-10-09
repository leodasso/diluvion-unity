using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion.Ships
{

    [CreateAssetMenu(fileName = "critical hit mod", menuName = "Diluvion/subs/mods/crit rate")]
    public class CritRateMod : ShipModifier
    {
        public List<WeaponModule> affectedModules = new List<WeaponModule>();

        public override void Modify(Bridge bridge, float value)
        {
            foreach (WeaponSystem ws in bridge.GetComponents<WeaponSystem>())
            {
                if (affectedModules.Contains(ws.module))
                    ws.ChangeCritRate(value);
            }
        }

        protected override string Test()
        {
            string s = base.Test();
            s += "This would set a critical hit rate for ";
            foreach (WeaponModule m in affectedModules) s += m.name + " ";
            s += "to " + TestingValue();
            return s;
        }
    }
}