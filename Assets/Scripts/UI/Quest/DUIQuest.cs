using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;
using TMPro;
using Quests;
using Diluvion;
using Sirenix.OdinInspector;

namespace DUI
{

    public class DUIQuest : MonoBehaviour
    {
        [InlineButton("Test"), AssetsOnly]
        public DQuest myQuest;
        
        [Space]
        public TextMeshProUGUI title;
        public TextMeshProUGUI description;
        public DUIQuestObjective objectivePrefab;
        public Transform objectiveGrid;
        //public Color completedColor;

        private Animator _animator;
        List<DUIQuestObjective> objectiveDisplays = new List<DUIQuestObjective>();

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Test ()
        {
            if (!myQuest) return;
            Init(myQuest);
        }

        /// <summary>
        /// Initializes from a quest object.
        /// </summary>
        public void Init (DQuest quest)
        {
            myQuest = quest;

            ClearObjectives();

            //Spawn in the objective objects
            foreach (ObjectiveContainer oc in myQuest.CurrentObjectives())
            {
                AddObjective(oc);
            }
            
            SetHeight();

            RefreshAll(0, 0);
        }

        /// <summary>
        /// sets the layout element component min height, based on number of objective displays. This allows for proper 
        /// displaying in the quest list.
        /// </summary>
        void SetHeight()
        {
            float additionalHeight = 20 * objectiveDisplays.Count;
            GetComponent<LayoutElement>().minHeight = 200 + additionalHeight;
        }

        public void ClearObjectives()
        {
            objectiveDisplays.Clear();
            GO.DestroyChildren(objectiveGrid);
        }

        /// <summary>
        /// Creates a display for the given objective container and adds it to the list.
        /// </summary>
        void AddObjective(ObjectiveContainer obj)
        {
            DUIQuestObjective newObjective = Instantiate(objectivePrefab, objectiveGrid.transform);
            newObjective.Init(obj, myQuest);

            objectiveDisplays.Add(newObjective);
            
            SetHeight();
        }

        /// <summary>
        /// Overwrite the currently displayed objectives with the given one. Will only show objectives that are related
        /// to the currently displayed quest.
        /// </summary>
        public void ShowObjective(Objective newObjective)
        {
            ClearObjectives();
            
            //Spawn in the objective objects
            foreach (ObjectiveContainer oc in myQuest.CurrentObjectives())
            {
                if (oc.objective != newObjective) continue;
                AddObjective(oc);
            }
        }

        void SetText (string titleText, string bodyText)
        {
            title.text = titleText.ToUpper();
            description.text = bodyText;
        }


        /// <summary>
        /// Refresh the specified delayCondition and delayStatus.
        /// </summary>
        /// <param name="delayCondition">How long to delay the refresh of conditions' status</param>
        /// <param name="delayStatus">How long to delay refresh of quest completion status AFTER condition status
        /// refreshes.</param>
        public void RefreshAll (float delayCondition, float delayStatus)
        {
            SetText(myQuest.GetLocTitle(), myQuest.GetLocDescription());
            StartCoroutine(RefreshSequence(delayCondition, delayStatus));
        }


        IEnumerator RefreshSequence (float delayCondition, float delayStatus)
        {
            yield return new WaitForSeconds(delayCondition);

            // Tell all the quest objective UI to refresh
            foreach (DUIQuestObjective objective in objectiveDisplays)
                objective.Refresh();

            yield return new WaitForSeconds(delayStatus);

            // Set the status icon (checkbox)
            _animator.SetBool("complete", myQuest.IsComplete());
        }

        public void EndMe ()
        {
            Destroy(gameObject);
        }
    }
}