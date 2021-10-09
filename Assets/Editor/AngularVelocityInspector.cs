using System.Collections;
using System.Collections.Generic;
using Diluvion.AI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[CustomEditor(typeof(AngularVelocityDrawer))]
public class AngularVelocityInspector : Editor
{
    private AngularVelocityDrawer avd;
    private Rigidbody rb;
    void OnEnable()
    {
        avd = target as AngularVelocityDrawer;
        rb = avd.GetComponent<Rigidbody>();
    }

    void OnSceneGUI()
    {
        serializedObject.Update();
        DrawTorques(avd.aiList);

    }


    void DrawTorques(List<AngularInstance> aiList)
    {
        Vector3 center = rb.worldCenterOfMass;
        float offsetPos = 0.3f;
        foreach (AngularInstance ai in aiList)
        {
            DrawTorque(ai.angularTorque, center, avd.transform.forward*offsetPos, ai.label, ai.myColor);
            offsetPos+=0.3f;
        }
    }
    
    void DrawTorque(Vector3 torque, Vector3 position, Vector3 startPosition, string label,  Color lineColor)
    {
        float size = HandleUtility.GetHandleSize(position);

        if (torque == Vector3.zero) return;
        
        //The normal is the direction of the spin
        Vector3 normalizedTorque = torque.normalized;
        Vector3 arcTop = Vector3.Cross(normalizedTorque, startPosition.normalized) * 4; 
                   
        float angle = TorqueLength(torque.magnitude);


        Handles.color = lineColor;
        Handles.DrawWireArc(position, normalizedTorque, -startPosition, angle, 4);
        Handles.Label(position+ -startPosition *2, new GUIContent(label+ "( " + torque + " )"));
        Handles.ArrowHandleCap(0, position + arcTop, Quaternion.LookRotation(startPosition.normalized), 1, EventType.Ignore);
      
    }

    float TorqueLength(float magnitude)
    {
        return magnitude * 15;
    }

}
