using UnityEngine;
using System.Collections;
using Diluvion.Ships;

public class TriggerPopup : Trigger {

	public PopupObject popup;


	public override void TriggerAction (Bridge otherBridge)
	{
		base.TriggerAction (otherBridge);
        popup.CreateUI();
	}
}
