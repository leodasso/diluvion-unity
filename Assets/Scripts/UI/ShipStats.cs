using System.Collections.Generic;
using Diluvion;
using UnityEngine;
using Diluvion.Ships;
using TMPro;
using Sirenix.OdinInspector;
using SpiderWeb;

namespace DUI { 
public class ShipStats : DUIPanel {

	public TextMeshProUGUI bonusSlots;
	public TextMeshProUGUI stationSlots;
	public TextMeshProUGUI bonusType;
	public TextMeshProUGUI powerplant;
	public TextMeshProUGUI maxSpeed;
	public TextMeshProUGUI mass;
	public RectTransform armamentsParent;
	public RectTransform forgeItemsParent;
	public DUIArmamentPanel armamentPanelPrefab;
	public GameObject forgeItemPrefab;

	[TabGroup("Preview"), AssetsOnly]
	public SubChassis testChassis;
	[Button]
	[TabGroup("Preview")]
	void Preview()
	{
		Preview(testChassis);
	}

	[ReadOnly]
	public List<DUIArmamentPanel> armamentPanels = new List<DUIArmamentPanel>();

	public void Preview(SubChassis chassis)
	{
		if (chassis == null)
		{
			Debug.LogError("can't preview a null chassis!");
			return;
		}
		
		Init(new SubChassisData(testChassis));
	}

	public void Init(SubChassisData sub)
	{
		SubChassis chassis = sub.ChassisObject();
		
		GO.DestroyChildren(forgeItemsParent);
		
		// Fill in bonus slots
		for (int i = 0; i < chassis.bonusSlots; i++)
		{
			ForgeItemPreview newPreview = Instantiate(forgeItemPrefab, forgeItemsParent).GetComponent<ForgeItemPreview>();

			// If the sub's applied slot count is less than or equal to i, than there's no slot for this index
			if (sub.appliedSlots.Count <= i) continue;
			
			// get the slot for this index
			Forging c = ItemsGlobal.GetChunk(sub.appliedSlots[i]);
			
			newPreview.ShowItem(c);
		}
		
		// Fill in stats
		// bonus slots count
		SetText(bonusSlots, chassis.bonusSlots.ToString());
		
		// station slots text
		SetText(stationSlots, chassis.stationSlots.ToString());
		
		// chassis bonus
		SetText(bonusType, chassis.chassisBonus.name + " " + chassis.bonusMultiplier.ToString());

		var mover = chassis.prefab.GetComponent<ShipMover>();
		
		// powerplant
		SetText(powerplant, mover.engine.maxEnginePower.ToString());
		
		// max speed
		SetText(maxSpeed, mover.MaxSpeedPerSecond().ToString("00.0"));
		
		// mass
		SetText(mass, (chassis.prefab.GetComponent<Rigidbody>().mass * 200).ToString());
		
		GO.DestroyChildren(armamentsParent);
		
		// Fill in armament info
		chassis.RefreshMounts();
		
		foreach (var mount in  chassis.armaments)
		{
			//get all the weapons
			if (mount.weaponModule) AddArmament(mount.weaponModule);
		}
	}

	
	void AddArmament(WeaponModule module)
	{
		// Check if a panel for this weapon already exists
		foreach (var panel in armamentPanels)
		{
			if (panel.weapon == module)
			{
				panel.qty++;
				panel.Refresh(module);
				return;
			}
		}
		
		// if not, make one
		DUIArmamentPanel newPanel = Instantiate(armamentPanelPrefab, armamentsParent) as DUIArmamentPanel;
		newPanel.Refresh(module);
		armamentPanels.Add(newPanel);
	}

	void SetText(TextMeshProUGUI tm, string newText)
	{
		tm.text = tm.text.Replace("#", newText);
	}
}
}