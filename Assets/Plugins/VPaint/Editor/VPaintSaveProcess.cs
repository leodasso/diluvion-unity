using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

[InitializeOnLoad]
public class VPaintSaveProcess : 
#if !UNITY_3_0 && !UNITY_3_1 && !UNITY_3_2 && !UNITY_3_3 && !UNITY_3_4 && !UNITY_3_5
	UnityEditor.
#endif
	AssetModificationProcessor 
{
	
	static VPaintSaveProcess ()
	{
		EditorApplication.update += TestSelection;
		TestSelection();
	}
	
	static UnityEngine.Object[] previousSelection;	
	static void TestSelection ()
	{
		if(previousSelection == null || !previousSelection.SequenceEqual(Selection.objects))
		{
			SelectionChanged();
			previousSelection = Selection.objects;
		}
	}
	
	static void SelectionChanged ()
	{
		var checkedGameObjects = new HashSet<GameObject>();
		
		foreach(var go in Selection.gameObjects)
		{			
			var prefabType = PrefabUtility.GetPrefabType(go);
			var parent = PrefabUtility.FindPrefabRoot(go);
			
			if(prefabType == PrefabType.PrefabInstance)
			{	
				var root = PrefabUtility.GetPrefabParent(parent) as GameObject;
				if(!checkedGameObjects.Contains(root))
				{
					CheckPrefab(root);
					checkedGameObjects.Add(root);
				}
			}
			
			if(prefabType == PrefabType.Prefab)
			{
				CheckPrefab(parent);
			}
		}
	}
	
	static void CheckPrefab (GameObject root)
	{
		var vpaintObjects = root.GetComponentsInChildren<VPaintObject>(true);
		foreach(var vpo in vpaintObjects)
		{
			if(vpo.GetComponent<MeshFilter>().sharedMesh != vpo.originalMesh)
				vpo.ResetInstances();
			
			if(vpo.editorCollider)
			{
				GameObject.DestroyImmediate(vpo.editorCollider.gameObject, true);
			}
		}
		
		var editorBehaviours = root.GetComponentsInChildren<VPaintEditorBehaviour>(true);
		foreach(var b in editorBehaviours)
		{
			GameObject.DestroyImmediate(b.gameObject, true);
		}
	}
	
	public static string[] OnWillSaveAssets (string[] paths)
	{
		bool isScene = false;
		foreach(var s in paths)
		{
			string ext = Path.GetExtension(s);
			if(ext == ".unity")
			{
				isScene = true;
				break;
			}
		}
		
		if(isScene)
		{
			PrepareScene();
		}
		
		if(VPaint.Instance) VPaint.Instance.dirty = false;
		
		return paths;
	}
	
	static void PrepareScene ()
	{		
		Dictionary<VPaintObject, Color[]> vcsToReset = new Dictionary<VPaintObject, Color[]>();
		var vcs = GameObject.FindObjectsOfType(typeof(VPaintObject));
		foreach(var obj in vcs)
		{
			var vc = obj as VPaintObject;
			if(vc._mesh)
			{
				vcsToReset.Add(vc, vc._mesh.colors);
				vc.ResetInstances();
			}
			if(vc.editorCollider)
			{
				GameObject.DestroyImmediate(vc.editorCollider.gameObject);
			}
		}
		var vertexEditor = VPaint.Instance;
		if(vertexEditor)
		{
			vertexEditor.Cleanup();
			EditorApplication.delayCall += ()=>{
				if(vertexEditor.enabled)
				{
					vertexEditor.ReloadLayers();
					if(vertexEditor.vertexColorPreviewEnabled) 
						vertexEditor.EnableVertexColorPreview();
//					vertexEditor.SetupColliders();
				}
			};
		}
		
		EditorApplication.delayCall += ()=>{
			foreach(var kvp in vcsToReset)
			{
				if(kvp.Key) kvp.Key.SetColors(kvp.Value);
			}
		};
	}
	
}
