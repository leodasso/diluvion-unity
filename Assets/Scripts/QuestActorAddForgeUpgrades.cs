using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion
{
	/// <summary>
	/// Can add upgrades to the attached forger based on quest status.
	/// </summary>
	[RequireComponent(typeof(Forger))]
	[AddComponentMenu("DQuest/Quest actor add forge upgrades")]
	public class QuestActorAddForgeUpgrades : QuestActor
	{
		public List<Forging> upgradesToAdd;

		Forger forger;

		// Use this for initialization
		void Start()
		{
			forger = GetComponent<Forger>();
		}

		// When the quest activates, 
		protected override void OnActivate()
		{
			base.OnActivate();

			if (!forger) forger = GetComponent<Forger>();
			if (!forger)
			{
				Debug.LogError("Can't add upgrades to forger, because no forge has been defined!", gameObject);
				return;
			}

			// Ensure forger info won't be null before checking
			forger.SetDefaultForgerInfo();
			
			foreach (var VARIABLE in upgradesToAdd)
			{
				if (forger.forgerInfo.possibleUpgrades.Contains(VARIABLE)) continue;
				forger.forgerInfo.possibleUpgrades.Add(VARIABLE);
			}
		}
	}
}