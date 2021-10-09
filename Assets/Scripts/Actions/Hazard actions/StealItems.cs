using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DUI;
using Loot;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Diluvion
{
	
	[CreateAssetMenu(fileName = "item steal", menuName = "Diluvion/actions/steal items")]
	public class StealItems : HazardAction 
	{
		[OnValueChanged("Refresh")]
		public int min = 10;

		[OnValueChanged("Refresh")]
		public int max = 50;

		[MinMaxSlider(0, 9999), ReadOnly]
		public Vector2 range = new Vector2(10, 50);

		public override bool DoAction(Object o)
		{
			Inventory inv = PlayerManager.PlayerInventory();
			if (!inv) return false;

			int valueOfTheft = Mathf.RoundToInt(Random.Range(range.x, range.y));

			// order the items by their gold value.
			List<StackedItem> orderedItems = inv.itemStacks.OrderBy(x => x.item.goldValue).ToList();
			// inverse so that highest value items come first in the list
			orderedItems.Reverse();
			
			// Cycle through items until finding one that I can steal.
			for (int i = 0; i < orderedItems.Count; i++)
			{
				DItem item = orderedItems[i].item;

				if (!item) continue;
				if (!item.IsStealable()) continue;
				if (item.goldValue > valueOfTheft) continue;

				inv.RemoveItem(item);
				// TODO loc
				BattleLog stealLog = new BattleLog(item.LocalizedName() + " was stolen!");
				BattlePanel.Log(stealLog);
				return true;
			}
			
			return false;
		}


		void Refresh()
		{
			min = Mathf.Clamp(min, 1, 9999);
			max = Mathf.Clamp(max, min, 9999);
			range = new Vector2(min, max);
		}

		public override string ToString()
		{
			return "Steals item(s) with a total value between " + min + " and " + max;
		}
	}
}
