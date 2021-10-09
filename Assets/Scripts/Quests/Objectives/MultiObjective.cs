using UnityEngine;
using System;

namespace Quests
{
    /// <summary>
    /// Good for stuff like 'destroy 5 ships'
    /// </summary>
    [CreateAssetMenu(fileName = "multi objective", menuName = "Diluvion/Quests/multi objective")]
    public class MultiObjective : Objective
    {
        public int qty = 1;

        public override void CheckObjective(DQuest forQuest)
        {
            //throw new NotImplementedException();
        }

        public override void ProgressObjective(DQuest forQuest)
        {
            base.ProgressObjective(forQuest);
        }

        public override GameObject CreateGUI(string overrideObjectiveName)
        {
            throw new NotImplementedException();
        }

        public override ObjectiveSave CreateSave()
        {
            MultiObjectiveSave newSave = new MultiObjectiveSave();
            newSave.GetObjectiveProperties(this);
            return newSave;
        }
    }

    [Serializable]
    public class MultiObjectiveSave : ObjectiveSave
    {
        public override void Progress()
        {
            // Get the goal of this objective
            MultiObjective obj = QuestsGlobal.GetObjective(name) as MultiObjective;
            int goal = obj.qty;

            progress++;

            // If goal is met, then set complete.
            if (progress >= goal)
                base.Progress();
        }
    }
}