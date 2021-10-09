using System.Collections;
using System.Collections.Generic;
using Diluvion;
using UnityEngine;

namespace DUI
{

	public class HazardVictoryPanel : DUIPanel
	{

		public LocTerm spoils;
		public ItemExchangePanel itemExchangePanel;

		Inventory _inventory;

		public void Init(Hazard hazard, int level)
		{
			GameManager.Freeze(this);
			_inventory = GetComponent<Inventory>();
			_inventory.invGenerator = hazard.reward;

			int goldAmount = Hazard.RewardAmount(level);
			Debug.Log("Generating hazard reward with " + goldAmount);
			
			_inventory.PopulateItems(goldAmount);

			// show the items that have been added
			itemExchangePanel.InitItemExchange(_inventory.itemStacks, spoils.LocalizedText(), false);
		}

		public void GiveVictoryItems()
		{
			// Give the items to the player
			foreach (var stack in _inventory.itemStacks)
			{
				PlayerManager.PlayerInventory().AddItem(stack, true);
			}
		}

		public void AnimEnd()
		{
			Debug.Log("Called anim end in victory panel");
			End();
		}

		public override void End()
		{
			GiveVictoryItems();
			Debug.Log("Called end in victory panel");
			GameManager.UnFreeze(this);
			base.End();
		}

		void OnDisable()
		{
			GameManager.UnFreeze(this);
		}
	}
}