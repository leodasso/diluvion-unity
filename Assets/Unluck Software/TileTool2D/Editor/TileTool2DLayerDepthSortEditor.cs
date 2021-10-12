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
		if (GUILayout.Button("Remove sorting script on all children")) 			RemoveScripts();
		if (GUILayout.Button("Depth sort all")) 								SortAll();
		if (GUILayout.Button("Scatter All"))									ScatterAll();
		if (GUILayout.Button("Reset Scatter All"))								ResetScatterAll();


	}

	void ScatterAll(){
		TileTool2DLayerDepthSort t = (TileTool2DLayerDepthSort)target;
		t.AddSortToChildren();

		for (int i = 0; i < t.transform.childCount; i++) {
			Transform c = t.transform.GetChild(i);
			TileTool2DDepthSort s = c.gameObject.GetComponent<TileTool2DDepthSort>();
			if (s.resetPosition == Vector3.zero) s.resetPosition = s.transform.position;
			s.transform.position = new Vector2(s.transform.position.x + Random.Range(-.1f, .1f), s.transform.position.y + Random.Range(-.1f, .1f));
			EditorUtility.SetDirty(s.transform);
		}
	}

	void ResetScatterAll() {
		TileTool2DLayerDepthSort t = (TileTool2DLayerDepthSort)target;
		t.AddSortToChildren();

		for (int i = 0; i < t.transform.childCount; i++) {
			Transform c = t.transform.GetChild(i);
			TileTool2DDepthSort s = c.gameObject.GetComponent<TileTool2DDepthSort>();
			if (s.resetPosition != Vector3.zero)
				s.transform.position = s.resetPosition;
			s.resetPosition = Vector3.zero;
			EditorUtility.SetDirty(s.transform);
		}
	}

	void SortAll() {
		TileTool2DLayerDepthSort t = (TileTool2DLayerDepthSort)target;
		t.AddSortToChildren();

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
			DestroyImmediate(c.gameObject.GetComponent<TileTool2DDepthSort>());
		}
	}
}