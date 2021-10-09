using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Quests;

namespace Diluvion
{
    [CreateAssetMenu(fileName = "quest action", menuName = "Diluvion/actions/quest action", order = 0)]
    public class QuestAction : Action
    {
        [Space]
        public DQuest quest;
        public bool startsQuest;
        public bool completesQuest;
        public List<Objective> objectives = new List<Objective>();
        public float delay = 1;

        public override bool GivesQuest ()
        {
            if (quest)
            {
                if (startsQuest || completesQuest) return true;
            }
            return false;
        }

        public override bool DoAction(Object o)
        {
            if (quest == null) return false;

            WorldControl.Get().StartCoroutine(DelayedStartQuest());

            return true;
        }

        IEnumerator DelayedStartQuest()
        {
            yield return new WaitForSeconds(delay);

            if (startsQuest)
            {
                quest.StartQuest();
                yield break;
            }

            if (completesQuest)
            {
                quest.CompleteQuest();
                yield break;
            }

            foreach (Objective obj in objectives)
            {
                if (obj == null) continue;
                obj.ProgressObjective(quest);
            }
        }

        protected override void Test()
        {
            //Debug.Log(ToString());
            DoAction(null);
        }

        public override string ToString()
        {
            string s = "";
            if (objectives.Count > 0)
            {
                s += "Complete objective(s) ";
                foreach (Objective o in objectives)
                {
                    s += o.name + " ";
                }
                s += "for quest " + quest.name;
            }
            if (completesQuest) s += "Completes quest " + quest.name;
            if (startsQuest) s += "Starts quest " + quest.name;

            return s;
        }
    }
}