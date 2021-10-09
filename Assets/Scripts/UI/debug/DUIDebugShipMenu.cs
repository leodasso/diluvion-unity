using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Diluvion.Ships;
using Diluvion.AI;
using Diluvion;
namespace DUI
{

    public class DUIDebugShipMenu : DUIDebug
    {
        public RectTransform shipSelectionTransform;
        public RectTransform shipButtonPrefab;
        bool invulnerable;
      

        void GetAllGameShips()
        {
            List<ShipBuild> debugBuilds = GlobalList.LoadObjects<ShipBuild>("Assets/Prefabs/AI/ShipBuilds");

            foreach (ShipBuild sb in debugBuilds)
            {
                BuildButton(sb);
            }
        }

        GameObject BuildButton(ShipBuild sbs)
        {
            RectTransform buttoninstance = Instantiate(shipButtonPrefab);
            Text text = buttoninstance.GetComponentInChildren<Text>();
            text.text = sbs.name;
            buttoninstance.SetParent(shipSelectionTransform, false);
            buttoninstance.GetComponent<Button>().onClick.AddListener(delegate { SwapShipTo(sbs.chassis, sbs.shipBuildSettings); });
            return buttoninstance.gameObject;
        }

        public void SwapShipTo(SubChassis chassis)
        {
            PlayerManager.DebugInstantiatePlayerSub(chassis, Vector3.zero, null);
        }
        public void SwapShipTo(SubChassis chassis, ShipBuildSettings sbs=null)
        {
            PlayerManager.DebugInstantiatePlayerSub(chassis, Vector3.zero, sbs);
        }

        public void ToggleInvul(bool invul)
        {
            CheatManager.Get().SetInvulnerable(invul);
        }

        public void SetHealth(float percentage)
        {
            CheatManager.Get().SetHealth(percentage);
        }

        public void AddCrushDepth(float amount)
        {
            CheatManager.Get().AddCrushDepth(amount);
        }

        public void AddSailor()
        {
            CheatManager.Get().AddRandomCrew();
        }
    }
}