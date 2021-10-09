using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
[CreateAssetMenu(fileName = "Random Crew List", menuName = "Diluvion/Random Crew List")]
public class RandomCrewLists : ScriptableObject {

	public List<RuntimeAnimatorController> femaleAnimators = new List<RuntimeAnimatorController>();
	public List<RuntimeAnimatorController> maleAnimators = new List<RuntimeAnimatorController>();
    [SerializeField]
	public List<Dialogue> genericDialogues = new List<Dialogue>();

	/// <summary>
	/// Returns a random animation controller for the given gender
	/// </summary>
	public RuntimeAnimatorController RandomController(Gender gender) 
	{
		List<RuntimeAnimatorController> possibleAnimators = new List<RuntimeAnimatorController>();

		if (gender == Gender.Female) possibleAnimators = femaleAnimators;
		else possibleAnimators = maleAnimators;

		int randomIndex = UnityEngine.Random.Range(0, possibleAnimators.Count);

		RuntimeAnimatorController selected = possibleAnimators[randomIndex];

		//Debug.Log("Chose a random animation controller " + selected.name);

		return selected;   
	}

	public RuntimeAnimatorController ControllerFromName(string name) {

		List<RuntimeAnimatorController> controllers = new List<RuntimeAnimatorController>();
		controllers.AddRange(femaleAnimators);
		controllers.AddRange(maleAnimators);

		foreach (RuntimeAnimatorController controller in controllers)
			if (controller.name == name) return controller;

		return null;
	}

	public Dialogue RandomDialogue() 
	{
		int randomIndex = Random.Range(0, genericDialogues.Count);

		Dialogue selected = genericDialogues[randomIndex];

		//Debug.Log("Chose a random dialog " + selected);

		return selected;
	}

	public Dialogue DialogueFromName(string name) {

		foreach (Dialogue d in genericDialogues) {

			if (d == null) continue;
			if (d.name == name) return d;
		}

        if ( genericDialogues.Count > 0 ) return genericDialogues[0];
		return null;
	}
}
