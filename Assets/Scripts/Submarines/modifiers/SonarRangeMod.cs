using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion.Sonar;

namespace Diluvion.Ships
{
    [CreateAssetMenu(fileName = "sonar range mod", menuName = "Diluvion/subs/mods/ping range")]
    public class SonarRangeMod : ShipModifier
    {

        public override void Modify (Bridge bridge, float value)
        {
            Pinger pinger = bridge.GetComponent<Pinger>();

            if (!pinger) return;

            pinger.rangeMult = 1 + value;
        }

        protected override string Test ()
        {
            string s = base.Test();
            s += "This would set a ship's sonar range to " + TestingValue() + " times the normal range.";
            Debug.Log(s);
            return s;
        }
    }
}