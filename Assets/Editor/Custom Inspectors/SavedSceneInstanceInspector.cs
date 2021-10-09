using UnityEngine;
using System.Reflection;
using UnityEditor;
using Diluvion.SaveLoad;

[CustomEditor(typeof(SavedInstance), true)]
public class SavedSceneInstanceInspector : Editor {

    SavedInstance instance;
    SerializedProperty myID;

    public void OnEnable()
    {
        if ( Application.isPlaying ) return;

        instance = (SavedInstance)target;

#if UNITY_EDITOR

        myID = serializedObject.FindProperty("myID");

        Debug.Log("Found property 'myID': " + myID.intValue);

        PropertyInfo inspectorModeInfo =
        typeof(SerializedObject).GetProperty("inspectorMode",
        BindingFlags.NonPublic | BindingFlags.Instance);

        
        //SerializedObject serializedObject = new SerializedObject(this);

        inspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);

        SerializedProperty localIdProp =
        serializedObject.FindProperty("m_LocalIdentfierInFile");

        Debug.Log ("found local ID prop: " + localIdProp.intValue);

        myID.intValue = localIdProp.intValue;

        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();

        //myID = localIdProp.intValue;
        //Important set the component to dirty so it won't be overriden from a prefab!
       // UnityEditor.EditorUtility.SetDirty(this);
#endif

        //instance.InitSavedInstance();
    }

    public override void OnInspectorGUI()
    {      
        DrawDefaultInspector();
    }
}
