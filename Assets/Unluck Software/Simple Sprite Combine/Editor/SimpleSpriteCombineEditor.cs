/****************************************
	Simple Sprite Combine
	Copyright 2016 Unluck Software	
 	www.chemicalbliss.com
*****************************************/

using UnityEngine;
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Sprites;

[CustomEditor(typeof(SimpleSpriteCombine))]
public class SimpleSpriteCombineEditor : Editor
{
	public SimpleSpriteCombine sTarget;
	GUIStyle buttonStyle;
	GUIStyle buttonStyle2;
	GUIStyle infoStyle;

	public void OnEnable() {
		sTarget = ((SimpleSpriteCombine)target);
	}

	public SpriteRenderer[] FindSpriteRenderers() {
		SpriteRenderer[] renderers = null;
		renderers = ((SimpleSpriteCombine)target).transform.GetComponentsInChildren<SpriteRenderer>();
		return renderers;
	}

	public void ReleaseSprites() {
		if (sTarget.combined) DestroyImmediate(sTarget.combined);
		ToggleRenderers(true);
		sTarget.editorOnlyTagged = false;
	}

	public void CopyExcludedFromCombine() {
		SpriteRenderer[] renderers = FindSpriteRenderers();
		if (sTarget.copyTargetExcluded) DestroyImmediate(sTarget.copyTargetExcluded);
		sTarget.copyTargetExcluded = new GameObject();
		sTarget.copyTargetExcluded.transform.parent = sTarget.copyTarget.transform;
		sTarget.copyTargetExcluded.name = "Excluded [SSC Clones]";
		for (int i = 0; i < renderers.Length; i++) {
			if (CheckExcludeFromCombine(renderers[i].gameObject)) {
				GameObject g = Instantiate(renderers[i].gameObject);
				g.transform.parent = sTarget.copyTargetExcluded.transform;
			}
		}
		sTarget.copyTargetExcluded.transform.localPosition = Vector3.zero;
	}

	public bool CheckExcludeFromCombine(GameObject go, bool checkPos = false) {
		if (sTarget.excludeFromCombine == null) return false;
		for (int i = 0; i < sTarget.excludeFromCombine.Length; i++) {
			if (checkPos && go.transform.localPosition == sTarget.excludeFromCombine[i].transform.localPosition) {
				return true;
			} else if (go == sTarget.excludeFromCombine[i]) {
				return true;
			}
		}
		return false;
	}



	public void CombineSprites() {
		SpriteRenderer[] renderers = FindSpriteRenderers();
		if (sTarget.combined) DestroyImmediate(sTarget.combined);
		Material m = null;
		if (sTarget.useCutoutShader)
			m = new Material(Shader.Find("Unlit/Transparent Cutout Vertex Colored"));
		else
			m = new Material(Shader.Find("Sprites/Default"));
		GameObject combineGameObject = new GameObject();
		combineGameObject.name = "" + sTarget.transform.name + " Combined [SSC]";
		combineGameObject.gameObject.AddComponent<MeshFilter>();
		combineGameObject.gameObject.AddComponent<MeshRenderer>();
		combineGameObject.transform.parent = sTarget.transform;
		combineGameObject.transform.SetSiblingIndex(1);
		sTarget.combined = combineGameObject.gameObject;
		Mesh meshSprites = new Mesh();
		CombineInstance[] combineInstace = new CombineInstance[renderers.Length];
		sTarget.oldZPositions = new float[renderers.Length];
		for (int i = 0; i < combineInstace.Length; i++) {
			EditorUtility.DisplayProgressBar("Combining", "" + i + " / " + combineInstace.Length, (float)i / combineInstace.Length);
			sTarget.oldZPositions[i] = renderers[i].transform.position.z;
			if (!CheckExcludeFromCombine(renderers[i].gameObject)) {
				combineInstace[i].mesh = ConvertSprite(renderers[i]);
				combineInstace[i].transform = renderers[i].transform.localToWorldMatrix;
			} else {
				if (renderers[i].gameObject.tag == "EditorOnly") renderers[i].gameObject.tag = "Untagged";
				combineInstace[i].mesh = new Mesh();
				combineInstace[i].transform = sTarget.transform.localToWorldMatrix;
			}
			renderers[i].transform.position = new Vector3(renderers[i].transform.position.x, renderers[i].transform.position.y, sTarget.oldZPositions[i]);
		}
		EditorUtility.ClearProgressBar();
		ToggleRenderers(false);
		m.SetTexture("_MainTex", SpriteUtility.GetSpriteTexture(renderers[0].sprite, false));
		combineGameObject.GetComponent<MeshFilter>().sharedMesh = meshSprites;
		combineGameObject.GetComponent<Renderer>().material = m;
		meshSprites.Clear();
		meshSprites.CombineMeshes(combineInstace);
		combineGameObject.GetComponent<Renderer>().sortingOrder = renderers[0].sortingOrder;
		combineGameObject.GetComponent<Renderer>().sortingLayerName = renderers[0].sortingLayerName;
		sTarget.vCount = meshSprites.vertexCount;
		if (sTarget.vCount > 65536) {
			Debug.LogWarning("Vertex Count: " + sTarget.vCount + "- Vertex Count too high, please divide mesh combine into more groups. Max 65536 for each mesh");
			sTarget._canGenerateLightmapUV = false;
		} else {
			sTarget._canGenerateLightmapUV = true;
		}
	}

	public Mesh ConvertSprite(SpriteRenderer spriteRenderer) {
		Sprite sprite = spriteRenderer.sprite;
		spriteRenderer.transform.position = new Vector3(spriteRenderer.transform.position.x, spriteRenderer.transform.position.y, spriteRenderer.transform.position.z + (spriteRenderer.sortingOrder * sTarget.sortingOrderToZPositionMultiplier + sTarget.sortingOrderToZPositionOffset));
		Mesh newMesh = new Mesh ();
		Vector3[] meshVerts = new Vector3[sprite.vertices.Length];
		Color[] colors = new Color[sprite.vertices.Length];
		Vector3[] normals = new Vector3[sprite.vertices.Length];
		for (int i = 0; i < sprite.vertices.Length; i++) meshVerts[i] = sprite.vertices[i];
		newMesh.vertices = meshVerts;
		for (int i = 0; i < sprite.vertices.Length; i++) {
			colors[i] = spriteRenderer.color;
			normals[i] = new Vector3(0, 0, -1);
		}
		newMesh.colors = colors;
		newMesh.normals = normals;
		newMesh.uv = SpriteUtility.GetSpriteUVs(sprite, false);
		int[] meshIndicies = new int[sprite.triangles.Length];
		for (int i = 0; i < meshIndicies.Length; i++) meshIndicies[i] = sprite.triangles[i];
		newMesh.SetIndices(meshIndicies, MeshTopology.Triangles, 0);
		newMesh.hideFlags = HideFlags.HideAndDontSave;
		return newMesh;
	}

	public void TagForExclusion(bool exclude) {
		SpriteRenderer[] renderers = FindSpriteRenderers();
		for (int i = 0; i < renderers.Length; i++) {
			if (!CheckExcludeFromCombine(renderers[i].gameObject) || !exclude) {
				if (exclude) {
					sTarget.editorOnlyTagged = true;
					renderers[i].transform.tag = "EditorOnly";
				} else {
					sTarget.editorOnlyTagged = false;
					renderers[i].transform.tag = "Untagged";
				}
			}
		}
	}



	public void ToggleRenderers(bool enable) {
		SpriteRenderer[] renderers = FindSpriteRenderers();
		for (int i = 0; i < renderers.Length; i++) {
			if (!CheckExcludeFromCombine(renderers[i].gameObject)) {
				renderers[i].enabled = enable;
			}
		}
	}


	void DestroyAllCombined() {
		Undo.RegisterFullObjectHierarchyUndo(sTarget.gameObject, "Destroy All Combined Objects");
		SpriteRenderer[] renderers = FindSpriteRenderers();
		for (int i = 0; i < renderers.Length; i++) {
			if (!CheckExcludeFromCombine(renderers[i].gameObject)) {
				GameObject.DestroyImmediate(renderers[i].gameObject);
			}
		}
	}


	public void ToggleColliders(bool enable) {
		SpriteRenderer[] renderers = FindSpriteRenderers();
		for (int i = 0; i < renderers.Length; i++) {
			if (!CheckExcludeFromCombine(renderers[i].gameObject)) {
				Collider2D collider= renderers[i].GetComponent<Collider2D>();
				if (collider) collider.enabled = enable;
			}
		}
	}


	public void GUISetup() {
		//if (buttonStyle != null) return;
		buttonStyle = new GUIStyle(GUI.skin.button);
		buttonStyle2 = new GUIStyle(GUI.skin.button);
		buttonStyle.fixedWidth = 150;
		buttonStyle.fixedHeight = 35;
		buttonStyle.fontSize = 15;
		infoStyle = new GUIStyle(GUI.skin.label);
		infoStyle.fontSize = 10;
		infoStyle.margin.top = 0;
		infoStyle.margin.bottom = 0;
	}

	void center() {
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
	}

	void centerEnd() {
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
	}

	public override void OnInspectorGUI() {
		GUISetup();
		//
		// EDITOR ONLY WARNING
		//
		if (!UnityEngine.Application.isPlaying) {
			GUI.enabled = true;
		} else {
			GUILayout.Label("Editor can't combine in play-mode", infoStyle);
			GUI.enabled = false;
		}
		GUILayout.Space(15.0f);
		//
		// COMBINE RELEASE BUTTONS
		//
		if (sTarget.combined == null) {
			center();
			if (GUILayout.Button("Combine", buttonStyle)) {
				CombineSprites();
				if (!sTarget.combined) return;
				sTarget.combined.isStatic = true;
				sTarget.combined.AddComponent<MeshSpriteSorting>();

			}
			centerEnd();

			sTarget.useCutoutShader = EditorGUILayout.Toggle("Use Cutout shader", sTarget.useCutoutShader);
			GUILayout.Space(15.0f);
			GUILayout.Label("Set sprite and mesh Z position\nbased on sprite sorting order.");
			sTarget.sortingOrderToZPositionMultiplier = EditorGUILayout.FloatField("Z Position Multiplier", sTarget.sortingOrderToZPositionMultiplier);
			sTarget.sortingOrderToZPositionOffset = EditorGUILayout.FloatField("Z Position Offset", sTarget.sortingOrderToZPositionOffset);
		} else {
			center();
			if (GUILayout.Button("Release", buttonStyle)) {
				ReleaseSprites();
			}
			centerEnd();
			GUILayout.Space(15.0f);

			if (GUILayout.Button("Generate Lightmap UVs")) {
				Unwrapping.GenerateSecondaryUVSet(sTarget.combined.GetComponent<MeshFilter>().sharedMesh);
			}
		}
		GUILayout.Space(5.0f);
		//
		// COMBINED FUNCTIONALITY
		//
		if (sTarget.combined != null) {
			if (!sTarget._canGenerateLightmapUV) {
				GUILayout.Label("Warning: Mesh has too high vertex count", EditorStyles.boldLabel);
				GUI.enabled = false;
			}
			GUILayout.Space(5.0f);
			if (sTarget.editorOnlyTagged) {
				GUI.color = Color.cyan;
			}
			if (GUILayout.Button("Tag Sprites \"EditorOnly\"", buttonStyle2)) {
				if (EditorUtility.DisplayDialog("Tag Sprites \"EditorOnly\"",
			"Tag all sprite gameobjects with \"EditorOnly\"? \n\nGameobjects and and their components like colliders will be IGNORED when building the scene.\n(Does not apply to excluded objects)"
			, "Yes", "No I need tags")) {
					TagForExclusion(true);
				}

			}
			GUI.color = Color.white;
			GUILayout.Space(5.0f);
			if (GUILayout.Button("Tag Sprites \"Untagged\"", buttonStyle2)) {
				if (EditorUtility.DisplayDialog("Tag Sprites \"Untagged\"",
			"Tag all sprite gameobjects with \"Untagged\"? \n\nGameobjects and their components like colliders will be INCLUDED when building the scene."
			, "Yes", "No I need tags")) {
					TagForExclusion(false);
				}
			}
			GUILayout.Space(20.0f);
			if (sTarget.combined.GetComponent<MeshFilter>().sharedMesh.name != "") {
				GUI.enabled = false;
			} else if (!UnityEngine.Application.isPlaying) {
				GUI.enabled = true;
			}
			if (GUILayout.Button("Save Mesh", buttonStyle2)) {
				string path = SaveFile("Assets/", sTarget.transform.name + " [SSC Mesh]", "asset");
				if (path != null && path != "") {
					UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(path, (Type)typeof(object));
					if (asset == null) {
						Debug.Log(path);
						AssetDatabase.CreateAsset(sTarget.combined.GetComponent<MeshFilter>().sharedMesh, path);
					} else {
						((Mesh)asset).Clear();
						EditorUtility.CopySerialized(sTarget.combined.GetComponent<MeshFilter>().sharedMesh, asset);
						AssetDatabase.SaveAssets();
					}
					sTarget.combined.GetComponent<MeshFilter>().sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(path, (Type)typeof(object));

					Debug.Log("Saved mesh asset: " + path);
				}
			}
			GUI.color = Color.white;
			GUILayout.Space(5.0f);
			if (!UnityEngine.Application.isPlaying) {
				GUI.enabled = true;
			}
			if (GUILayout.Button("Save Material", buttonStyle2)) {
				string path = SaveFile("Assets/", sTarget.transform.name + " [SSC Material]", "mat");
				if (path != null && path != "") {
					UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath(path, (Type)typeof(object));
					if (asset == null) {
						AssetDatabase.CreateAsset(sTarget.combined.GetComponent<Renderer>().sharedMaterial, path);
					} else {
						EditorUtility.CopySerialized(sTarget.combined.GetComponent<Renderer>().sharedMaterial, asset);
						AssetDatabase.SaveAssets();
					}
					sTarget.combined.GetComponent<Renderer>().sharedMaterial = (Material)AssetDatabase.LoadAssetAtPath(path, (Type)typeof(object));
					if (sTarget.copyTarget) sTarget.copyTargetMesh.GetComponent<Renderer>().sharedMaterial = (Material)AssetDatabase.LoadAssetAtPath(path, (Type)typeof(object));
					Debug.Log("Saved material asset: " + path);
				}
			}
			GUILayout.Space(5.0f);
			if (sTarget.toggleCollidersStatus) {
				if (GUILayout.Button("Disable Colliders", buttonStyle2)) {
					ToggleColliders(false);
					sTarget.toggleCollidersStatus = false;
				}
			} else {
				if (GUILayout.Button("Enable Colliders", buttonStyle2)) {
					ToggleColliders(true);
					sTarget.toggleCollidersStatus = true;
				}
			}
			GUILayout.Space(5.0f);
			if (GUILayout.Button("Copy Colliders >> Self", buttonStyle2)) {
				CopyColliders(true);
			}
			GUILayout.Space(20.0f);
		}
		if (!UnityEngine.Application.isPlaying) {
			GUI.enabled = true;
		}
		if (sTarget.combined != null) {
			string bText = "Create Clone";
			if (sTarget.combined.GetComponent<MeshFilter>().sharedMesh.name == "") {
				bText = bText + " (Save mesh first)";
				GUI.enabled = false;
			} else if (!UnityEngine.Application.isPlaying) {
				GUI.enabled = true;
			}
			if (GUILayout.Button(bText, buttonStyle2)) {
				GameObject newCopy = new GameObject();
				GameObject newCopy2 = new GameObject();
				newCopy2.transform.parent = newCopy.transform;
				newCopy2.transform.localPosition = sTarget.combined.transform.localPosition;
				newCopy2.transform.localRotation = sTarget.combined.transform.localRotation;
				newCopy.name = sTarget.name + " Clone [SSC]";
				newCopy2.name = "Mesh Clone [SSC]";
				newCopy.transform.position = sTarget.transform.position;
				newCopy.transform.rotation = sTarget.transform.rotation;
				MeshFilter mf = newCopy2.AddComponent<MeshFilter>();
				newCopy2.AddComponent<MeshRenderer>();
				mf.sharedMesh = sTarget.combined.GetComponent<MeshFilter>().sharedMesh;
				sTarget.copyTarget = newCopy;
				sTarget.copyTargetMesh = newCopy2;
				CopyMaterials(newCopy2.transform);
				CopyColliders();
				Selection.activeTransform = newCopy.transform;

			}
			GUILayout.Space(5.0f);
			if (!sTarget.copyTarget) {
				GUI.enabled = false;
			} else if (!UnityEngine.Application.isPlaying) {
				GUI.enabled = true;
			}
			if (GUILayout.Button("Copy Colliders >> Clone", buttonStyle2)) {
				CopyColliders();
			}
			GUILayout.Space(5.0f);
			if (GUILayout.Button("Copy Materials >> Clone", buttonStyle2)) {
				CopyMaterials(sTarget.copyTargetMesh.transform);
			}

			if (!UnityEngine.Application.isPlaying) {
				GUI.enabled = true;
			}
			sTarget.destroyOldColliders = EditorGUILayout.Toggle("Destroy old colliders", sTarget.destroyOldColliders);
			sTarget.keepStructure = EditorGUILayout.Toggle("Keep collider structure", sTarget.keepStructure);
			sTarget.copyTarget = (GameObject)EditorGUILayout.ObjectField("Copy to clone: ", sTarget.copyTarget, typeof(GameObject), true);
		}
		GUILayout.Space(15.0f);
		if (sTarget.combined) {
			GUI.enabled = false;
		}
		serializedObject.Update();
		SerializedProperty excludeProperty;
		excludeProperty = serializedObject.FindProperty("excludeFromCombine");
		EditorGUILayout.PropertyField(excludeProperty, new GUIContent("Exclude GameObjects From Combine"), true);
		serializedObject.ApplyModifiedProperties();
		if (sTarget.excludeFromCombine != null && sTarget.excludeFromCombine.Length == 0) EditorGUILayout.LabelField("(Lock inspector to drag many at once.)");
		GUILayout.Space(5.0f);
		if (!sTarget.copyTarget) {
			GUI.enabled = false;
		} else if (!UnityEngine.Application.isPlaying) {
			GUI.enabled = true;
		}
		GUILayout.Space(5.0f);
		if (sTarget.excludeFromCombine != null && sTarget.excludeFromCombine.Length > 0 && sTarget.combined) {
			if (GUILayout.Button("Copy Excluded >> Clone", buttonStyle2)) {
				CopyExcludedFromCombine();
			}
		}
		GUILayout.Space(5.0f);
		if (sTarget.combined != null) {
			GUILayout.Label("Vertex count: " + sTarget.vCount + " / 65536", infoStyle);
		} else {
			GUILayout.Label("Vertex count: - / 65536", infoStyle);
		}

		GUI.enabled = true;

		GUILayout.Space(15.0f);
		if (GUILayout.Button("Destroy All Combined Objects", buttonStyle2)) {
			DestroyAllCombined();
		}

		if (GUI.changed) {
			EditorUtility.SetDirty(sTarget);
		}


	}

	public void DestroyComponentsExeptColliders(Transform t) {
		Component[] transforms = t.GetComponentsInChildren(typeof(Transform));
		foreach (Transform trans in transforms) {
			if (trans != null && trans.parent != sTarget.transform && trans.name == "Colliders Copy [SSC]") DestroyImmediate(trans.gameObject);
			else if (trans != null && !sTarget.keepStructure && trans.transform.parent != t && trans.transform != t && (trans.GetComponent(typeof(Collider2D)) != null)) {
				trans.transform.name = "" + GetParentStructure(t, trans.transform);
				trans.transform.parent = t;
			}
		}
		Component[] components = t.GetComponentsInChildren(typeof(Component));
		foreach (Component comp in components) {
			if ((comp is Collider2D) && !((Collider2D)comp).enabled) {
				//		((Collider2D)comp).enabled = true;
				DestroyImmediate(comp.gameObject);
				
			}else
			if (!(comp is Collider2D) && !(comp is Transform)) {
				DestroyImmediate(comp);
			}
		}
	}

	public string GetParentStructure(Transform root, Transform t) {
		Transform ct = t;
		string s = "";
		while (ct != root) {
			s = s.Insert(0, ct.name + " - ");
			ct = ct.parent;
		}
		s = s.Remove(s.Length - 3, 3);
		return s;
	}

	public void DestroyEmptyGameObjects(Transform t) {
		Component[] components = t.GetComponentsInChildren(typeof(Transform));
		foreach (Transform comp in components) {
			if ((comp != null) && (comp.childCount == 0 || !CheckChildrenForColliders(comp))) {
				Collider2D col = (Collider2D)comp.GetComponent(typeof(Collider2D));
				if (col == null || CheckExcludeFromCombine(comp.gameObject, true)) {
					DestroyImmediate(comp.gameObject);
				}
			}
		}
	}
	public bool CheckChildrenForColliders(Transform t) {
		Component[] components = t.GetComponentsInChildren(typeof(Collider2D));
		if (components.Length > 0) {
			return true;
		}
		return false;
	}

	public void CopyMaterials(Transform t) {
		Renderer r = t.GetComponent<Renderer>();
		r.sharedMaterials = sTarget.combined.transform.GetComponent<Renderer>().sharedMaterials;
	}

	public void CopyColliders(bool copyToSelf = false) {
		GameObject clone;
		if (copyToSelf) clone = (GameObject)Instantiate(sTarget.gameObject, sTarget.transform.position, sTarget.transform.rotation);
		else clone = (GameObject)Instantiate(sTarget.gameObject, sTarget.copyTarget.transform.position, sTarget.copyTarget.transform.rotation);
		if (!copyToSelf && sTarget.destroyOldColliders) {
			if (sTarget.copyTargetColliders) {
				DestroyImmediate(sTarget.copyTargetColliders);
			}
		} else if (copyToSelf && sTarget.destroyOldColliders) {
			if (sTarget.selfColliders) {
				DestroyImmediate(sTarget.selfColliders);
			}
		}

		if (copyToSelf) {
			clone.transform.name = "Colliders Copy [SSC]";
			clone.transform.parent = sTarget.transform;
			sTarget.selfColliders = clone;
		} else {
			clone.transform.name = "Colliders Clone [SSC]";
			clone.transform.parent = sTarget.copyTarget.transform;
			sTarget.copyTargetColliders = clone;
		}
		clone.transform.SetSiblingIndex(2);
		DestroyComponentsExeptColliders(clone.transform);
		DestroyEmptyGameObjects(clone.transform);
	}

	public void ExportMesh(MeshFilter meshFilter, string folder, string filename) {
		string path = SaveFile(folder, filename, "obj");
		if (path != null) {
			StreamWriter sw = new StreamWriter(path);
			sw.Write(MeshToString(meshFilter));
			sw.Flush();
			sw.Close();
			AssetDatabase.Refresh();
			Debug.Log("Exported OBJ file to folder: " + path);
		}
	}

	public string MeshToString(MeshFilter meshFilter) {
		Mesh sMesh = meshFilter.sharedMesh;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("g ").Append(meshFilter.name).Append("\n");
		foreach (Vector3 vert in sMesh.vertices) {
			Vector3 tPoint = meshFilter.transform.TransformPoint(vert);
			stringBuilder.Append(String.Format("v {0} {1} {2}\n", -tPoint.x, tPoint.y, tPoint.z));
		}
		stringBuilder.Append("\n");
		foreach (Vector3 norm in sMesh.normals) {
			Vector3 tDir = meshFilter.transform.TransformDirection(norm);
			stringBuilder.Append(String.Format("vn {0} {1} {2}\n", -tDir.x, tDir.y, tDir.z));
		}
		stringBuilder.Append("\n");
		foreach (Vector3 uv in sMesh.uv) {
			stringBuilder.Append(String.Format("vt {0} {1}\n", uv.x, uv.y));
		}
		for (int material = 0; material < sMesh.subMeshCount; material++) {
			stringBuilder.Append("\n");
			int[] tris = sMesh.GetTriangles(material);
			for (int i = 0; i < tris.Length; i += 3) {
				stringBuilder.Append(String.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n", tris[i] + 1, tris[i + 1] + 1, tris[i + 2] + 1));
			}
		}
		return stringBuilder.ToString();
	}

	public string SaveFile(string folder, string name, string type) {
		string newPath = "";
		string path = EditorUtility.SaveFilePanel("Select Folder ", folder, name, type);
		if (path.Length > 0) {
			if (path.Contains("" + UnityEngine.Application.dataPath)) {
				string s = "" + path + "";
				string d = "" + UnityEngine.Application.dataPath + "/";
				string p = "Assets/" + s.Remove(0, d.Length);
				newPath = p;
				bool cancel = false;
				if (cancel) Debug.Log("Save file canceled");
			} else {
				Debug.LogError("Prefab Save Failed: Can't save outside project: " + path);
			}
		}
		return newPath;
	}

}
