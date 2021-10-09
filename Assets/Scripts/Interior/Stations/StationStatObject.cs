using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
[CreateAssetMenu(fileName = "New Station Stat Type", menuName = "Diluvion/Station Stat")]
public class StationStatObject: StatObject // Accuracy, FireRate ETC
{
    public List<CrewStationValues> affectedBy;

    /// <summary>
    /// Returns a list of summed weapon Stat for each relevant crewStat;
    /// </summary>
    public List<CrewStationValues> GetSummedStationStat(CrewStats cs)
    {
        List<CrewStationValues> returnValues = new List<CrewStationValues>();

        //For each type of stat we are affected by
        foreach (CrewStationValues ssv in affectedBy)
        {
            CrewStationValues crewStationValue = new CrewStationValues(ssv);//Create a copy of the crewStationValues with the summed stats
            float statTotal = 0; //store all the stats of type           
            foreach (CrewStatValue csv in cs.stats)//check the stats that are equal to this station stat multiplier value
            {            
                if (csv.statBase.IsEqualTo(ssv.statBase))// if it is indeed the stat we are looking for
                {                   
                    statTotal += csv.value;//add it to the total
                  //DEBUG  comparingStats += csv.statBase.statName +" = " +csv.value +" and " + ssv.statBase.statName +" ="+ ssv.multiplier + "= " +statTotal +"\n";
                }
            }            
            crewStationValue.SumStats(statTotal);
            returnValues.Add(crewStationValue); //Add it to the returnList;
        }
        //DEBUG Debug.LogError(comparingStats);
        return returnValues;
    }
}

[System.Serializable]
///Class that holds the stat bonus and stat values for a station
public class CrewStationValues //TODO REFACTOR crewStationValue and CrewValues should derive from same class
{
    public StatObject statBase;     //the stat this station is using
    [Range(0, 2)]
    public float multiplier;        //start multiplier for this station
    [ReadOnly]
    public float totalStats;        //total amount of this stat affecting this station
    [ReadOnly]
    public float totalStatBonus;    //total bonus for this station


    // copy constructor
    public CrewStationValues (CrewStationValues incomingStat)
    {
        statBase = incomingStat.statBase;
        multiplier = incomingStat.multiplier;
        totalStats = incomingStat.totalStats;
        totalStatBonus = incomingStat.totalStatBonus;
    }

    public void SumStats(float incomingStat)
    {
        totalStats =  incomingStat;
        SumBonus();
    }

    /// <summary>
    /// Recalculates and sets the totalSum;
    /// </summary>
    public float SumBonus()
    {
        return totalStatBonus = totalStats * multiplier;
    }
}