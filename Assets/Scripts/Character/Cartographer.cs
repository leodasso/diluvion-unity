using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DUI;
using Diluvion;
using Diluvion.SaveLoad;
using Sirenix.OdinInspector;

/// <summary>
/// Attaches to a character, and when they're spoken to, checks player's inventory for new charts, adds charts that have been added to
/// save file, and rewards the player. If all charts have been found, unlocks the big treasure.
/// </summary>
[RequireComponent(typeof(Character))]
public class Cartographer : MonoBehaviour {

    public List<ChartInfo> charts = new List<ChartInfo>();

    /// <summary>
    /// Charts the player has found that are new (unsaved    ) and will be rewarded.
    /// </summary>
    List<ChartInfo> foundCharts = new List<ChartInfo>();

    Inventory playerInv;

	// Use this for initialization
	void Start () {
        // load charts from save file
        LoadCharts();
    }
	
    public void CheckCharts()
    {
        playerInv = PlayerManager.pBridge.GetInventory();
        if ( playerInv == null ) return;

        foundCharts.Clear();

        Debug.Log("Checking player's landmark charts!");

        // Check through each of the items I'm looking for to see if the player has it in their inventory.
        foreach (ChartInfo chart in charts)
        {
            if ( playerInv.HasItem(chart.chartItem))
            {
                Debug.Log("Player found " + chart.chartItem.niceName + "; discovered: " + chart.found);
                if ( !chart.found ) {
                    chart.found = true;
                    foundCharts.Add(chart);
                }
            }
        }

        // show GUI of new charts found.
        CartographerPanel panel = UIManager.Create(UIManager.Get().cartographerPanel as CartographerPanel);
        panel.paymentAccepted += PayPlayer;
        panel.paymentAccepted += SaveCharts;
        panel.Init(foundCharts);
    }

    /// <summary>
    /// Pays the player for each new chart discovered.
    /// </summary>
    void PayPlayer()
    {
        int amt = 0;
        foreach ( ChartInfo chart in foundCharts ) amt += chart.reward;

        playerInv.AddGold(amt);
    }
    
    /// <summary>
    /// Saves charts into dSave.current
    /// </summary>
    void SaveCharts()
    {
        if ( DSave.current == null ) return;
        Debug.Log("Saving charts' status.");
        foreach ( ChartInfo chart in foundCharts ) DSave.current.SaveChart(chart);
    }

    /// <summary>
    /// Load charts player's already turned in from dSave.current
    /// </summary>
    void LoadCharts()
    {
        if ( DSave.current == null ) return;
        Debug.Log("Loading charts' status.");
        foreach ( ChartInfo chart in charts ) chart.found = DSave.current.LoadChartStatus(chart);
    }
}

[System.Serializable]
public class ChartInfo
{
    [AssetsOnly]
    public Loot.DItem chartItem;
    public int reward = 1000;
    public bool found = false;
}
