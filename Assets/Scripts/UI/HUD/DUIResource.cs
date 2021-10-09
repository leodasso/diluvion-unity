using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Loot;
using Diluvion;

public class DUIResource : MonoBehaviour {

	public DItem myItem;
	public Image icon;
    public TextMeshProUGUI qtyText;

	int _itemQty;
	int _oldQty;
	Animator _animator;
	bool _showedPopup;
    bool _empty ;

	public void Init(DItem newItem) 
	{
		myItem = newItem;
		icon.sprite = myItem.icon;

		//Set colors
		icon.color = myItem.myColor;
        qtyText.color = myItem.myColor;

		_animator = GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {

		if (PlayerManager.pBridge == null)
			return;

		//get the quantity of this item
		_itemQty = PlayerManager.pBridge.GetInventory().RemainingItems (myItem);

		//Check if the amount has changed
		if (_itemQty != _oldQty) {

            if ( _itemQty < 1 ) _empty = true;
            else _empty = false;

			UseResource ();
        }

		//display quantity
		qtyText.text = _itemQty.ToString ();

        // Adjust color to show qty
        if ( _empty ) icon.color = Color.red;
        else icon.color = myItem.myColor;

		_oldQty = _itemQty;
	}

	public void UseResource() {

		_animator.SetTrigger ("use");

		//Show as red if you're low on this resource
		if (_itemQty <= myItem.lowQty) {
			qtyText.color = Color.red;
		}else {
            qtyText.color = myItem.myColor;
		}

		//Show warning if low on resource
		if (_itemQty <= myItem.lowQty) {

            // Get localized text for the warning
            string text = SpiderWeb.Localization.GetFromLocLibrary("GUI/resource_warning", "warnin");
            text = string.Format(text, myItem.LocalizedName());

			// TODO
            //DUIController.Get().AddResourceNotifier(myItem, Color.red, text);
			//showedWarning = true;

		}
	}
}
