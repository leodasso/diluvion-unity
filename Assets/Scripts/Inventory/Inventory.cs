using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;
using Loot;
using DUI;
using Diluvion.Roll;
using Sirenix.OdinInspector;
using Diluvion.Achievements;
using Diluvion.SaveLoad;

public enum TransactMode
    {
        playerBuys,
        playerSells,
        playerTakes,
        playerGives
    }


    [Serializable]
    public class StackedItem
    {
        public DItem item;
        public int qty;

        public StackedItem ()
        {
            qty = 0;
        }

        public StackedItem (DItem newItem)
        {
            item = newItem;
            qty = 1;
        }

        public StackedItem (DItem newItem, int newqty)
        {
            item = newItem;
            qty = newqty;
        }

        public StackedItem (StackedItem itemStack)
        {
            item = itemStack.item;
            qty = itemStack.qty;
        }

        /// <summary>
        /// The number of items that can still fit in this stack
        /// </summary>
        public int RoomForItem ()
        {
            float remainingRoom = 1f - Size();
            int qtyLeft = Mathf.CeilToInt(remainingRoom / item.size);
            return qtyLeft;
        }

        /// <summary>
        /// How many slots will this stack take up?
        /// </summary>
        public int RequiredSlots ()
        {
            float num = Size() * qty;
            return Mathf.CeilToInt(num);
        }

        public void Add ()
        {
            qty++;
        }

        public bool HasRoom ()
        {
            return (RoomForItem() > 0);
        }

        public float Size ()
        {
            if (item == null) return 0;
            return item.size * qty;
        }

        public int GoldValue ()
        {
            if (item == null) return 0;
            return item.goldValue * qty;
        }

        public override string ToString()
        {
            if (item == null) return "null item";

            return item.name + " X " + qty;
        }
    }

namespace Diluvion
{
    [Serializable]
    public class Inventory : MonoBehaviour
    {
        #region declare
        [AssetList(Path = "Prefabs/Inventories", AutoPopulate = false), AssetsOnly]
        public InvGenerator invGenerator;

        /// <summary>
        /// If true, the popup explaining that items have been added to storage has already been shown this session.
        /// </summary>
        public static bool shownStoragePopup;

        public event System.Action OnInitComplete;

        bool HasGen()
        {
            return invGenerator != null;
        }

        [ShowIf("HasGen"), InlineButton("TestPopulate")]
        [Tooltip("Gold amount to use for test population. This uses the same code path as normal pop, searching through " +
                 "the hierarchy for an item table holder.")]
        public int testGold = 100;

        void TestPopulate()
        {
            Clear();
            PopulateItems(testGold);
        }

        [ReadOnly]
        public int gold;

        [Tooltip("Extra slots added from the inventory upgrade kit on ships")]
        [ReadOnly]
        public int extraSlots;

        public List<StackedItem> itemStacks   = new List<StackedItem>();

        /// <summary>
        /// Callback every time inventory is accessed
        /// </summary>
        public delegate void InventoryChanged ();
        public InventoryChanged inventoryChanged;

        /// <summary>
        /// Callback when the player leaves this inventory
        /// </summary>
        public delegate void InventoryLeft (int remainingItems, Inventory inv);
        public InventoryLeft inventoryLeft;
        public bool init;

        /// <summary>
        /// revenue builds for merchants at a rate of InvGenerator.goldPerSecond
        /// </summary>
        [ReadOnly]
        public float revenue;
        
        
        public string LocalizedName ()
        {
            if (invGenerator.unlimitedSize)
                return Localization.GetFromLocLibrary("inv_storage", "no loc");
            if (invGenerator.playerShipInventory)
                return Localization.GetFromLocLibrary("inv_player", "no loc");
            if (invGenerator.merchant)
                return Localization.GetFromLocLibrary("inv_merchant", "no loc");

            return Localization.GetFromLocLibrary("inv_loot", "no loc");
        }

        public static bool IsKeyItem(StackedItem i)
        {
            return i.item.keyItem;
        }

        public static bool IsNotKeyItem(StackedItem i)
        {
            return !i.item.keyItem;
        }
        
        #endregion
        
        #region init

        void Start () { InitInventory(); }
        void OnEnable () { InitInventory(); }
        void OnSpawned () { InitInventory(); }

        public void ResetItemSpawn ()
        {
            Debug.Log("resetting item spawn");
            init = false;
        }

        public void InitInventory ()
        {
            if (doingStartSetup || init) return;
            StartCoroutine(InitBehaviors());
        }

        bool doingStartSetup;
        IEnumerator InitBehaviors ()
        {
            doingStartSetup = true;

            // Wait for an inv gen to be placed in me.
            while (invGenerator == null) yield return null;
            while (DSave.current == null) yield return null;
            invGenerator.InitializeInventory(this);
            
            init = true;
            doingStartSetup = false;
            
            OnInitComplete?.Invoke();
        }
        
#endregion

        float _updateTimer;
        float _merchantUpdateTime = 5;
        void Update()
        {
            // Progress towards an update 
            _updateTimer += Time.deltaTime;
            if (_updateTimer >= _merchantUpdateTime)
            {
                // Only do the update if trade panel isn't open. This way merchants aren't adding items DURING a transaction, 
                // which would be confusing to the player.
                if (UIManager.GetPanel<TradePanel>() == null)
                    InvUpdate();
                _updateTimer = 0;
            }
        }

        void InvUpdate()
        {
            if (invGenerator) invGenerator.UpdateInventory(this);
        }
        
        void OnDisable ()
        {
            doingStartSetup = false;
        }

        /// <summary>
        /// Finds the nearest table ancestor in the hierarchy, and uses it to generate items. Assumes that an invGenerator
        /// has already been set.
        /// </summary>
        public void PopulateItems(float goldValue)
        {
            if (!invGenerator)
            {
                Debug.LogError("Item population on " + name + " failed because no inventory generator is linked.",gameObject);
                return;
            }

            // Roll with it bruh - Have the inventory generator create items based on the gold value
            invGenerator.RollForItems(this, Mathf.RoundToInt(goldValue));
        }

        /// <summary>
        /// Opens either the full trade panel or the Loot panel, depending on the type  of inventory this is. Also plays
        /// the 'lootbox open' sfx
        /// </summary>
        public void CreateUI ()
        {
            if (!invGenerator) return;
            SpiderSound.MakeSound("Play_Lootbox_Open", gameObject);
            
            // create a traditional trade panel
            if (invGenerator.merchant || invGenerator.unlimitedSize || invGenerator.playerShipInventory)
            {
                invGenerator.onOpenTrade.Invoke();
                ShowTradePanel(this);
            }
            
            // if it's loot, create a loot panel
             else LootPanel.ShowLoot(this);
        }

        public static void ShowTradePanel(Inventory inv)
        {
            TradePanel newPanel = UIManager.Create(UIManager.Get().tradePanel as TradePanel);
            newPanel.Init(inv);
        }

        public void RemoveUI ()
        {
            UIManager.Clear<TradePanel>();
            UIManager.Clear<LootPanel>();
        }

        public void OnChanged ()
        {
            inventoryChanged?.Invoke();
        }

        public void OnLeftInventory ()
        {
            inventoryLeft?.Invoke(CurrentSize(), this);
        }

        public void EndedTransaction ()
        {
            OnLeftInventory();
        }

        /// <summary>
        /// Removes all items and gold from this instance.
        /// </summary>
        public void Clear ()
        {
            gold = 0;
            itemStacks = new List<StackedItem>();
        }

        #region getting items lists

        /// <summary>
        /// Returns a list of item stacks where the stack's item is the given type.
        /// </summary>
        public List<StackedItem> GetStacksOfType<T>() where T : DItem
        {
            List<StackedItem> returnList = new List<StackedItem>();
            foreach (StackedItem stack in AllItems())
            {
                if (stack.item == null) continue;
                if (stack.item is T) returnList.Add(stack);
            }

            return returnList;
        }

        /// <summary>
        /// Returns a list of item stacks where the stack's item is the given type, and passes the given item query
        /// </summary>
        /// <param name="itemQuery">A function taking an item stack and returning a bool</param>
        public List<StackedItem> GetStacksOfType<T> (Func<StackedItem, bool> itemQuery) where T : DItem
        {
            List<StackedItem> returnList = new List<StackedItem>();
            foreach (StackedItem stack in GetStacksOfType<T>())
            
                if (itemQuery(stack)) returnList.Add(stack);

            return returnList;
        }

        /// <summary>
        /// Returns a list of all items of the given item type.
        /// </summary>
        /// <typeparam name="T">Must be a type of DItem</typeparam>
        public List<T> GetItemsOfType<T> () where T : DItem
        {
            List<T> returnList = new List<T>();
            foreach (StackedItem stack in AllItems())
            {
                if (stack.item == null) continue;
                if (stack.item as T != null) returnList.Add(stack.item as T);
            }

            return returnList;
        }

        /// <summary>
        /// Returns a list of all items of the given type that fit into the given query.
        /// </summary>
        /// <typeparam name="T">Type of item to return</typeparam>
        /// <param name="itemQuery">Function for further filtering items. If the function is true, the item will be added to the list.</param>
        public List<T> GetItemsOfType<T> (Func<T, bool> itemQuery) where T : DItem
        {
            List<T> returnList = new List<T>();

            foreach (T item in GetItemsOfType<T>())
            {
                if (itemQuery(item)) returnList.Add(item);
            }

            return returnList;
        }



        /// <summary>
        /// Returns a list of all items cotained both in this inventory and in the given
        /// list of items.
        /// </summary>
        /// <typeparam name="T">Type of Ditem</typeparam>
        /// <param name="matchItems">Items to check for</param>
        public List<T> GetItemsOfType<T> (List<T> matchItems) where T : DItem
        {
            List<T> returnList = new List<T>();
            foreach (T foundItem in GetItemsOfType<T>())
                if (matchItems.Contains(foundItem)) returnList.Add(foundItem);

            return returnList;
        }

        /// <summary>
        /// Returns all items in the inventory
        /// </summary>
        public List<StackedItem> AllItems ()
        {
            List<StackedItem> newList = new List<StackedItem>(itemStacks);
            return newList;
        }

        #endregion


        /// <summary>
        /// Adds the given item to the inventory on worldControl. This can be accessed at any town.
        /// </summary>
        void AddToGlobalStorage (DItem i)
        {
            if (WorldControl.Get() == null)
            {
                Debug.LogWarning("World control couldn't be found while attempting to add " + i.niceName + " to global storage.");
                return;
            }

            Inventory globalInv = WorldControl.Get().GetComponent<Inventory>();
            globalInv.AddItem(i);
        }

        /// <summary>
        /// For merchants only, returns a list of all items missing from the merchant's initial stock
        /// </summary>
        public List<StackedItem> MissingItems()
        {
            if (!invGenerator) return null;
            
            List<StackedItem> missing = new List<StackedItem>();
            
            // Get a list of all the items in the original stock, where each item only appears once
            List<DItem> stocks = new List<DItem>();
            foreach (StackedItem stockItem in invGenerator.startingItems)
            {
                if (stocks.Contains(stockItem.item)) continue;
                stocks.Add(stockItem.item);
            }

            foreach (var i in stocks)
            {
                int max = RemainingItems(i, invGenerator.startingItems);
                int have = RemainingItems(i);

                int need = max - have;

                if (need > 0)
                    missing.Add(new StackedItem(i, need));
            }

            // Order list by gold value
            missing = missing.OrderBy(x => x.item.goldValue).ToList();

            return missing;
        }

        [Button]
        void LogMissing()
        {
            if (!invGenerator) Debug.LogError("Must have an inv generator to log missing items.");

            foreach (var element in MissingItems())
            {
                Debug.Log(element.ToString());
            }
        }

        /// <summary>
        /// Returns the gold value of the given item stack taking into account the merchant's sell / buy percentage.
        /// </summary>
        /// <param name="selling">if true, uses this inventory's sell percentage. otherwise uses buy percentage</param>
        public int WeightedGoldValue (StackedItem itemStack, bool selling)
        {
            float multiplier = 1;
            if (invGenerator.merchant)
            {
                if (selling) multiplier = invGenerator.sellPercentage;
                else multiplier = invGenerator.buyPercentage;
            }

            return Mathf.RoundToInt(itemStack.GoldValue() * multiplier);
        }

        /// <summary>
        /// Transfers the given item stack from this inventory to another inventory.
        /// </summary>
        public bool Transaction (Inventory recievingInv, StackedItem items, bool cashTransfer, float costMultiplier)
        {
            // check if the recieving inventory is full
            if (!recievingInv.CanFit(items))
            {
                // popup for full inventory -- old locKeys: "misc_noSpace" "misc_noSpaceNoStorage"
                PopupsGlobal.GetPopup("no space").CreateUI();
                
                return false;
            }

            int goldValue = Mathf.RoundToInt(items.GoldValue() * costMultiplier);

            //If there's a cash transfer for this transaction, check if there's enough gold 
            //and transact the gold
            if (cashTransfer)
            {
                if (recievingInv.gold < goldValue)
                {
                    // show popup for no money
                    PopupsGlobal.GetPopup("not enough cash").CreateUI();
                    // old loc keys: "misc_noGold" "misc_noGoldDescr"
                    return false;
                }

                // transfer the money from recieving inventory to this inventory
                recievingInv.SpendGold(goldValue);
                AddGold(goldValue);
            }

            //Make a duplicate stack to add fartsnacks
            StackedItem duplicate = new StackedItem(items);

            //remove items from this inventory 
            RemoveItem(items);

            //Add the duplicate stack to the other inv
            recievingInv.AddItem(duplicate);

            // TODO fix this acheivement thing to work with new system
            //if (duplicate.item.GetComponent<AchievementTrigger>())
            //  duplicate.item.GetComponent<AchievementTrigger>().UpdateAch();

            // Save the new inventory values MOVED TO add and remove item functions
            //SaveInventory();
            //recievingInv.SaveInventory();
            
            InventoryClicker.RefreshSprites();

            return true;
        }
        

        /// <summary>
        /// Replaces the current items list with all the items in the given list of stacks.
        /// </summary>
        public void ReplaceItemsList (List<StackedItem> stacks)
        {
            itemStacks.Clear();
            foreach (StackedItem stack in stacks) AddItem(stack);
        }


        #region add and remove items

        /// <summary>
        /// Tries to add the item to inventory.  If there's not enough room in any existing stacks of the given
        /// item, tries to create a new stack. If not enough room for a new stack, returns false.
        /// </summary>
        public bool AddItem (DItem item, bool overflowToStorage = false)
        {
            if (item == null) return false;

            //Check all the stacks to find one that's matching this item
            foreach (StackedItem stack in itemStacks)
            {
                if (stack.item != item) continue;
                if (stack.HasRoom())
                {
                    stack.qty++;
                    CompressedSave();
                    return true;
                }
            }

            //If you've gotten here it means that no stacks match the item. 
            //Check if there's room to create a new stack
            if (RemainingSlots() >= 1 || item.keyItem)
            {
                //Create a new stack, put the item in it
                StackedItem newStack = new StackedItem(item);

                //Add the stack to the list
                itemStacks.Add(newStack);
                CompressedSave();
                return true;
            }

            if (overflowToStorage)
            {
                if (!shownStoragePopup)
                {
                    // TODO show storage popup
                    Debug.Log("POPUP: Adding items to storage");
                    PopupsGlobal.GetPopup("storage inventory").CreateUI();
                    shownStoragePopup = true;
                }
                
                Debug.Log("storage inventory adding " + item.name);
                PlayerManager.PlayerStorage().AddItem(item);
                return true;
            }

            return false;
        }

        public bool AddItem (StackedItem stack, bool overflowToStorage = false)
        {
            for (int j = 0; j < stack.qty; j++)
            {
                if (!AddItem(stack.item, overflowToStorage)) 
                    return false;
                
                if (j == 0 && IsPlayerInventory())
                    stack.item.OnGain();
            }
            
            if (IsPlayerInventory()) QuestManager.Tick();
            
            return true;
        }

        /// <summary>
        /// Add Multiple Items
        /// </summary>
        public void AddItems (List<DItem> items)
        {
            foreach (DItem i in items) AddItem(i);
        }


        public bool RemoveItem (StackedItem stack)
        {
            int i = 0;
            int qty = stack.qty;

            while (i < qty)
            {
                RemoveItem(stack.item);
                i++;
                if (i > 10000) break;
            }

            return true;
        }

        /// <summary>
        /// Removes one of the given item from the inventory.
        /// </summary>
        public bool RemoveItem (DItem theItem)
        {
            Debug.Log("Removing one " + theItem.name + " from " + name, gameObject);
            
            bool removed = false;
            foreach (StackedItem stack in itemStacks)
            {
                if (stack.item == theItem && stack.qty > 0)
                {
                    stack.qty--;
                    removed = true;
                    break;
                }
            }
            RemoveEmptyStacks();

            CompressedSave();

            return removed;
        }


        public void RemoveEmptyStacks ()
        {
            itemStacks = itemStacks.Where(x => (x.item != null && x.qty > 0)).ToList();
        }

        #endregion

        #region queries
        
        /// <summary>
        /// Returns the sum of the gold values of every item in the list
        /// </summary>
        public static int SumItemValue (List<StackedItem> itemList)
        {
            int v = 0;
            if (itemList == null) return 0;
            foreach (StackedItem i in itemList)
            {
                if (i == null) continue;
                v += i.GoldValue();
            }
            return v;
        }

        /// <summary>
        /// Returns the sum item gold value of everry item in this inventory.
        /// </summary>
        public int SumItemValue ()
        {
            return SumItemValue(itemStacks);
        }

        /// <summary>
        /// Is this inventory completely empty?
        /// </summary>
        public bool IsEmpty()
        {
            RemoveEmptyStacks();
            return itemStacks.Count < 1;
        }

        /// <summary>
        /// Returns the inventory generator's max size plus the extra slots.
        /// </summary>
        public int MaxSize ()
        {
            if (invGenerator == null) return 0;
            if (invGenerator.unlimitedSize) return 0;
            return invGenerator.maxSize + extraSlots;
        }

        /// <summary>
        /// Checks if the inventory gen restricts purchase.
        /// </summary>
        /// <returns></returns>
        public bool CanTakeItem (DItem i)
        {
            if (!invGenerator) return false;
            if (!invGenerator.restrictedPurchase) return true;
            if (invGenerator.allowedItems.Contains(i)) return true;
            return false;
        }
        
        public bool CanFit (StackedItem items)
        {
            if (invGenerator.unlimitedSize) return true;
            if (items.item.keyItem) return true;
            if (CurrentSize() + 1 <= MaxSize()) return true;

            //This is the quantity of items that needs to fit
            int qty = items.qty;

            //Check each stack in my list of stacks
            foreach (StackedItem stack in itemStacks)
            {
                //If the items match
                if (stack.item == items.item)
                {
                    //reduce the qty of items that need to fit by my stack's room left
                    qty -= stack.RoomForItem();
                }
            }

            //If all the items that needed to fit did, return true
            if (qty <= 0) return true;
            else return false;
        }
        
        /// <summary>
        /// Returns the current size (in slots) of all the items in this inventory.
        /// </summary>
        public int CurrentSize ()
        {
            int size = 0;

            RemoveEmptyStacks();

            //Count the size of each item in stacks
            foreach (StackedItem itemStack in itemStacks)
            {
                if (itemStack.item == null) continue;
                if (itemStack.item.keyItem) continue;
                size++;
            }
            return size;
        }
        
        /// <summary>
        /// How many slots are still available in this inventory
        /// </summary>
        public int RemainingSlots ()
        {
            if (invGenerator == null) return 0;
            if (invGenerator.unlimitedSize) return 999;
            return MaxSize() - CurrentSize();
        }

        public bool IsPlayerInventory ()
        {
            if (!invGenerator) return false;
            return invGenerator.playerShipInventory;
        }

        public bool IsStorageInventory()
        {
            return PlayerManager.PlayerStorage() == this;
        }

        //Uses an item, adds husk
        public bool UseItem (DItem theItem)
        {

            if (theItem == null) return false;
            if (RemoveItem(theItem)) return true;
            return false;
        }

        public bool HasItem (DItem item)
        {
            RemoveEmptyStacks();
            foreach (StackedItem i in itemStacks)
                if (i.item == item) return true;

            return false;
        }

        /// <summary>
        /// The total number of the given item that are in this inventory
        /// </summary>
        public int RemainingItems (DItem theItem)
        {
            return RemainingItems(theItem, itemStacks);
        }

        int RemainingItems(DItem theItem, List<StackedItem> itemList)
        {
            int qty = 0;

            //Check all the item stacks
            foreach (StackedItem itemStack in itemList)
            {
                //If the stack is of the given item, add its qty
                if (itemStack.item == theItem) qty += itemStack.qty;
            }
            return qty;
        }
        
        #endregion

        #region gold money exchange

        /// <summary>
        /// If the requested amount isn't available, returns false without taking any action.
        /// </summary>
        public bool SpendGold (int amount)
        {
            if (amount > gold) return false;
            
            if (PlayerManager.PlayerInventory() == this)
                DUIMoneyPanel.ShowChange(gold, gold - amount);
            
            gold -= amount;

            CompressedSave();
            return true;
        }

        public void StealGold(int amount)
        {
            if (PlayerManager.PlayerInventory() == this)
                DUIMoneyPanel.ShowChange(gold, gold - amount);
            gold -= amount;
            if (gold < 0) gold = 0;

            CompressedSave();
        }

        public void AddGold (int amount)
        {
            if (PlayerManager.PlayerInventory() == this)
                DUIMoneyPanel.ShowChange(gold, gold + amount);
            gold += amount;

            CompressedSave();
        }

        public void UpdateGoldAchievement ()
        {
            if (IsPlayerInventory())
                SpiderAchievementHandler.Get().SetAchievement("ach_millionaire", gold);
        }
        
        #endregion
        
        #region save load
        
        /// <summary>
        /// Loads inventory from an inventory save file
        /// </summary>
        public void LoadInventory (InventorySave inv, bool overflowToStorage)
        {
           // Debug.Log(name + " is loading inventory from save file.");
            gold = inv.gold;
            itemStacks = new List<StackedItem>();

            List<DItem> itemsFromSave = ItemsGlobal.GetItems(inv.invStrings);
            itemsFromSave = itemsFromSave.Where(x => x != null).ToList();

           // Debug.Log(name + " loaded inventory from save file. Items: " + itemsFromSave.Count, gameObject);

            foreach (DItem i in itemsFromSave)
            {
                if (!AddItem(i))
                {
                    if (overflowToStorage)
                    {
                        //Debug.Log("Item " + i.name + " doesn't fit in " + name + ", so it's being placed in player's storage.");
                        PlayerManager.PlayerStorage().AddItem(i);
                    }
                    else Debug.LogError("Failed to add " + i.name, gameObject);
                }
            }
          
            OnChanged();
        }


        bool _alreadySaving;
        void CompressedSave()
        {
            if (_alreadySaving) return;
            _alreadySaving = true;
            StartCoroutine(DelaySave());
        }

        IEnumerator DelaySave()
        {
            //Debug.Log("staring delay save");
            float counter = 0;
            while (counter < 1)
            {
                counter += Time.unscaledDeltaTime;
                yield return null;
            }
            
            SaveInventory();
            _alreadySaving = false;
        }
        
        /// <summary>
        /// Saves this inventory (if it needs to be saved)
        /// </summary>
        void SaveInventory ()
        {
            if (invGenerator) invGenerator.TrySave(this);
        }
        
        #endregion
    }
}