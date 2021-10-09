using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Quests
{
    /// <summary>
    /// An objective acquired once certain conversations are said
    /// </summary>
    [CreateAssetMenu(fileName = "talky objective", menuName = "Diluvion/Quests/talky objective")]
    public class TalkyObjective : Objective
    {
        public Convo convo;

        /// <summary>
        /// Progress the objective if the given conversation has been read.
        /// </summary>
        public override void CheckObjective (DQuest forQuest)
        {
            if (convo.BeenRead())
                ProgressObjective(forQuest);
        }

        public override GameObject CreateGUI (string overrideObjectiveName)
        {
            throw new NotImplementedException();
        }
    }
}