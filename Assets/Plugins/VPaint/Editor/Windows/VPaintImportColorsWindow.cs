using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Valkyrie.VPaint;

public class VPaintImportColorsWindow : VPaintWindowBase
{
	public override bool LockSelection () { return true; }
	
//	int layerToImport = -1;
	public override void OnValidatedGUI ()
	{
//		IVPaintable paintable = VPaint.GetIVPaintable(Selection.activeGameObject);
//		if(paintable != null)
//		{
//			var obj = paintable as UnityEngine.Object;
//			if(EditorUtility.IsPersistent(obj)) paintable = null;
//		}
//		if(paintable == VPaint.Instance.layerCache) paintable = null;
		
		VPaintGroup paintable = null;
		if(Selection.activeGameObject) paintable = Selection.activeGameObject.GetComponent<VPaintGroup>();
		
		GUIStyle style = GetWordWrappedLabel();
		GUILayout.Label("Select a VPaint Group to import into the currently selected layer stack.", style);
		
		GUILayout.Space(10);
		
		EditorGUILayout.ObjectField(new GUIContent("Import Target:"), paintable, typeof(VPaintGroup), true);
		
//		List<string> layerNames = new List<string>();
//		layerNames.Add("All Layers");
//		if(paintable != null) {
//			foreach(var l in paintable.GetLayerStack().layers) {
//				layerNames.Add(l.name);
//			}
//			GUI.enabled = true;
//		}else{
//			layerToImport = -1;
//			GUI.enabled = false;
//		}
//		layerToImport = EditorGUILayout.Popup("Import Layers", layerToImport+1, layerNames.ToArray())-1;
		
		GUILayout.FlexibleSpace();
		
		EditorGUILayout.BeginHorizontal();
		
		GUILayout.FlexibleSpace();
		
		GUI.enabled = paintable != null;
		if(GUILayout.Button("Import"))
		{
			Import(paintable);
		}
		
		EditorGUILayout.EndHorizontal();
		
		maxSize = new Vector2(400, 120);
	}
	
	void Import (VPaintGroup paintable)
	{
		foreach(var layer in paintable.GetLayerStack().layers)
			VPaint.Instance.layerStack.layers.Add(layer.Clone());
		VPaint.Instance.layerStack.Sanitize();
		VPaint.Instance.ReloadLayers();
	}
}