using UnityEngine;
using UnityEditor;
using System.Collections;
using Quests;
using SpiderWeb;

/*
[CustomEditor(typeof(DQuest)), CanEditMultipleObjects]
public class DQuestInspector : Editor {

    DQuest dQuest;

    SerializedProperty locKey;
    SerializedProperty sequential;
    SerializedProperty title;
    SerializedProperty description;
    SerializedProperty prereqsToStart;
    SerializedProperty objectiveContainers;

    const string failedLoc = "no loc!";

    private void OnEnable()
    {
        dQuest = (DQuest)target;

        locKey = serializedObject.FindProperty("locKey");
        sequential = serializedObject.FindProperty("sequential");
        title = serializedObject.FindProperty("title");
        description = serializedObject.FindProperty("description");
        prereqsToStart = serializedObject.FindProperty("prereqsToStart");
        objectiveContainers = serializedObject.FindProperty("objectiveCs");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        string locTitle = dQuest.GetLocTitle();
        if (locTitle.Contains("_")) GUI.color = Color.red;
        EditorGUILayout.LabelField(locTitle);
        GUI.color = Color.white;
        string locDescr = dQuest.GetLocDescription();
        if (locDescr.Contains("_")) GUI.color = Color.red;
        EditorGUILayout.LabelField(locDescr, GUILayout.MinHeight(32));
        GUI.color = Color.white;

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(title);
        EditorGUILayout.PropertyField(description, GUILayout.MinHeight(60));

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(locKey);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Get text from Library")) Localize();
        if (GUILayout.Button("Add keys to Library")) dQuest.AddToKeyLib();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Check Status")) dQuest.CheckStatus();
        if (GUILayout.Button("Start Quest")) dQuest.StartQuest();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(prereqsToStart, true);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(sequential);
        EditorGUILayout.PropertyField(objectiveContainers, new GUIContent("objectives"), true);

        serializedObject.ApplyModifiedProperties();
    }

    void Localize()
    {
        title.stringValue = dQuest.GetLocTitle();
        description.stringValue = dQuest.GetLocDescription();
        EditorUtility.SetDirty(target);
    }
}
*/