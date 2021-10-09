using UnityEngine;
using Diluvion.AI;

[CreateAssetMenu(fileName = "captain chassis", menuName = "Diluvion/global lists/captains")]
public class CaptainsGlobal : GlobalTableLists<Captain>
{
 
    private const string resourceName = "global captains";
  
    public static CaptainsGlobal tablesGlobal;
    
    public static CaptainsGlobal Get()
    {
        if (tablesGlobal != null) return tablesGlobal;
        tablesGlobal = Resources.Load(resourcesPrefix +resourceName)as CaptainsGlobal;
        return tablesGlobal;
    }
    
    
    
    /// <summary>
    /// Returns the chassis of the given name.
    /// </summary>
    public static Captain GetCaptain(string name)
    {
        return Get().GetEntry(name);
    }

   

    public override void FindAll()
    {
        
        ConfirmObjectExistence(Get(), resourcesPrefix + resourceName);
        allEntries = LoadObjects<Captain>("Assets/Prefabs/Captains");
#if UNITY_EDITOR
        SetDirty(this);
#endif
    }
}