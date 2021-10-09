/*

using UnityEngine;
using System.Collections.Generic;
using Diluvion.SaveLoad;
using Diluvion;

namespace DUI
{
    /// <summary>
    /// Parent object of all tutorial items. Manages tutorials so they don't overlap, show in the right order, and reserve properly.
    /// </summary>
    public class DUITutorial : DUIPanel
    {
        static DUITutorial duiTutorial;

        public CanvasGroup bg;

        List<DUITutorialItem> tutorialItems;    // List of tutorial items in the order they'll appear
        DUITutorialItem currentItem;
        float groupAlpha = 1;
        float bgAlpha = 0;
        Color bgInitColor;
        float slowTime = .07f;
        float targetTimeScale = 1;
        bool ending;

        public static DUITutorial Get()
        {
            if (duiTutorial != null) return duiTutorial;
            duiTutorial = UIManager.Create(UIManager.Get().tutorialPanel as DUITutorial);
            return duiTutorial;
        }

        // Use this for initialization
        protected override void Start()
        {
            base.Start();

            //Add all tutorial items to the list
            tutorialItems = new List<DUITutorialItem>();
            tutorialItems.AddRange(GetComponentsInChildren<DUITutorialItem>());

            bg.alpha = 0;
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            if (GameManager.State() != GameState.Running) return;

            if (currentItem != null) bgAlpha = currentItem.alpha;
            else bgAlpha = 0;

            //adjust the BG color
            bg.alpha = Mathf.Lerp(bg.alpha, bgAlpha, Time.unscaledDeltaTime * 5);

            //adjust timescale
            if (!ending)
                TimeControl.SetTimeScale(Mathf.Lerp(TimeControl.timeScale, targetTimeScale, Time.unscaledDeltaTime * 5));
        }


        /// <summary>
        /// Other scripts can send tags to this to allow certain tutorial items to show.
        /// when a tag is added, this will go through each tutorial item to see if the tag is 
        /// relevant to the item.
        /// </summary>
        public void AddTag(string tag)
        {
            if (tutorialItems == null) return;
            if (DSave.current != null)
                if (DSave.current.tutorialSkipped) return;

            foreach (DUITutorialItem item in tutorialItems)
                if (item.needsTag) item.CheckTag(tag);
        }

        /// <summary>
        /// Show the given tutorial item. Skips if the save file already has it marked as shown.
        /// </summary>
        public void ShowItem(DUITutorialItem itemToShow)
        {
            if (ending) return;
            if (DSave.current != null)
                if (DSave.current.tutorialSkipped)
                {
                    currentItem = null;
                    return;
                }

            currentItem = itemToShow;

            if (itemToShow.slowsTime)
            {
                targetTimeScale = slowTime;
                OrbitCam.Get().BeginDOF();
            }

            //play success audio
            if (GetComponent<AKTriggerPositive>())
                GetComponent<AKTriggerPositive>().TriggerPos();
        }

        public void Reserve(DUITutorialItem item)
        {
            currentItem = item;
        }

        public void HideItem(DUITutorialItem itemToHide)
        {
            currentItem = null;

            targetTimeScale = 1;

            OrbitCam.Get().EndDOF();

            //play success audio
            //TODO WWISE UI FLAT AUDIO
            GetComponent<AKTriggerNegative>().TriggerNeg();
        }

        public bool Busy()
        {
            if (currentItem != null) return true;
            else return false;
        }
    }
}
*/