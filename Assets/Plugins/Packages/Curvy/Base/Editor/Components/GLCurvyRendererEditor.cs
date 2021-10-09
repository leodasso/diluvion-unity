// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.DevToolsEditor;
using FluffyUnderware.Curvy.Components;
using UnityEditor;
using FluffyUnderware.Curvy;
using UnityEditorInternal;
using System.Collections.Generic;

namespace FluffyUnderware.CurvyEditor.Components
{
    [CustomEditor(typeof(CurvyGLRenderer))]
    public class GLCurvyRendererEditor : DTEditor<CurvyGLRenderer>
    {
        bool ShowWarning;

        protected override void OnEnable()
        {
            base.OnEnable();
            ShowWarning = Target.GetComponent<Camera>() == null;
        }

        protected override void SetupArrayEx(DTFieldNode node, DevTools.ArrayExAttribute attribute)
        {
            node.ArrayEx.elementHeight = 23;
            node.ArrayEx.drawElementCallback = drawSlot;
        }

        
        void drawSlot(Rect rect, int index, bool isActive, bool isFocused)
        {
            var slot = Target.Splines[index];
            Rect r=new Rect(rect);
            r.height = 19;
            r.width = rect.width - 60;
            r.y += 2;
            slot.Spline = EditorGUI.ObjectField(r,slot.Spline, typeof(CurvySplineBase),true) as CurvySplineBase;
            r.x += r.width+2;
            r.width = 50;
            slot.LineColor = EditorGUI.ColorField(r, slot.LineColor);

            // Separator
            if (index > 0)
            {
                DTHandles.PushHandlesColor(new Color(0.1f, 0.1f, 0.1f));
                Handles.DrawLine(new Vector2(rect.xMin - 5, rect.yMin), new Vector2(rect.xMax + 4, rect.yMin));
                DTHandles.PopHandlesColor();
            }
        }

        List<CurvySplineBase> getDragAndDropSplines()
        {
            var res = new List<CurvySplineBase>();
            if (DragAndDrop.objectReferences.Length > 0)
            {
                foreach (Object o in DragAndDrop.objectReferences)
                {
                    if (o is GameObject)
                    {
                        var spl = ((GameObject)o).GetComponent<CurvySplineBase>();
                        if (spl)
                            res.Add(spl);
                    }
                }
            }
            return res;
        }

        public override void OnInspectorGUI()
        {
            if (ShowWarning)
            {
                EditorGUILayout.HelpBox("This component needs a GameObject with a camera component present!", MessageType.Error);
                return;
            }
            GUILayout.Box(new GUIContent("Drag & Drop Splines here!"), EditorStyles.miniButton, GUILayout.Height(32));
            Rect r = GUILayoutUtility.GetLastRect();

            base.OnInspectorGUI();

            var ev = Event.current;
            switch (ev.type)
            {
                case EventType.DragUpdated:
                    if (r.Contains(ev.mousePosition))
                    {
                        DragAndDrop.visualMode = (getDragAndDropSplines().Count > 0) ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                    }
                    break;
                case EventType.DragPerform:
                    var splinesToAdd = getDragAndDropSplines();
                    Undo.RecordObject(Target, "Add Spline to list");
                    foreach (var spl in splinesToAdd)
                        Target.Splines.Add(new GLSlotData(){Spline=spl});
                    break;
            }
        }

    }
}
