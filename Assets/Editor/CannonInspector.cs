using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Diluvion;
#pragma warning disable 618

[CustomEditor(typeof(Cannon), true), CanEditMultipleObjects]
public class CannonInspector : Editor {

    SerializedProperty exitPoint;
    SerializedProperty exitVector;

    Cannon c;

	void OnEnable()
    {
        c = target as Cannon;

        exitPoint = serializedObject.FindProperty("exitPoint");
        exitVector = serializedObject.FindProperty("exitVector");
    }

    void OnSceneGUI()
    {
        if (Selection.gameObjects.Length > 1) return;

        Handles.color = Color.green;

        // Set the matrix to the local transform
        Matrix4x4 rotMatrix = Matrix4x4.TRS(c.transform.position, c.transform.transform.rotation, c.transform.localScale);
        Handles.matrix = rotMatrix;

        float size1 = HandleUtility.GetHandleSize(c.exitPoint);

        EditorGUI.BeginChangeCheck();
        Vector3 pos = Handles.FreeMoveHandle(c.exitPoint, Quaternion.identity, .1f * size1, new Vector3(.2f, .2f, .02f), Handles.CubeCap); 
        if (EditorGUI.EndChangeCheck())
        {
            exitPoint.vector3Value = pos;
        }

        EditorGUI.BeginChangeCheck();
        Vector3 dir = Handles.FreeMoveHandle(c.exitVector * size1 + pos, Quaternion.identity, .2f * size1, new Vector3(.1f, .1f, .1f), Handles.CircleCap);
        Handles.ConeCap(1, dir, Quaternion.LookRotation(dir - pos), .1f * size1);
        Vector3 relDir = dir - pos;

        if (EditorGUI.EndChangeCheck())
        {
            exitVector.vector3Value = relDir;
        }

        Handles.DrawDottedLine(pos, dir, 2);

        exitVector.vector3Value = exitVector.vector3Value.normalized;

        Handles.color = new Color(0, .5f, 1, .1f);
        float radius = 5 * size1;
        Handles.DrawSolidArc(pos, Vector3.up, relDir, c.shotSpread, radius);
        Handles.DrawSolidArc(pos, Vector3.up, relDir, -c.shotSpread, radius);
        Handles.DrawSolidArc(pos, Vector3.right, relDir, c.shotSpread, radius);
        Handles.DrawSolidArc(pos, Vector3.right, relDir, -c.shotSpread, radius);

        Handles.Label(pos, new GUIContent("Exit point: " + pos));
        Handles.Label(dir, new GUIContent("Exit vector: " + dir));

        

        //serializedObject.ApplyModifiedProperties();
    }
}