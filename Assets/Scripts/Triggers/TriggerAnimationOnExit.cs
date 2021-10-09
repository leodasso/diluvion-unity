using UnityEngine;
using System.Collections;

public class TriggerAnimationOnExit : MonoBehaviour {

	public AnimationHelper animationToStart;

	//public MusicTrack trackToPlay;

	// Use this for initialization
	void Start () {
	
	}

	public void OnInteriorExit() {

		if (animationToStart != null) {
			animationToStart.Play();
		}

        /*
		if (trackToPlay != null) {
			Music.Get().AddMusic(trackToPlay);
		}
        */
	}
}
