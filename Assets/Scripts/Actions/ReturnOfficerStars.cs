using System.Collections.Generic;
using Diluvion.SaveLoad;
using UnityEngine;
using DUI;
using Loot;
using Sirenix.OdinInspector;

namespace Diluvion
{

	/// <summary>
	/// Can return all officer stars from a given character. Reads data from a duplicate crew-save in dsave that never
	/// removes officers.
	/// </summary>
	[CreateAssetMenu(menuName = "Diluvion/actions/return officer stars")]
	public class ReturnOfficerStars : Action
	{
		[MinValue(0), Tooltip("Amount of stars to give on top of stars salvaged from the officer.")]
		public int defaultStars;
		
		public LocTerm givenStarsPanelTitle;
		
		[Tooltip("The character to give officer stars from")]
		public CharacterInfo officer;

		public DItem officerStar;
		
		List<StackedItem> _starsToGive = new List<StackedItem>();
		
		public override bool DoAction(Object o)
		{
			// Check that save file exists
			if (DSave.current == null)
			{
				Debug.LogError("No save data exists, unable to return officer stars.");
				return false;
			}
			
			
			// check if the given character is contained in the save file
			DSave.current.UpdateCharacterHistory();
			var charSave = DSave.current.DataForCharacterInfo(officer);
			if (charSave == null)
			{
				Debug.Log("No save history exists for character " + officer.name + ", therefore not giving any officer stars.");
				return true;
			}
			
			
			// Get the count of officer stars to return
			int level = charSave.savedLevel + defaultStars;
			if (level <= 1)
			{
				Debug.Log("Character " + officer.name + " was saved at default level, therefore there's no officer stars to return.");
				return true;
			}
			
			
			// Remember how many officer stars need to be given 
			_starsToGive.Clear();
			_starsToGive.Add(new StackedItem(officerStar, level - 1));
			
			// Give the officer stars to the player
			var panel = ItemExchangePanel.ShowItemExchange(_starsToGive, givenStarsPanelTitle.LocalizedText(), false);
			panel.onEnd += GiveStars;
			return true;
		}

		
		void GiveStars()
		{
			PlayerManager.GivePlayerItems(_starsToGive, true);
		}

		public override string ToString()
		{
			return "gives officer stars (if any) from " + officer.name + " back to the player.";
		}

		protected override void Test()
		{
			DoAction(null);
		}
	}
}