using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Diluvion;
using Diluvion.Ships;

namespace DUI
{

    public class DUIDebugTeleportMenu : DUIDebug
    {
        public RectTransform buttonPrefab;
        public Transform buttonParent;

        List<CheckPoint> sceneCheckPoints;
        List<LandMark> sceneLandmarks;

        List<GameObject> currentTeleportButtons = new List<GameObject>();



        List<Bridge> AllNPCBridgesInScene()
        {
            List<Bridge> returnBridge = new List<Bridge>();
            foreach (Bridge b in FindObjectsOfType<Bridge>())
            {
                if (b.IsPlayer()) continue;
                returnBridge.Add(b);
            }
            return returnBridge;
        }

        List<GameObject> AllExplorables()
        {
            List<GameObject> returnExplorables = new List<GameObject>();
            foreach (Explorable b in FindObjectsOfType<Explorable>())
            {
                returnExplorables.Add(b.gameObject);
            }
            foreach (DebugExplorable b in FindObjectsOfType<DebugExplorable>())
            {
                returnExplorables.Add(b.gameObject);
            }
            return returnExplorables;
        }

        protected override void Start()
        {
            sceneLandmarks = new List<LandMark>(LandMark.AllLandmarks());
            if (sceneCheckPoints == null) sceneCheckPoints = new List<CheckPoint>();
            sceneCheckPoints.Clear();
            sceneCheckPoints.AddRange(FindObjectsOfType<CheckPoint>());
        }

        //Construct a list of checkPoint buttons and their callbacks
        public void ConstructCheckPointList()
        {
            ClearButtonList();
            foreach (CheckPoint cp in sceneCheckPoints)
                currentTeleportButtons.Add(BuildButton(cp.gameObject, cp.name));
        }

        public void ConstructCameraList()
        {
            ClearButtonList();
            foreach (CinemaCam cc in FindObjectsOfType<CinemaCam>())
                currentTeleportButtons.Add(BuildButton(cc.gameObject, cc.name));
        }



        public void ConstructLandmarkList()
        {
            ClearButtonList();
            foreach (LandMark lm in sceneLandmarks)
                currentTeleportButtons.Add(BuildButton(lm.gameObject, lm.niceName));
        }

        public void ConstructEnemyList()
        {
            ClearButtonList();
            foreach (Bridge b in AllNPCBridgesInScene())
                currentTeleportButtons.Add(BuildButton(b.gameObject, b.name));
        }

        public void ConstructExplorableList()
        {
            ClearButtonList();
            foreach (GameObject e in AllExplorables())
                currentTeleportButtons.Add(BuildButton(e, e.name));
        }

        void ClearButtonList()
        {
            foreach (GameObject go in currentTeleportButtons)
                Destroy(go);
            currentTeleportButtons.Clear();
        }


        GameObject BuildButton(GameObject go, string inputName)
        {
            RectTransform buttoninstance = Instantiate(buttonPrefab);
            Text text = buttoninstance.GetComponentInChildren<Text>();
            text.text = inputName;
            buttoninstance.SetParent(buttonParent, false);

            buttoninstance.GetComponent<Button>().onClick.AddListener(delegate { TeleportTo(go); });
            return buttoninstance.gameObject;
        }

        public void TeleportTo(GameObject go)
        {
            if (go.GetComponent<Explorable>())
            {
                if (go.GetComponent<Explorable>().debugEx != null)
                    go.GetComponent<Explorable>().debugEx.Activate(true);
            }

            if (go.GetComponent<DebugExplorable>())
            {
                go.GetComponent<DebugExplorable>().Activate(true);
            }
            CheatManager.Get().WarpPlayerTo(go.transform.position - OrbitCam.Get().transform.forward * 15);
        }
    }
}