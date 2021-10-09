using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Text))]
public class LoadingTicker : MonoBehaviour {

	public List<string> funnys;
	public float waitTime = 3;

	int index = 0;
	Text textComponent;
	float alpha = 0;

	// Use this for initialization
	void Start () {
	
		textComponent = GetComponent<Text>();

		StartCoroutine (ShowText());
	}

	void Update() {

		textComponent.color = Color.Lerp( textComponent.color, 
		                                 new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, alpha), Time.deltaTime * 5);
	}

	IEnumerator ShowText() {

		//set alpha to 0 to make text invis
		alpha = 0;

		//wait a bit at the intro for the fade in
		yield return new WaitForSeconds(.5f);

		//set text
		string currentText = funnys[index];
		textComponent.text = currentText;

		//make text visible
		alpha = 1;

		//iterate index
		index ++;
		if (index >= funnys.Count) index = 0;

		//wait
		yield return new WaitForSeconds(waitTime);

		//reset coroutine
		StartCoroutine(ShowText());
	}

}
