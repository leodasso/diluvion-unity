using UnityEngine;
using System.Collections.Generic;
using Loot;
using Diluvion;

[CreateAssetMenu(fileName = "inventory global", menuName = "Diluvion/global lists/inventory")]
public class InvGlobal : GlobalList
{

    public LocTerm mainItemsTitle;
    public LocTerm keyItemsTitle;
    public LocTerm weaponsTitle;
    
    public static InvGlobal invGlobal;
    public InvGenerator playerShipInv;
    public InvGenerator playerStorageInv;
    public List<InvGenerator> allInvGens = new List<InvGenerator>();

    public static InvGlobal Get()
    {
        if (invGlobal) return invGlobal;
        invGlobal = Resources.Load(AssetName()) as InvGlobal;
        return invGlobal;
    }

    protected override void TestAll()
    {
        base.TestAll();
        TestAllObjects(allInvGens, GetInventory);
    }

    public static InvGenerator PlayerShipInvGen()
    {
        return Get().playerShipInv;
    }

    public static InvGenerator PlayerStorageInvGen()
    {
        return Get().playerStorageInv;
    }

    public static InvGenerator GetInventory(string name)
    {
        foreach (InvGenerator inv in Get().allInvGens)
        {
            if (inv == null) continue;
            if (inv.name == name) return inv;
        }
        return null;
    }

    
    public override void FindAll()
    {
#if UNITY_EDITOR
        allInvGens = LoadObjects<InvGenerator>("Assets/Prefabs/Inventories");
        SetDirty(this);
        Debug.Log("Finding all inventory generators.");
#endif
    }

    /// <summary>
    /// Checks the given inventory generator; if it saves, checks that it has a unique name. Heavy, so it should
    /// only be used in editor.
    /// </summary>
    public static bool ValidName(InvGenerator obj)
    {
        if (!obj.saves) return true;
        if (!ConfirmObjectExistence(Resources.Load(AssetName()), AssetName())) return false;

        string n = obj.name;

        foreach (InvGenerator ig in Get().allInvGens)
        {
            if (ig == obj) continue;
            if (ig.name == n) return false;
        }
        return true;
    }  
    


    static string AssetName()
    {
        return resourcesPrefix + "inventory global";
    }
}