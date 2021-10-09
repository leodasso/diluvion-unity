
 using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Diluvion;

[System.Serializable]
[CreateAssetMenu(fileName = "New Crew Stats List", menuName = "Diluvion/Characters/Crew Stats List")]
public class CrewStatsCollection : ScriptableObject {

    public CrewStats crewStats;
	
	void InspectorRandomize() 
	{
		crewStats.Randomize(10, -1);
	}

}

[System.Serializable]
public class CrewStats {

    // The default stats prefab from resources
    public static CrewStatsCollection statsPrefab;

    public List<CrewStatValue> stats = new List<CrewStatValue>();

    /// <summary>
    /// Returns a crewstats that's the sum of these two 
    /// </summary>
    public static CrewStats Sum(CrewStats cs1, CrewStats cs2)
    {
        CrewStats sumStats = new CrewStats(cs1);

        // For each stats in the second guy
        foreach (CrewStatValue csv in cs2.stats)
        {
            sumStats.AddStat(csv); 
        }

        return sumStats;
    }



    public static CrewStats SumList(List<CrewStats> crewStatsList)
    {
        if (crewStatsList.Count < 1) return null;

        CrewStats returnStats = new CrewStats();

        foreach (CrewStats cs in crewStatsList)
        {
            returnStats = CrewStats.Sum(returnStats, cs);
        }

        return returnStats;
    }

    /// <summary>
    /// Adds the new crew stat value to this crew stat
    /// </summary>
    public void AddStat(CrewStatValue csv)
    {
          foreach (CrewStatValue statValue in stats)
        {
            if (statValue.Equals(csv))
            {
                statValue.Add(csv.value);
                return;
            }
        }

        stats.Add(csv);
     }



	public CrewStats()
    {
        stats = new List<CrewStatValue>();
    }

    /// <summary>
    /// Constructs a copy of the Crewstats object with summed stats from the input crew
    /// </summary>
    /// <param name="crewToSum"></param>
    public CrewStats(List<Sailor> crewToSum)
    {
        // set default values to start with
        SetDefaults(0);
 
        if (crewToSum == null) { /*Debug.Log("Null Crew to Sum.");*/ stats = new List<CrewStatValue>(); return; }
        if (crewToSum.Count < 1) { /*Debug.Log("Less than 1 Crew to Sum.");*/ stats = new List<CrewStatValue>(); return; }

        stats = SumCrew(crewToSum);
    }


    public CrewStats(CrewStats original) {
		stats = new List<CrewStatValue>();
        foreach (CrewStatValue csv in original.stats)
            stats.Add(new CrewStatValue(csv));
	}

    public float GetHighestStatValue()
    {
        float highestValue = -1;
        foreach (CrewStatValue statValue in stats)
            if (statValue.value > highestValue)
                highestValue = statValue.value;
        return highestValue;
    }

	public float GetStatValue(CrewStatObject forStat) {

		foreach (CrewStatValue statValue in stats) 
			if (statValue.statBase == forStat) return statValue.value;
	
		return 0;
	}
    /// <summary>
    /// Gets a list of crew and adds all their stat to this crewStats
    /// </summary>
    public List<CrewStatValue> SumCrew(List<Sailor> crew)
    {
        // Get a list of all the crew stats from the given crew
        List<CrewStats> allCrewStats = new List<CrewStats>();
        foreach (Sailor sailor in crew) allCrewStats.Add(sailor.GetSailorStats());

        return SumList(allCrewStats).stats;
    }

    float GetStatValue(CrewStatObject statType,  CrewStats data)
    {
        float sumFloat = 0;
        foreach(CrewStatValue csv in data.stats)
        {
            if (csv.statBase == statType)
                sumFloat += csv.value;
        }
        return sumFloat;
    }


	/// <summary>
	/// Applies a list of floats to create a list of stat values from a save
	/// </summary>
	public void ApplyStatValues(List<float> statValues) 
	{
		for (int i = 0; i < statValues.Count; i++) {

			if (i >= stats.Count) {
				Debug.LogError("Trying to apply a value to crew stat at index " + i + " which doesn't exist!");
				return;
			}

			stats[i].value = statValues[i];
		}
	}

	/// <summary>
	/// Gives this instance the same default list of stats that the 'default crew stats' in resources has.
	/// </summary>
	public void SetDefaults(float newValue = 1) {

        if (statsPrefab == null)
            statsPrefab = Resources.Load<CrewStatsCollection>("defaultCrewStats");

        stats = new List<CrewStatValue>();
        foreach (CrewStatValue csv in statsPrefab.crewStats.stats)
            stats.Add(new CrewStatValue(csv));

        foreach ( CrewStatValue csv in stats )
            csv.value = newValue;
	}


	/// <summary>
	/// Randomizes the crew stats
	/// </summary>
	/// <param name="totalPoints">Total points to be allocated across all stats.</param>
	/// <param name="startingValue">Starting base value of stats.</param>
	public void Randomize (int totalPoints, int startingValue) 
	{
		foreach (CrewStatValue stat in stats)
			stat.value = startingValue;

		int statsCount = stats.Count;

		// randomly give out points
		for (int i = 0; i < totalPoints; i++) {
			int rand = Random.Range(0, statsCount);
			stats[rand].value ++;
		}
	}

	/// <summary>
	/// Returns a list of floats for use in save file.
	/// </summary>
	public List<float> GetStatValues() 
	{
		List<float> statValues = new List<float>();
		foreach (CrewStatValue stat in stats) {
			statValues.Add(stat.value);
		}

		return statValues;
	}
}


[System.Serializable]
public class CrewStatValue {

    public CrewStatObject statBase;
    public float value = 0;

    public CrewStatValue() { }

    public CrewStatValue(CrewStatValue newValue)
    {
        statBase = newValue.statBase;
        value = newValue.value;
    }

    public CrewStatValue(CrewStationValues newValue)
    {
        statBase = (CrewStatObject)newValue.statBase;
        value = newValue.totalStatBonus;

    }

	public override string ToString()
	{
		if (statBase == null) return "null";
		return statBase.statName + ": " + value;
	}

	public void Add(float v)
    {
        value += v;
    }

    public override bool Equals(object obj)
    {
        CrewStatValue comparison = obj as CrewStatValue;
        if (comparison == null) return false;

        if (comparison.statBase == statBase) return true;
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public enum SkillType {

	None,
	Bolt,
	Sonar,
	Repair,
	Navigating,
	Helm,
	XO,
	Torpedo,
	Cook,
	Blacksmith
}

public enum Gender {
	Male ,
	Female
}

public enum VoiceType
{
    Energetic,
    Naive,
    Somber,
    Old,
    Sassy
}