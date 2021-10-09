using UnityEngine;
using System.Collections;
using Quests;
using Diluvion.SaveLoad;

namespace Queries
{
    /// <summary>
    /// Checks if the given objective as part of the given quest is a certain state.
    /// </summary>
    [CreateAssetMenu(fileName = "objective query", menuName = "Diluvion/queries/Objective", order = 1)]
    public class ObjectiveQuery : Query
    {
        [Space]
        public Objective objective;
        public DQuest quest;
        public QuestStatus objStatus;

        public override bool IsTrue(Object o)
        {
            if (DSave.current == null) return false;
            return (objective.IsOfStatus(objStatus, quest));
        }

        protected override void Test()
        {
            Debug.Log(ToString() + ": " + IsTrue(null).ToString());
        }

        public override string ToString()
        {
            return "Objective " + objective.name + " in quest " + quest.name + " is " + objStatus.ToString();
        }
    }
}