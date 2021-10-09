using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class VPaintModelImporter : AssetPostprocessor 
{

	public void OnPostprocessModel (GameObject go) 
	{
		var path = assetPath;
		EditorApplication.delayCall += 
		()=>{		
			go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
			
			var allVpaintObjects = GameObject.FindObjectsOfType(typeof(VPaintObject));
			var meshFilters = go.GetComponentsInChildren<MeshFilter>(true);
			var meshes = new List<Mesh>();
			foreach(var mf in meshFilters) meshes.Add(mf.sharedMesh);
			
			bool doReload = false;
			foreach(var obj in allVpaintObjects)
			{
				var vpaintObject = obj as VPaintObject;
				if(vpaintObject.originalMesh && meshes.Contains(vpaintObject.originalMesh))
				{
					vpaintObject.ResetInstances();
					vpaintObject.GetMeshInstance();
					doReload = true;
				}
			}
			if(doReload)
			{
				if(VPaint.Instance) VPaint.Instance.ReloadLayers();
			}
			
			if(VPaint.Instance) VPaint.Instance.BuildObjectInfo();
		};
	}
	
}
