using UnityEngine;
using UnityEngine.UI;
using Diluvion;
using System.Collections.Generic;
using Diluvion.SaveLoad;

namespace DUI
{

    public class DUITravel : DUIView
    {
        public Button travelButton;
        public DUITravelSelector travelSelector;

        GameZone currentlySelectedZone;
        
        public static List<GameZone> newZones = new List<GameZone>();
        
        protected override void Start()
        {
            base.Start();
            travelButton.interactable = false;
        }

        public void SelectZone(DUIZoneButton selected)
        {
            travelSelector.ApplySelection(selected.gameObject);
            SelectNoise();
            currentlySelectedZone = selected.zone;
            
            travelButton.interactable = true;
            if (currentlySelectedZone == GameManager.CurrentZone())
                travelButton.interactable = false;
        }

        public void SelectNoise()
        {
            GetComponent<AKTriggerPositive>().TriggerPos();
        }

        public void HoverNoise()
        {
            GetComponent<AKTriggerNegative>().TriggerNeg();
        }

        public void Travel()
        {
            if (!currentlySelectedZone) return;
            
            // If travelling to the zone we're already in, just cancel
            if (currentlySelectedZone == GameManager.CurrentZone())
                return;
            
            GameManager.TravelTo(currentlySelectedZone);
        }

        public override void End()
        {
            newZones.Clear();
            base.End();
        }
    }
}