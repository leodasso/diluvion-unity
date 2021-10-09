using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StationStats
{
    public StationStatsCollection statObjectReference;
    public List<StationStat> localStats = new List<StationStat>();

    public StationStats(){ }

    public void InitStats()
    {
        if (statObjectReference == null) return;
        localStats = new List<StationStat>(statObjectReference.stationStats);
    }

    public void ResetStats()
    {
        foreach (StationStat ss in localStats)
            ss.Reset();
    }

    public void UpdateLocalStats(CrewStats data)
    {
        if (statObjectReference == null) { Debug.LogError("StatObjectReference Missing"); return; }
        if (data == null) { Debug.LogError("Crew Stat Data missing"); return; }
        localStats = statObjectReference.GetStationStatFromCrew(data);
    }


    /// <summary>
    /// Returns the value of the given stat based on the crew currently operating this station.
    /// </summary>
    public float GetBonus(string statKeypath)
    {
        StationStat linkedStat = null;

        if (localStats == null) return 0;
        if (localStats.Count < 1) return 0;

        // Get the crew stat related to this stat name
        foreach (StationStat stationStat in localStats)
        {
            if (stationStat.baseStat.statName == statKeypath)
            {
                linkedStat = stationStat;
                break;
            }
        }

        // Check if the given stat was found
        if (linkedStat == null) return 0;
        else return linkedStat.totalBonus;
    }

    //Gets the sum of the bonus on this co
    public bool UsesStat(CrewStatObject cso)
    {
        foreach (StationStat st in localStats)
        {
            if (st.GetStatBonus(cso) > 0)
                return true;
        }
        return false;
    }
}