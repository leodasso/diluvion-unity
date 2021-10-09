using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Diluvion;
using Diluvion.SaveLoad;
using Supyrb;

[CustomEditor(typeof(GameManager))]
public class GameManagerInspector : Editor {

    List<FileInfo> saveFiles = new List<FileInfo>();
    string[] saveFileNames;
    int index = 0;
    GameManager gameManager;

    SerializedProperty saveFile;
    SerializedProperty currentState;
    SerializedProperty currentZone;
    SerializedProperty gameMode;
    SerializedProperty buildSettings;
    SerializedProperty rewiredInputManager;
    SerializedProperty musicSettings;
    SerializedProperty wWiseGlobal;
    SerializedProperty saveData;
    SerializedProperty poolPrefab;
    SerializedProperty saveAs;
    SerializedProperty newSaveName;
    SerializedProperty travelCompass;

    SerializedProperty loadZone;
    SerializedProperty loadCosmeticScenes;
    SerializedProperty debug;
    

    void OnEnable()
    {
        gameManager = (GameManager)target;

        saveFile = serializedObject.FindProperty("saveFile");
        currentState = serializedObject.FindProperty("currentState");
        gameMode = serializedObject.FindProperty("gameMode");
        rewiredInputManager = serializedObject.FindProperty("rewiredInputManager");
        wWiseGlobal = serializedObject.FindProperty("wwiseBanks");
        musicSettings = serializedObject.FindProperty("musicGlobals");
        currentZone = serializedObject.FindProperty("currentZone");
        saveData = serializedObject.FindProperty("saveData");
        poolPrefab = serializedObject.FindProperty("poolPrefab");
        saveAs = serializedObject.FindProperty("saveAs");
        newSaveName = serializedObject.FindProperty("newSaveName");
        travelCompass = serializedObject.FindProperty("travelCompassPrefab");
        buildSettings = serializedObject.FindProperty("buildSettings");
        loadZone = serializedObject.FindProperty("loadZone");
        loadCosmeticScenes = serializedObject.FindProperty("loadCosmeticScenes");
        debug = serializedObject.FindProperty("debug");

        // Get a list of all the save file infos
        saveFiles.Clear();
        saveFiles = DSave.GetSaveFileInfo();

        // Create a list of the save file names
        saveFileNames = new string[saveFiles.Count];
        for (int j = 0; j < saveFiles.Count; j++)
        {
            string newName = Path.GetFileNameWithoutExtension(saveFiles[j].Name);
            saveFileNames[j] = newName;
            if (newName == saveFile.stringValue) index = j;
        }
    }
    bool _allowOverWrite;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        PropertyFieldWarning(buildSettings);
        PropertyFieldWarning(gameMode);
        PropertyFieldWarning(rewiredInputManager);
        PropertyFieldWarning(wWiseGlobal);
        PropertyFieldWarning(musicSettings);
        PropertyFieldWarning(poolPrefab);
        EditorGUILayout.PropertyField(travelCompass);
        
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(debug);
        EditorGUILayout.PropertyField(loadZone);
        EditorGUILayout.PropertyField(loadCosmeticScenes);
        
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Game state", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(currentState);
        EditorGUILayout.PropertyField(currentZone);
        
        
        EditorGUILayout.Space();

        if (EditorApplication.isPlaying)
        {

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Freeze")) GameManager.Freeze(gameManager);
            if (GUILayout.Button("Un-freeze")) GameManager.UnFreeze(gameManager);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Starts the game using the save data in the 'save file' slot.", MessageType.Info);
            if (GUILayout.Button("Begin Selected File"))
            {
                GameManager.LoadSaveData();
                GameManager.BeginGame(GameManager.Get().saveData);
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Starts a new game using the 'starting save' in the current mode..", MessageType.Info);
            if (GUILayout.Button("Begin New Game"))
            {
                GameManager.LoadNewGame();
                GameManager.BeginGame(GameManager.Get().saveData);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Enter play mode to view more testing options.", MessageType.Info);
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        
        EditorGUILayout.BeginHorizontal();
        // create a dropdown menu to choose a file to load from
        if (saveFileNames.Length > 0)
        {
            EditorGUI.BeginChangeCheck();
            index = EditorGUILayout.Popup(index, saveFileNames);
            if (EditorGUI.EndChangeCheck()) {
                saveFile.stringValue = saveFileNames[index];
            }
        }
        if (GUILayout.Button("Load")) GameManager.LoadSaveData();
        
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(saveAs);
        if (saveAs.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(newSaveName);
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();

        _allowOverWrite = EditorGUILayout.Toggle("Allow Editing", _allowOverWrite);
        if(!_allowOverWrite)
            serializedObject.ApplyModifiedProperties();
        
        if (_allowOverWrite)
            EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(saveData, true);

        if (_allowOverWrite)
        {
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("SAVE CHANGED FILE"))
            {
                serializedObject.targetObject.GetType().GetMethod("DebugSave").Invoke(serializedObject.targetObject, null);
                //System.Object mySaveObject = saveData.objectReferenceValue as System.Object;// SerializedPropertyExtensions.GetFieldOrPropertyValue<DiluvionSaveData>("saveData", serializedObject, true);
                /* DiluvionSaveData mysaveData = 
     Debug.Log("Savedataname " + mySaveData.saveFileName);
     DSave.Save(mySaveData, mySaveData.saveFileName);*/
            }
           

        }
    }

    void PropertyFieldWarning(SerializedProperty property)
    {
        EditorGUILayout.PropertyField(property);
        if (!property.objectReferenceValue)
            EditorGUILayout.HelpBox("Enter the " + property.name + " object!", MessageType.Error);
    }
}
