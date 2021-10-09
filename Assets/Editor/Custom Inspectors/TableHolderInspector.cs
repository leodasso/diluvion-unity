using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Diluvion.Roll;

//[CustomEditor(typeof(TableHolder))]
/*
[CanEditMultipleObjects]
public class TableHolderInspector : Editor
{

    SerializedProperty tableList;
    SerializedProperty addTable;
    TableHolder theTable;

    void OnEnable()
    {
     
        tableList = serializedObject.FindProperty("tables");
        addTable = serializedObject.FindProperty("addTable");
        theTable = (TableHolder)target;
        theTable.GetAllTypes();
      
    }

    int elementDivision = 3;
    void DrawTypes()
    {
        EditorGUILayout.BeginHorizontal();
        int typeCount = 0;
        //Debug.Log("Drawing " + theTable.containedTypes.Count + " Types");
        foreach(SerializableSystemType t in theTable.containedTypes)
        {
            typeCount++;
            DrawType(t);
            if (typeCount % elementDivision == 0)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }        
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawType(SerializableSystemType type)
    {
        //Debug.Log("trying to draw the type " + type.Name);
        EditorGUILayout.LabelField(type.Name, GUILayout.MaxWidth(Screen.width / elementDivision+1));
        GUI.Box(GUILayoutUtility.GetLastRect(), "");
    }

    int oldListLength = 0;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.LabelField("Contained Types: ");
        EditorGUILayout.BeginVertical(GUILayout.Width(Screen.width));
        DrawTypes();
        EditorGUILayout.EndVertical();
        GUI.Box(GUILayoutUtility.GetLastRect(), "");
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(Screen.width / 3));
            EditorGUILayout.LabelField("Add One Entry>>", GUILayout.MaxWidth(Screen.width / 6));
            EditorGUILayout.PropertyField(addTable, GUIContent.none, GUILayout.MaxWidth(Screen.width / 6));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        if (addTable.objectReferenceValue != null)
        {
            theTable.tables.Add((Table)addTable.objectReferenceValue);
         
        }

        EditorGUILayout.LabelField("Searching Rollers will grab the first relevant Table");
        EditorGUILayout.PropertyField(tableList, new GUIContent("Tables: "),true );
        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
        GUI.Box(GUILayoutUtility.GetLastRect(), "");
     
        if (tableList.arraySize != oldListLength)
        {
            oldListLength = tableList.arraySize;
            addTable.objectReferenceValue = null;
            theTable.GetAllTypes();
        }
        serializedObject.ApplyModifiedProperties();
    }
    
}
*/