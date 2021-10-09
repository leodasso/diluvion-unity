using UnityEngine;
using System.Collections.Generic;
using Loot;
using Diluvion.SaveLoad;
using Sirenix.OdinInspector;
using Diluvion.Roll;
using UnityEngine.Events;

namespace Diluvion
{

    /// <summary>
    /// A scriptable object that plugs into any inventory, and has all the settings for populating it.
    /// </summary>
    [CreateAssetMenu(fileName = "inventory generator", menuName = "Diluvion/inventory generator")]

    public class InvGenerator : Entry, IRoller
    {

        #region declare

        [ToggleLeft]
        public bool debug;

        [SerializeField, DrawWithUnity]
        public UnityEvent onSave;
        
        [SerializeField, DrawWithUnity]
        public UnityEvent onOpenTrade;
        
        [ToggleGroup("merchant"), OnValueChanged("MerchantCheck")]
        public bool merchant;

        void MerchantCheck()
        {
            if (merchant) saves = true;
        }

        [ToggleGroup("merchant")] public int goldPerSecond = 2;

        [ToggleLeft]
        public bool unlimitedSize;

        [ToggleLeft]
        public bool unlimitedGoldValue;
 
        [ToggleLeft]
        [InfoBox("This inventory's name isn't unique! It must have a unique name to save correctly.", InfoMessageType.Error, "InvalidName")]
        public bool saves;

        [ToggleLeft]
        public bool playerShipInventory;

        [ToggleLeft]
        public bool restrictedPurchase;

        [ToggleLeft]
        public bool overrideAppearance;

        [ToggleLeft] 
        public bool overrideItemTable;
        
        [ShowIf("overrideItemTable"), AssetsOnly] 
        public Table itemTable;

        [Tooltip("The items that this inventory is allowed to accept."), ShowIf("restrictedPurchase")]
        public List<DItem> allowedItems;

        [ToggleGroup("merchant")]
        public float sellPercentage = .9f;

        [ToggleGroup("merchant")]
        public float buyPercentage = .5f;

        [ToggleGroup("merchant")]
        public int maxGold = 5000;

        [ShowIf("overrideAppearance")]
        [AssetList(Path ="Prefabs/Treasure Sprites/")]
        public SpriteSet spriteSet;

        [ToggleLeft]
        [HideIf("playerShipInventory")]
        public bool createRandomOnAwake;

        [ShowIf("createRandomOnAwake")]
        public int goldPool = 500;

        int _localGoldPool;

        public List<Tag> rollingTags = new List<Tag>();

        [HideIf("unlimitedSize")]
        public int maxSize = 10;

        public List<StackedItem> startingItems = new List<StackedItem>();

        /// <summary>
        /// How many times can an item roll fail before we break the loop
        /// </summary>
        const int _failsPermitted = 25;

        
        #if UNITY_EDITOR
        bool InvalidName ()
        {
            if (!saves) return false;
            return !InvGlobal.ValidName(this);
        }
        #endif

        bool InvalidGen()
        {
            return (unlimitedGoldValue && unlimitedSize);
        }

        #endregion

        public void UpdateInventory(Inventory inv)
        {
            if (!merchant) return;
            
            // attempt to sell off the additional items
            TrySellItem(inv);
            
            // attempt to restock missing items first
            foreach (var item in inv.MissingItems())
            {
                float price = item.item.goldValue * buyPercentage;
                if (inv.revenue >= price)
                {
                    inv.AddItem(item.item);
                    inv.revenue -= price;
                    return;
                }
            }
            
            // once restocked, add to gold cache
            if (inv.gold < maxGold)
            {
                inv.AddGold(Mathf.RoundToInt(inv.revenue));
                inv.revenue = 0;
            }

        }

        /// <summary>
        /// Checks if any of the items in the given inventory are new (that is, not part of the starting items) and sell one of them.
        /// </summary>
        void TrySellItem(Inventory inv)
        {
            foreach (var item in inv.itemStacks)
            {
                if (IsStartingItem(item.item))
                {
                    // Check if we have enough that we can sell one
                    if (inv.RemainingItems(item.item) > TotalStock(item.item))
                    {
                        // sell one
                        SellItem(inv, item.item);
                    }
                }

                else
                {
                    // Sell one the item
                    SellItem(inv, item.item);
                    return;
                }
            }
        }

        void SellItem(Inventory inv, DItem item)
        {
            if (inv.RemoveItem(item)) inv.revenue += item.goldValue;
        }

        /// <summary>
        /// The total starting stock of the given item. This is how many merchants will try to carry.
        /// </summary>
        int TotalStock(DItem item)
        {
            int amount = 0;
            
            foreach (var stack in startingItems)
            {
                if (stack.item == item)
                {
                    amount += stack.qty;
                }
            }

            return amount;
        }

        public bool IsStartingItem(DItem item)
        {
            // Get a list of all the items in the original stock, where each item only appears once
            foreach (StackedItem stockItem in startingItems)
            {
                if (stockItem.item == item) return true;
            }

            return false;
        }

        /// <summary>
        /// Rolls for items
        /// </summary>
        /// <param name="table">The item table to use for rolling</param>
        /// <param name="inv">The inventory to place the items into</param>
        /// <param name="gold">The amount of gold available for population</param>
        public void RollForItems(Inventory inv, int gold)
        {

            Table table = null;

            // Select the items table
            if (overrideItemTable) table = itemTable;
            else table = TableHolder.FindTableForInterior<DItem>(inv.transform);
            
            if (!table)
            {
                Debug.LogError("No items table found by " + inv.name + ", gen: " + name, inv);
                return;
            }
            if (InvalidGen()) return;
            _localGoldPool = gold;
            
            int fails = 0;

            // Roll until the gold run out, or we reach the max number of fails!
            while (_localGoldPool > 0)
            {
                DItem i = table.Roll<DItem>(RollQuery) as DItem;
                if (i == null) break;

                // Try to add the item
                if (inv.AddItem(i))
                    _localGoldPool -= i.Value();

                // If we've tried enough times and still havent been able to add an item, break the loop
                else
                {
                    fails++;
                    if (fails >= _failsPermitted) break;
                }
            }
        }

        public bool RollQuery (Entry entry)
        {
            DItem item = entry as DItem;
            if (item == null) return false;

            if (item.Value() <= 0) return false;

            if (item.Value() > _localGoldPool) return false;

            return true;
        }

        public void CombineTagList(List<Tag> tags)
        {        }

        public List<Tag> RollingTags
        {
            get {
                return rollingTags;
            }
            set {
                rollingTags = value;
            }
        }


        /// <summary>
        /// Starts the inventory
        /// </summary>
        public void InitializeInventory (Inventory inv)
        {
            if (inv.init) return;// dont init twice
            inv.invGenerator = this;

            if (playerShipInventory) maxSize = SubChassisGlobal.Get().defaultInvSize;

            // Load the basic items in
            foreach (StackedItem item in startingItems) inv.AddItem(item);

            if (merchant) inv.gold = maxGold;

            // Load the player's ship inventory
            if (playerShipInventory)
            {
                if (DSave.current == null)
                {
                    Debug.LogError("DSave was found to be null while trying to load player inventory.");
                    return;
                }
                
                //Debug.Log("Attempting log of player's ship inventory.");
                InventorySave save = DSave.current.LoadShipInventory();
                TryLoad(save, inv, true);
                return;
            }

            // Load the saved inventory
            if (saves)
            {
                if (debug) Debug.Log(name + " attempting to load to instance " + inv.name);

                if (DSave.current == null)
                {
                    if (debug) Debug.LogError("No save file currently exists, can't load.");
                    return;
                }
                InventorySave save = DSave.current.TryGetInventorySave(name);
                TryLoad(save, inv, false);
                return;
            }

            // Add random items to the inventory. 
            if (createRandomOnAwake)
            {
                RollForItems(inv, goldPool);
            }

            
            if (overrideAppearance)
            {
                
                InventoryClicker c = inv.GetComponent<InventoryClicker>();
                if (!c) return;
                c.spriteSet = spriteSet;
                c.OnPointerExit();
            }
        }

        /// <summary>
        /// Represents the base gold value of this inventory generator (before random population)
        /// </summary>
        public override int Value ()
        {
            int v = 0;
            foreach (StackedItem i in startingItems)
                v += i.item.Value() * i.qty;

            return v;
        }

        #region save load

        /// <summary>
        /// Attempts to load from the save file. If no data exists, creates new data. 
        /// Otherwise loads the save to the given inventory.
        /// </summary>
        /// <param name="overflowToStorage">When loading from a save file into an inventory, this allows for items that don't fit in the
        /// inventory instance to be placed in player's storage inventory.</param>
        void TryLoad (InventorySave save, Inventory inv, bool overflowToStorage)
        {
            // If there's no save for this inventory, create it.
            if (save == null) Save(inv);

            // Otherwise, load saved items into the instance.
            else inv.LoadInventory(save, overflowToStorage);
        }

        /// <summary>
        /// If this inventory is a type that requires saving, saves it. otherwise the call is ignored.
        /// </summary>
        /// <param name="inv"></param>
        public void TrySave(Inventory inv)
        {
            if (debug) Debug.Log(name + " attempting to save inventory instance " + inv.name);
            if (saves || unlimitedSize || playerShipInventory) Save(inv);
        }

        [Button]
        void InvokeOnSaved()
        {
            //Debug.Log("invoking: " + onSave.GetPersistentEventCount() + " listeners on " + this.name );
            onSave.Invoke();
        }
        
        
        void Save (Inventory inv)
        {
            if (debug) Debug.Log("Trying save on: " + name, this);
            if (DSave.current == null) return;
            if (playerShipInventory)
            {
                DSave.current.SavePlayerInventory(inv);
                InvokeOnSaved();
                return;
            }

            DSave.current.SaveMiscInventory(inv, name);
            InvokeOnSaved();
        }
        
        #endregion
    }
}