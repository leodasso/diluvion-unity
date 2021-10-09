using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndGameDockTrigger : MonoBehaviour {

    //public List<Prerequisite> prereqForEnd = new List<Prerequisite>();

	// Use this for initialization
	void Start () {
	}

    public void OnUndock()
    {
        Debug.Log("Triggered on undock ");

        // check prerequesites
        //foreach (Prerequisite pr in prereqForEnd) if (!pr.IsPrereqMet()) return;
        // end the game
        //StageManager.Get().EndGameCredits();
    }
}
