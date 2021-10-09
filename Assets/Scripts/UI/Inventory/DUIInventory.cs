using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using Diluvion;
using System.Collections.Generic;
using Loot;
using SpiderWeb;


namespace DUI
{
    /// <summary>
    /// GUI Display for an inventory. Shows items in categories. Uses a LootViewer to display item grids.
    /// </summary>
    public class DUIInventory : DUIPanel
    {
        [ReadOnly, TabGroup("inv", "General")]
        public Inventory myInventory;

        [ReadOnly, TabGroup("inv", "General")]
        public bool playersInventory;

        [TabGroup("inv", "Setup")]
        public TextMeshProUGUI invName;

        [TabGroup("inv", "Setup")]
        public TextMeshProUGUI goldAmount;

        [TabGroup("inv", "Setup")]
        public CanvasGroup goldGroup;

        [TabGroup("inv", "Setup")]
        public Transform categoriesParent;

        [TabGroup("inv", "Setup")]
        public Item itemDisplayPrefab;

        [TabGroup("inv", "Setup")]
        public LootViewer itemViewerPrefab;

        float _goldDisplay;
        TradePanel _tradePanel;
        RectTransform _rect;
        float _tradeMultiplier = 1;

        List<LootViewer> _lootTabs = new List<LootViewer>();

        /// <summary>
        /// Refreshes all instances of the inventory UI
        /// </summary>
        public static void RefreshAll ()
        {
            foreach (DUIInventory inv in UIManager.GetPanels<DUIInventory>())
                inv.Refresh();
        }

        public void Init (Inventory inv, TradePanel newTradePanel)
        {
            //get the trade panel
            _tradePanel = newTradePanel;

            Init(inv);
        }

        public void Init (Inventory inv)
        {
            // destroy previous tabs from the prefab
            GO.DestroyChildren(categoriesParent);

            
            //get the inventory I'm displaying
            myInventory = inv;
            if (myInventory == null) return;

            // Determine if this is the player's inventory or not
            playersInventory = myInventory.IsPlayerInventory();

            //show / hide gold display
            if (_tradePanel)
                if (_tradePanel.tradeMode == TradePanel.TradeMode.loot) goldGroup.alpha = 0;

            // Set the name text
            invName.text = myInventory.LocalizedName();

            Refresh();

        }

        // Update is called once per frame
        protected override void Update ()
        {
            base.Update();

            if (myInventory == null) return;

            //lerp value to display money in a spiffy way
            _goldDisplay = Mathf.Lerp(_goldDisplay, myInventory.gold, Time.unscaledDeltaTime * 6);

            //show gold amount
            if (goldAmount) goldAmount.text = Mathf.RoundToInt(_goldDisplay).ToString();
        }


        public void Refresh ()
        {
            // Key items list
            List<StackedItem> keyStacks = myInventory.GetStacksOfType<DItem>(Inventory.IsKeyItem);
            Populate(keyStacks, InvGlobal.Get().keyItemsTitle);

            // main items list
            List<StackedItem> mainStacks = myInventory.GetStacksOfType<DItem>(Inventory.IsNotKeyItem);
            Populate(mainStacks, InvGlobal.Get().mainItemsTitle, myInventory.MaxSize());

            List<StackedItem> weapons = myInventory.GetStacksOfType<DItemWeapon>();
            Populate(weapons, InvGlobal.Get().weaponsTitle, myInventory.MaxSize());
        }

        LootViewer Populate(List<StackedItem> stackList, LocTerm title, int maxSize = 0)
        {
            // If the tab has already been created, just refresh it
            foreach (var tab in _lootTabs)
            {
                if (tab.tabTitle == title)
                {
                    tab.Refresh(stackList, playersInventory, this, _tradePanel, title);
                    return tab;
                }
            }
            
            if (stackList.Count < 1) return null;

            // Instantiate a tab to show the stack, refresh them to show my items
            LootViewer newViewer = Instantiate(itemViewerPrefab, categoriesParent, false);
            newViewer.maxSize = maxSize;
            _lootTabs.Add(newViewer);

            newViewer.Refresh(stackList, playersInventory, this, _tradePanel, title);
            return newViewer;
        }
    }
}