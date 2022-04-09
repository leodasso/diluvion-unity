using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PseudoVelocity))]
public class VelocityInspector : Editor
{
    PseudoVelocity v;
    
    void OnEnable()
    {
        v = target as PseudoVelocity;
    }

    void OnSceneGUI()
    {
        serializedObject.Update();

        DrawLinearVelocity(v.transform.position);

    }

    void DrawLinearVelocity(Vector3 position)
    {
        float size = HandleUtility.GetHandleSize(position);
        
        Vector3 velo = v.velocity;
        
        if (velo == Vector3.zero) return;
        
        Vector3 positionVelo = v.transform.position + v.velocity;
   
        if (velo.magnitude <= 0) return;
        
		Handles.ArrowHandleCap(0, positionVelo, Quaternion.LookRotation(velo.normalized), size * .5f, EventType.MouseDown);
        // Handles.ArrowCap(0, positionVelo, Quaternion.LookRotation(velo.normalized), size * .5f);
        Handles.DrawDottedLine(v.transform.position, positionVelo, 2);
        Handles.Label(positionVelo, new GUIContent("Velocity: " + velo));
    }
}
