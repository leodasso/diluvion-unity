//FixTheGap.cs (this does nothing, just attach it to any gameobject)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FixTheGap : MonoBehaviour
{
}

//FixTheGapEditor.cs 
//This script generates a PolygonCollider2d which covers all tiles with "Platform" tag.
[CustomEditor(typeof(FixTheGap))]

public class FixTheGapEditor : Editor
{
	string PlatformTag = "Respawn";

	public override void OnInspectorGUI() {
		serializedObject.Update();
		GUILayout.Label("Your Platform Prefab's Tag:");
		PlatformTag = GUILayout.TextField(PlatformTag);

		if (GUILayout.Button("Polygon Collider Generator")) {
			FixTheGap ts = ((FixTheGap)target);
			GameObject ob = ts.gameObject;
			Debug.Log(target.name);
			if (!ob.GetComponent<PolygonCollider2D>()) ob.AddComponent<PolygonCollider2D>();
			PolygonCollider2D p = ob.GetComponent<PolygonCollider2D> ();
			if (!ob.GetComponent<PlatformEffector2D>()) ob.AddComponent<PlatformEffector2D>();
			PlatformEffector2D pt = ob.GetComponent<PlatformEffector2D> ();
			GameObject[]objs= GameObject.FindGameObjectsWithTag (PlatformTag);

			pt.useOneWay = false;
			p.usedByEffector = true;
			p.pathCount = 0;
			Dictionary<Vector2, bool> points=new Dictionary<Vector2, bool> ();
			foreach (GameObject g in objs) {
				int x = Mathf.RoundToInt (g.transform.position.x);
				int y = Mathf.RoundToInt (g.transform.position.y);
				points.Add(new Vector2(x, y), true);
				//Debug.Log (new Vector2 (x, y));
			}
			List<Vector2> em = new List<Vector2> (points.Keys);

			foreach (Vector2 k in em) {
				Debug.Log(k + "?");
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
					Debug.Log(i + "!");
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
				p.pathCount++;
				p.SetPath(p.pathCount - 1, corner.ToArray());
			}
		}
	}
}