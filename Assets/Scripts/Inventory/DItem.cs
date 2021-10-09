using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Sirenix.OdinInspector;
using SpiderWeb;
using DUI;
using Diluvion.Roll;
using Diluvion;
using Diluvion.SaveLoad;
using Sirenix.Serialization;

namespace Loot
{
    [CreateAssetMenu(fileName = "new item", menuName = "Diluvion/items/item")]
    public class DItem : Entry
    {
        [PropertyOrder(-999)]
        public string locKey;

        [ButtonGroup("Loc"), PropertyOrder(-998)]
        void UseName()
        {
            locKey = name;
        }
        
        [ButtonGroup("Loc"), PropertyOrder(-998)]
        void AddToLocLibrary()
        {
            AddToLibrary();
        }
        
        [Title("Name"), HideLabel]
        public string niceName;
        [TextArea(1, 3)]
        public string description;
        
        public Color myColor = Color.white;

        public int goldValue;

        [Range(0, 1)]
        public float size = 1.0f;         // .5 will fit 2 in a slot, .2 will fit 5, etc
        

        [ToggleLeft]
        public bool keyItem;

        [ToggleLeft]
        public bool useable;
        
        public Sprite icon;

        [Title("On Acquire"), ToggleLeft]
        [Tooltip("When this is acquired for the first time, should a description window be shown?")]
        public bool showDescription;
        
        [ToggleLeft, LabelText("show popup")]
        public bool popupOnAcquire;
        [ShowIf("popupOnAcquire")]
        public PopupObject onAcquirePopup;
        
        [DrawWithUnity]
        public UnityEvent onAcquire;
        
        

        [Title("Resource Info")]
        public int lowQty = 1;

        [Range(0, 5)]
        public int timesToWarn = 1;
        
        [Button]
        protected void AddToShip()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Can't add when not playing.");
                return;
            }

            Inventory i = PlayerManager.PlayerInventory();
            if (!i)
            {
                Debug.LogWarning("No player ship found.");
                return;
            }

            i.AddItem(this);
            
            DUIInventory.RefreshAll();
        }

        public static DItem ItemFromString(string itemName)
        {
            return ItemsGlobal.GetItem(itemName);
        }

        public override int Value()
        {
            return goldValue;
        }

        
        public virtual void AddToLibrary()
        {
            Localization.AddToKeyLib("item_" + locKey, niceName);
            Localization.AddToKeyLib("item_descr_" + locKey, description);
        }

        public string LocalizedName()
        {
            return Localization.GetFromLocLibrary("item_" + locKey, niceName);
        }

        public virtual string LocalizedBody()
        {
            return Localization.GetFromLocLibrary("item_descr_" + locKey, description);
        }


        public virtual void Use()
        {

        }

        /// <summary>
        /// Removes from player inventory.
        /// </summary>
        protected void RemoveFromPlayerInventory()
        {
            // Removes the item from this inventory

            PlayerManager.pBridge.GetInventory().RemoveItem(this);

            DUIInventory.RefreshAll();
        }

        public virtual void OnGain()
        {
            if (DSave.current == null) return;
            
            onAcquire.Invoke();
            
            if (popupOnAcquire) onAcquirePopup?.CreateUI();

            if (!showDescription) return;
            
            // check if save file has this item. if not, show the new item window 
            if (!DSave.current.HasGottenItem(name)) 
                ItemDescriptor.ShowNewItemPanel(this);
        }

        public virtual bool IsStealable()
        {
            return !keyItem;
        }
       
        public virtual void OnLose() { }
    }
}