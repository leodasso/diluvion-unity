using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BlendModes
{
	public class DemoBlendModePicker : MonoBehaviour
	{
		public static BlendMode SelectedBlendMode = BlendMode.ColorDodge;

		private void Start ()
		{
			var buttons = new List<Button>(26);
			foreach (Transform element in transform)
				if (element.GetComponent<Button>()) 
					buttons.Add(element.GetComponent<Button>());

			for (int i = 0; i < 26; i++)
			{
				int ic = i;

				buttons[ic].GetComponentInChildren<Text>().text = Regex.Replace(((BlendMode)ic).ToString(), "(\\B[A-Z])", " $1");
				if (SelectedBlendMode == (BlendMode)ic)
					buttons[ic].GetComponent<Image>().color = Color.green;

				buttons[ic].onClick.RemoveAllListeners();
				buttons[ic].onClick.AddListener(() => {
					SelectedBlendMode = (BlendMode)ic;
					foreach (var button in buttons) 
						button.GetComponent<Image>().color = button == buttons[ic] ? Color.green : Color.white;
				});
			}
		}
	}
}
