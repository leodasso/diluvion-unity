using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;
using TMPro;
using Loot;
using Diluvion;

namespace DUI
{
    public class TradePanel : DUIView
    {

        public static TradePanel tradePanel;

        public CanvasGroup centerPanel;
        public DUIInventory inventoryPanelPrefab;
        public RectTransform inv1parent;
        public RectTransform inv2parent;
        //public QtySelect qtySelectPrefab;
        //public QtySelect qtySelectInstance;
        public TradeMode tradeMode;
        public enum TradeMode { cash, loot, none };
        public TextMeshProUGUI giveText;
        public TextMeshProUGUI takeText;
        public Button takeAllButton;

        GameObject _lastSelected;        // The most recently selected object (by event system)
        DUIInventory _inventory1;
        DUIInventory _inventory2;

        /// <summary>
        /// The inventory in the trade panel that doesn't belong to the player. If only showing player inventory,
        /// returns null.
        /// </summary>
        public Inventory NonPlayerInventory()
        {
            if (!_inventory2) return null;
            return _inventory2.myInventory;
        }


        float buyMultiplier =1;
        public float BuyMultiplier
        {
            get {
                return buyMultiplier;
            }
            private set {
                buyMultiplier = value;
            }
        }


        private float sellMultiplier = 1;
        public float SellMultiplier
        {
            get {
                return sellMultiplier;
            }
            private set {
                sellMultiplier = value;
            }
        }

        public static TradePanel Get ()
        {

            if (tradePanel != null) return tradePanel;

            tradePanel = FindObjectOfType<TradePanel>();
            return tradePanel;
        }

        public void Init (Inventory inv)
        {

            centerPanel.alpha = 0;

            Show();

            //play intro audio
            //HANDLED BY WWISE START

            Inventory playerInv = PlayerManager.pBridge.GetInventory();

            // Display the player inventory. There's no instance where it's not displayed
            _inventory1 = InstantiateInventory(inv1parent);
            _inventory1.Init(playerInv, this);

            //the inventory called was the player inventory.  Only show one.
            if (inv == playerInv) tradeMode = TradeMode.none;

            else
            {

                //Set trade mode
                if (inv.invGenerator.merchant) tradeMode = TradeMode.cash;
                else tradeMode = TradeMode.loot;

                //the inventory called was not player inventory, show it on right.
                _inventory2 = InstantiateInventory(inv2parent);
                _inventory2.Init(inv, this);
            }

            //set text strings
            if (tradeMode == TradeMode.cash)
            {
                //giveText.text = Localization.GetFromLocLibrary("misc_sell", "sell").ToUpper();
                //takeText.text = Localization.GetFromLocLibrary("misc_buy", "buy").ToUpper();
                SellMultiplier = inv.invGenerator.sellPercentage;
                BuyMultiplier = inv.invGenerator.buyPercentage;
            }
            
            else if (tradeMode == TradeMode.loot)
            {
                //giveText.text = Localization.GetFromLocLibrary("misc_give", "give").ToUpper();
                //takeText.text = Localization.GetFromLocLibrary("misc_take", "take").ToUpper(); ;
            }

            if (tradeMode != TradeMode.none) centerPanel.alpha = 1;


            //take all button
            if (tradeMode == TradeMode.loot)
                takeAllButton.gameObject.SetActive(true);
            else
                takeAllButton.gameObject.SetActive(false);

            SetDefaultSelectable();
        }

        protected override void Update ()
        {
            base.Update();

            if (player.GetButtonDown("cancel") && fullyShowing)
                End();

            SelectOnMove();

            if (OrbitCam.CamMode() != CameraMode.Interior) End();
        }

        protected override void SetDefaultSelectable ()
        {
            // For looting, select the 'take all' button
            if (tradeMode == TradeMode.loot)
            {
                if (takeAllButton != null)
                {
                    if (takeAllButton.gameObject.activeInHierarchy) SetCurrentSelectable(takeAllButton.gameObject);
                    return;
                }
            }

            // check if the last selected item is still around
            if (_lastSelected != null)
            {
                SetCurrentSelectable(_lastSelected);
                return;
            }

            // If there's any item to select, set that as active
            Item childItem = GetComponentInChildren<Item>();
            if (childItem)
                SetCurrentSelectable(childItem.gameObject);
        }

        /// <summary>
        /// Meant to be called by child DUIItems. Will remember when they're selected by the 
        /// event system. This can then be used to return to the last selected item when returning 
        /// from another window. Yay convenience!
        /// </summary>
        /// <param name="selected"></param>
        public void RememberSelection (GameObject selected)
        {
            _lastSelected = selected;
        }

        /// <summary>
        /// Has the event system select the given item. If the panel is in 'loot' mode, will select the 'take all' button
        /// instead.
        /// </summary>
        public void SelectItem (GameObject item)
        {
            if (tradeMode == TradeMode.loot) SetDefaultSelectable();
            else SetCurrentSelectable(item);
        }

        /// <summary>
        /// Instantiates the inventory into the given parent, returns the 
        /// inventory UI instance.
        /// </summary>
        DUIInventory InstantiateInventory (RectTransform parent)
        {
            DUIInventory newInventory = UIManager.Create(inventoryPanelPrefab, parent);
            newInventory.transform.SetAsFirstSibling();

            return newInventory;
        }



        public float ValueMultiplier (DUIInventory inventoryPanel)
        {
            if (tradeMode != TradeMode.cash) return 1;

            if (_inventory2 == inventoryPanel) return _inventory2.myInventory.invGenerator.sellPercentage;
            return _inventory2.myInventory.invGenerator.buyPercentage;
        }

        /*

        /// <summary>
        /// Adds panel for selecting a quantity of items out of a stack
        /// </summary>
        public void AddQtySelection (Item forItem, DUIInventory inv)
        {
            if (qtySelectInstance) qtySelectInstance.DeselectAndEnd();

            if (forItem.stack == null) return;

            qtySelectInstance = Instantiate(qtySelectPrefab, transform.position, Quaternion.identity) as QtySelect;

            //set parent to this so it sorts on top of the items grid
            qtySelectInstance.transform.SetParent(transform.parent, false);
            Vector2 pos = forItem.transform.position;
            qtySelectInstance.GetComponent<RectTransform>().position = pos;

            qtySelectInstance.tradePanel = this;
            qtySelectInstance.inventoryPanel = inv;

            //put on top
            qtySelectInstance.transform.SetAsLastSibling();

            // Get the base value per item
            float valuePerItem = forItem.stack.item.goldValue;

            if (tradeMode == TradeMode.cash)
            {
                bool selling = !inv.playersInventory;

                // display value of the transaction
                if (selling) valuePerItem = _inventory2.myInventory.invGenerator.sellPercentage * forItem.stack.item.goldValue;
                else valuePerItem = _inventory2.myInventory.invGenerator.buyPercentage * forItem.stack.item.goldValue;
                //valuePerItem = inventory2.myInventory.WeightedGoldValue(forItem.stack, selling);
            }

            qtySelectInstance.Init(forItem, valuePerItem);
        }
        */


        public void Trade (DUIInventory inv, StackedItem items)
        {
            Debug.Log("Trading...");

            //if item is moving from player inventory to other
            if (inv == _inventory1) Transaction(_inventory1.myInventory, _inventory2.myInventory, items);

            //if item is moving from other inventory to player
            if (inv == _inventory2) Transaction(_inventory2.myInventory, _inventory1.myInventory, items);

        }

        /// <summary>
        /// Transacts the given items.
        /// </summary>
        void Transaction (Inventory givingInv, Inventory recievingInv, StackedItem items)
        {
            //if either inventory is a merchant, it's a cash transfer
            bool cashTransfer = recievingInv.invGenerator.merchant || givingInv.invGenerator.merchant;

            //Selling to Merchant
            if (givingInv == _inventory1.myInventory && cashTransfer) SpiderSound.MakeSound("Play_Stinger_Sell", gameObject);
            if (givingInv == _inventory2.myInventory && cashTransfer) SpiderSound.MakeSound("Play_Stinger_Buy", gameObject);
            if (givingInv == _inventory1.myInventory && !cashTransfer) SpiderSound.MakeSound("Play_Stinger_Take", gameObject);

            float costMult = 1;

            if (cashTransfer)
            {
                if (recievingInv == _inventory2.myInventory) costMult = _inventory2.myInventory.invGenerator.buyPercentage;
                else costMult = _inventory2.myInventory.invGenerator.sellPercentage;

            }

            // Attempt the transaction between inventories
            bool successfulTransfer = givingInv.Transaction(recievingInv, items, cashTransfer, costMult);

            Debug.Log("Transaction was successful: " + successfulTransfer);

            if (!successfulTransfer) return;

            //update displays
            DUIInventory.RefreshAll();
        }

        
        /// <summary>
        /// Takes all items from inventory 2, attempts to move them to inventory 1.
        /// </summary>
        public void TakeAll ()
        {
            //Transfer each stack from this inventory to the other
            foreach (StackedItem stackedItem in _inventory2.myInventory.AllItems())
            {
                //If the transfer couldn't be completed, break the loop. 
                if (_inventory2.myInventory.Transaction(_inventory1.myInventory, stackedItem, false, 1) == false)
                {
                    Debug.Log("Breaking loop");
                    break;
                }
            }

            SpiderSound.MakeSound("Play_Stinger_Take_All", gameObject);

            //After transaction complete, update the inventory cells
            DUIInventory.RefreshAll();
        }

        public override void End ()
        {
            // Check if qty select is up first. If it is, ignore the request. The qty select panel
            // will remove itself.
            if (!QtySelect.Exists()) base.End();
        }

        protected override void FadeoutComplete ()
        {
            Debug.Log("Fadeout complete on inventory!");

            OrbitCam.ClearFocus();
            _inventory1.myInventory.OnChanged();
            // _inventory1.myInventory.UpdateGoldAchievement();
            if (_inventory2 != null)
            {
                _inventory2.myInventory.OnChanged();
                _inventory2.myInventory.EndedTransaction();
            }

            Destroy(gameObject);
        }


        public bool SingleInventory ()
        {
            if (_inventory2 == null) return true;
            if (_inventory2.gameObject.activeInHierarchy) return false;
            return true;
        }


        /// <summary>
        /// Can the other inventory player's trading with take this item?
        /// </summary>
        public bool ItemAllowed (DItem i)
        {
            // yes, if only viewing 1 inventory
            if (tradeMode == TradeMode.none) return true;
            if (_inventory2 == null) return true;

            return _inventory2.myInventory.CanTakeItem(i);
        }
    }
}