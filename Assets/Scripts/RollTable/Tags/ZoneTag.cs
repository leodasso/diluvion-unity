using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion.Roll
{
    [CreateAssetMenu(fileName = "new zoneTag", menuName = "Diluvion/RollTags/ZoneTag")]
    public class ZoneTag : Tag
    {
        public override bool IsValid(IRoller roller)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
