// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.DevToolsEditor;
using FluffyUnderware.Curvy;

namespace FluffyUnderware.CurvyEditor
{
    [CustomEditor(typeof(MetaCGOptions))]
    [CanEditMultipleObjects]
    public class MetaCGOptionsEditor : DTEditor<MetaCGOptions>
    {

        [DrawGizmo(GizmoType.Active|GizmoType.NonSelected|GizmoType.InSelectionHierarchy)]
        static void MetaGizmoDrawer(MetaCGOptions data, GizmoType context)
        {
            if (CurvyGlobalManager.ShowMetadataGizmo && data.Spline.ShowGizmos)
            {
                if (data.HardEdge)
                {
                    Vector3 p = data.ControlPoint.position;
                    p.y += HandleUtility.GetHandleSize(p) * 0.4f;
                    Handles.Label(p, "<b><color=\"#660000\">^</color></b>",DTStyles.BackdropHtmlLabel);
                }
                if (data.MaterialID!=0)
                    Handles.Label(data.ControlPoint.Spline.ToWorldPosition(data.ControlPoint.Interpolate(0.5f)), "<b><color=\"#660000\">" + data.MaterialID.ToString() + "</color></b>", DTStyles.BackdropHtmlLabel);
                

            }
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();
            
        }

        void CBSetFirstU()
        {
            if (!Target.UVEdge && !Target.HasDifferentMaterial && GUILayout.Button("From Neighbours"))
            {
                if (Target.ControlPoint.IsFirstVisibleControlPoint)
                    Target.FirstU = 0;
                else
                {
                    CurvySplineSegment ccwRef;
                    CurvySplineSegment cwRef;
                    var ccwOptions = Target.GetPreviousDefined(out ccwRef);
                    var cwOptions = Target.GetNextDefined(out cwRef);
                    

                    if (!cwRef)
                        Target.FirstU = 1;
                    else
                    {
                        float frag=(Target.ControlPoint.Distance - ccwRef.Distance) / (cwRef.Distance - ccwRef.Distance);
                        
                        Target.FirstU = Mathf.Lerp(cwOptions.GetDefinedFirstU(1),ccwOptions.GetDefinedSecondU(0),frag);
                    }
                }
                EditorUtility.SetDirty(target);
            }
        }
    }

    
}
