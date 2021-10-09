// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.CurvyEditor;
using FluffyUnderware.Curvy;
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.DevToolsEditor.Extensions;
using FluffyUnderware.DevToolsEditor;
using FluffyUnderware.CurvyEditor.UI;
using System.Linq;
using UnityEditorInternal;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.CurvyEditor
{
    [CustomEditor(typeof(CurvySplineSegment)), CanEditMultipleObjects]
    public class CurvySplineSegmentEditor : CurvyEditorBase<CurvySplineSegment>
    {
        public static float SmoothingOffset = 0.3f;

        bool mValid;
        SerializedProperty tT0;
        SerializedProperty tB0;
        SerializedProperty tC0;
        SerializedProperty tT1;
        SerializedProperty tB1;
        SerializedProperty tC1;
        SerializedProperty tOT;
        SerializedProperty tOB;
        SerializedProperty tOC;
        
        
        Quaternion mBezierRot;

        EditorKeyBinding hkToggleBezierAutoHandles;

        CurvyConnectionEditor mConnectionEditor;

        

        protected override void OnEnable()
        {
            base.OnEnable();
            hkToggleBezierAutoHandles = CurvyProject.Instance.RegisterKeyBinding(new EditorKeyBinding("Bezier: Toggle AutoHandles", "", KeyCode.H));
            
            tT0 = serializedObject.FindProperty("m_StartTension");
            tC0 = serializedObject.FindProperty("m_StartContinuity");
            tB0 = serializedObject.FindProperty("m_StartBias");
            tT1 = serializedObject.FindProperty("m_EndTension");
            tC1 = serializedObject.FindProperty("m_EndContinuity");
            tB1 = serializedObject.FindProperty("m_EndBias");
            tOT = serializedObject.FindProperty("m_OverrideGlobalTension");
            tOC = serializedObject.FindProperty("m_OverrideGlobalContinuity");
            tOB = serializedObject.FindProperty("m_OverrideGlobalBias");
           
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;

            mBezierRot = Quaternion.identity;
        }

        

        protected override void OnDisable()
        {
            base.OnDisable();
            Editor.DestroyImmediate(mConnectionEditor);
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyWindowItemOnGUI;
            // just in case
            Tools.hidden = false;
        }

        protected override void OnReadNodes()
        {
            if (!mConnectionEditor)
            {
                // ensure all selected CPs join a single connection. Otherwise don't show Connections Inspector
                if (!serializedObject.FindProperty("m_Connection").hasMultipleDifferentValues && Target.Connection != null)
                {
                    mConnectionEditor = (CurvyConnectionEditor)Editor.CreateEditor(Target.Connection, typeof(CurvyConnectionEditor));
                    var sec = Node.AddSection("Connection", ConnectionGUI);
                    if (sec)
                    {
                        sec.SortOrder = 1;
                        sec.HelpURL = CurvySpline.DOCLINK + "curvyconnection";
                    }
                }
            }
        }

        void checkHotkeys()
        {
            if (Target && Target.Spline && Target.Spline.Interpolation == CurvyInterpolation.Bezier && hkToggleBezierAutoHandles.IsTriggered(Event.current))
                DTSelection.GetAllAs<CurvySplineSegment>().ForEach((CurvySplineSegment seg) => { seg.AutoHandles = !seg.AutoHandles; });
        }

        protected override void OnSceneGUI()
        {
            // Bounds
            if (CurvyGlobalManager.Gizmos.HasFlag(CurvySplineGizmos.Bounds))
            {
                DTHandles.PushHandlesColor(Color.gray);
                DTHandles.WireCubeCap(Target.Bounds.center, Target.Bounds.size);
                DTHandles.PopHandlesColor();
            }
            checkHotkeys();
            if (Target.Spline.RestrictTo2D && Tools.current==Tool.Move && !SceneView.currentDrawingSceneView.in2DMode)
            {
                Tools.hidden = true;
                Vector3 handlePos = (Tools.handlePosition != Target.transform.position) ? DTSelection.GetPosition() : Tools.handlePosition;                
                Vector3 delta;
                EditorGUI.BeginChangeCheck();

                
                if (CurvyProject.Instance.UseTiny2DHandles)
                    delta = DTHandles.TinyHandle2D(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive), handlePos, Target.Spline.transform.rotation, 1) - handlePos;
                else 
                    delta = DTHandles.PositionHandle2D(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive), handlePos, Target.Spline.transform.rotation, 1) - handlePos;
               
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (var t in Selection.transforms)
                    {
                        Undo.ClearUndo(t);
                        Undo.RecordObject(t, "Move");
                        t.position += delta;
                    }
                } 
            }
            else
               Tools.hidden = false;
            
            
            // Bezier-Handles
            if (Target.Spline.Interpolation == CurvyInterpolation.Bezier && !Target.AutoHandles)
            {
                #region --- Bezier Rotation Handling ---
                if (Tools.current == Tool.Rotate)
                {
                    Event e = Event.current;
                    if (e.shift && DTHandles.MouseOverSceneView)
                    {
                        Tools.hidden = true;
                        DTToolbarItem._StatusBar.Set("BezierRotate");
                        // This looks good, but then diff down below isn't working like intended:
                        //mBezierRot = Quaternion.LookRotation(Vector3.Cross(Target.HandleIn, Target.GetOrientationUpFast(0)), Target.GetOrientationUpFast(0));
                        Quaternion newRot = Handles.RotationHandle(mBezierRot, Target.transform.position);
                        if (newRot!=mBezierRot)
                        {
                            Quaternion diff = Quaternion.Inverse(mBezierRot) * newRot;
                            mBezierRot = newRot;
                            var mode = CurvyProject.Instance.BezierMode | CurvyBezierModeEnum.Direction;
                            Undo.RecordObject(Target, "Rotate Handles");
                            Target.SetBezierHandleIn(diff * Target.HandleIn, Space.Self, mode);
                            Target.SetDirty();
                            EditorUtility.SetDirty(Target);
                        }
                        
                    }
                    else
                    {
                        DTToolbarItem._StatusBar.Set("Hold <b>[Shift]</b> to rotate Handles", "BezierRotate");
                        Tools.hidden = false;
                    }
                }
                else
                    DTToolbarItem._StatusBar.Set("BezierRotate");
                #endregion

                #region --- Bezier Movement Handling ---
                //Belongs to Constraint Length:
                //Vector3 handleOut = Target.HandleOutPosition;
                //Vector3 handleIn = Target.HandleInPosition;

                EditorGUI.BeginChangeCheck();
                Vector3 pOut;
                Vector3 pIn;
                bool chgOut=false;
                bool chgIn=false;
                if (Target.Spline.RestrictTo2D)
                {
                    
                    Quaternion r = (Tools.pivotRotation == PivotRotation.Global) ? Target.Spline.transform.localRotation : Target.Spline.transform.rotation;
                    Handles.color = Color.yellow;
                    pIn = Target.HandleInPosition;
#if UNITY_5_6_OR_NEWER
                    pIn = DTHandles.TinyHandle2D(GUIUtility.GetControlID(FocusType.Passive), pIn, r, CurvyGlobalManager.GizmoControlPointSize * 0.7f, Handles.CubeHandleCap);
#else
                    pIn = DTHandles.TinyHandle2D(GUIUtility.GetControlID(FocusType.Passive), pIn, r, CurvyGlobalManager.GizmoControlPointSize * 0.7f, Handles.CubeCap);
#endif
                    if (!CurvyProject.Instance.UseTiny2DHandles)
                        pIn = DTHandles.PositionHandle2D(GUIUtility.GetControlID(FocusType.Passive), pIn, r, 1);
                    chgIn = Target.HandleInPosition != pIn;
                    Handles.color = Color.green;
                    pOut = Target.HandleOutPosition;
#if UNITY_5_6_OR_NEWER
                    pOut = DTHandles.TinyHandle2D(GUIUtility.GetControlID(FocusType.Passive), pOut, r, CurvyGlobalManager.GizmoControlPointSize * 0.7f, Handles.CubeHandleCap);
#else
                    pOut = DTHandles.TinyHandle2D(GUIUtility.GetControlID(FocusType.Passive), pOut, r, CurvyGlobalManager.GizmoControlPointSize * 0.7f, Handles.CubeCap);
#endif
                    if (!CurvyProject.Instance.UseTiny2DHandles)
                        pOut = DTHandles.PositionHandle2D(GUIUtility.GetControlID(FocusType.Passive), pOut, r, 1);

                    chgOut = Target.HandleOutPosition != pOut;
                        
                }
                else
                {
                    pIn = Handles.PositionHandle(Target.HandleInPosition, Tools.handleRotation);
                    chgIn = Target.HandleInPosition != pIn;
                    DTHandles.PushHandlesColor(Color.yellow);
#if UNITY_5_6_OR_NEWER
                    Handles.CubeHandleCap(0, pIn, Quaternion.identity, HandleUtility.GetHandleSize(pIn) * CurvyGlobalManager.GizmoControlPointSize * 0.7f,EventType.Repaint);
#else
                    Handles.CubeCap(0, pIn, Quaternion.identity, HandleUtility.GetHandleSize(pIn) * CurvyGlobalManager.GizmoControlPointSize * 0.7f);
#endif
                    DTHandles.PopHandlesColor();
                    pOut = Handles.PositionHandle(Target.HandleOutPosition, Tools.handleRotation);
                    chgOut = Target.HandleOutPosition!=pOut;
                    DTHandles.PushHandlesColor(Color.green);
#if UNITY_5_6_OR_NEWER
                    Handles.CubeHandleCap(0, pOut, Quaternion.identity, HandleUtility.GetHandleSize(pOut) * CurvyGlobalManager.GizmoControlPointSize * 0.7f,EventType.Repaint);
#else
                    Handles.CubeCap(0, pOut, Quaternion.identity, HandleUtility.GetHandleSize(pOut) * CurvyGlobalManager.GizmoControlPointSize * 0.7f);
#endif
                    DTHandles.PopHandlesColor();    
                }
                    
                Handles.color = Color.yellow;
                Handles.DrawLine(pIn, Target.transform.position);
                Handles.color = Color.green;
                Handles.DrawLine(pOut, Target.transform.position);


                if ( chgOut || chgIn)
                {
                    Undo.RecordObject(Target,"Move Handle");
                    if (chgOut)
                    {
                        Target.SetBezierHandleOut(pOut, Space.World, CurvyProject.Instance.BezierMode);
                        Target.HandleOut = DTMath.SnapPrecision(Target.HandleOut, 3);
                    }
                    else
                    {
                        Target.SetBezierHandleIn(pIn, Space.World, CurvyProject.Instance.BezierMode);
                        Target.HandleIn = DTMath.SnapPrecision(Target.HandleIn, 3);
                    }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(Target);
                    Target.SetDirty();
                    /*
                    var lcons = CurvyProject.Instance.FindItem<TBCPLengthConstraint>();
                    if (lcons.On && Target.Spline.Length > lcons.MaxSplineLength)
                    {
                        Target.HandleOutPosition = handleOut;
                        Target.HandleInPosition = handleIn;
                        Target.SetDirty();
                    }
                     */
                }
#endregion


            }
            if (mConnectionEditor)
                mConnectionEditor.OnSceneGUI();
            // Snap Transform
            if (Target && DT._UseSnapValuePrecision)
            {
                Target.transform.localPosition = DTMath.SnapPrecision(Target.transform.localPosition, 3);
                Target.transform.localEulerAngles = DTMath.SnapPrecision(Target.transform.localEulerAngles, 3);
                //Target.TTransform.FromTransform(Target.transform);
            }
        }

        void OnHierarchyWindowItemOnGUI(int instanceid, Rect selectionrect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceid) as GameObject;
            if (obj && obj == Selection.activeObject)
                    checkHotkeys();
        }


        void TCBOptionsGUI()
        {
            if (Target.Spline.Interpolation == CurvyInterpolation.TCB)
            {
                if (tOT.boolValue || tOC.boolValue || tOB.boolValue)
                {
                    EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button("Set Catmul"))
                    {
                        tT0.floatValue = 0; tC0.floatValue = 0; tB0.floatValue = 0;
                        tT1.floatValue = 0; tC1.floatValue = 0; tB1.floatValue = 0;
                    }
                    if (GUILayout.Button("Set Cubic"))
                    {
                        tT0.floatValue = -1; tC0.floatValue = 0; tB0.floatValue = 0;
                        tT1.floatValue = -1; tC1.floatValue = 0; tB1.floatValue = 0;
                    }
                    if (GUILayout.Button("Set Linear"))
                    {
                        tT0.floatValue = 0; tC0.floatValue = -1; tB0.floatValue = 0;
                        tT1.floatValue = 0; tC1.floatValue = -1; tB1.floatValue = 0;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        void ConnectionGUI(DTInspectorNode node)
        {
            if (mConnectionEditor != null && Target.Connection != null)
            {
                EditorGUILayout.BeginHorizontal();
                Target.Connection.name = EditorGUILayout.TextField("Name", Target.Connection.name);
                if (GUILayout.Button(new GUIContent("Select","Select the Connection GameObject")))
                    DTSelection.SetGameObjects(Target.Connection);
                EditorGUILayout.EndHorizontal();
                mConnectionEditor.OnInspectorGUI();
                
            }
        }

        protected override void OnCustomInspectorGUI()
        {

            GUILayout.Space(5);

            EditorGUILayout.HelpBox("Distance: " + Target.Distance +
                                   "\nLength: " + Target.Length +
                                   "\nCache Points: "+ Target.CacheSize +
                                   "\nSpline Length: " + Target.Spline.Length,
                                   MessageType.Info);
            checkHotkeys();
        }

        
        public override void OnInspectorGUI()
        {
            if (Target && Target.Connection && mConnectionEditor == null)
                ReadNodes();
            base.OnInspectorGUI();
        }

        void CBBakeOrientation()
        {
            if (Target && !Target.AutoBakeOrientation && GUILayout.Button("Bake Orientation to Transform"))
            {
                Target.BakeOrientation();
            }
            GUI.enabled = true;
        }
       
    }
}
