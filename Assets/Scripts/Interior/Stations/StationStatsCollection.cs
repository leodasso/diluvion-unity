
﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
[CreateAssetMenu(fileName = "New Station Stat Collection", menuName = "Diluvion/Station Stat Collection")]
///Data ScriptableObject for the proper stat/station references
public class StationStatsCollection : ScriptableObject {

    public HeavyDutyInspector.Keyword stationName; // weapons/sonar etc
    public List<StationStat> stationStats; // list of object stat types

    //Updates our stationStat types based on what kind of crew members we have
    public List<StationStat> GetStationStatFromCrew(CrewStats data)
    {
        List<StationStat> returnStats = new List<StationStat>();
        StationStat newstat;
        foreach (StationStat st in stationStats)
        {
            newstat = new StationStat(st);
            newstat.UpdateStationStats(data);
            returnStats.Add(newstat);
        }
        return returnStats;
    }  
}

//Data class for the station stats
[System.Serializable]
public class StationStat
{
        public StationStatObject baseStat; //reference to the UNIQUE station stat scriptableobject
        [HideInInspector]
        public List<CrewStationValues> statValues; ///how much each crew stat affects this statoin stat
        [Range(-1,2)]
        public float totalBonus = 0; //the sum of all statvalues, or the total station stat bonus


        public StationStat ()
        {

        }

    
        public CrewStats BonusValueCrewStats()
        {
            CrewStats baseList = new CrewStats();
            baseList.SetDefaults();
            foreach (CrewStatValue csv in baseList.stats)
            {
                csv.value = 0;
                csv.Add(GetStatBonus(csv.statBase));
            }

            return baseList;
        }

    public void Reset()
    {
        foreach (CrewStationValues csv in statValues)
            csv.SumStats(0);
        SumStationBonuses();
    }
    /// <summary>
    /// Returns true if this stat collection uses the skill from the given crew stat value csv.
    /// </summary>
    public bool UsesCrewStat(CrewStatValue csv)
    {
        if ( baseStat == null ) return false;

        foreach (CrewStationValues stationValue in baseStat.affectedBy)
        {
            if ( stationValue.statBase == csv.statBase ) return true;
        }

        return false;
    }

    /// <summary>
    /// Station Stat Copy constructor
    /// </summary>
    /// <param name="stat"></param>
    public StationStat(StationStat stat)
    {
        baseStat = stat.baseStat;
        statValues = new List<CrewStationValues>(stat.statValues);
        totalBonus = SumStationBonuses();
    }

    public bool IsStat(string statName)
    {
        if (baseStat == null) return false;
        if (baseStat.statName != statName) return false;
        return true;
    }

    /// <summary>
    /// Gets the bonus contribution for a certain crewStat name on this stationStat, returns 0 if not applicable
    /// </summary>
    public float GetStatBonus(string crewStatName)
    {
        foreach (CrewStationValues csv in statValues)
            if (csv.statBase.statName == crewStatName)
                return csv.totalStatBonus;

        return 0;
    }


    /// <summary>
    /// Gets the bonus contribution for a certain crewStat on this stationStat, returns 0 if not applicable
    /// </summary>
    public float GetStatBonus(CrewStatObject crewStat)
    {
        foreach (CrewStationValues csv in statValues)
            if (csv.statBase == crewStat)
                return csv.totalStatBonus;

        return 0;
    }

    /// <summary>
	/// Updates the station stats based on the input crewStats
    /// </summary>
    public void UpdateStationStats(CrewStats theCrewStats)
    {
        statValues = GetValuesForStationStat(theCrewStats);
        totalBonus = SumStationBonuses();
        //DEBUG Debug.Log(baseStat.statName + " has a total bonus of " + totalBonus);
    }



    /// <summary>
    /// Processes the total values of stats and bonuses through the base stat reference, using the list of crew
    /// </summary>
    public List<CrewStationValues> GetValuesForStationStat(CrewStats theCrewStats)
    {
        return baseStat.GetSummedStationStat(theCrewStats);
    }


    /// <summary>
    /// Gets the summed station bonuses from its crew;
    /// </summary>
    public float SumStationBonuses()
    {
        float returnfloat = 0;
        foreach (CrewStationValues v in statValues)
            returnfloat += v.totalStatBonus;
        return returnfloat;

    }
}
