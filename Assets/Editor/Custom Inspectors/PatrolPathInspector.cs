using UnityEngine;
using System.Collections;
using UnityEditor;
using Diluvion;

[CanEditMultipleObjects]
[CustomEditor(typeof(PatrolPath))]
public class PatrolPathInspector : Editor {

	InteriorManager interiorManager;
    PatrolPath interiorPath;
	bool wayPointError = false;

	// Use this for initialization
	void OnEnable ()
    {	
		interiorPath = (PatrolPath)target;	
		wayPointError = false;
		interiorPath.GetAllWaypoints();

		foreach (WaypointNode cw in interiorPath.allWPS) {
			if (!cw.SetupCorrectly()) wayPointError = true;
			break;
		}
	}
	
	// Update is called once per frame
	public override void OnInspectorGUI ()
    {

        DrawDefaultInspector();
		EditorGUILayout.Space();
		EditorGUI.indentLevel++;

		if (wayPointError) {
			EditorGUILayout.HelpBox("One or more of your Waypoints has errors.  Open the " +
			                        "Waypoints foldout for more details.", MessageType.Error);
		}

		if (interiorPath.allWPS.Count < 1) {
			EditorGUILayout.HelpBox("There are no Waypoints in this interior.  This component will" +
				"be automatically removed on awake.", MessageType.Warning);
			return;
		}

		//foldout to show Waypoints
		interiorPath.foldout = EditorGUILayout.Foldout(interiorPath.foldout, "All Waypoints");
		if (interiorPath.foldout) {
			interiorPath.GetAllWaypoints();

			foreach (WaypointNode cw in interiorPath.allWPS) {
				EditorGUILayout.ObjectField(cw, typeof(WaypointNode), true);

				if (!cw.SetupCorrectly()) {
					ShowWarning2(cw.name);
				}

				if (cw.Neighbours == null) {
					ShowWarning(cw.name);
					continue;
				}

				if (cw.Neighbours.Count < 1) {
					ShowWarning(cw.name);
					continue;
				}
			}
		}

		EditorGUI.indentLevel--;

		EditorUtility.SetDirty(target);
	
	}

	void ShowWarning(string objName) {
		EditorGUI.indentLevel+=2;
		EditorGUILayout.HelpBox(objName + " has no connected Waypoints.", MessageType.Warning);
		EditorGUI.indentLevel-=2;
		EditorGUILayout.Space();
	}

	void ShowWarning2(string objName) {
		EditorGUI.indentLevel+=2;
		EditorGUILayout.HelpBox(objName + " Has broken connections.", MessageType.Warning);
		EditorGUI.indentLevel-=2;
		EditorGUILayout.Space();
	}
}
