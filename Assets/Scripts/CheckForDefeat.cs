using UnityEngine;
using System.Collections.Generic;
using Diluvion;

/// <summary>
/// Checks a given list of hulls to see if they're defeated. 
/// Can also check if a given percentage have been defeated.
///  Will set the given quest to a given status when the conditions are met.
/// </summary>
public class CheckForDefeat : MonoBehaviour {
	
	public List<Hull> hullsToDefeat;
	[Range(0, 1)]	public float percentageToDefeat = 1;
	public string questLocKey;
	public QuestAction questAction;

	int deadHulls = 0;
	bool triggered = false;

	// Use this for initialization
	void Start () {
		foreach (Hull h in hullsToDefeat) h.myDeath += OnDeath;
	}

	// This gets called whenever one of the hulls in my list dies
	void OnDeath(Hull hullThatDied, string byWho) {

		if (triggered) return;

		deadHulls ++;
		float percentage = (float)deadHulls / (float)hullsToDefeat.Count;

		if (percentage >= percentageToDefeat) OnConditionMet();

	}

	void OnConditionMet() {

		triggered = true;

        // TODO
		// call quest manager to end / start quest
		//if (questAction == QuestAction.completeQuest) QuestManager.Get().CompleteQuest(questLocKey);

		//if (questAction == QuestAction.startQuest) QuestManager.Get().AddQuest(questLocKey);
	}
}
