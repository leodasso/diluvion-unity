using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
using Diluvion.Roll;
using Diluvion;

public class ScriptableObjectUtility
{
    #if UNITY_EDITOR
    
    /// <summary>
    //	This makes it easy to create, name and place unique new ScriptableObject asset files.
    /// </summary>
    public static T CreateAsset<T> (string newPath, string newAssetName) where T : ScriptableObject
    {
        // create scriptable object
        T asset = ScriptableObject.CreateInstance<T>();

        //create a new path name
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(newPath + newAssetName + ".asset");

        // create the asset
        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return asset;
    }

    /// <summary>
    //	Returns an asset of the given type with the given path and name. Asset must be a type of scriptable object.
    /// </summary>
    public static T GetAsset<T> (string path, string name) where T : ScriptableObject
    {
        //create a new path name
        string assetPathAndName = path + "/" + name + ".asset";

        // create the asset
        T obj = AssetDatabase.LoadAssetAtPath(assetPathAndName, typeof(ScriptableObject)) as T;

        if (obj == null)
        {
            Debug.LogError("No object was found at " + assetPathAndName);
            return null;
        }
        Debug.Log("Object at path: " + assetPathAndName, obj);

        return obj;
    }
    

    /// <summary>
    //	This makes it easy to create, name and place unique new ScriptableObject asset files.
    /// </summary>
    public static T CreateSpawnableAsset<T>(GameObject go) where T:SpawnableEntry
    {
        T asset = ScriptableObject.CreateInstance<T>();

        asset.prefab = go;

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + go.name + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        return asset;
    }


    /// <summary>
    //	This makes it easy to create, name and place unique new ScriptableObject asset files.
    /// </summary>
    public static RoomEntry CreateRoomAsset (GameObject go)
    {
        RoomEntry newEntry = GetAsset<RoomEntry>("Assets/Prefabs/RollTableObjects/Room Entries", go.name + " entry");

        if (newEntry == null)
            newEntry = CreateAsset<RoomEntry>("Assets/Prefabs/RollTableObjects/Room Entries/", go.name + " entry");

        newEntry.prefab = PrefabUtility.FindPrefabRoot(go) as GameObject;
        
        EditorUtility.SetDirty(newEntry);
        
        return newEntry;
    }
    
    #endif

}
