using UnityEngine;
using System.Collections;

public class ActivateBoss : MonoBehaviour {

	public ArkResentment ark;
    public GameObject startMusic;

	// Use this for initialization
	void Start () {
	
	}
	
	public void BeaconEnabled() {
		ark.StartIntroRoutine();
        if(startMusic)
            startMusic.SetActive(true);

    }
}
