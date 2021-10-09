using UnityEngine;
using Diluvion;

[CreateAssetMenu(fileName = "loadout chassis", menuName = "Diluvion/global lists/loadouts")]
public class LoadoutsGlobal : GlobalTableLists<SubLoadout>
{//TODO Set up globaltTableList for the three chasiss types

 

    public Sprite defaultBonusSlot;

    private const string resourceName = "global loadouts";
    
    public static LoadoutsGlobal loadoutsGlobal;
    
    public static LoadoutsGlobal Get()
    {
        if (loadoutsGlobal != null) return loadoutsGlobal;
        loadoutsGlobal = Resources.Load(resourcesPrefix +resourceName)as LoadoutsGlobal;
        return loadoutsGlobal;
    }
    
    /// <summary>
    /// Returns the chassis of the given name.
    /// </summary>
    public static SubLoadout GetLoadout(string name)
    {
        return Get().GetEntry(name);
    }



    public override void FindAll()
    {
#if UNITY_EDITOR
        ConfirmObjectExistence(Get(), resourcesPrefix + resourceName);
        allEntries = LoadObjects<SubLoadout>("Assets/Prefabs/Loadouts");
        SetDirty(this);
#endif
    }
    
   

  
}