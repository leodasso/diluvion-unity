#if UNITY_EDITOR
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
[ExecuteInEditMode]
public class Tile2DLayer : MonoBehaviour
{
	public bool autoHideChildObjects = true;
	[HideInInspector]
	public  int childCountSave;
	[HideInInspector]
	public int delayCounter;

	CompositeCollider2D thisCompositeCollider2D;


	public bool locked;

	public void EnableCompositeCollider2D() {
		locked = true;
		Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
		if (!thisCompositeCollider2D) thisCompositeCollider2D = gameObject.AddComponent<CompositeCollider2D>();
		transform.GetComponent<Rigidbody2D>().isKinematic = true;

		for (int i = 0; i < transforms.Length; i++) {
			Tile2D tile = transforms[i].GetComponent<Tile2D>();
			if (!tile) continue;
			if (!tile.cacheCollider.enabled) continue;
			tile.cacheCollider.usedByComposite = true;
		}
		thisCompositeCollider2D.generationType = CompositeCollider2D.GenerationType.Manual;
		thisCompositeCollider2D.GenerateGeometry();
		SceneView.RepaintAll();

	}
	public void DisableCompositeCollider2D() {
		locked = false;
		Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
		if (!thisCompositeCollider2D) thisCompositeCollider2D = gameObject.GetComponent<CompositeCollider2D>();
		if (!thisCompositeCollider2D) return;
		//DestroyImmediate(GetComponent<CompositeCollider2D>());
		//	DestroyImmediate(GetComponent<Rigidbody2D>());
		for (int i = 0; i < transforms.Length; i++) {
			Tile2D tile = transforms[i].GetComponent<Tile2D>();
			if (!tile) continue;
			if (!tile.cacheCollider.enabled) continue;
			tile.cacheCollider.usedByComposite = false;
		}
		thisCompositeCollider2D.GenerateGeometry();
		SceneView.RepaintAll();
	}

	public void HideChildren(bool hide = true) {
		Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
		for (int i = 0; i < transforms.Length; i++) {
			if (!transforms[i].GetComponent<Tile2D>()) continue;
			if (!hide) {
				transforms[i].gameObject.hideFlags = HideFlags.None;
			} else if (transforms[i] != transform) {
				if (transforms[i].gameObject.hideFlags != HideFlags.HideInHierarchy) transforms[i].gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.NotEditable | HideFlags.HideInInspector;
			}
		}
		EditorApplication.DirtyHierarchyWindowSorting();
	}

	public void CombineColliders() {

		if (!gameObject.GetComponent<PolygonCollider2D>()) gameObject.AddComponent<PolygonCollider2D>();
		PolygonCollider2D polyCollider = gameObject.GetComponent<PolygonCollider2D> ();
		if (!gameObject.GetComponent<PlatformEffector2D>()) gameObject.AddComponent<PlatformEffector2D>();
		Transform[]transforms= gameObject.GetComponentsInChildren<Transform>();
		polyCollider.pathCount = 0;
		Dictionary<Vector2, bool> points=new Dictionary<Vector2, bool> ();
		foreach (Transform transf in transforms) {
			Collider2D goCol = transf.GetComponent<Collider2D>();
			if (!goCol) continue;
			if (!goCol.enabled) continue;
			float x = transf.position.x;
			float y = transf.position.y;
			points.Add(new Vector2(x, y), true);
		}
		List<Vector2> em = new List<Vector2> (points.Keys);

		foreach (Vector2 k in em) {
			if (!points[k])
				continue;
			if (points.ContainsKey(k + Vector2.left) || points.ContainsKey(k + Vector2.down) || points.ContainsKey(k + Vector2.down + Vector2.left))
				continue;
			//only check tiles with nothing on its left or bottom
			Vector2[] next = { Vector2.left, Vector2.down, Vector2.right, Vector2.up };
			Vector2[] check = { Vector2.left + Vector2.down, Vector2.down, Vector2.zero, Vector2.left };
			int d = 1;
			Vector2 i = k;
			List<Vector2> corner = new List<Vector2> ();
			do {
				if (points.ContainsKey(i)) points[i] = false;
				if (points.ContainsKey(i + check[(d + 3) % 4])) {
					d = (d + 3) % 4;
					corner.Add(i);
				}//turn right
				else if (!points.ContainsKey(i + check[d])) {
					d = (d + 1) % 4;
					corner.Add(i);
				}//turn left
				i += next[d];
			} while (i != k);
			polyCollider.pathCount++;
			polyCollider.SetPath(polyCollider.pathCount - 1, corner.ToArray());
		}
	}

	void OnDisable() {
		EditorApplication.update -= Update;
	}
	public void OnEnable() {
		if (!Application.isPlaying) {
			childCountSave = delayCounter = 0;
			EditorApplication.update -= Update;
			EditorApplication.update += Update;
		}
	}

	public void AutoHideChildren() {
		if (!autoHideChildObjects && delayCounter == 0) {
			if (childCountSave != -1) HideChildren(false);
			childCountSave = -1;
			return;
		}
		delayCounter++;
		if (delayCounter < 200) return;
		delayCounter = 0;
		if (childCountSave >= transform.childCount) {
			childCountSave = transform.childCount;
			return;
		}
		childCountSave = transform.childCount;
		HideChildren(autoHideChildObjects);
	}

	void Update() {
		AutoHideChildren();
		//	EnableCompositeCollider2D();
	}
}


//DisableColliderOnAwake

[CustomEditor(typeof(Tile2DLayer))]

public class Tile2DLayerEditor : Editor
{
	void DisableColliders() {
		Tile2DLayer thisTarget = (Tile2DLayer)target;
		Transform[] transforms = thisTarget.gameObject.GetComponentsInChildren<Transform>();
		for (int i = 0; i < transforms.Length; i++) {
			Tile2D td = transforms[i].GetComponent<Tile2D>();
			Tile2DColliderDisabler tile = transforms[i].GetComponent<Tile2DColliderDisabler>();
			if(td) td.DisableColliderOnAwake();
			if (tile) {
				tile.DisableCollider();
			}
		}
	}


	private void EnableColliders() {
		Tile2DLayer thisTarget = (Tile2DLayer)target;
		Collider2D[] transforms = thisTarget.gameObject.GetComponentsInChildren<Collider2D>();
		for (int i = 0; i < transforms.Length; i++) {
			transforms[i].enabled = true;
		}
	}




	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		EditorGUILayout.HelpBox("Disables colliders", MessageType.Info);
		//if (GUILayout.Button("Disable editor colliders")) {
		//	DisableColliders();
		//}

		//if (GUILayout.Button("Enable editor colliders")) {
		//	EnableColliders();
		//}

		if (GUILayout.Button("Enable Composite Collider")) {
			DisableColliders();
			Tile2DLayer thisTarget = (Tile2DLayer)target;
			thisTarget.EnableCompositeCollider2D();
		}

		if (GUILayout.Button("Disable Composite Collider")) {
			EnableColliders();
			Tile2DLayer thisTarget = (Tile2DLayer)target;
			thisTarget.DisableCompositeCollider2D();
		}

		if (GUI.changed) {
		}
	}


}

#endif

