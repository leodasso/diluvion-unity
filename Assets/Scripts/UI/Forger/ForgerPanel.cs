using System.Collections.Generic;
using System.Linq;
using Diluvion;
using Diluvion.SaveLoad;
using Diluvion.Ships;
using UnityEngine;
using Sirenix.OdinInspector;
using SpiderWeb;

namespace DUI
{
	public class ForgerPanel : DUIPanel
	{

		public DUIPanel mainMenu;
		public DUIPanel synthMenu;
		public DUIPanel dismantleMenu;

		[Space]
		public LocTerm forgeSuccess;
		public PopupObject noSlotsLeft;
		public PopupObject dismantleSuccessful;

		[Space]
		public ForgeConfirmPanel synthConfirmPanel;
		public ForgeConfirmPanel dismantleConfirmPanel;

		[ReadOnly]
		public Forger forger;
		
		[Space]
		public RectTransform layoutPanel;
		public ForgeItemPanel forgeItemPanelPrefab;

		public static void Refresh()
		{
			ForgerPanel instance = UIManager.GetPanel<ForgerPanel>();
			if (!instance) return;
			instance.RefreshInstance();
		}

		protected override void Start()
		{
			base.Start();
			MainMenu();
		}

		public void InitForgePanel(Forger theForger)
		{
			forger = theForger;
		}

		void DisableAllMenus()
		{
			mainMenu.SetInteractive(false);
			synthMenu.SetInteractive(false);
			dismantleMenu.SetInteractive(false);
		}

		/// <summary>
		/// Displays the main forger menu
		/// </summary>
		public void MainMenu()
		{
			DisableAllMenus();
			mainMenu.SetInteractive(true);
		}

		/// <summary>
		/// Displays the menu for synthesizing new bonus chunks
		/// </summary>
		public void SynthMenu()
		{
			if (RefreshSynth())
			{
				DisableAllMenus();
				synthMenu.SetInteractive(true);
			}
		}

		/// <summary>
		/// Displays the menu for dismantling bonus chunks from the player ship.
		/// </summary>
		public void DismantleMenu()
		{
			RefreshDismantle();
			DisableAllMenus();
			dismantleMenu.SetInteractive(true);
		}

		bool RefreshSynth()
		{
			if (!forger)
			{
				Debug.LogError("No forger is set!.");
				return false;
			}
			
			synthMenu.GetComponent<ForgeUpgradeList>().InitUpgradeList(forger.forgerInfo.possibleUpgrades, synthConfirmPanel, ForgeSelection);
			return true;
		}

		bool RefreshDismantle()
		{
			Bridge player = PlayerManager.pBridge;
			if (!player)
			{
				Debug.LogError("No player ship exists, displaying dismantle menu in debug mode.");
				return false;
			}
			
			// Create a list of upgrades which can be dismantled
			List<Forging> dismantleableChunks = new List<Forging>();
			dismantleableChunks.AddRange(player.bonusChunks);
			dismantleableChunks = dismantleableChunks.Where(x => x.canDismantle).ToList();

			dismantleMenu.GetComponent<ForgeUpgradeList>()
					.InitUpgradeList(dismantleableChunks, dismantleConfirmPanel, DismantleSelection);

			return true;
		}

		void RefreshInstance()
		{
			RefreshSynth();
			RefreshDismantle();
		}


		bool DismantleSelection(Forging chunk)
		{
			Bridge b = PlayerManager.pBridge;
			if (!b)
			{
				Debug.LogError("No player ship could be found!");
				return false;
			}

			if (!chunk.CanDismantle()) return false;

			// remove the chunk from the ship
			chunk.RemoveFromShip(b, b.chassis);
			
			// apply to save file
			if (DSave.current != null)
			{
				SubChassisData shipData = DSave.current.SavedPlayerShip();
				shipData.appliedSlots.Remove(chunk.name);
				Debug.Log("Removed " + chunk.name + " from " + shipData.chassisName + " in the save file");
			}

			// place items in player storage
			foreach (StackedItem salvagedItem in chunk.DismantleMaterials())
			{
				PlayerManager.PlayerStorage().AddItem(salvagedItem);
			}
			
			// Show the popup for a successful dismantle!
			List<string> popupStrings = new List<string>();
			popupStrings.Add(chunk.LocalizedName());
			dismantleSuccessful.CreateUI(popupStrings);
			
			SpiderSound.MakeSound("Play_Dismantle_Upgrade", gameObject);

			return true;
		}

		bool ForgeSelection(Forging chunk)
		{
			Bridge b = PlayerManager.pBridge;
			if (!b)
			{
				Debug.LogError("No player ship could be found!");
				return false;
			}
			
			// check if the ship has any bonus slots left 
			if (b.NextAvailableBonusSlot() == null)
			{
				// Create a popup asking if player wants to dismantle an old upgrade from their ship.
				noSlotsLeft.CreateUI(DismantleMenu, CancelSynth);
				return false;
			}
			
			chunk.ApplyToPlayerShip();
			chunk.TakeSynthMaterials();
			
			// apply to save file
			if (DSave.current != null)
			{
				SubChassisData shipData = DSave.current.SavedPlayerShip();
				shipData.appliedSlots.Add(chunk.name);
				Debug.Log("Added " + chunk.name + " to " + shipData.chassisName + " in the save file");

				DSave.current.forgedUpgrades++;
				Debug.Log("Forged upgrades total: " + DSave.current.forgedUpgrades);
			}
			
			QuestManager.Tick();
			
			// Show the popup for a successful synthesis!
			string locText = forgeSuccess.LocalizedText();
			locText = string.Format(locText, chunk.LocalizedName());
			Notifier.DisplayNotification(locText, Color.cyan);

			return true;
		}

		void CancelSynth()
		{}
	}
}