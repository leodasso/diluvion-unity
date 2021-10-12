using UnityEngine;
using System.Collections;

public class ExampleTopDownOpenDoor : MonoBehaviour {

	// Rotates colliders based on what side this door is facing.
	void Awake () {
		if (transform.parent.GetComponent<Tile2D>().tileSprite == "CW") {
			transform.eulerAngles = new Vector3(0, 0, 90);
			return;
		}
		if (transform.parent.GetComponent<Tile2D>().tileSprite == "CS") {
			transform.eulerAngles = new Vector3(0, 0, 180);
			return;
		}
		if (transform.parent.GetComponent<Tile2D>().tileSprite == "CE") {
			transform.eulerAngles = new Vector3(0, 0, 270);
			return;
		}
	}
}
