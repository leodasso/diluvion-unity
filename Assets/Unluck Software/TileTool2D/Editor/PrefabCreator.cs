using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using System.Collections;
using System.IO;
#pragma warning disable 0618
public class PrefabCreator : EditorWindow {
	public static EditorWindow win;

	static bool createBoxCollider;

	[MenuItem("Window/TileTool2D/Prefab Creator")]
	public static void ShowWindow() {
		win = EditorWindow.GetWindow(typeof(PrefabCreator));
		win.titleContent = new GUIContent("Prefab Creator");
		win.minSize = new Vector2(200.0f, 200.0f);
	}

	void OnGUI() {
		GUIInstructionsMain();
		EditorGUILayout.BeginVertical("Box");
		GUIInstructionsSprites();
		GUIPrefabCreationSetup();
		GUIButtonCreateGameObjectsFromSprites();
		EditorGUILayout.EndVertical();
		GUI.enabled = true;
		EditorGUILayout.BeginVertical("Box");
		GUIInstructionsGameObjects();
		GUIButtonCreatePrefabsFromGameObjects();
		EditorGUILayout.EndVertical();
	}

	void GUIPrefabCreationSetup() {
		createBoxCollider = EditorGUILayout.Toggle("Create Box Collider", createBoxCollider);
	}

	void CreateNew(GameObject obj, String localPath) {
		UnityEngine.Object prefab  = PrefabUtility.CreateEmptyPrefab(localPath);
		PrefabUtility.ReplacePrefab(obj, prefab, ReplacePrefabOptions.ConnectToPrefab);
	}


	void GUIButtonCreatePrefabsFromGameObjects() {
		if (!Selection.activeTransform) GUI.enabled = false;
		else
			GUI.enabled = true;
		if (GUILayout.Button("Save Prefabs")) {

			if (Selection.transforms.Length > 500) {
				if (!EditorUtility.DisplayDialog("Warning", "Creating " + Selection.transforms.Length + " tiles will a very long time, please split into several folders for TileTool 2D to handle them all", "OK, I'll just grab a cup of coffee then"))
					return;
			}else if (Selection.transforms.Length > 100) {
				if (!EditorUtility.DisplayDialog("Warning", "Creating " + Selection.transforms.Length + " tiles will take several minutes", "OK"))
					return;
			}
			CreatePrefabsFromGameObjects();
		}
	}

	void GUIButtonCreateGameObjectsFromSprites() {
		if (Selection.activeTransform || Selection.objects.Length == 0) GUI.enabled = false;
		else if (Selection.objects[0].GetType() == typeof(Sprite))
			GUI.enabled = true;
		else
			GUI.enabled = false;
		if (GUILayout.Button("Create GameObjects")) CreateGameObjectsFromSprites();
	}

	static void CreateGameObjectsFromSprites() {
		for (int i = 0; i < Selection.objects.Length; i++) {
			Sprite s = Selection.objects[i] as Sprite;
			if (s != null) {
				CreateTile(s, createBoxCollider);
			} else {
				Debug.LogWarning(Selection.objects[i].name + " is not as sprite, ignoring the object");
			}
		}
	}

	public static Tile2D CreateTile(Sprite sprite, bool addCollider) {
		Tile2D t = null;
		GameObject go = new GameObject();
		SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
		go.AddComponent<Tile2D>();
		t = go.GetComponent<Tile2D>();
		if (sprite) {
			sr.sprite = sprite;
			Sprite[] sa = t.tileA = new Sprite[1];
			sa[0] = sprite;
			go.name = sprite.name;
		} else {
			go.name = "New Tile2D";
		}
		if (addCollider) {
			go.AddComponent<BoxCollider2D>();
		}
		EditorUtility.SetDirty(t);
		return t;
	}

	void GUIInstructionsMain() {
		GUI.enabled = true;
		GUI.color = Color.grey;
		EditorGUILayout.BeginHorizontal("Box");
		GUIStyle lab = new GUIStyle();
		lab.wordWrap = true;
		lab.richText = true;
		GUILayout.Label("This tool is designed to easily be able to create single TileTool2D tiles, without any autotiling functionality.\n\nIt is best to use this tool in a <b>new scene</b>.", lab);
		EditorGUILayout.EndHorizontal();
		GUI.color = Color.white;
	}

	void GUIInstructionsSprites() {
		GUI.enabled = true;
		EditorGUILayout.BeginHorizontal();
		GUIStyle lab = new GUIStyle();
		lab.wordWrap = true;
		lab.richText = true;
		GUILayout.Label("Select 1 or more <b>Sprites</b> in the <b>Project Window</b>\n", lab);
		EditorGUILayout.EndHorizontal();
	}

	void GUIInstructionsGameObjects() {
		GUI.enabled = true;
		EditorGUILayout.BeginHorizontal();
		GUIStyle lab = new GUIStyle();
		lab.wordWrap = true;
		lab.richText = true;
		GUILayout.Label("Select 1 or more <b>GameObjects</b> in the <b>Hierarchy Window</b>\n", lab);
		EditorGUILayout.EndHorizontal();
	}

	void Update() {
		Repaint();
		EditorUtility.ClearProgressBar();
	}

	void CreatePrefabsFromGameObjects() {
		if (Selection.gameObjects.Length > 0) {
			var path = EditorUtility.SaveFolderPanel("Select Folder ", "Assets/", "");
			if (path.Length > 0) {
				if (path.Contains("" + Application.dataPath)) {
					String s  = "" + path + "/";
					String d = "" + Application.dataPath + "/";
					String p = "Assets/" + s.Remove(0, d.Length);
					var _selection = Selection.transforms;
					bool cancel = false;
					for (int i = 0; i < _selection.Length; i++) {
						float x = i;
						
							if (EditorUtility.DisplayCancelableProgressBar(
				"Creating Prefabs",
				"",
				x / _selection.Length)) {
								EditorUtility.ClearProgressBar();
								return;
							} else { 
							if (!cancel) {
							if (AssetDatabase.LoadAssetAtPath(p + _selection[i].gameObject.name + ".prefab", typeof(GameObject))) {
								var option = EditorUtility.DisplayDialogComplex("Are you sure?", "" + _selection[i].gameObject.name + ".prefab" + " already exists. Do you want to overwrite it?", "Yes", "No", "Cancel");
								switch (option) {
									case 0:
									CreateNew(_selection[i].gameObject, p + _selection[i].gameObject.name + ".prefab");
									break;
									case 1:
									break;
									case 2:
									cancel = true;
									break;
									default:
									Debug.LogError("Unrecognized option.");
									break;
								}
							} else
								CreateNew(_selection[i].gameObject, p + _selection[i].gameObject.name + ".prefab");
						}
					}
					}
				} else {
					Debug.LogError("Prefab Save Failed: Can't save outside project: " + path);
				}
			}
		} else {
			Debug.LogWarning("No GameObjects Selected");
		}
		EditorUtility.ClearProgressBar();
	}
}