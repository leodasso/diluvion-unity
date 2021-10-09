using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DUI;
using Diluvion.Ships;
using Sirenix.OdinInspector;

public class ShipBroker : MonoBehaviour {

	public List<SubChassis> myShips;
	[Tooltip("For purchasing new ships, these will be added to all blank subs.")]
	public List<Forging> defaultForgings = new List<Forging>();
    public bool dlcShips = false;
	public string shipyardName;
	[Range(1, 4)]
	public int engineerLevel;

    public List<SubChassis> MyShips()
    {
        List<SubChassis> scList = new List<SubChassis>(myShips);
        if (dlcShips)
            scList.AddRange(AssetManager.Get().DLCShips);
        return scList;
    }

	[Button]
	public void DoAction() {

        // Opens the shipyard menu
        Shipyard shipyard = UIManager.Create(UIManager.Get().shipyard as Shipyard);
        shipyard.Init(this);
	}
}
	