using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class TileTool2DCanvas : MonoBehaviour
{

    [ReadOnly]
    public bool editable;

    [ReadOnly]
    public Transform parent;

    [ReadOnly]
    public Vector3 memPos;

    [ReadOnly]
    public Vector3 memEulers;

    [ReadOnly]
    public Vector3 memScale;

    [ButtonGroup]
    public void MemorizeTransform()
    {
        parent = transform.parent; 
        memPos = transform.localPosition;
        memEulers = transform.localEulerAngles;
        memScale = transform.localScale;

#if UNITY_EDITOR 
        EditorUtility.SetDirty(this);
#endif
    }
    [ButtonGroup]
    public void ReturnTransform()
    {
        transform.parent = parent;
        transform.localPosition = memPos;
        transform.localEulerAngles = memEulers;
        transform.localScale = memScale;
        editable = false;

        FrameSelection(false);
    }

    [Button]
    public void PosForEdit()
    {
        transform.parent = null;
        transform.position = transform.eulerAngles = Vector3.zero;
        transform.localScale = Vector3.one;
        editable = true;

        FrameSelection(true);
    }

    /// <summary>
    /// Frames this canvas in the scene view.
    /// </summary>
    /// <param name="flat">2D mode in the scene view on or off</param>
    void FrameSelection(bool flat)
    {
#if UNITY_EDITOR
        Selection.activeGameObject = gameObject;
        SceneView.lastActiveSceneView.in2DMode = flat;
        SceneView.lastActiveSceneView.FrameSelected();
#endif
    }
}
