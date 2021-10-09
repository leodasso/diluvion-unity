using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavyDutyInspector;

namespace Diluvion.Roll
{
    /// <summary>
    /// Inclusive Tag for checking positions, if a object has this tag, it needs to find itself in the input tag list
    /// </summary>

    [CreateAssetMenu(fileName = "new inclusiveTag", menuName = "Diluvion/RollTags/Inclusive")]
    [System.Serializable]
    public class InclusiveTag : Tag
    {

        public override bool IsValid(IRoller roller)
        {
            return roller.RollingTags.Contains(this);
        }

        public override string ToString()
        {
            return this.name;
        }
    }
}
