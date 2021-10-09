using UnityEngine;
using System.Collections;
using Diluvion;

public class IntroScene : MonoBehaviour {

	void Update() {

		//if (Input.GetKeyDown(KeyCode.S)) EndScene();
		if (GameManager.Player().GetButtonDown("skip tut")) EndScene();
	}

	/// <summary>
	/// Ends the cutscene and continues on to load a new game
	/// </summary>
	public void EndScene() {
		
		// bring in ship select panel
		//UIManager.Create(UIManager.Get().shipSelectPanel);

		//Destroy(gameObject);
		
		FadeOverlay.FadeInThenOut(3, Color.black, GameManager.LoadShipSelection);
	}
}
