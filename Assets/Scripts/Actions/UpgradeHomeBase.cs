using System.Collections;
using System.Collections.Generic;
using Diluvion.SaveLoad;
using DUI;
using UnityEngine;

namespace Diluvion
{

	[CreateAssetMenu(menuName = "Diluvion/actions/upgrade homebase")]
	public class UpgradeHomeBase : Action
	{
		public PopupObject homeBaseChoice;
		public PopupObject notEnoughGold;
		public PopupObject successfulUpgrade;
		public List<int> upgradeCosts = new List<int>();
		
		public override bool DoAction(Object o)
		{
			string upgradeCost = CostToUpgrade().ToString();
			
			// Create the popup that gives player a choice of whether to upgrade their homebase
			homeBaseChoice.CreateUI(TryUpgrade, DenyUpgrade, new List<string>{upgradeCost});

			return true;
		}

		int CostToUpgrade()
		{
			if (DSave.current == null)
			{
				Debug.LogError("Can't check upgrade cost because there's no current save file!");
				return 0;
			}
			// Check if there's enough money to upgrade
			return upgradeCosts[DSave.current.homeBaseLevel];
		}

		void DenyUpgrade()
		{
			Debug.Log("Player has denied the upgrade.");
		}

		public void TryUpgrade()
		{
			// Close out dialog window
			UIManager.Clear<DialogBox>();
			
			
			if (DSave.current == null)
			{
				Debug.LogError("Can't perform home base actions because there's no current save file!");
				return;
			}
			
			// Find current homebase level
			int hbLevel = DSave.current.homeBaseLevel;
			
			// If not, display popup 'not enough money'
			if (PlayerManager.PlayerInventory().gold < CostToUpgrade())
			{
				notEnoughGold.CreateUI();
				return;
			}
			
			// if so, take the money and perform the upgrade
			PlayerManager.PlayerInventory().SpendGold(CostToUpgrade());
			hbLevel++;
			DSave.current.homeBaseLevel = hbLevel;
			successfulUpgrade.CreateUI();
		}

		public override string ToString()
		{
			return "Upgrades the home base.";
		}

		protected override void Test()
		{
			Debug.Log(ToString());
			DoAction(null);
		}
	}
}