using System.Collections;
using System.Collections.Generic;
using Diluvion;
using Diluvion.SaveLoad;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Quests
{

	/// <summary>
	/// An objective that requires the player have the given upgrades
	/// </summary>
	[CreateAssetMenu(fileName = "upgrade objective", menuName = "Diluvion/Quests/upgrade objective")]
	public class UpgradeObjective : Objective
	{
		[Tooltip("If true, checks the quantity of upgrades that the player has forged EVER, not the qty that's currently" +
		         " equipped to their ship.")]
		public bool checkQtyOnly;
		[ShowIf("checkQtyOnly"), MinValue(1)]
		public int qty = 2;
		
		[HideIf("checkQtyOnly")]
		public List<Forging> upgradesNeeded = new List<Forging>();
		
		public override void CheckObjective(DQuest forQuest)
		{
			if (!PlayerManager.pBridge)
			{
				Debug.LogError("No player bridge found");
				return;
			}

			if (checkQtyOnly)
			{
				// TODO 
				if (DSave.current == null) return;
				if (DSave.current.forgedUpgrades < qty) return;
				//Debug.Log("Checking quantity of bonus chunks...");
				//if (PlayerManager.pBridge.bonusChunks.Count < qty) return;
				// progress the objective
				ProgressObjective(forQuest);
			}
			else
			{
				//Debug.Log("Comparing player's bonus chunks to upgradesNeeded");
				// If any of the required upgrades are missing, return
				foreach (var upgrade in upgradesNeeded)
				{
					if (!PlayerManager.pBridge.bonusChunks.Contains(upgrade))
					{
						//Debug.Log("Player needs " + upgrade.name + " but doesn't have it.");
						return;
					}
				}
				// progress the objective
				ProgressObjective(forQuest);
			}
		}

		public override GameObject CreateGUI(string overrideObjectiveName)
		{
			throw new System.NotImplementedException();
		}
	}
}