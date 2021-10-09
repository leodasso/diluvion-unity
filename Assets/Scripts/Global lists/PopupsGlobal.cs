using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "popups global", menuName = "Diluvion/global lists/popups")]
public class PopupsGlobal : GlobalList {

    static PopupsGlobal popupsGlobal;

    public List<PopupObject> allPopups = new List<PopupObject>();

    const string resourceName = "popups global";

    public static PopupsGlobal Get()
    {
        if (popupsGlobal) return popupsGlobal;
        popupsGlobal = Resources.Load(resourcesPrefix + resourceName) as PopupsGlobal;
        return popupsGlobal;
    }

    public static PopupObject GetPopup(string name)
    {
        return GetObject(name, Get().allPopups);
    }


    protected override void TestAll()
    {
        TestAllObjects(allPopups, new GetObjectDelegate(GetPopup));
    }

    public override void FindAll()
    {
#if UNITY_EDITOR
        ConfirmObjectExistence(Get(), resourcesPrefix + resourceName);
        allPopups = LoadObjects<PopupObject>("Assets/Prefabs/Popups");
        Debug.Log("Finding all popups");
        SetDirty(this);
#endif
    }

}