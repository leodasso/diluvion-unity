using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

using Diluvion.SaveLoad;

[System.Serializable]
public class SwitchSave  {

    public string uniqueID;
    [ReadOnly]
    public bool value;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ID">the unique ID of the switch to check</param>
    /// <param name="defaultValue">The default value if the switch hasn't been saved yet</param>
    /// <returns></returns>
    public static bool GetSavedValue(string ID, bool defaultValue)
    {
        if (DSave.current == null) return defaultValue;

        if (DSave.current.savedSwitches == null) return defaultValue;

        SwitchSave s = GetSavedObject(ID);
        if (s != null) return s.value;

        return defaultValue;
    }

    public static SwitchSave GetSavedObject(string ID)
    {
        if (DSave.current == null) return null;
        if (DSave.current.savedSwitches == null) return null;

        foreach (SwitchSave s in DSave.current.savedSwitches)
        {
            if (s.uniqueID == ID) return s;
        }
        return null;
    }

    /// <summary>
    /// Saves this value to the current dsave
    /// </summary>
    public void SaveValue()
    {
        SwitchSave s = GetSavedObject(uniqueID);
        if (s != null)
        {
            s.value = value;
            return;
        }

        DSave.current.savedSwitches.Add(this);
    }
}
