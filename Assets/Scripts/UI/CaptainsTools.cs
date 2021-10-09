using UnityEngine;
using UnityEngine.UI;
using Diluvion;

namespace DUI {
    public class CaptainsTools : DUIView
    {

        public TabPanel worldMap;
        public TabPanel zoneMap;
        public TabPanel quests;

        public static CaptainsTools Create ()
        {
            CaptainsTools instance = UIManager.Create(UIManager.Get().captainsTools as CaptainsTools);
            GameManager.Freeze(instance);
            return instance;
        }

        public static CaptainsTools ShowWorldMap()
        {
            CaptainsTools instance = Create();
            instance.ShowTab(instance.worldMap);
            return instance;
        }

        public static DUITravel WorldMap()
        {
            var instance = UIManager.GetPanel<CaptainsTools>();
            if (!instance) return null;

            return instance.GetComponentInChildren<DUITravel>();
        }

        void ShowTab(TabPanel tabPanel)
        {
            worldMap.defaultTab = zoneMap.defaultTab = quests.defaultTab = false;
            tabPanel.defaultTab = true;
            tabPanel.ActivateTab();
        }

        protected override void Start()
        {
            base.Start();
            fullyShowing = true;
        }

        protected override void SetDefaultSelectable()
        {
            Selectable firstSelectable = GetComponentInChildren<Selectable>();
            if ( firstSelectable == null ) return;

            SetCurrentSelectable(firstSelectable.gameObject);
            //base.SetDefaultSelectable();
        }

        protected override void Update()
        {
            base.Update();
            if ( player.GetButtonDown("captains tools") && fullyShowing ) BackToTarget();
        }

        protected override void FadeoutComplete()
        {
            GameManager.UnFreeze(this);
            Destroy(gameObject);
        }
    }
}