using System.Collections;
using System.Collections.Generic;
using Diluvion;
using UnityEngine;

namespace DUI
{
	public class LootPanel : DUIView
	{
		public Transform invPanelParent;
		public DUIInventory inventoryPanelPrefab;
		static Inventory _loot;

		DUIInventory _invPanelInstance;

		public static void ShowLoot(Inventory inv)
		{
			_loot = inv;
			var instance = UIManager.Create(UIManager.Get().lootPanel as LootPanel);
			instance.Init();
		}

		public static void Clear()
		{
			UIManager.Clear<LootPanel>();
		}

		void Init()
		{
			_invPanelInstance = Instantiate(inventoryPanelPrefab, invPanelParent);
			_invPanelInstance.Init(_loot);
		}

		protected override void Update()
		{
			base.Update();
			
			if (_loot)
				transform.position = FollowTransform(_loot.transform.position, 100, InteriorView.InteriorCam());
		}

		/// <summary>
		/// Opens up a full exchange between the given inventory and the player
		/// </summary>
		public void FullExchange()
		{
			Inventory.ShowTradePanel(_loot);
			End();
		}

		public void TakeAll()
		{
			if (!_loot) return;
			if (!PlayerManager.PlayerInventory()) return;
			
			//Transfer each stack from this inventory to the other
			foreach (StackedItem stackedItem in _loot.AllItems())
			{
				//If the transfer couldn't be completed, break the loop. 
				if (_loot.Transaction(PlayerManager.PlayerInventory(), stackedItem, false, 1) == false)
				{
					Debug.Log("Breaking inventory transfer loop");
					break;
				}
			}

			SpiderWeb.SpiderSound.MakeSound("Play_Stinger_Take_All", gameObject);

			//After transaction complete, update the inventory cells
			_invPanelInstance.Refresh();
			InventoryClicker.RefreshSprites();
			DelayedEnd(.5f);
		}
	}
}