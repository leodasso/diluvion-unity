using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace DUI
{

	public class Notifier : DUIPanel
	{

		public TextMeshProUGUI text;

		float _timer;
		float _time;

		/// <summary>
		/// Display a UI notification on screen with the given text.
		/// </summary>
		public static void DisplayNotification(string newText, Color textColor)
		{
			Notifier instance = UIManager.Create(UIManager.Get().notifier as Notifier);
			instance.text.text = newText;
			instance.text.color = textColor;
		}
	}
}