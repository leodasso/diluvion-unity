using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TileTool2DLayerDepthSort))]
public class TileTool2DLayerDepthSortEditor : Editor {
	
	public override void OnInspectorGUI() {
		DrawCustomInspector();
		DrawDefaultInspector();
	}

	public void DrawCustomInspector() {
		if (GUILayout.Button("Add sorting script to all children")) 			AddScripts();
		if (GUILayout.Button("Remove sorting script to all children")) 			RemoveScripts();
		if (GUILayout.Button("Depth sort all")) 								SortAll();
	}

	void SortAll() {
		TileTool2DLayerDepthSort t = (TileTool2DLayerDepthSort)target;
		for (int i = 0; i < t.transform.childCount; i++) {
			Transform c = t.transform.GetChild(i);
			TileTool2DDepthSort s = c.gameObject.GetComponent<TileTool2DDepthSort>();
			s.lastPos = 999999999;
			s.CacheComponents();
			s.SetSortingOrder();
			EditorUtility.SetDirty(s.cacheSpriteRenderer);
		}
		SceneView.RepaintAll();
	}

	void AddScripts() {
		TileTool2DLayerDepthSort t = (TileTool2DLayerDepthSort)target;
		t.AddSortToChildren();
	}

	void RemoveScripts() {
		TileTool2DLayerDepthSort t = (TileTool2DLayerDepthSort)target;
		for (int i = 0; i < t.transform.childCount; i++) {
			Transform c = t.transform.GetChild(i);
			Undo.DestroyObjectImmediate(c.gameObject.GetComponent<TileTool2DDepthSort>());
		}
	}
}