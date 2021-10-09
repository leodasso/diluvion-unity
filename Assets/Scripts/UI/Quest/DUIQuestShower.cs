using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Quests;

namespace DUI
{

    /// <summary>
    /// Shows updates to a quest whenever changes happen, such as progress on an objective.
    /// </summary>
    public class DUIQuestShower : DUIPanel
    {
        public DUIQuest questDisplayPrefab;
        public DQuest displayedQuest;
        public RectTransform displayParent;

        public float defaultHeight = 200;
        public float heightPerObjective = 60;

        public bool showing;

        DUIQuest _questDisplayInstance;
        Animator _questAnimator;

        static bool _instanceShowing;
        
        static List<DQuest> pendingQuests = new List<DQuest>();

        public static void ShowQuest(DQuest quest)
        {
            // If there are no quests showing, show this one
            if (!_instanceShowing)
            {
                _instanceShowing = true;
                DUIQuestShower instance = UIManager.Create(UIManager.Get().questShower as DUIQuestShower);
                instance.Init(quest);
            }

            else
            {
                // Otherwise, just add this to the list of pending quests
                if (!pendingQuests.Contains(quest))
                    pendingQuests.Add(quest);
            }
        }

        /// <summary>
        /// If there's any quests waiting to be shown, this shows the next one and removes it from the list.
        /// </summary>
        static void ShowNextPendingQuest()
        {
            if (pendingQuests.Count < 1) return;

            DQuest nextQuest = pendingQuests[0];
            ShowQuest(nextQuest);
            pendingQuests.RemoveAt(0);
        }


        /// <summary>
        /// Instantiates a quest display, calls to notify
        /// </summary>
        void Init (DQuest quest)
        {
            _questAnimator = GetComponent<Animator>();

            displayedQuest = quest;

            _questDisplayInstance = Instantiate(questDisplayPrefab);
            _questDisplayInstance.transform.SetParent(displayParent, false);
            _questDisplayInstance.Init(quest);

            _questDisplayInstance.transform.SetAsFirstSibling();

            Notify();
        }

        /// <summary>
        /// Brings the display down so it's visible, refreshes the display.
        /// </summary>
        void Notify ()
        {
            if (showing) return;
            if (displayedQuest == null) return;

            float totalHeight = defaultHeight + heightPerObjective * displayedQuest.CurrentObjectives().Count; 

            displayParent.sizeDelta = new Vector2(displayParent.sizeDelta.x, totalHeight);

            showing = true;

            // Play SFX
            if (displayedQuest.IsComplete())
                SpiderWeb.SpiderSound.MakeSound("Play_Quest_Complete", gameObject);
            else
                SpiderWeb.SpiderSound.MakeSound("Play_MUS_New_Quest", gameObject);

            // Calls animator to bring this into position
            _questAnimator.SetTrigger("notify");

            // Refreshes the quest display once it's in position
            _questDisplayInstance.RefreshAll(1, .5f);
        }

        /// <summary>
        /// normie function that can be called as an animation event
        /// </summary>
        public void AnimationEnd()
        {
            End();
        }

        /// <summary>
        /// When ending, show the next pending quest
        /// </summary>
        public override void End()
        {
            _instanceShowing = false;
            ShowNextPendingQuest();
            base.End();
        }
    }
}