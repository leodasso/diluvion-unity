using UnityEngine;
using Loot;

namespace Diluvion.Ships
{

    public class HeartStation : Station
    {

        float repairInterval = 30;      // Gets 1 HP per crew every 30 seconds
        float _timer;
        Hull _hull;
        DItemRepair _repairKitItem;

        protected override void Awake()
        {
            base.Awake();
            _repairKitItem = ItemsGlobal.GetItem("item_repair kit") as DItemRepair;
            operational = true;
        }

        protected override void Update()
        {

            base.Update();

            int crew = GetCrewCount();
            if (crew < 1) return;

            float totalInterval = repairInterval / crew;
            _timer += Time.deltaTime;
            if (_timer >= totalInterval)
            {
                Repair();
                _timer = 0;
            }
        }

        
        Hull MyHull()
        {
            if (_hull != null) return _hull;
            _hull = Interior().GetWorldParent().GetComponent<Hull>();
            return _hull;
        }

        /// <summary>
        ///Null safe repairkit item check
        /// </summary>
        bool CanUseRepairKit()
        {
            if (!PlayerManager.PlayerInventory()) return false;
            if (!PlayerManager.PlayerInventory().HasItem(_repairKitItem)) return false;
            if (MyHull() == null) return false;
            if (MyHull().currentHealth > _hull.maxHealth - _repairKitItem.HPPerUse) return false;
            return true;
        }

        /// <summary>
        /// Checks for repair kits to use to repair the ship. If no kits, just repairs 1 health.
        /// </summary>
        void Repair()
        {
            //If a repair kit was found, and we are missing health, use it.
            if (CanUseRepairKit())
            {
                _repairKitItem.Use();
                return;
            }

            MyHull().Repair(1);
        }


        public override void UpdateStation()
        {
            if (GetComponentsInChildren<Character>().Length > 0)
                EnableStation();
            else DisableStation();
        }

        public override void OnPointerEnter()
        {
        }


        public override void OnPointerExit()
        {
        }


        public override void DisableStation()
        {
            if (!animator) return;
            animator.SetBool("operational", false);
        }

        public override void EnableStation()
        {
            if (!animator) return;
            animator.SetBool("operational", true);
        }


        public override void OnRelease() { }

        public override bool HasRoomForCrew(GameObject joiningCrew)
        {
            return true;
        }
    }
}