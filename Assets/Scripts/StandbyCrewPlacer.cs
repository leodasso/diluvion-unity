using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;
using Diluvion;

/// <summary>
/// This component, when placed on an interior object, will create crew instances from the standby crew list
/// and place them in available character placers.  Must generate a list of character placers in editor for
/// it to work.
/// </summary>
public class StandbyCrewPlacer : MonoBehaviour {

	[Comment("Don't forget to click the button to generate list of placers.", CommentType.Info)]
	[Button("Find Character Placers", "FindCharacterPlacers", true)]
	public bool hidden;

	public List<CharacterPlacer> placers;

	// Use this for initialization
	void Start () {
	
	}

	void FindCharacterPlacers() {
		placers = new List<CharacterPlacer>();
		placers.AddRange( GetComponentsInChildren<CharacterPlacer>());
	}

	public void Populate( List<Character> crewList) {

		//Create a list to store AVAILABLE placers.  They'll be removed from the list as theyre 
		//used up.
		List<CharacterPlacer> availablePlacers = new List<CharacterPlacer>();

		//Only add placers that aren't reserved to spawn another character
		foreach (CharacterPlacer placer in placers) {
			if (placer.character == null && placer.createRandomSailor == false)
				availablePlacers.Add(placer);
		}


		foreach (Character crew in crewList) {

			//Make sure there's still placers left
			if (availablePlacers.Count < 1) {
				Debug.Log("Trying to place a character in home base, but no placers remain.");
				return;
			}

			//Chose a random index
			int index = Random.Range(0, availablePlacers.Count);

			//Get the character placer at availablePlacers[index]
			CharacterPlacer chosenPlacer = availablePlacers[index];

			//Place the crew in that placer
			chosenPlacer.FinalizePlacement(crew);

			//Remove the placer from the list
			availablePlacers.Remove(chosenPlacer);

		}
	}
}
