using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Diluvion.Roll
{
    /// <summary>
    /// base class for any single entry that is spawned by an explorable
    /// </summary>
    [CreateAssetMenu(fileName = "new spawnable", menuName = "Diluvion/RollTables/Spawnable")]
    public class ExplorableEntry : SpawnableEntry
    {

        [InlineEditor(Expanded = true), AssetsOnly]
        [Tooltip("These curves are used by room placer to determine how much value / danger to place in each room in the" +
                 "sequence. Any explorable without a room placer component won't need an object here.")]
        public ExplorableCurves curves;

        [Range(0, 25)]
        [Tooltip("Used to populate the rewards in this interior. The value per room ends up being [placer's value] " +
                 "X [gold value multiplier] X [room's evaluation on gold curve]")]
        public float goldValueMultiplier = 1;

        [Range(0, 25)]
        [Tooltip("Used to populate the dangers in this interior. The danger per room ends up being [placer's danger] " +
                 "X [danger multiplier] X [room's evaluation on danger curve]")]
        public float dangerMultiplier = 1;
       
        /// <summary>
        /// Returns the count of mesh renderers in the explorable prefab
        /// </summary>
        /*public override int TechCost()
        {
            if (SpawnablePrefab<Explorable>() == null) return 0;
            if (resourceCost.techCost > 0) return resourceCost.techCost;
            return resourceCost.techCost = (int)SpawnablePrefab<Explorable>().TechCost();
        }*/


        public override string ToString()
        {
            fullString += " and <color=yellow><b>" + prefab.name + "</b></color> is thinner (<color=yellow><b>" + Width() + 
                          "</b></color>)  than the input width \n";
            if (positionSearches == null) return fullString;
            foreach (RandomSpotFinder dsp in positionSearches)
            {
                if (dsp == null) continue;
                if (positionSearches.IndexOf(dsp) == 0)
                    fullString += "AND can find a spot with: \n  " + dsp;
                else
                    fullString += "  OR find a spot with, \n  " + dsp;
            }
            return fullString;
        }
    }
}


#region old code
/*
          //If this is the first one, it fits
          if (spawnedObjects.Count < 1) { return areaFree; }


          //If it is the second through 9th one, check the previous spawns to see if this space intersects with one of them
          for (int i = 0; i < spawnedObjects.Count; i++)
          {
              Spawnable spawned = spawnedObjects[i];
              if (spawned == null) continue;
              //Check to see if the new position intersects with anything we already spawned
              // Debug.Log("Testing:  " + prefab.name + " and " + spawned.name);
              if (Calc.SpheresIntersect(0.1f, legalpos, (int)prefab.size * 5f * radius, spawned.transform.position, (int)spawned.size * 5f * scatterRadius))
              {
                  AddTestSphere(Color.red, legalpos, (int)prefab.size * 5f * radius);
                  areaFree = false;
                  break;
              }
          }
          //If we wmake it through all the spawned objects without intersecting, 
          if (areaFree)
          {
            ///AddTestSphere(Color.green, legalpos, (int)prefab.size * 5f * scatterRadius);
             /// Debug.Log("Succeeded in finding a spot after " + posTryCount + " tries");
             /// return true;
          }

      }

    ///  Debug.Log("Found no open slot after " + legalPosTries + " tries, stopping further population", gameObject);
      return false;
  */
#endregion