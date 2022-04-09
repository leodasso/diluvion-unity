using UnityEngine;
using UnityEditor;
using Diluvion;

[CustomEditor(typeof(Torpedo), true)]
public class TorpedoInspector : Editor {

    Torpedo t;
    Munition m;

    void OnEnable()
    {
        t = target as Torpedo;
    }

	
    void OnSceneGUI()
    {
        serializedObject.Update();

        // Draw target position
        if (t.target)
        {
            float tSize = HandleUtility.GetHandleSize(t.target.position);
            // Handles.CircleCap(0, t.target.position, Quaternion.identity, tSize);
			Handles.CircleHandleCap(0, t.target.position, Quaternion.identity, tSize, EventType.MouseDown);
        }
        string details = "Turn ability: " + t.TurningPower() + " / 1\n";
        details += "Velocity: " + t.ForwardVelocity() + " / " + t.driveSpeed + "\n";
        details += "Success chance: " + t.calibration + "\n";

        Handles.Label(t.transform.position, new GUIContent(details), EditorStyles.textArea);
    }
}
