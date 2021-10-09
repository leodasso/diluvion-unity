using UnityEngine;
using UnityEditor;
using RootMotion.FinalIK;
using System.Collections;
using Diluvion;


public class GCCControllerInspector : RotationLimitInspector
{

    GroundCreatureController gcc;
    Transform head;

    void OnEnable()
    {
        gcc = (GroundCreatureController)target;
        if (gcc.head != null)
            head = gcc.head.transform;
        else
            head = gcc.transform;

        ///THE RIGGED COMPONENTS OF THE CRAB HAVE -X AS FORWARD, SO THATS WHAT WE DO
    
    }


    void OnSceneGUI()
    {

        gcc.axis = -head.transform.right;
        Vector3 swing = gcc.axis.normalized;

        // Display limits
        lastPoint = head.transform.position;
        if(gcc.GetComponent<SphereCollider>())
            gcc.visionDistance = gcc.GetComponent<SphereCollider>().radius;
        for (int i = 0; i < 360; i += 2)
        {
            Quaternion offset = Quaternion.AngleAxis(i, swing);
            Quaternion limitedRotation = Quaternion.AngleAxis(gcc.visionCone, offset * gcc.crossAxis);

            Vector3 limitedDirection = Direction(limitedRotation * swing)*gcc.visionDistance;

            Handles.color = colorDefaultTransparent;

            Vector3 limitPoint = head.position + limitedDirection;

            if (i == 0) zeroPoint = limitPoint;

            Handles.DrawLine(head.position, limitPoint);

            if (i > 0)
            {
                Handles.color = colorDefault;
                Handles.DrawLine(limitPoint, lastPoint);
                if (i == 358) Handles.DrawLine(limitPoint, zeroPoint);
            }

            lastPoint = limitPoint;
        }

        Handles.color = Color.white;
    }

    private Vector3 lastPoint, zeroPoint;

    /*
     * Converting directions from local space to world space
     * */
    private Vector3 Direction(Vector3 v)
    {
        if (gcc.transform.parent == null) return head.transform.localRotation * v;
        return gcc.transform.parent.rotation * (head.transform.localRotation * v);
    }
    /* int iterations = Mathf.RoundToInt(gcc.visionDistance / 10f);

     Vector3 disc1Pos = head.position +  head.right * gcc.visionDistance;

     Vector3 angledRotation = Quaternion.AngleAxis(gcc.visionCone, head.transform.up) * -head.right;
     Vector3 projectedVector = Vector3.Project(head.transform.right * gcc.visionDistance, angledRotation);

     float coneMaxWidth = Vector3.Distance(disc1Pos, head.transform.position + projectedVector);
     float coneStepSize = coneMaxWidth / iterations;
     for (int i = 0; i < iterations; i++)
     {
         Handles.DrawWireDisc(head.position -(head.right * 10)* i, -head.transform.right, coneStepSize*i);
     }*/
}
