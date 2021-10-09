using System.Collections;
using System.Collections.Generic;
using Diluvion;
using UnityEngine.UI;
using TMPro;
using System;

namespace DUI
{

	public class ItemExchangePanel : DUIView
	{
		public DUIItemRequiredPanel itemPanelPrefab;
		public TextMeshProUGUI titleText;
		public GridLayoutGroup gridLayout;

		public event System.Action onEnd;

		protected override void Start()
		{
			base.Start();
			GameManager.Freeze(this);
		}

		/// <summary>
		/// Opens the item exchange panel.
		/// </summary>
		/// <param name="items">Which items should be displayed?</param>
		/// <param name="title">The title which appears at the top of the panel.</param>
		/// <param name="showAsFraction">Show the items as [player qty / item qty]?</param>
		public static ItemExchangePanel ShowItemExchange(List<StackedItem> items, string title, bool showAsFraction)
		{
			var instance = UIManager.Create(UIManager.Get().itemExchangePanel as ItemExchangePanel);
			instance.InitItemExchange(items, title, showAsFraction);
			return instance;
		}

		
		public void InitItemExchange(List<StackedItem> items, string title, bool showAsFraction)
		{
			SpiderWeb.GO.DestroyChildren(gridLayout.transform);
			titleText.text = title;
			
			foreach (var stack in items)
			{
				var newPanel = Instantiate(itemPanelPrefab, gridLayout.transform);
				newPanel.Init(stack, showAsFraction);
			}
		}

		public override void End()
		{
			onEnd?.Invoke();
			GameManager.UnFreeze(this);
			base.End(0);
		}

		void OnDestroy()
		{
			if (GameManager.Exists())
				GameManager.UnFreeze(this);
		}
	}
}