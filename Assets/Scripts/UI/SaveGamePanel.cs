using System.Collections;
using System.Collections.Generic;
using Diluvion.SaveLoad;
using TMPro;
using UnityEngine;

namespace DUI
{

	public class SaveGamePanel : DUIView
	{
		public TMP_InputField inputText;

		protected override void Start()
		{
			base.Start();
			inputText.text = DSave.currentSaveName + " copy";
		}

		public void SaveAs()
		{
			DSave.Save(inputText.text);
			DSave.currentSaveName = inputText.text;
			group.interactable = false;
			DelayedEnd(1);
		}

		public override void End()
		{
			PauseMenu.Reset();
			base.End();
		}
	}
}