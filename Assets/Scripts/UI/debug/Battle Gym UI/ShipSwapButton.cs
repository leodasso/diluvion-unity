using System.Collections;
using System.Collections.Generic;
using Diluvion;
using Diluvion.Ships;
using UnityEngine;
using UnityEngine.UI;

namespace DUI
{

	[RequireComponent(typeof(Button))]
	public class ShipSwapButton : MonoBehaviour
	{

		public Text nameText;
		public SubChassis subChassis;
		
		// Use this for initialization
		void Start()
		{
			nameText.text = subChassis.name;
		}

		public void SwitchPlayerShip()
		{
			if (PlayerManager.PlayerShip() == null)
			{
				Debug.Log("Can't switch player ship because no ship was found.");
				return;
			}
			PlayerManager.SwapPlayerShip(subChassis);
		}
	}
}