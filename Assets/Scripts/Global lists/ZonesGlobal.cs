using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "zones global", menuName = "Diluvion/global lists/zones")]
public class ZonesGlobal : GlobalList {

    public static ZonesGlobal zonesGlobal;

    public List<GameZone> zones = new List<GameZone>();
    const string resourceName = "zones global";

    public static ZonesGlobal Get()
    {
        if (zonesGlobal) return zonesGlobal;
        zonesGlobal = Resources.Load(resourcesPrefix + resourceName) as ZonesGlobal;
        return zonesGlobal;
    }
    
    
    /// <summary>
    /// Returns a zone by it's zone object name.
    /// </summary>
    public static GameZone GetZone(string name)
    {
        return GetObject(name, Get().zones);
    }

    /// <summary>
    /// Returns a zone by it's 'Zone' enum
    /// </summary>
    public static GameZone GetZone(Zones zone)
    {
        foreach (GameZone z in Get().zones)
        {
            if (z.zone == zone) return GetZone(z.name);
        }
        return null;
    }



    public override void FindAll()
    {
      
        zones.Clear();

        zones = LoadObjects<GameZone>("Assets/Game/Zones");
        Debug.Log("Found all zones.");
#if UNITY_EDITOR
        SetDirty(this);
#endif
    }
    
    protected override void TestAll()
    {
        
        base.TestAll();
        TestAllObjects(zones, GetZone);
    }


}
