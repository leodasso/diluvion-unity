using System.Collections;
using System.Collections.Generic;
using Diluvion;
using UnityEngine;

[CreateAssetMenu(fileName = "weaken character", menuName = "Diluvion/actions/weaken char")]
public class WeakenCharacter : HazardAction
{

	[Tooltip("The hazard will add the following stats to the character. To weaken, you will want these values to be negative.")]
	public List<CrewStatValue> attackValues = new List<CrewStatValue>();


	public override bool DoAction(Object o)
	{
		Sailor s = Hazard.sailorToAttack;
		if (!s)
		{
			Debug.LogError("Attempting to " + ToString() + " but no sailor has been defined to attack by hazard.");
			return false;
		}
		
		s.ChangeTemporaryStats(attackValues);
		return true;
	}

	public override string ToString()
	{
		string s = "temporarily drops a sailors stats by ";

		foreach (var variable in attackValues)
		{
			s += variable + " ";
		}

		return s;
	}
}
