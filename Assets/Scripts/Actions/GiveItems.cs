using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DUI;

namespace Diluvion
{

	[CreateAssetMenu(fileName = "give items action", menuName = "Diluvion/actions/give items")]
	public class GiveItems : Action
	{

		public bool useUpgradeChunk;
		[ShowIf("useUpgradeChunk")]
		public Forging upgrade;
		
		[HideIf("useUpgradeChunk")]
		public List<StackedItem> itemsToGive;

		[Tooltip("If the palyer doesnt have room, place the extra items in the storage inventory.")]
		public bool overflowToStorage = true;
		
		public bool useDefaultTitle = true;
		[Tooltip("The title to display when taking items."), HideIf("useDefaultTitle")]
		public string title;

		List<StackedItem> ItemsToGive()
		{
			if (useUpgradeChunk)
			{
				if (!upgrade)
				{
					Debug.LogError(name + " uses items from an upgrade chunk, but no chunk is set!", this);
					return null;
				}
				return upgrade.synthesisMaterials;
			}
			return itemsToGive;
		}
		
		public override bool DoAction(Object o)
		{
			// Open the panel to show that items have been given
			string newTitle = title;
			// TODO LOC
			if (useDefaultTitle) newTitle = "Received items";
			var panel = ItemExchangePanel.ShowItemExchange(ItemsToGive(), newTitle, false);

			panel.onEnd += GiveActualItems;
			return true;
		}

		void GiveActualItems()
		{
			PlayerManager.GivePlayerItems(ItemsToGive(), overflowToStorage);
			/*
			// Find the player inventory
			Inventory inv = PlayerManager.PlayerInventory();
			if (inv == null)
			{
				Debug.LogError("Player inventory couldn't be found! Can't take items.", this);
				return;
			}
			
			// give the items
			foreach (StackedItem items in ItemsToGive())
			{
				inv.AddItem(items, overflowToStorage);
			}\
			*/
		}

		protected override void Test()
		{
			if (PlayerManager.PlayerInventory() == null)
			{
				Debug.LogWarning("Can't test while there's no player inventory!");
				return;
			}
			DoAction(null);
		}

		public override string ToString ()
		{
			string s =  "On invoke, gives ";
			foreach (StackedItem i in ItemsToGive())
			{
				s += i.item.LocalizedName() + " X " + i.qty + ", ";
			}

			s += "to player.";
			return s;
		}
	}
}