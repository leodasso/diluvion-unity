using System.Collections;
using System.Collections.Generic;
using Diluvion.Ships;
using DUI;
using UnityEngine;

namespace Diluvion
{
	public class HeatSink : MonoBehaviour
	{

		public LocTerm coolantAppliedText;
		public float minCoolant = .5f;
		public float maxCoolant = 1;

		public float coolant;

		// Use this for initialization
		void Start()
		{
			coolant = Random.Range(minCoolant, maxCoolant);
		}


		public void ApplyCoolantToPlayer()
		{
			if (coolant < .1f) return;
			if (!PlayerManager.PlayerShip()) return;

			ShipMover playerShipMover = PlayerManager.PlayerShip().GetComponent<ShipMover>();
			float coolantUsed = playerShipMover.ApplyCoolant(coolant);
			coolant -= coolantUsed;
			
			// show GUI
			string guiString = string.Format(coolantAppliedText.LocalizedText(), coolantUsed.ToString("0.0"));
			Notifier.DisplayNotification(guiString, Color.cyan);
		}
	}
}