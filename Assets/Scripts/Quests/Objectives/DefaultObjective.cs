using UnityEngine;
using System.Collections;
using System;

namespace Quests
{
    /// <summary>
    /// A basic objective, can be called by an action trigger.
    /// </summary>
    [CreateAssetMenu(fileName = "default objective", menuName = "Diluvion/Quests/default objective")]
    public class DefaultObjective : Objective
    {
        public override void CheckObjective(DQuest forQuest)
        {
            //throw new NotImplementedException();
        }

        public override GameObject CreateGUI(string overrideObjectiveName)
        {
            // TODO
            //throw new NotImplementedException();
            return null;
        }
    }
}
