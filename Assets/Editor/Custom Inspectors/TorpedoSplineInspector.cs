using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(TorpedoSpline)), CanEditMultipleObjects]
public class TorpedoSplineInspector : Editor
{

    protected virtual void OnSceneGUI()
    {
        TorpedoSpline ts = (TorpedoSpline) target;

        if (!ts.editPositions) return;
        for (int i = 0; i < ts.middleCPPositions.Length; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newTargetPosition = Handles.PositionHandle(ts.middleCPPositions[i], Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, "Change Look At Target Position");
                ts.middleCPPositions[i] = newTargetPosition;
                EditorUtility.SetDirty(ts);
            }
        }
    }


}
