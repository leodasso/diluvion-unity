using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DUI
{

	/// <summary>
	/// A panel that displays info for an individual forge upgrade. Can do functions on the upgrade when clicked.
	/// </summary>
	public class ForgeItemPanel : MonoBehaviour
	{
		[InlineButton("Preview"), AssetsOnly]
		public Forging upgrade;
		public TextMeshProUGUI titleText;
		public TextMeshProUGUI descrText;
		public Image previewImage;

		[Space] public ForgeConfirmPanel confirmPanelPrefab;
		
		Func<Forging, bool> myConfirmMethod;

		void Preview()
		{
			if (!upgrade) return;
			ApplyChunk(upgrade, null);
		}

		public void ApplyChunk(Forging chunk)
		{
			upgrade = chunk;
			titleText.text = upgrade.LocalizedName();
			descrText.text = upgrade.LocalizedBody();
			previewImage.sprite = upgrade.previewSprite;

		}

		public void ApplyChunk(Forging chunk, Func<Forging, bool> confirmMethod)
		{
			myConfirmMethod = confirmMethod;
			ApplyChunk(chunk);
		}

		public void OnSelected()
		{
			var instance = UIManager.Create(confirmPanelPrefab);
			instance.InitForgePanel(upgrade, myConfirmMethod);
		}
	}
}