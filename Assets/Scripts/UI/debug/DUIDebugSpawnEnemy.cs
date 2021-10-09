using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;
using Diluvion;
using Diluvion.Ships;
using Diluvion.AI;

namespace DUI
{

    public class DUIDebugSpawnEnemy : DUIDebug
    {
        public List<CaptainScriptableObject> ais;
        CaptainScriptableObject selectedAI;
        public Text currentlySelectedAIName;
        public PatrolPath aiPath;
        public Button textButtonReference;
        public Transform shipSpawnTarget;
        public Transform aiSpawnTarget;

        List<GameObject> aiButtons = new List<GameObject>();
        List<GameObject> shipButtons = new List<GameObject>();

        protected override void Start()
        {
            BuildAIButtons();
            BuildEnemyButtons();
        }

        #region AI
        public void BuildAIButtons()
        {
            shipButtons.Clear();
            foreach (CaptainScriptableObject ai in ais)
            {
                if (ai == null) continue;
                shipButtons.Add(AddEnemyAIButton(ai, ai.name));
            }
            SetAI(ais[0]);
        }


        public GameObject AddEnemyAIButton(CaptainScriptableObject ai, string itemName)
        {
            Button buttonInstance = Instantiate<Button>(textButtonReference);
            buttonInstance.transform.SetParent(aiSpawnTarget);
            buttonInstance.GetComponent<Button>().onClick.AddListener(delegate { SetAI(ai); });
            buttonInstance.GetComponentInChildren<Text>().text = itemName;
            return buttonInstance.gameObject;

        }

        public void SetAI(CaptainScriptableObject person)
        {
            selectedAI = person;
            currentlySelectedAIName.text = "AI: " + person.name;
        }
        #endregion

        #region Ships
        public void BuildEnemyButtons()
        {
            shipButtons.Clear();
            /*
            foreach (GameObject go in PrefabDicts.Get().GetPrefabsByPrefix("ship"))
            {
                if (go == null) continue;
                if (!go.GetComponent<Bridge>()) continue;
                shipButtons.Add(AddEnemySpawnButton(go, go.name));
            }
            foreach (GameObject go in PrefabDicts.Get().GetPrefabsByPrefix("ark"))
            {
                if (go == null) continue;
                if (!go.GetComponent<ArkCreature>()) continue;
                shipButtons.Add(AddEnemySpawnButton(go, go.name));
            }
            */
        }


        public GameObject AddEnemySpawnButton(GameObject prefab, string itemName)
        {
            Button buttonInstance = Instantiate<Button>(textButtonReference);
            buttonInstance.transform.SetParent(shipSpawnTarget);
            buttonInstance.GetComponent<Button>().onClick.AddListener(delegate { AddEnemy(prefab); });
            buttonInstance.GetComponentInChildren<Text>().text = itemName;
            return buttonInstance.gameObject;

        }

        #region path
        Vector3 spawnLocation;
        public void AddEnemy(GameObject prefab)
        {
            spawnLocation = PlayerSub().transform.position + OrbitCam.Get().transform.forward * 30;

            Spawn(prefab, spawnLocation);

        }

        PatrolPath NewPath(Vector3 loc)
        {
            PatrolPath returnPath = Instantiate(aiPath, loc, Quaternion.identity) as PatrolPath;
            return returnPath;

        }

        public PatrolPath GetPath(Vector3 loc)
        {
            if (!NavigationManager.Get()) return NewPath(loc);
            /* PatrolPath closestPath = NavigationManager.Get().GetClosestPatrolPath(loc);
             if (closestPath == null) return NewPath(loc);
             if (!Calc.WithinDistance(50, closestPath.GetClosestLegalPos(loc), loc)) return NewPath(loc);*/
            return null;
        }

        public void Spawn(GameObject go, Vector3 location)
        {

            Quaternion startRotation = Quaternion.LookRotation(-Camera.main.transform.forward, transform.up);
            GameObject spawnedInstance = Instantiate(go, spawnLocation, startRotation) as GameObject;

            Bridge instanceBridge = spawnedInstance.GetComponent<Bridge>();
            //  ArkCreature arkCreature = spawnedInstance.GetComponent<ArkCreature>();
            if (instanceBridge)
            {
                // Set new crush depth
                Hull spawnedHull = spawnedInstance.GetComponent<Hull>();
                if (spawnedHull != null)
                    spawnedHull.testDepth = PlayerSub().GetHull().testDepth;

                Captain myCaptain = spawnedInstance.GetComponentInChildren<Captain>();
                if (myCaptain == null) return;

               /* myCaptain.ChangePersonality(selectedAI);
                myCaptain.FollowNewPath(GetPath(location));*/
            }
        }
        #endregion
        #endregion
    }
}