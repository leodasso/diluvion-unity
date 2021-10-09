// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy;
using UnityEditorInternal;
using FluffyUnderware.DevToolsEditor;
using System.Collections.Generic;

namespace FluffyUnderware.CurvyEditor
{
    [CustomEditor(typeof(CurvySplineGroup)), CanEditMultipleObjects]
    public class CurvySplineGroupEditor : CurvyEditorBase<CurvySplineGroup>
    {

        GUIStyle mLargeFont;

        ReorderableList mSplineList;
        

        protected override void OnEnable()
        {
            base.OnEnable();
            mLargeFont = new GUIStyle();
            mLargeFont.normal.textColor = new Color(0.8f, 0.8f, 1, 0.5f);
            mLargeFont.fontSize = 60;

        }

        
        public override void OnUndoRedo()
        {
            Target.SetDirtyAll();
        }

        protected override void OnSceneGUI()
        {
            var cam = SceneView.currentDrawingSceneView.camera;
            if (cam)
            {
                for (int i = 0; i < Target.Count; i++)
                    if (Target.Splines[i])
                        Handles.Label(DTHandles.TranslateByPixel(Target.Splines[i].transform.position, -15, 30), i.ToString(), mLargeFont);
            }
        }

        protected override void SetupArrayEx(DTFieldNode node, DevTools.ArrayExAttribute attribute)
        {
            mSplineList = node.ArrayEx;
            mSplineList.onChangedCallback = OnChanged;
        }
        
        void OnChanged(UnityEditorInternal.ReorderableList list)
        {
            Target.SetDirtyAll();
        }

        List<CurvySpline> getDragAndDropSplines()
        {
            var res = new List<CurvySpline>();
            if (DragAndDrop.objectReferences.Length > 0)
            {
                foreach (Object o in DragAndDrop.objectReferences)
                {
                    if (o is GameObject)
                    {
                        var spl = ((GameObject)o).GetComponent<CurvySpline>();
                        if (spl)
                            res.Add(spl);
                    }
                }
            }
            return res;
        }

        protected override void OnCustomInspectorGUI()
        {
            GUILayout.Space(5);
            EditorGUILayout.HelpBox("Total Length: " + Target.Length, MessageType.Info);
        }

        public override void OnInspectorGUI()
        {

            GUILayout.Box(new GUIContent("Drag & Drop Splines here!"), EditorStyles.miniButton, GUILayout.Height(32));
            Rect r = GUILayoutUtility.GetLastRect();

            base.OnInspectorGUI();
            
            var ev = Event.current;
            switch (ev.type)
            {
                case EventType.DragUpdated:
                    if (r.Contains(ev.mousePosition))
                    {
                        DragAndDrop.visualMode=(getDragAndDropSplines().Count>0) ? DragAndDropVisualMode.Copy: DragAndDropVisualMode.Rejected;
                    }
                    break;
                case EventType.DragPerform:
                    var splinesToAdd = getDragAndDropSplines();
                    Undo.RecordObject(Target, "Add Spline to Group");
                    int idx=Mathf.Clamp(mSplineList.index>-1 ? mSplineList.index+1:Target.Splines.Count,0,Target.Splines.Count);
                    Target.Splines.InsertRange(idx,splinesToAdd);
                    Target.SetDirtyAll();
                    mSplineList.index = idx;
                    break;
            }
        }


    }
}
