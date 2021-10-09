using UnityEngine;
using System.Collections.Generic;
using Loot;
using System.Linq;
using HeavyDutyInspector;

[CreateAssetMenu(fileName = "item list", menuName = "Diluvion/global lists/items")]
public class ItemsGlobal : GlobalList
{
    [Button("Convert rarity to Weight","ConvertRareToWeight", true )]
    public bool convertRarity;
    public List<DItem> allItems = new List<DItem>();
    public static ItemsGlobal itemsGlobalStatic;
    const string resourceName = "item list";

    public static ItemsGlobal Get()
    {
        if (itemsGlobalStatic != null) return itemsGlobalStatic;
        itemsGlobalStatic = Resources.Load(resourcesPrefix + resourceName) as ItemsGlobal;
        return itemsGlobalStatic;
    }

    public static List<DItem> AllNonKeyItems()
    {
        return Get().allItems.Where(x => !x.keyItem).ToList();
    }

    public void ConvertRareToWeight()
    {
        foreach(DItem item in allItems)
        {
            //float popularity = 1 - item.rarity;
            //item.weight = Mathf.CeilToInt(popularity * 100);
        }
    }

    protected override void TestAll()
    {
        TestAllObjects(allItems, new GetObjectDelegate(GetItem));
 
        Debug.Log("Testing to get from Loc keys");
        foreach (DItem item in allItems)
        {
            string test2 = "Testing " + item.locKey;
            TestObject(GetItem(NameForLocKey(item.locKey)));
        }
        Debug.Log("<color=green>Testing complete.</color>");
    }

    /// <summary>
    /// Returns the name of the scriptable object for the given locKey name. Use this 
    /// for getting items from a save file older than 1.2
    /// </summary>
    public string NameForLocKey(string locKey)
    {
        foreach (DItem i in allItems)
        {
            if (i.locKey == locKey) return i.name;
        }
        return "";
    }
    


    public override void FindAll()
    {
#if UNITY_EDITOR
        ConfirmObjectExistence(Get(), (resourcesPrefix + resourceName));
       
        allItems = LoadObjects<DItem>("Assets/Items");
        SetDirty(this);
        Debug.Log("Loading all items");
#endif
    }
    


    public static DItem GetItem(string nameKey)
    {
        return GetObject(nameKey, Get().allItems) as DItem;
    }

    /// <summary>
    /// For forgings, returns the item with the given key as a BonusChunk
    /// </summary>
    public static Forging GetChunk(string key)
    {
        DItem item = GetItem(key);

        if (!(item is Forging))
        {
            Debug.LogError(item.name + " isn't a bonus chunk!");
            return null;
        }
        
        return item as Forging;
    }

    public static List<DItem> GetItems(List<string> itemNames)
    {
        return GetObjects(itemNames, Get().allItems);
    }
}