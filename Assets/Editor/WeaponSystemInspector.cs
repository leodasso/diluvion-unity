using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Diluvion.Ships;
#pragma warning disable 618

//[CustomEditor(typeof(WeaponSystem))]
public class WeaponSystemInspector : Editor {

    WeaponSystem ws;

	void OnEnable()
    {
        ws = target as WeaponSystem;
    }

    void OnSceneGUI()
    {
        serializedObject.Update();

        float size = HandleUtility.GetHandleSize(ws.transform.position);

        // Set the matrix to the local transform
        Matrix4x4 rotMatrix = Matrix4x4.TRS(ws.transform.position, ws.transform.rotation, Vector3.one);
        Handles.matrix = rotMatrix;

        Handles.color = Color.magenta;

        Vector3 aimDir = ws.CleanAimPosition() - ws.transform.position;

        Vector3 aimPos = Handles.FreeMoveHandle(aimDir * size, Quaternion.identity, size * .3f, Vector3.one * .1f, Handles.CircleCap);
        Handles.ArrowCap(0, aimPos, Quaternion.LookRotation(aimPos), size);
        //ws.aim = aimPos.normalized;

        Handles.color = Color.white;

        serializedObject.ApplyModifiedProperties();
    }
}
