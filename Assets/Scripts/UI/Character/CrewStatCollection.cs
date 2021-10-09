using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Diluvion;
using Diluvion.Ships;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DUI
{

    /// <summary>
    /// DUI element for displaying a list of crew stats. Use in conjunction with a layout group.
    /// </summary>
    public class CrewStatCollection : MonoBehaviour
    {
        [ReadOnly]
        public Character myChar;
        public DUICharacterStat crewStatPrefab;
        
        List<ShipModifier> _mods = new List<ShipModifier>();

            //ShipModifier mod;
        List<CrewStatValue> myCrewStats;

        bool _displayingStationTotals;

        public void StatCollectionInit(List<CrewStatValue> statsCollection, Character ch = null, List<ShipModifier> shipMods = null, bool forStationTotals = false)
        {
            _displayingStationTotals = forStationTotals;
            myChar = ch;
            myCrewStats = statsCollection;
            _mods = shipMods;
            Refresh();
        }

        public void Refresh()
        {
            Clear();

            // Show character's stats
            foreach (CrewStatValue csv in myCrewStats)
            {
                bool usesStat = false;

                // If there's a station stat for this...
                if (_mods != null)
                {
                    foreach (var m in _mods)
                    {
                        if (m.UsesStat(csv)) usesStat = true;
                    }
                }

                // make a new DUI stats instance
                DUICharacterStat newStat = Instantiate(crewStatPrefab, transform);
                //newStat.transform.SetParent(transform, false);
                newStat.Init(csv, myChar, usesStat, displayStationUsage:_displayingStationTotals);
            }
        }

        public void Clear()
        {
            SpiderWeb.GO.DestroyChildren(transform);
        }

#if UNITY_EDITOR 
        [ButtonGroup]
        void AddPrefab()
        {
            if (!crewStatPrefab) return;
            DUICharacterStat newObj = PrefabUtility.InstantiatePrefab(crewStatPrefab) as DUICharacterStat;
            newObj.transform.SetParent(GetComponent<Transform>(), false);
        }

        /// <summary>
        /// Clear the stats grid.
        /// </summary>
        [ButtonGroup]
        public void EditorClear()
        {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform t in transform) children.Add(t.gameObject);

            children.ForEach(DestroyImmediate);
        }
#endif
    }

}