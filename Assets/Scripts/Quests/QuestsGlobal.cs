using UnityEngine;
using System.Collections.Generic;
using Quests;

[CreateAssetMenu( fileName = "quests global", menuName = "Diluvion/global lists/quests")]
public class QuestsGlobal : GlobalList
{
    public List<DQuest> allQuests = new List<DQuest>();
    public List<Objective> allObjectives = new List<Objective>();
    public static QuestsGlobal questsGlobalStatic;
    const string resourceName = "quests global";


    public static QuestsGlobal Get()
    {
        if (questsGlobalStatic != null) return questsGlobalStatic;
        questsGlobalStatic = Resources.Load(resourcesPrefix + resourceName) as QuestsGlobal;
        return questsGlobalStatic;
    }

    public static Objective GetObjective(string nameKey)
    {
        return GetObject(nameKey, Get().allObjectives) as Objective;
    }

    /// <summary>
    /// Returns the name of the scriptable object for the given locKey name. Use this 
    /// for getting items from a save file older than 1.2
    /// </summary>
    public string NameForLocKey(string locKey)
    {
        foreach (DQuest q in allQuests)
        {
            if (!q) continue;
            if (q.locKey == locKey) return q.name;
        }
        return "";
    }

    public static DQuest GetQuest(string nameKey)
    {
        return GetObject(nameKey, Get().allQuests) as DQuest;
    }
    


    public override void FindAll()
    {
#if UNITY_EDITOR
        allQuests = LoadObjects<DQuest>("Assets/Prefabs/Quests");
        allObjectives = LoadObjects<Objective>("Assets/Prefabs/Quests");
        Debug.Log("Finding all quests.");
        SetDirty(this);
#endif
    }
    
    protected override void TestAll()
    {
        TestAllObjects(allQuests, new GetObjectDelegate(GetQuest));

        Debug.Log("Testing to get from Loc keys");
        foreach (DQuest q in allQuests)
        {
            string test2 = "Testing " + q.locKey;
            TestObject(GetQuest(NameForLocKey(q.locKey)));
        }

        TestAllObjects(Get().allObjectives, new GetObjectDelegate(GetObjective));

        Debug.Log("<color=green>Testing complete.</color>");
    }

}