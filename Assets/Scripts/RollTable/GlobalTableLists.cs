using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion.Roll;
using Diluvion;
using Diluvion.AI;

public abstract class GlobalTableLists<T> : GlobalList where T : Entry
{

    public Table objectTable;
    
    public List<T> allEntries = new List<T>();
    
    //Returns the weighted rolling list for chassi' or creates one if its not there
    public Table Table()
    {
        if (objectTable != null) return objectTable;
        
#if UNITY_EDITOR
        
        objectTable = ScriptableObjectUtility.CreateAsset<Table>("Assets/Prefabs/RollTableObjects", "new entry table");
        objectTable.ClearTables();
        objectTable.AddRoll(allEntries);

#endif
        return objectTable;
        
    }
    
    public virtual T GetEntry(string name)
    {
        foreach (T t in allEntries)
            if (t.name == name) return t;

        return null;
    }
 


    #if UNITY_EDITOR

 
    protected override void TestAll()
    {
        base.TestAll();
        TestAllObjects(allEntries, new GetObjectDelegate(GetEntry));
    }

    #endif
}