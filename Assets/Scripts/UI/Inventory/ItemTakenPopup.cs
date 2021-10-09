using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using SpiderWeb;
using Loot;
using Diluvion;

namespace DUI
{

    public class ItemTakenPopup : DUIView
    {
        public LootViewer lootViewerPrefab;

        [Space] public RectTransform lootViewerParent;
        public TextMeshProUGUI descriptionText;

        
        string descriptionLocKey = "misc_itemTakenDesc";
        //KeyItemGate keyItemGate;
        private LootViewer lootViewer;

        protected override void Start()
        {
            base.Start();
            lootViewer = Instantiate(lootViewerPrefab, lootViewerParent);
        }

        public static void Create (DItem item)
        {
            StackedItem newStack = new StackedItem(item, 1);
            List<StackedItem> newList = new List<StackedItem>();
            newList.Add(newStack);
            Create(newList);
        }

        public static void Create(List<StackedItem> stacks)
        {
            ItemTakenPopup newPopup = UIManager.Create(UIManager.Get().itemTakenPopup as ItemTakenPopup);
            newPopup.Init(stacks);
        }

	    // Use this for initialization
	    void Init (List<StackedItem> stacks) {

            GameManager.Freeze(this);
            lootViewer.Refresh(stacks);
	    }

        protected override void Update()
        {
            base.Update();

            // hit exit button to exit
            if ( player.GetButtonDown("pause") )
            {
                End();
                return;
            }

            ShowOnTop();
        }

        public override void End()
        {
            GameManager.UnFreeze(this);
            BackToTarget();
        }

        protected override void FadeoutComplete()
        {
            //keyItemGate.ContinueToConversation();
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (GameManager.Exists())
                GameManager.UnFreeze(this);
        }
    }
}