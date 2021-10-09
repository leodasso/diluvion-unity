using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using Object = UnityEngine.Object;
using Sirenix.OdinInspector;


/// <summary>
/// Container for all of a certain type of thing. This is the base class that they will extend from.
/// For example, globalItems contains all items, and can easily retrieve them from a string.
/// </summary>
public abstract class GlobalList : ScriptableObject {


    public delegate Object GetObjectDelegate(string objName);
    protected const string resourcesPrefix = "global lists/";

    [Button("Refresh")]
    public virtual void OnInspectorEnable() { FindAll(); }

    /// <summary>
    /// Tests all the objects in the list to see if they can be found by a string name.
    /// This confirms that they can be loaded from a save file.
    /// </summary>
    [Button]
    protected virtual void TestAll() { }

    protected virtual void TestAllObjects<T>(List<T> list, GetObjectDelegate objGetter) where T:Object
    {
        Debug.Log("Testing all values...");
        foreach (T item in list)
            TestObject(objGetter(item.name));

        Debug.Log("<color=green>Testing complete.</color>");
    }

    /// <summary>
    /// Gets an object of string key from the given list.
    /// </summary>
    public static T GetObject<T>(string key, List<T> objectsList) where T : Object
    {
        for (int i = 0; i < objectsList.Count; i++)
        {
            if (objectsList[i] == null)
            {
                Debug.LogError(typeof(T) + " at index " + i + " is null!");
                continue;
            }
            if (objectsList[i].name == key) return objectsList[i];
        }

        return null;
    }

    /// <summary>
    /// Returns a list of objects which correspond to the given list of strings.
    /// </summary>
    public static List<T> GetObjects<T>(List<string> keys, List<T> objectsList) where T : Object
    {
        List<T> returnList = new List<T>();

        for (int i = 0; i < keys.Count; i++)
        {
            returnList.Add(GetObject(keys[i], objectsList));
        }
        return returnList;
    }

    /// <summary>
    /// Logs if this object passed the test.
    /// </summary>
    protected void TestObject(Object obj)
    {
        string testString = "Testing '" + obj.name + "' " + obj + "...";
        if (obj != null)
        {
            testString += " <color=green>found successfully.</color>";
            Debug.Log(testString);
        }
        else
        {
            testString += "error.";
            Debug.LogError(testString);
        }
    }

    /// <summary>
    /// Finds all the required objects from the project folder.
    /// </summary>
    public abstract void FindAll();


    /// <summary>
    /// Loads the objects of the specified type from directory. Sets the objecct target as dirty
    /// (so it will save) once it's done.
    /// </summary>
    public static List<T> LoadObjects<T>(string directory, string suffix = "*.asset") where T : Object
    {
        // Create a generic list
        List<T> returnList = new List<T>();
#if UNITY_EDITOR

        DirectoryInfo path = new DirectoryInfo(directory);
        FileInfo[] fInfo = path.GetFiles(suffix, SearchOption.AllDirectories);

        // Get the objects from each file info
        foreach (FileInfo f in fInfo)
        {
            string nicePath = f.FullName;
            string nicerPath = nicePath.Substring(nicePath.LastIndexOf("Assets"));

            Object result = AssetDatabase.LoadAssetAtPath(nicerPath, typeof(Object));
            if (result == null) continue;
            if (result as T != null) returnList.Add(result as T);
        }
#endif
        return returnList;
    }

    /// <summary>
    /// Saves the target in editor.
    /// </summary> 
    public static void SetDirty(Object target)
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(target);
#endif
    }

    /// <summary>
    /// Confirms if the object exists in the resources folder. Throws error if not.
    /// </summary>
    protected static bool ConfirmObjectExistence(Object listObject,  string nameInResources)
    {
#if UNITY_EDITOR
        if (listObject == null)
        {
            Debug.LogError("Global items list couldn't be found! It must be in resources and named '" + nameInResources + "'.");
            return false; 
        }

#endif
        return true;
    }


}