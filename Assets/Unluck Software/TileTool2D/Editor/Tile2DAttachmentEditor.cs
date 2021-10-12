using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(Tile2DAttachment))]
public class Tile2DAttachmentEditor : Editor {

	private string[] aOptions = new [] {
		""
		, "A"
		, "C"
		, "CE"
		, "CN"
		, "CS"
		, "CW"
		, "E"
		, "N"
		, "NE"
		, "NS"
		, "NW"
		, "S"
		, "SE"
		, "SW"
		, "W"
		, "WE"
		, "CNW"
		, "CNE"
		, "CSW"
		, "CSE"
		, "CNWE"
		, "CENS"
		, "CSWE"
		, "CWNS"
		, "CNSWE"
		, "CWE"
		, "CNS"
		, "CSN"
		, "CEW"
		, "CWNN"
		, "CNEE"
		, "CSWW"
		, "CESS"
		, "CNSS"
		, "CENN"
		, "CWSS"
		, "CSEE"
		, "SENW"
		, "SWNE"
		, "NESW"
		, "NWSE"
		, "CENSW"
		, "CNWES"
		, "CSWEN"
		, "CWNSE"
		, "CNWSE"
		, "CNESW"
	};
	
	public override void OnInspectorGUI() {
		DrawCustomInspector();
		DrawDefaultInspector();
	}

	public void DrawCustomInspector() {
		if (PrefabUtility.GetCorrespondingObjectFromSource(Selection.activeObject) != null) return;
		EditorGUILayout.BeginHorizontal("Box");
		GUIStyle lab = new GUIStyle();
		lab.richText = true;
		GUILayout.Label("Attachments are added to tiles when drawing", lab);

		EditorGUILayout.EndHorizontal();
		Tile2DAttachment myTarget = (Tile2DAttachment)target;
		myTarget.aIndex = EditorGUILayout.Popup("Attach to Tile", myTarget.aIndex, aOptions);

		if (GUI.changed && myTarget.aIndex > 0) {
			myTarget.replaceTile = aOptions[myTarget.aIndex];
			EditorUtility.SetDirty(myTarget);
		}



	}
}
