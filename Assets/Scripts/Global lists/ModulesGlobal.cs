using UnityEngine;
using System.Collections.Generic;
using Diluvion.Ships;

[CreateAssetMenu(fileName = "module list", menuName = "Diluvion/global lists/ship modules")]
public class ModulesGlobal : GlobalList {

    public List<ShipModule> allModules = new List<ShipModule>();
    public static ModulesGlobal modulesGlobalStatic;
    const string resourceName = "module list";

    public static ModulesGlobal Get()
    {
        if (modulesGlobalStatic) return modulesGlobalStatic;
        modulesGlobalStatic = Resources.Load(resourcesPrefix + resourceName) as ModulesGlobal;
        return modulesGlobalStatic;
    }
    
    public static ShipModule GetModule(string name)
    {
        return GetObject(name, Get().allModules) as ShipModule;
    }
    

    protected override void TestAll()
    {

        base.TestAll();
        TestAllObjects(allModules, new GetObjectDelegate(GetModule));
    }

    public override void FindAll()
    {
#if UNITY_EDITOR
        ConfirmObjectExistence(Get(), resourcesPrefix + resourceName);
        allModules = LoadObjects<ShipModule>("Assets/Prefabs/Ships/_modules");
        SetDirty(this);
        Debug.Log("finding all modules.");
#endif
    }
    

}