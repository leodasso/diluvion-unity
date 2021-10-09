using System.Collections.Generic;
using Diluvion;
using UnityEngine;
using Diluvion.SaveLoad;
using Diluvion.Ships;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace DUI
{
    public class DUIShipSelect : DUIView
    {
        public TalkyText description;
        public ShipSelection defaultShip;
        [ReadOnly]
        public ShipSelection selectedShip;

        List<ShipSelection> shipSelections = new List<ShipSelection>();


        protected override void Start()
        {
            base.Start();
            shipSelections.AddRange(GetComponentsInChildren<ShipSelection>());

            SelectAShip(defaultShip);
            
        }

        public void SelectAShip(ShipSelection s)
        {
            selectedShip = s;

            foreach (var sel in shipSelections)
                sel.Defocus();
            
            s.Focus();
            
            string descr = SpiderWeb.Localization.GetFromLocLibrary(s.descriptionLocKey, s.descriptionLocKey);
            
            description.Clear();
            description.inputText = descr;
        }

        public void StartWithShip()
        {
                        
            // create chassis data
            SubChassisData myData = new SubChassisData(selectedShip.chassis);
            
            // add loadout to the data
            selectedShip.loadout.AddToChassis(myData);
            
            if (DSave.current == null)
            {
                Debug.LogError("Can't select starting sub because there's no current dsave!");
                return;
            }

            DSave.current.AddShip(myData, true);
            
            GameManager.BeginGame(DSave.current);
        }
    }
}