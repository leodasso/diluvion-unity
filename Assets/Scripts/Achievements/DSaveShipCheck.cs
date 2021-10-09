using System.Collections;
using System.Collections.Generic;
using Diluvion.SaveLoad;
using UnityEngine;
using Diluvion.Ships;

namespace Diluvion.Achievements
{
    [CreateAssetMenu(fileName = "Save ship check", menuName = "Diluvion/Achievement/ShipAchievementCheck")]
    public class DSaveShipCheck : DSaveAchievement
    {
        public List<SubChassis> shipsToCheckFor = new List<SubChassis>();

        private string debugString="";
        public override int Progress(DiluvionSaveData checkFile)
        {
            base.Progress(checkFile);

            foreach (SubChassis sc in shipsToCheckFor)
            {
                if (debug)
                    debugString += sc.name;
                bool found = false;
                foreach (SubChassisData scd in checkFile.playerShips)
                {
                    if (sc.name == scd.chassisName)
                    {
                        found = true;
                        progress++;
                        break;
                    }
                }
               
                if (debug)
                    if (found)
                    {
                        debugString += " (FOUND) ";
                    }
                    else
                    {
                        debugString += " (NOT FOUND) ";
                    }
            }

            if (debug)
            {
                Debug.Log("Checked Save File for " + debugString + progress + " / " + shipsToCheckFor.Count );
            }
               
            return progress;
        }
    }
}
    