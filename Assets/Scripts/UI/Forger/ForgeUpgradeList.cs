using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpiderWeb;

namespace DUI
{

	/// <summary>
	/// A GUI panel that displays a list of upgrades that can be applied to ships.
	/// </summary>
	public class ForgeUpgradeList : MonoBehaviour
	{
		public RectTransform layoutPanel;
		public ForgeItemPanel forgeItemPanelPrefab;

		/// <summary>
		/// Initializes the upgrade list. Call this once it's instantiated.
		/// </summary>
		/// <param name="chunks">The list of upgrade chunks to display.</param>
		/// <param name="confirmPanel">The confirm panel prefab to display once an upgrade is selected.</param>
		/// <param name="OnConfirmed">Function that happens when the player confirms their selection.</param>
		public void InitUpgradeList(List<Forging> chunks, ForgeConfirmPanel confirmPanel, Func<Forging, bool> confirmMethod)
		{
			// clear previous children from the layout
			GO.DestroyChildren(layoutPanel);

			// create panels for each upgrade chunk
			foreach (Forging chunk in chunks)
			{
				var newPanel = Instantiate(forgeItemPanelPrefab, layoutPanel);
				newPanel.ApplyChunk(chunk, confirmMethod);
				newPanel.confirmPanelPrefab = confirmPanel;
			}
		}
	}
}