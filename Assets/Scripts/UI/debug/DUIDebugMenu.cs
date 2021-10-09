using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;

namespace DUI
{

    public class DUIDebugMenu : DUIDebug
    {

        public RectTransform teleportPrefab;
        public RectTransform inventoryPrefab;
        public RectTransform shipPrefab;
        public RectTransform scenePrefab;
        public RectTransform questPrefab;
        public RectTransform spawnPrefab;
        public RectTransform achPrefab;


        public bool showExplorables = true;
        public DUIExplorableFollower explorableContextFollower;
        List<GameObject> duiContextList;
        Dictionary<Explorable, DUIFollower> duiContextPairing = new Dictionary<Explorable, DUIFollower>();

        public void AddTeleportMenu()
        {
            RectTransform teleportInstance = Instantiate(teleportPrefab);
            teleportInstance.SetParent(transform, false);
            teleportInstance.SetAsLastSibling();
        }

        public void AddInventoryMenu()
        {
            RectTransform inventoryInstance = Instantiate(inventoryPrefab);
            inventoryInstance.SetParent(transform, false);
            inventoryInstance.SetAsLastSibling();
        }

        public void AddShipMenu()
        {
            RectTransform shipinstance = Instantiate(shipPrefab);
            shipinstance.SetParent(transform, false);
            shipinstance.SetAsLastSibling();
        }

        public void AddSceneMenu()
        {
            RectTransform sceneInstance = Instantiate(scenePrefab);
            sceneInstance.SetParent(transform, false);
            sceneInstance.SetAsLastSibling();
        }

        public void AddQuestMenu()
        {
            RectTransform questInstance = Instantiate(questPrefab);
            questInstance.SetParent(transform, false);
            questInstance.SetAsLastSibling();

        }

        public void AddFightMenu()
        {
            RectTransform fightInstance = Instantiate(spawnPrefab);
            fightInstance.SetParent(transform, false);
            fightInstance.SetAsLastSibling();

        }

        public void AddAchMenu()
        {
            RectTransform achInstance = Instantiate(achPrefab);
            achInstance.SetParent(transform, false);
            achInstance.SetAsLastSibling();

        }

        public override void Kill()
        {
            CheatManager.Get().showing = false;
            base.Kill();
        }


        protected override void Start()
        {
            GetNearByObjectsToFollow();
        }

        void OnDestroy()
        {
            KillAllFollowers();
        }


        List<Collider> allColliders = new List<Collider>();
        List<DUIFollower> activeFollowers = new List<DUIFollower>();
        void GetNearByObjectsToFollow()
        {
            
#if UNITY_EDITOR
            if (PlayerSub() == null) return;
            if (showExplorables)
            {
                allColliders.Clear();

                foreach (Explorable e in FindObjectsOfType<Explorable>())
                    e.StartDebug();

                allColliders.AddRange(Physics.OverlapSphere(PlayerSub().transform.position, 200, LayerMask.GetMask("Tools")));
                foreach (Collider e in allColliders)
                    AddExploreFollower(e.GetComponent<DebugExplorable>());
            }
#endif
        }

        void AddExploreFollower(DebugExplorable explorable)
        {
            if (explorable == null) return;
            DUIFollower duiFollow = Instantiate(explorableContextFollower);
            duiFollow.transform.SetParent(transform);
            duiFollow.transform.SetAsFirstSibling();
            duiFollow.AddTarget(explorable.gameObject);
        }

        void KillAllFollowers()
        {
            foreach (DUIFollower dui in activeFollowers)
                Destroy(dui.gameObject);
        }

    }

}