using System.Collections;
using System.Collections.Generic;
using Diluvion;
using UnityEngine;
using Loot;
using TMPro;
using UnityEngine.UI;

namespace DUI
{

	public class CombatItemButton : MonoBehaviour
	{

		public DItem item;
		public Character myChar;
		public Image itemIcon;

		public TextMeshProUGUI damageText;

		Hazard _hazard;
		HazardReaction _hazardReaction;

		public void Init(Character character, DItem newItem)
		{
			myChar = character;
			item = newItem;

			itemIcon.sprite = item.icon;

			// Get the hazard
			_hazard = BattlePanel.GetHazard();
			if (!_hazard) return;

			// Check if the hazard is vulnerable to my item
			if (_hazard.VulnerableToItem(item, out _hazardReaction))
			{
				damageText.text = _hazardReaction.damage.ToString();
			}
		}

		public void UseItem()
		{
			if (item == null) return;
			if (!myChar) return;

			BattlePanel.NewItemUse(item, myChar);
		}
	}
}