using Diluvion.Ships;
using UnityEngine;

[CreateAssetMenu(fileName = "global chassis", menuName = "Diluvion/global lists/chassis")]
public class SubChassisGlobal : GlobalTableLists<SubChassis>
{//TODO Set up globaltTableList for the three chasiss types

    public int defaultInvSize = 12;
    public int defaultHP = 15;
    public int defaultCrewSize = 6;
    public float defaultCrushDepth = -200;
    public float defaultListenRange = 50;

    [Tooltip("The particle for when a submarine is destroyed")]
    public GameObject defaultDestructionParticle;

    [Tooltip("The particle for healing a submarine")]
    public GameObject healParticle;

    const string resourceName = "global chassis";


    /// <summary>
    /// Returns the particle effect for when a sub is destroyed.
    /// </summary>
    public static GameObject DestructionParticle()
    {
        return Get().defaultDestructionParticle;
    }

    public static GameObject HealParticle()
    {
        return Get().healParticle;
    }
    
    public static SubChassisGlobal chassisGlobal;
    
    public static SubChassisGlobal Get()
    {
        if (chassisGlobal != null) return chassisGlobal;
        chassisGlobal = Resources.Load(resourcesPrefix +resourceName)as SubChassisGlobal;
        return chassisGlobal;
    }
    
    /// <summary>
    /// Returns the chassis of the given name.
    /// </summary>
    public static SubChassis GetChassis(string name)
    {
        foreach (SubChassis chassis in Get().allEntries)
        {
            if(chassis==null)continue;
            
            if (chassis.name == name) return chassis;
        }
        return null;
    }


    /// <summary>
    /// Returns the chassis from the alternate name, i.e. the name of the prefab.
    /// </summary>
    public static SubChassis GetChassisFromPrefab(string prefabName)
    {
        foreach (SubChassis chassis in Get().allEntries)
        {
            if(chassis==null)continue;
           
            if (chassis.shipPrefab == null) continue;
            if (chassis.shipPrefab.name == prefabName) return chassis;
            if (chassis.altNames.Contains(prefabName)) return chassis;
        }
        return null;
    }
    
 

    public override void FindAll()
    {
#if UNITY_EDITOR
        ConfirmObjectExistence(Get(), resourcesPrefix + resourceName);
        allEntries = LoadObjects<SubChassis>("Assets/Prefabs/Chassis");
        SetDirty(this);
#endif
    }
    
   


}