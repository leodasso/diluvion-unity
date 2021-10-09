using UnityEngine;
using UnityEngine.UI;
using Diluvion;
using TMPro;

public class DUIItemRequiredPanel : MonoBehaviour {

	public Image itemIcon;
	public TextMeshProUGUI itemName;
	public TextMeshProUGUI itemAmt;

	int qtyReq;
	Loot.DItem item;
	Color initColor;


	/// <summary>
	/// Inits the element based on an item stack.  The qty of the stack is used
	/// as the amount required.
	/// </summary>
	public void Init(StackedItem newItem, bool fraction = true) 
	{
		initColor = itemAmt.color;
		item = newItem.item;
		qtyReq = newItem.qty;
		Refresh(fraction);
	}

	/// <summary>
	/// Refreshes the display of this panel.
	/// </summary>
	/// <param name="formatAsFraction">If true, format is like 15/20 [player has]/[req] otherwise its just [req]</param>
	public void Refresh(bool formatAsFraction = true) {

		itemIcon.sprite = item.icon;
		itemName.text = item.LocalizedName();

		if (formatAsFraction) itemAmt.text = AmountDisplay();
		else itemAmt.text = qtyReq.ToString();

		//set text to default color
		itemAmt.color = initColor;

		// Set text to red if player doesn't have enough
		if (!HasEnough() && formatAsFraction) itemAmt.color = Color.red;
	}

	string AmountDisplay() 
	{
		if (!PlayerManager.PlayerShip())
		{
			return "N/A / " + qtyReq;
		}
		
		int playerAmt = PlayerManager.pBridge.GetInventory().RemainingItems(item);
		return playerAmt + " / " + qtyReq;
	}

	/// <summary>
	/// Does the player have at least this many of this item in their inventory?
	/// </summary>
	public bool HasEnough()
	{
		if (PlayerManager.PlayerShip() == null) return false;
		
		int playerAmt = PlayerManager.pBridge.GetInventory().RemainingItems(item);

		if (playerAmt < qtyReq) return false;
		return true;
	}

}
