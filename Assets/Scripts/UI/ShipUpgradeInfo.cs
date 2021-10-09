using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Diluvion.Ships;


namespace DUI {

/// <summary>
/// Container for showing the ship header and the ship detailed info window
/// </summary>
public class ShipUpgradeInfo : DUIPanel {

	public RectTransform headerParent;
	public RectTransform infoParent;

	CompareAction buttonAction;
	ShipHeader shipHeaderPanel;
	ShipStats shipStatsPanel;

	[TabGroup("Preview")]
	public SubChassis previewChassis;

	[TabGroup("Preview"), Button]
	void Preview()
	{
		if (!previewChassis)
		{
			Debug.LogError("Enter a chassis to preview.");
			return;
		}
		
		Init(new SubChassisData(previewChassis));
	}
	
	public void Init(SubChassisData sub) {
		
		SpiderWeb.GO.DestroyChildren(headerParent);
		SpiderWeb.GO.DestroyChildren(infoParent);

        // Spawn in and initialize the header
		shipHeaderPanel = UIManager.Create(UIManager.Get().shipHeader as ShipHeader, headerParent);
		shipHeaderPanel.Init( sub, null);

		//Turn off the header button 
		shipHeaderPanel.GetComponent<Button>().interactable = false;

        // Spawn in and initialize the main info
		shipStatsPanel = UIManager.Create(UIManager.Get().shipStats as ShipStats, infoParent);
		shipStatsPanel.Init(sub);
	}
}

}