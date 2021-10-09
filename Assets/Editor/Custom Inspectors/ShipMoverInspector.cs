using UnityEngine;
using UnityEditor;
using Diluvion.Ships;

[CanEditMultipleObjects]
[CustomEditor(typeof(ShipMover))]
public class ShipMoverInspector : Editor
{
	ShipMover smi;
	Rigidbody myRigidbody;
	float inspectorPartition;

    SerializedProperty engine;
    SerializedProperty throttle;
	private Bounds shipBounds;

	public void OnEnable()
	{
		smi = (ShipMover)target;
		if(smi.GetComponent<Rigidbody>())
			myRigidbody = smi.GetComponent<Rigidbody>();

        engine = serializedObject.FindProperty("engine");
        throttle = serializedObject.FindProperty("throttle");
		/*smi.SaveBounds();
		shipBounds = smi.ShipBounds;*/
	}
    
	public override void OnInspectorGUI()	
	{
		serializedObject.Update();

		if (myRigidbody == null) {
			EditorGUILayout.HelpBox("Object needs a rigidbody for shipmover to work.", MessageType.Warning);
			return;
		}

        // Engine
        EditorGUILayout.PropertyField(engine);
        if (engine.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox("Insert an engine for this to work!", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space();

        // Throttle
        EditorGUILayout.PropertyField(throttle);

        // Show max speed
        EditorGUILayout.BeginHorizontal();
        if (engine.objectReferenceValue != null)
        {
            EditorGUILayout.LabelField("Max Speed: ");
            EditorGUILayout.LabelField(smi.MaxSpeedPerSecond() + "m / sec");
        }
		/*EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Width: "+ shipBounds.size.x);
		EditorGUILayout.LabelField("Height: "+ shipBounds.size.y);
		EditorGUILayout.LabelField("Length: "+ shipBounds.size.z);
		
		EditorGUILayout.EndHorizontal();*/

        serializedObject.ApplyModifiedProperties();
	}
}
