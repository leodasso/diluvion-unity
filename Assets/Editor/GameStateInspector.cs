using UnityEngine;
using UnityEditor;
using System.Collections;

/*
[CustomEditor(typeof(DiluvionGameState))]
public class GameStateInspector : Editor {

	DiluvionGameState gameState;

	SerializedProperty configContainer;

	// Use this for initialization
	void OnEnable () {
		gameState = (DiluvionGameState)target;

		configContainer = serializedObject.FindProperty("configContainer");
	}
	
	public override void OnInspectorGUI ()
	{
		//base.OnInspectorGUI ();

		EditorGUILayout.Space();

		if (configContainer.objectReferenceValue == null)
			EditorGUILayout.HelpBox("WARNING! You have to select a build config for game to work.", MessageType.Error);

		EditorGUILayout.PropertyField(configContainer, new GUIContent("Selected Config"));

		EditorGUILayout.Space();

		DisplayConfigInfo();

		serializedObject.ApplyModifiedProperties();
	}
		

	void DisplayConfigInfo() {

		// Properties of the game state
		if (configContainer == null) return;
		if (configContainer.objectReferenceValue == null) return;

		BuildConfigContainer refConfig = configContainer.objectReferenceValue as BuildConfigContainer;
		DiluvionConfigData configData = refConfig.configData;

		EditorGUILayout.LabelField("Selected Config Settings");
		EditorGUILayout.HelpBox("To modify these properties, do it on the referenced object itself. Select it by clicking the 'config container' reference.", MessageType.Info);

		GUI.color = Color.grey;

		EditorGUILayout.LabelField("Name: " + refConfig.name);
		EditorGUILayout.ToggleLeft("Use in editor: ", configData.useInEditor);
		EditorGUILayout.ToggleLeft("Don't load scenes: ", configData.dontLoadScenes);
		EditorGUILayout.ToggleLeft("Debug: ", configData.debug);
		EditorGUILayout.ToggleLeft("Use Debug Saves: ", configData.useDebugSaves);
		EditorGUILayout.ToggleLeft("Load Async: ", configData.loadAsync);
		EditorGUILayout.ObjectField(configData.defaultSave, typeof(DebugSave), false);

		GUI.color = Color.white;
	}
}
*/