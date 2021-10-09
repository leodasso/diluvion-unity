using UnityEngine;
using System.Collections;
using Quests;
using Diluvion.SaveLoad;

namespace Queries
{
    /// <summary>
    /// Checks if the given quest is a certain state.
    /// </summary>
    [CreateAssetMenu(fileName = "quest query", menuName = "Diluvion/queries/Quest status", order = 1)]
    public class QuestQuery : Query
    {
        [Space]
        public DQuest quest;
        public QuestStatus questStatus;

        public override bool IsTrue(Object o)
        {
            if (DSave.current == null) return false;
            return (DSave.current.IsQuestStatus(quest, questStatus));
        }

        protected override void Test()
        {
            Debug.Log(ToString() + ": " + IsTrue(null).ToString());
        }

        public override string ToString()
        {
            return "Quest " + quest.name + " is " + questStatus.ToString();
        }
    }
}