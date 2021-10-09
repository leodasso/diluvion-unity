using UnityEngine;
using System.Collections.Generic;
using HeavyDutyInspector;
using Diluvion.Ships;

namespace Diluvion.SaveLoad
{

    [CreateAssetMenu(fileName = "DebugSave", menuName = "Diluvion/Debug Save")]
    public class DebugSave : ScriptableObject
    {
        public Inventory importInventory;
        public CrewPackage importCrew;
        public List<Bridge> importBridges;
        [Button("Import Objects To save", "ImportObjects", true)]
        public bool importButton;


        [SerializeField]
        public DiluvionSaveData saveD;

        public DebugSave() { }


        //Converts the importInventory, importCrew and importBridges above to saveFileData
        public void ImportObjects()
        {
            if (importInventory != null)
            {
                saveD.SavePlayerInventory(importInventory);

            }
            /*
            if (importCrew != null)
            {
                List<CrewData> cData = new List<CrewData>();
                foreach (Crew c in importCrew.theCrew)
                    cData.Add(c.myData);
                saveD.SavePlayerCrew(cData);
            }
            /*
            if(importBridges!=null)
                if(importBridges.Count>0)
                {
                    List<LiveShipData> shipData = new List<LiveShipData>();
                    foreach (Bridge b in importBridges)
                        shipData.Add(new LiveShipData(new ShipSave(b)));
                    //saveD.SavePlayerShips(shipData);               
                }
                */
        }


        public void OnEnable()
        {
            saveD.saveFileName = name;
        }

        public void ExportToFile()
        {
            //DSave.Save(saveD, saveD.saveFileName);
        }


        public bool readOnly = false;
    }
}