using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;
using Diluvion;

namespace DUI
{
    /// <summary>
    /// Displays a list of items
    /// </summary>
    public class LootViewer : DUIPanel
    {
        
        public List<StackedItem> items = new List<StackedItem>();

        public TextMeshProUGUI title;

        public LocTerm tabTitle;

        public Item itemDisplayPrefab;

        [InfoBox("Select a layout grid to place item icons in!", InfoMessageType.Warning, "HasNoLayout")]
        public Transform gridLayout;

        [Tooltip("Max size of 0 means unlimited size."), ReadOnly]
        public int maxSize = 0;

        [ReadOnly]
        public bool playersInventory;

        [ReadOnly]
        public DUIInventory inventoryPanel;

        [ShowInInspector, ReadOnly]
        TradePanel _tradePanel;

        bool HasNoLayout()
        {
            if (gridLayout == null) return true;
            return false;
        }

        void DestroyChildren ()
        {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform t in gridLayout) children.Add(t.gameObject);
            children.ForEach(child => Destroy(child));
        }

        public void Refresh(List<StackedItem> newItems, bool isPlayer, DUIInventory invPanel, TradePanel tradeP, LocTerm newTitle)
        {
            items = newItems;
            playersInventory = isPlayer;
            inventoryPanel = invPanel;
            _tradePanel = tradeP;
            tabTitle = newTitle;
            title.text = tabTitle.LocalizedText();
            Refresh();
        }

        public void Refresh(List<StackedItem> newItems)
        {
            items = newItems;
            Refresh();
        }

        [Button()]
        public void Refresh()
        {
            StartCoroutine(SpawnItemCells(items));
        }

        /// <summary>
        /// Creates the grid of item cells based on the given items list. 
        /// </summary>
        IEnumerator SpawnItemCells (List<StackedItem> itemsList)
        {
            //create a new list of items in case the parameter ref itemsList gets cleared by another function
            List<StackedItem> stackedItems = new List<StackedItem>(itemsList);
            List<Item> itemDisplays = new List<Item>();

            DestroyChildren();

            int i = 0;
            int s = maxSize;
            if (maxSize <= 0) s = itemsList.Count;
            //spawn a cell for each slot of the inventory
            while (i < s)
            {
                Item newItem = Instantiate(itemDisplayPrefab, transform.position, Quaternion.identity) as Item;
                newItem.transform.SetParent(gridLayout, false);
                newItem.playersItem = playersInventory;

                // Select the first item in the event system
                if (i == 0 && _tradePanel) _tradePanel.SelectItem(newItem.gameObject);

                //add display to list
                itemDisplays.Add(newItem);
                i++;

                if (i > 999) break;
            }

            yield return null;

            //give the cells their graphics
            UpdateCells(stackedItems);

            //do fancy thing turning on animators one at a time
            foreach (Item display in itemDisplays)
            {
                if (display == null) continue;
                display.animator.speed = 5;
                yield return null;
            }

            yield break;
        }


        /// <summary>
        /// Updates all DUIItem children in the given parent with the item info from the given item list
        /// </summary>
        /// <param name="itemsList">A list of the item datas</param>
        void UpdateCells (List<StackedItem> itemsList)
        {
            if (itemsList == null) return;

            //update all the item displays with the latest stack
            int i = 0;
            foreach (Item itemDisplay in gridLayout.GetComponentsInChildren<Item>())
            {
                itemDisplay.tradePanel = _tradePanel;
                itemDisplay.inventoryPanel = inventoryPanel;

                // Display an empty grid cell
                if (i >= itemsList.Count)
                    itemDisplay.Refresh(null);

                // Display the item graphic
                else
                {
                    //get the stacked item that relates to this index
                    StackedItem itemStack = itemsList[i];
                    if (itemStack != null)
                    {
                        bool canTrade = true;
                        if (playersInventory && _tradePanel) canTrade = _tradePanel.ItemAllowed(itemStack.item);

                        itemDisplay.Refresh(itemStack, canTrade); 
                    }
                }

                //iterate
                i++;
            }
        }
    }
}
