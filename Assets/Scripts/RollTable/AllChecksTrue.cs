using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion.Roll
{
    /// <summary>
    /// Inherited SpotFinder class, returns true on ValidCheck if all the checks are true
    /// </summary>
    [CreateAssetMenu(fileName = "new allValid spotfinder", menuName = "Diluvion/SpotFinder/allValid spotfinder")]
    public class AllChecksTrue : RandomSpotFinder
    {

        /// <summary>
        /// This Validcheck will only return true if all its checks are true
        /// </summary>

        public override bool ValidCheck(Vector3 startPos, float radius, ref Vector3 pos, ref Quaternion rot)
        {
            bool allValid = true;

            //For each check we have in our checkList, run a validCheck
            for (int i = 0; i < checks.Count; i++)
            {
                if (checks[i] == null) continue;
                SpawnCheck check = checks[i];

                if (!check.ValidCheck(startPos, radius, ref pos, ref rot))// Will break out of this try early if any of the checks return false
                {
                    allValid = false;
                    break;
                }
            }

            return allValid;
        }

        public override string ToString()
        {
            string allCheckTexts = " <color=white><b>" + this.name + "</b></color>: Will try up to " + maxTryCount + " times to find a spot where \n";
            foreach (SpawnCheck ds in checks)
            {
                allCheckTexts += ds.ToString();
                if (checks.IndexOf(ds) != checks.Count - 1)
                    allCheckTexts += " AND, \n";
            }
            allCheckTexts += "\n are all <color=white><b>TRUE</b></color>";

            return allCheckTexts;
        }
    }
}
