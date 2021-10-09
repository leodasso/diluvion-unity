using UnityEngine;
using UnityEngine.UI;
using Loot;
using System.Collections;

public class DUIResourceWarning : MonoBehaviour {

	public Image iconImage;
	public Text warningText;
	public CanvasGroup canvasGroup;

	DItem myItem;

	// Use this for initialization
	public void Init (DItem newItem) {
	
		myItem = newItem;

		iconImage.sprite = myItem.icon;
		iconImage.color = myItem.myColor;
		warningText.color = myItem.myColor;
		warningText.text += myItem.LocalizedName();
	}

	public void EndMe() {
		Destroy(gameObject);
	}
}
