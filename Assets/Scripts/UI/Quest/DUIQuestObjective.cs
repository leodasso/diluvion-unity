using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Quests;

namespace Diluvion
{

    [RequireComponent(typeof(Animator))]
    public class DUIQuestObjective : MonoBehaviour
    {
        public TextMeshProUGUI objectiveDisplay;

        DQuest quest;
        Objective objective;
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Does the given waypoint fit to this objective display?
        /// </summary>
        public bool MatchesWaypoint (QuestActor wp)
        {
            if (quest != wp.quest) return false;
            if (objective != wp.objective) return false;

            return true;
        }

        /// <summary>
        /// Displays the given condition
        /// </summary>
        public void Init (ObjectiveContainer oc, DQuest q)
        {
            _animator = GetComponent<Animator>();
            
            objective = oc.objective;
            quest = q;
            objectiveDisplay.text = q.LocalizedObjective(oc);
            Refresh();
        }


        public void Refresh ()
        {
            _animator.SetBool("complete", objective.IsComplete(quest));
        }
    }
}