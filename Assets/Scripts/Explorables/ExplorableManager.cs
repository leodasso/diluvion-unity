using UnityEngine;
using SpiderWeb;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;
using Random = UnityEngine.Random;
using Diluvion.Roll;


public class ExplorableManager : MonoBehaviour {

    #region vars
    public static ExplorableManager explorableManager;

    public static ExplorableManager Get()
    {
        if (explorableManager != null) return explorableManager;
        explorableManager = FindObjectOfType<ExplorableManager>();
        return explorableManager;
    }

    
    [Button("Convert Scene Placers to new Sizes", "ConvertOldExplorables", true)]
    public bool convertScenePlacers; 

    #region conversions

    [Comment("If this is true, we have already converted all the placers in this scene to the new numbers.")]
    public bool converted = false;
    /// <summary>
    /// Converts the old explorableNumbers to new for this scene
    /// </summary>
    public void ConvertOldExplorables()
    {
        if (converted) { Debug.LogError("This scene has already been converted once, are you sure you want to do it again?, this will screw up the explorables that are converted correctly"); return; }
        foreach(ExplorablePlacer ep in GetAllExplorablePlacers())
        {
            SpawnableSize newSize = ConvertSize((int)ep.size);
            ep.size = newSize;
        }
        converted = true;
    }
    /// <summary>
    ///All the old numbers  were used as index, they are now indicators of size, so: 0, 1 ,2 ,3 is now 1, 2, 3, 4
    /// </summary>
    SpawnableSize ConvertSize(int oldSize)
    {
        switch (oldSize)
        {
            case 0:
                {
                    return SpawnableSize.Small;
                }
            case 1:
                {
                    return SpawnableSize.Medium;
                }
            case 2:
                {
                    return SpawnableSize.Large;
                }
            case 3:
                {
                    return SpawnableSize.Huge;
                }
            default:
                {
                    return SpawnableSize.Medium;
                }
        }
    }
    #endregion
    /// <summary>
    /// Handy Explorable Placer Getter
    /// </summary>
    /// <returns></returns>
    public List<ExplorablePlacer> GetAllExplorablePlacers()
    {
        List<ExplorablePlacer> tempList = new List<ExplorablePlacer>();
        tempList.AddRange(FindObjectsOfType<ExplorablePlacer>());
        return tempList;
    }

    /*
    public static Table DefaultDockables()
    {
        return Get().defaultDockableTable;
    }
    */

    public Table defaultDockableTable;
    public Table defaultChallengeTable;


    #endregion
}
