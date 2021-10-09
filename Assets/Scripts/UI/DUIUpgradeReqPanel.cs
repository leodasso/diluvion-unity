using UnityEngine;
using UnityEngine.UI;
using Diluvion;
using Diluvion.Ships;
using Loot;
using TMPro;

/// <summary>
/// Displays the rerquirements for upgrading to a certain ship. 
/// </summary>
public class DUIUpgradeReqPanel : MonoBehaviour {

	public TextMeshProUGUI goldCostAmt;
	int _shipCost;

	public void Init(SubChassisData chassis, CompareAction newMode) 
	{
		int playerGold = PlayerManager.PlayerInventory().gold;
		_shipCost = chassis.ChassisObject().cost.goldCost;
		
		foreach (DItem i in ItemsGlobal.GetItems(chassis.appliedSlots)) 
		{
			Forging forging = i as Forging;
			if (!forging)
			{
				Debug.LogError("Upgrade listed as " + i.name + " is not actually a forging."); 
				continue;
			}

			_shipCost += forging.goldValue;
		}  

		Color goldColor = Color.white;
		if (playerGold < _shipCost) goldColor = Color.red;

		// Display the gold cost
		goldCostAmt.text = playerGold + " / " + _shipCost;
		goldCostAmt.color = goldColor;
	}

	public bool RequirementsMet()
	{
		// Check if player has enough gold
		return PlayerManager.pBridge.GetInventory().gold >= _shipCost;
	}
}
