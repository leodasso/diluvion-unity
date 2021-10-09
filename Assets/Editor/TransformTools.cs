using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class TransformTools : MonoBehaviour {

    [MenuItem("Tools/Transform/Group selection %g")]
    public static void GroupSelection()
    {
        // Get the top level children from the selection
        Transform[] children = Selection.GetTransforms(SelectionMode.TopLevel);
        GroupObjects(children.ToList());
    }

    static void GroupObjects(List<Transform> transforms, string customName = null)
    {
        if (transforms.Count < 1)
        {
            Debug.LogWarning("Group operation failed because there were no transforms given to group!");
            return;
        }
        
        Undo.SetCurrentGroupName("group objects");
        int group = Undo.GetCurrentGroup();

        Transform[] transformsArray = transforms.ToArray();

        // create an empty object to put selection into
        GameObject newGroup = new GameObject();
        Undo.RegisterCreatedObjectUndo(newGroup, "create new group");
        
        Undo.RecordObjects(transformsArray, "group");

        int siblingIndex = transforms[0].GetSiblingIndex();
        Transform newParent = transforms[0].parent;
        string newName = transforms[0].name;

        // Find the selection's center point
        Vector3 pivot = FindThePivot(transformsArray);

        // Place the group at the pivot point
        newGroup.transform.position = pivot;

        // put the children in the group
        foreach (Transform child in transforms) Undo.SetTransformParent(child, newGroup.transform, "change parent");

        // set the new group's parent to the same as the first child's parent.
        newGroup.transform.parent = newParent;
        newGroup.transform.SetSiblingIndex(siblingIndex);

        if (!string.IsNullOrEmpty(customName)) newName = customName;
        newGroup.transform.name = newName + " group";
        
        // Collapse all the undo operations into a single 'group objects' operation
        Undo.CollapseUndoOperations(group);
    }

    [MenuItem("Tools/Transform/Ungroup selection #%g")]
    public static void UngroupSelection()
    {
        Undo.SetCurrentGroupName("ungroup selection");
        int group = Undo.GetCurrentGroup();
        
        foreach (GameObject GO in Selection.gameObjects)
        {
            if (!GO.activeInHierarchy) continue;
            Ungroup(GO);
        }
        
        Undo.CollapseUndoOperations(group);
    }

    static void Ungroup(GameObject parent)
    {
        if (!parent) return;
        
        Undo.SetCurrentGroupName("ungroup " + parent.name);
        int group = Undo.GetCurrentGroup();
        
        foreach (Transform child in parent.transform)
        {
            Undo.RecordObject(child.transform, "ungroup");
            child.transform.parent = parent.transform.parent;
        }
        
        Undo.DestroyObjectImmediate(parent);
        Undo.CollapseUndoOperations(group);
        
    }

    [MenuItem("Tools/Transform/Group children by prefab %&g")]
    public static void GroupChildrenByPrefab()
    {
        List<GameObject> prefabs = new List<GameObject>();
        List<Transform> objectsToGroup = new List<Transform>();
        
        // the object to sort the children of is the current selection
        GameObject GO = Selection.activeGameObject;
        if (!GO) return;
        
        Undo.SetCurrentGroupName("group children by prefab");
        int group = Undo.GetCurrentGroup();

        // Find the prefabs of all children
        foreach (Transform child in GO.transform)
        {
            GameObject prefab = PrefabUtility.GetPrefabParent(child.gameObject) as GameObject;
            if (!prefab) continue;
            if (prefabs.Contains(prefab)) continue;
            prefabs.Add(prefab);
        }
        
        
        // Group the instances of each prefab found
        foreach (GameObject prefab in prefabs)
        {
            objectsToGroup.Clear();
            
            foreach (Transform child in GO.transform)
            {
                GameObject myPrefab = PrefabUtility.GetPrefabParent(child.gameObject) as GameObject;
                
                if (myPrefab != prefab) continue;
                
                objectsToGroup.Add(child.transform);
            }
            
            if (objectsToGroup.Count > 1)
                GroupObjects(objectsToGroup, prefab.name);
        }
        
        // Collapse all the undo operations into a single operation
        Undo.CollapseUndoOperations(group);
    }


    /// <summary>
    /// To validate the 'group selection' command, there must be a selection of 
    /// at least one item. 
    /// </summary>
    /// <returns>True if current selection is greater than 0</returns>
    [MenuItem("Tools/Transform/Group selection %g", validate = true)]
    public static bool ValidateGroupSelection()
    {
        return (Selection.gameObjects.Length > 0);
    }



    /// <summary>
    /// You can use Selection.transforms to get access to all the transforms. 
    /// The pivot will be the point half way between min and max X, Y, and Z positions. 
    /// You can pass Selection.transforms to this method and get back the pivot position.
    /// <para>http://answers.unity3d.com/questions/501003/get-the-center-point-of-current-selection-in-edito.html</para>
    /// </summary>
    /// <param name="trans">List of transforms to find the center of.</param>
    /// <returns></returns>
    static Vector3 FindThePivot (Transform [] trans)
    {
        if (trans == null || trans.Length == 0)
            return Vector3.zero;
        if (trans.Length == 1)
            return trans [0].position;

        float minX = Mathf.Infinity;
        float minY = Mathf.Infinity;
        float minZ = Mathf.Infinity;

        float maxX = -Mathf.Infinity;
        float maxY = -Mathf.Infinity;
        float maxZ = -Mathf.Infinity;

        foreach (Transform tr in trans)
        {
            if (tr.position.x < minX)
                minX = tr.position.x;
            if (tr.position.y < minY)
                minY = tr.position.y;
            if (tr.position.z < minZ)
                minZ = tr.position.z;

            if (tr.position.x > maxX)
                maxX = tr.position.x;
            if (tr.position.y > maxY)
                maxY = tr.position.y;
            if (tr.position.z > maxZ)
                maxZ = tr.position.z;
        }

        return new Vector3((minX + maxX) / 2.0f, (minY + maxY) / 2.0f, (minZ + maxZ) / 2.0f);
    }
}
