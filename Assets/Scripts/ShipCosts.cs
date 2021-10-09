using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;
using Diluvion;

[CreateAssetMenu(fileName = "new ship costs", menuName = "Diluvion/Ship Costs Object")]
public class ShipCosts : ScriptableObject {

	CaptainScriptableObject newCaptain;
	public List<StackedItem> requiredItems = new List<StackedItem>();
	public int goldCost = 100;
}
