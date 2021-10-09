using System;
using Diluvion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

namespace DUI
{

    /// <summary>
    /// UI element that displays an inventory item & quantity. Can be clicked for more info / transactions
    /// </summary>
    public class Item : MonoBehaviour 
    {
        public Image displayImage;
        public TextMeshProUGUI qtyText;
        
        [ReadOnly]
        public StackedItem stack;
        public Animator animator;
        [ReadOnly]
        public int selectedQty;
        
        [ReadOnly]
        public TradePanel tradePanel;
        
        [ReadOnly]
        public DUIInventory inventoryPanel;

        [ReadOnly]
        public bool playersItem;

        Button _button;
        CanvasGroup _canvasGroup;

        void Start ()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            animator = GetComponent<Animator>();
            animator.speed = 0;
        }


        /// <summary>
        /// Opens the item descriptor panel to display the details about this item
        /// </summary>
        public void ShowDescription ()
        {
            if (Math.Abs(animator.speed) < .01f) return;
            if (stack == null) return;
            if (stack.item == null || stack.qty < 1) return;
            
            // Determine if this is showing for the player's inventory
            bool playersInv = false;
            if (inventoryPanel) playersInv = inventoryPanel.playersInventory;

            ItemDescriptor.DisplayForItem(this, playersInv);
        }

        public void HideDescription ()
        {
            //if (Math.Abs(animator.speed) < .01f) return;
            ItemDescriptor.RemoveItemDisplay(this);
        }

        /// <summary>
        /// Meant to be called by the onSelected handler. Tells the trade panel that this 
        /// was the most recently selected item.
        /// </summary>
        public void TellTradePanelSelection ()
        {
            if (tradePanel == null) return;
            tradePanel.RememberSelection(gameObject);
        }

        public float ValueMultiplier ()
        {
            if (!tradePanel) return 1;
            return tradePanel.ValueMultiplier(inventoryPanel);
        }

        public void Refresh (StackedItem newItem, bool allowed = true) //, float multi = 1)
        {
            stack = newItem;
            if (_button == null) _button = GetComponent<Button>();
            if (stack == null)
            {
                _button.interactable = false;
                displayImage.color = Color.clear;
                qtyText.text = "";
                return;
            }

            if (allowed)
            {
                _canvasGroup.alpha = 1;
                _canvasGroup.interactable = true;
            }
            else
            {
                _canvasGroup.alpha = .4f;
                _canvasGroup.interactable = false;
            }

            _button.interactable = true;

            displayImage.color = stack.item.myColor;
            displayImage.sprite = stack.item.icon;

            //show quantity
            if (stack.qty > 1) qtyText.text = stack.qty.ToString();
            else qtyText.text = "";
        }




        public void Trade ()
        {
            //dont allow for player to give away key items
            if (playersItem && stack.item.keyItem) return;

            if (!inventoryPanel)
            {
                Debug.LogError("An inventory panel is required to trade items.", gameObject);
                return;
            }

            // Do the trade via the trade panel.
            if (tradePanel)
            {
                // If the trade is only showing one inventory, then it's just the player inventory. No trade is possible.
                if (tradePanel.SingleInventory()) return;

                // If there's only one, just trade it. If multiple, open a quantity selection panel.
                if (stack.qty < 2) tradePanel.Trade(inventoryPanel, stack);
                else if (stack.qty > 1) QtySelect.AddQtySelection(this, inventoryPanel, tradePanel);
            }

            // Do the trade via inventory
            else
            {
                if (stack.qty < 2)
                {
                    inventoryPanel.myInventory.Transaction(PlayerManager.PlayerInventory(), stack, false, 1);
                    inventoryPanel.Refresh();
                }
                else QtySelect.AddQtySelection(this, inventoryPanel);
            }

            // Remove the description panel
            ItemDescriptor.RemoveItemDisplay(this);
        }
    }
}