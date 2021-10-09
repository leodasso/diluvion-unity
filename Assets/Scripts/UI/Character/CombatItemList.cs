using System.Collections;
using System.Collections.Generic;
using Diluvion;
using Loot;
using UnityEngine;

namespace DUI
{
	public class CombatItemList : MonoBehaviour
	{

		public CombatItemButton itemButtonPrefab;
		Hazard _hazard;

		public void Init(Character character)
		{
			return;
			
			_hazard = BattlePanel.GetHazard();
			if (!_hazard) return;

			foreach (DItem i in _hazard.GetItemUsages())
			{
				CombatItemButton newButton = Instantiate(itemButtonPrefab, transform);
				newButton.Init(character, i);
			}
		}
	}
}