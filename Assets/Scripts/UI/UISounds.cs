using UnityEngine;
using System.Collections;
using SpiderWeb;

public class UISounds : MonoBehaviour {

	
	public void PlayOnHover() {
        //Debug.Log("Hover");
        SpiderSound.MakeSound("Play_UI_Button_HoverOver", gameObject);
    }

	public void PlayDialogHover()
	{
		SpiderSound.MakeSound("Play_Conversation_Mouse_Over", gameObject);
	}

	public void PlayDialogClick()
	{
		SpiderSound.MakeSound("Play_Conversation_Button_Press", gameObject);
	}

	public void PlayOnClick() {
		SpiderSound.MakeSound("Play_Click_Generic_Accept", gameObject);
	}

	public void PlayPurchase() {
        //SpiderSound.MakeSound("Play_Stinger_Purchased", gameObject);
    }

	public void PlayPopup() {
        SpiderSound.MakeSound("Play_Dialog_Popup", gameObject);
    }

	public void PlayExit() {
        SpiderSound.MakeSound("Play_OnExit", gameObject);
    }
}
