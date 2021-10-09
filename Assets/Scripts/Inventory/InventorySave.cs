using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class InventorySave
{
    public string locKey;
    public List<string> invStrings;
    public int gold;
    public int netWorth;

    public string lastUsedBolt = "";
    public string lastUsedTorpedo = "";

    public InventorySave ()
    {
        invStrings = new List<string>();
    }

    public InventorySave (InventorySave invSave)
    {
        locKey = invSave.locKey;
        gold = invSave.gold;
        lastUsedBolt = invSave.lastUsedBolt;
        lastUsedTorpedo = invSave.lastUsedTorpedo;
        invStrings = new List<string>(invSave.invStrings);
    }

    public InventorySave (Diluvion.Inventory inv, string invName = "playerShip")
    {
        locKey = invName;
        gold = inv.gold;

        invStrings = new List<string>();
        netWorth = 0;
        foreach (StackedItem i in inv.itemStacks)
        {
            if (i == null) continue;
            if (i.item == null) continue;
            netWorth += i.GoldValue();
            //Debug.Log("Saved Item: " + i.item.niceName);
            for (int j = 0; j < i.qty; j++)
                invStrings.Add(i.item.name);
        }
    }
}