using UnityEngine;
using System.Collections;


// Station Light to indicate how many sailors are in a station
public class StationLight : MonoBehaviour {

	public Sprite offSprite;
	public Sprite onSprite;
	public bool isOn = false;

	SpriteRenderer lightSprite;

	// Use this for initialization
	void Start () {
	
		lightSprite = GetComponent<SpriteRenderer>();
	}

	public void SetState(bool newState) {

		isOn = newState;

		if (lightSprite == null) lightSprite = GetComponent<SpriteRenderer>();

		if (isOn) lightSprite.sprite = onSprite;
		else lightSprite.sprite = offSprite;
	}
}
