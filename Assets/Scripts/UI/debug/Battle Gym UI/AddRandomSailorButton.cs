using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion;
using UnityEngine.UI;

namespace DUI
{
	public class AddRandomSailorButton : MonoBehaviour
	{
		public Text text;
		public int points = 6;

		void Start()
		{
			if (text)
			{
				text.text = "Add Sailor ({0} stat points)";
				text.text = string.Format(text.text, points);
			}
		}

		public void AddSailor()
		{
			Sailor s = Instantiate(CharactersGlobal.SailorTemplate());
			s.Randomize(points);

			var playerCrew =  PlayerManager.PlayerCrew();
			playerCrew.AddCrewman(s);
		}

		public void RemoveAllSailors()
		{
			foreach (var sailor in PlayerManager.PlayerCrew().AllSailorsOnBoard())
			{
				sailor.Die(false);
			}
		}
	}
}