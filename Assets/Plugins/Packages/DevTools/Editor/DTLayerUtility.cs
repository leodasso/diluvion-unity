// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public static class DTLayerUtility 
{
    #region ### Layers ###

    public static bool HasLayer(string name)
    {
        return (GetLayer(name, false) > -1);
    }

    public static int GetLayer(string name, bool createIfMissing=false)
    {
        int res = LayerMask.NameToLayer(name);
        if (createIfMissing && res == -1)
            return AddLayer(name);

        return res;
    }

    public static int AddLayer(string name)
    {
        SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = manager.FindProperty("layers");
        int found = LayerMask.NameToLayer(name);
        if (found > -1)
            return found;

        for (int i = 8; i < 32; i++)
        {
            SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
            if (sp != null && string.IsNullOrEmpty(sp.stringValue))
            {
                sp.stringValue = name;
                manager.ApplyModifiedProperties();
                return i;
            }
        }
        return -1;
    }

    public static void RemoveLayer(string name)
    {
        RemoveLayer(GetLayer(name));
    }

    public static void RemoveLayer(int index)
    {
        if (index == -1 || index<8 || index>31)
            return;
        SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = manager.FindProperty("layers");
        layersProp.GetArrayElementAtIndex(index).stringValue = string.Empty;
        manager.ApplyModifiedProperties();
    }

    public static bool LayerIsLocked(int index)
    {
        return (Tools.lockedLayers & index) == index;
    }

    public static void SetLayerLock(int index, bool locked)
    {
        if (locked)
            Tools.lockedLayers |= 1 << index;
        else
            Tools.lockedLayers &= ~ (1 << index);
    }

    public static bool LayerIsVisible(int index)
    {
        return (Tools.visibleLayers & index) == index;
    }

    public static void SetLayerVisibility(int index, bool visible)
    {
        if (visible)
            Tools.visibleLayers |= 1 << index;
        else
            Tools.visibleLayers &= ~ (1 << index);
    }

    #endregion

    #region ### Tags ###

    public static bool TagExists(string name)
    {
        List<string> DefaultTags = new List<string>() { "Untagged", "Respawn", "Finish", "EditorOnly", "MainCamera", "Player", "GameController" };
        if (DefaultTags.Contains(name))
            return true;

        SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = manager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(name))
                return true;
        }
        return false;
    }

    public static void AddTag(string name)
    {
        if (!TagExists(name))
        {
            SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = manager.FindProperty("tags");
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
            n.stringValue = name;
            manager.ApplyModifiedProperties();
        }

    }

    public static void RemoveTag(string name)
    {
        if (TagExists(name))
        {
            SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = manager.FindProperty("tags");
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
                if (t.stringValue.Equals(name))
                {
                    tagsProp.DeleteArrayElementAtIndex(i);
                    manager.ApplyModifiedProperties();
                    return;
                }
            }
        }
    }

    #endregion

}

public class TempLayerGroup
{
    GameObject[] mGameObjects = new GameObject[0];
    int mTempLayerIndex;
    int[] mLayers;
    int mLockState;
    int mVisState;
    bool mDelete;

    public TempLayerGroup(string layerName, params GameObject[] gameObjects)
    {
        if (gameObjects != null)
            mGameObjects = gameObjects;
        backupStates();
        mDelete = DTLayerUtility.HasLayer(layerName);
        mTempLayerIndex = DTLayerUtility.GetLayer(layerName, true);
        foreach (var o in mGameObjects)
            o.layer = mTempLayerIndex;
    }

    public void Rollback()
    {
        restoreStates();
        if (mDelete)
            DTLayerUtility.RemoveLayer(mTempLayerIndex);
    }

    public TempLayerGroup LockState(bool locked)
    {
        DTLayerUtility.SetLayerLock(mTempLayerIndex, locked);
        return this;
    }
    public TempLayerGroup VisibleState(bool visible)
    {
        DTLayerUtility.SetLayerVisibility(mTempLayerIndex, visible);
        return this;
    }
    public TempLayerGroup Solo()
    {
        Tools.visibleLayers = 1 << mTempLayerIndex;
        return this;
    }

    void backupStates()
    {
        mLayers = new int[mGameObjects.Length];
        for (int i = 0; i < mGameObjects.Length; i++)
            mLayers[i] = mGameObjects[i].layer;
        mLockState = Tools.lockedLayers;
        mVisState = Tools.visibleLayers;
    }

    void restoreStates()
    {
        for (int i = 0; i < mGameObjects.Length; i++)
            mGameObjects[i].layer = mLayers[i];
        Tools.lockedLayers = mLockState;
        Tools.visibleLayers = mVisState;
    }

}
