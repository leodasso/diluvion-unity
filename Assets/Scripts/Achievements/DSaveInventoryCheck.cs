using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using Loot;
using NodeCanvas.BehaviourTrees;
using Sirenix.OdinInspector;
using UnityEngine.Networking;


namespace Diluvion.Achievements
{
	
	/// <summary>
	/// Checks inventory for gold check.
	/// </summary>
	[CreateAssetMenu(fileName = "inventoryCheck", menuName = "Diluvion/Achievement/InventoryAchievementCheck")]
	public class DSaveInventoryCheck : DSaveAchievement
	{
		[SerializeField]
		bool checkGold = false;

		
		[SerializeField,HideIf("checkGold")]
		List<DItem> itemComparisons = new List<DItem>();
		
		public override int Progress(DiluvionSaveData dsd)
		{
			base.Progress(dsd);
			
			InventorySave inv = dsd.shipInventory;
			if (checkGold)
				progress = inv.gold;
			else
			{
				List<DItem> itemsFromSave = ItemsGlobal.GetItems(inv.invStrings);
				itemsFromSave = itemsFromSave.Where(x => x != null).ToList();
				//Debug.Log("Got " + itemsFromSave.Count + " items from " + dsd.saveFileName);
				foreach (DItem di in itemsFromSave)
					if (itemComparisons.Contains(di))
						progress++;
			}
			return progress;
		}
	}
}