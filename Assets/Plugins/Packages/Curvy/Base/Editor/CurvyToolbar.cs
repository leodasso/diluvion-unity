// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.DevToolsEditor;
using UnityEditor;
using FluffyUnderware.Curvy;
using FluffyUnderware.DevTools.Extensions;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.Curvy.Shapes;
using FluffyUnderware.Curvy.Generator;
using System.Linq;
using FluffyUnderware.CurvyEditor.Generator;
using FluffyUnderware.Curvy.Generator.Modules;

namespace FluffyUnderware.CurvyEditor.UI
{
    [ToolbarItem(10,"Curvy","Options","Curvy Options","curvyicon_dark,24,24","curvyicon_light,24,24")] 
    public class TBOptions : DTToolbarToggleButton
    {

        public override string StatusBarInfo { get { return "Open Curvy Options menu"; } }


        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            SetElementSize(ref r, 32, 32);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconPrefs, "Preferences")))
            {
                DT.OpenPreferencesWindow();
                On = false;
            }
            Advance(ref r);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconHelp, "Online Manual")))
            {
                AboutWindow.OpenDocs();
                On = false;
            }
            Advance(ref r);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconWWW, "Curvy Website")))
            {
                AboutWindow.OpenWeb();
                On = false;
            }
            Advance(ref r);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconBugReporter, "Report Bug")))
            {
                CurvyEditorUtility.SendBugReport();
                On = false;
            }
            Advance(ref r);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconAbout, "About Curvy")))
                AboutWindow.Open();
                
            
            
        }

        public override void OnSelectionChange()
        {
            Visible = CurvyProject.Instance.ShowGlobalToolbar || DTSelection.HasComponent<CurvySplineBase, CurvySplineSegment, CurvyController,CurvyGenerator>(true);
        }
        
    }

    [ToolbarItem(12,"Curvy","View","View Settings","viewsettings,24,24")]
    public class TBViewSetttings : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Set Curvy Scene View Visiblity"; } }

     

        public override void OnSelectionChange()
        {
            Visible = CurvyProject.Instance.ShowGlobalToolbar || DTSelection.HasComponent<CurvySplineBase, CurvySplineSegment, CurvyController, CurvyGenerator>(true);
        }

        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            bool b;
            bool v;

            
            Background(r, 100, 150);
            SetElementSize(ref r, 100, 19);

            EditorGUI.BeginChangeCheck();
            b = (CurvyGlobalManager.Gizmos == CurvySplineGizmos.None);
            b = GUI.Toggle(r, b, "None");
            if (b)
                CurvyGlobalManager.Gizmos = CurvySplineGizmos.None;
            // Curve
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowCurveGizmo;
            v = GUI.Toggle(r, b, "Curve");
            if (b != v)
                CurvyGlobalManager.ShowCurveGizmo = v;
            // Approximation
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowApproximationGizmo;
            v = GUI.Toggle(r, b, "Approximation");
            if (b != v)
                CurvyGlobalManager.ShowApproximationGizmo = v;
            // Orientation
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowOrientationGizmo;
            v = GUI.Toggle(r, b, "Orientation");
            if (b != v)
                CurvyGlobalManager.ShowOrientationGizmo = v;
            // Tangents
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowTangentsGizmo;
            v = GUI.Toggle(r, b, "Tangents");
            if (b != v)
                CurvyGlobalManager.ShowTangentsGizmo = v;
            // UserValues
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowMetadataGizmo;
            v = GUI.Toggle(r, b, "Metadata");
            if (b != v)
                CurvyGlobalManager.ShowMetadataGizmo = v;
            // Labels
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowLabelsGizmo;
            v = GUI.Toggle(r, b, "Labels");
            if (b != v)
                CurvyGlobalManager.ShowLabelsGizmo = v;
            // Bounds
            AdvanceBelow(ref r);
            b = CurvyGlobalManager.ShowBoundsGizmo;
            v = GUI.Toggle(r, b, "Bounds");
            if (b != v)
                CurvyGlobalManager.ShowBoundsGizmo = v;

            if (EditorGUI.EndChangeCheck())
                CurvyProject.Instance.SavePreferences();
        }

    }

    [ToolbarItem(30, "Curvy", "Create", "Create", "add,24,24")]
    public class TBNewMenu : DTToolbarToggleButton
    {

        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            SetElementSize(ref r, 32, 32);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconNewShape, "Shape")))
            {
                CurvyMenu.CreateCurvySpline(new MenuCommand(Selection.activeGameObject));
                Project.FindItem<TBSplineSetShape>().OnClick();
                On = false;
            }
            Advance(ref r);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconNewGroup, "Group")))
            {
                var splines = DTSelection.GetAllAs<CurvySpline>();
                var grp = CurvySplineGroup.Create(splines.ToArray());
                Undo.RegisterCreatedObjectUndo(grp.gameObject, "Create Spline Group");
                DTSelection.SetGameObjects(grp);
                On = false;
            }
            Advance(ref r);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconNewCG, "Generator")))
            {
                var cg = new GameObject("Curvy Generator", typeof(CurvyGenerator)).GetComponent<CurvyGenerator>();
                Undo.RegisterCreatedObjectUndo(cg.gameObject, "Create Generator");
                if (cg)
                {
                    var parent=DTSelection.GetGameObject(false);
                    if (parent != null)
                    {
                        Undo.SetTransformParent(cg.transform, parent.transform, "Create Generator");
                        cg.transform.localPosition = Vector3.zero;
                        cg.transform.localRotation = Quaternion.identity;
                        cg.transform.localScale = Vector3.one;
                    }
                    // if a spline is selected, create an Input module
                    if (DTSelection.HasComponent<CurvySpline>()){
                        var mod=cg.AddModule<InputSplinePath>();
                        mod.Spline = DTSelection.GetAs<CurvySpline>();
                    }
                    DTSelection.SetGameObjects(cg);
                    CGGraph.Open(cg);
                }
                On = false;
            }
        }

        public override void OnSelectionChange()
        {
            Visible = CurvyProject.Instance.ShowGlobalToolbar || DTSelection.HasComponent<CurvySplineBase, CurvySplineSegment, CurvyController, CurvyGenerator>(true);
        }

    }

    [ToolbarItem(32,"Curvy","Draw Spline","Draw Splines","draw,24,24")]
    public class TBDrawControlPoints : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Create Splines and Control Points"; } }

        enum ModeEnum
        {
            None = 0,
            Add = 1,
            Pick = 2,
         
        }

        ModeEnum Mode = ModeEnum.None;

        bool usePlaneXY;
        bool usePlaneXZ=true;
        bool usePlaneYZ;

        CurvySplineSegment selCP;
        CurvySpline selSpline;

        public TBDrawControlPoints()
        {
            KeyBindings.Add(new EditorKeyBinding("Toggle Draw Mode", "",KeyCode.Space));
        }

      
        
        public override void HandleEvents(Event e)
        {
 	         base.HandleEvents(e);
             if (On && DTHandles.MouseOverSceneView)
             {
                 Mode = ModeEnum.None;
                 if (e.control && !e.alt) // Prevent that Panning (CTRL+ALT+LMB) creates CP'S
                 {
                     Mode = ModeEnum.Add;
                     if (e.shift)
                         Mode |= ModeEnum.Pick;
                 }

                 if (e.type == EventType.MouseDown)
                 {
                     if (Mode.HasFlag(ModeEnum.Add))
                     {
                         addCP(e.mousePosition, Mode.HasFlag(ModeEnum.Pick), e.button == 1);
                         DTGUI.UseEvent(GetHashCode(),e);
                     }

                 }
                 if (Mode.HasFlag(ModeEnum.Add))
                 {
                     if (Mode.HasFlag(ModeEnum.Pick))
                         _StatusBar.Set("<b>[LMB]</b> Add Control Point   <b>[RMB]</b> Add & Smart Connect","DrawMode");
                     else
                         _StatusBar.Set("<b>[Shift]</b> Raycast   <b>[LMB]</b> Add Control Point   <b>[RMB]</b> Add & Smart Connect", "DrawMode");
                 }
                 else
                     _StatusBar.Set("Hold <b>[Ctrl]</b> to add Control Points", "DrawMode");
             }
             else
                 _StatusBar.Clear("DrawMode");
        }

        

        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            
            SetElementSize(ref r, 32, 32);
            if (!selSpline || !selSpline.RestrictTo2D && (!SceneView.currentDrawingSceneView.in2DMode))
            {
                
                usePlaneXY = GUI.Toggle(r, usePlaneXY, new GUIContent(CurvyStyles.IconAxisXY, "Use XY Plane"), GUI.skin.button);
                if (usePlaneXY)
                {
                    usePlaneXZ = false;
                    usePlaneYZ = false;
                }
                Advance(ref r);
                usePlaneXZ = GUI.Toggle(r, usePlaneXZ, new GUIContent(CurvyStyles.IconAxisXZ, "Use XZ Plane"), GUI.skin.button);
                if (usePlaneXZ)
                {
                    usePlaneXY = false;
                    usePlaneYZ = false;
                }
                Advance(ref r);
                usePlaneYZ = GUI.Toggle(r, usePlaneYZ, new GUIContent(CurvyStyles.IconAxisYZ, "Use YZ Plane"), GUI.skin.button);
                if (usePlaneYZ)
                {
                    usePlaneXY = false;
                    usePlaneXZ = false;
                }
                Advance(ref r);
            }
            
            SetElementSize(ref r, 24, 24);
            r.y += 4;
            GUI.DrawTexture(r, Mode.HasFlag(ModeEnum.Add) ? CurvyStyles.IconCP : CurvyStyles.IconCPOff);

            Advance(ref r);
            GUI.DrawTexture(r, Mode.HasFlag(ModeEnum.Pick) ? CurvyStyles.IconRaycast : CurvyStyles.IconRaycastOff);
        }

        CurvySplineSegment insertCP(CurvySpline spline, CurvySplineSegment current, Vector3 worldPos)
        {
            var seg = spline.InsertAfter(current);
            seg.position = worldPos;
            Undo.RegisterCreatedObjectUndo(seg.gameObject, "Add ControlPoint");
            return seg;
        }

        bool pickScenePoint(Vector2 mousePos, bool castRay, CurvySpline referenceSpline, CurvySplineSegment referenceCP, out Vector3 pos)
        {
            // Pick a point following this rules:
            // Raycast into Scene (if castRay)
            // Spline is 2D ?
            //      - Project onto Spline's local X/Y-Plane
            // OR SceneView is 2D ?
            //      - Project onto X/Y with Z defined by Spline.Z
            // OR use plane choosen (either local or global)
            // ELSE project onto camera/position-plane
            
            Ray R = HandleUtility.GUIPointToWorldRay(mousePos);
            //R = RectTransformUtility.ScreenPointToRay(SceneView.currentDrawingSceneView.camera, mousePos);
            Plane P;
            float dist;

            // try Raycast if in pick mode
            RaycastHit hit;
            if (castRay && Physics.Raycast(R, out hit))
            {
                pos = hit.point;
                return true;
            }

            Transform tRef = (referenceCP != null) ? referenceCP.transform : referenceSpline.transform;

            if (referenceSpline.RestrictTo2D)
            { // 2D-Spline
                P = new Plane(referenceSpline.transform.forward, tRef.position);
                if (P.Raycast(R, out dist))
                {
                    pos = R.GetPoint(dist);
                    return true;
                }
            }
            else if (SceneView.currentDrawingSceneView.in2DMode)
            { // 2D SceneView
                P = new Plane(Vector3.forward, tRef.position);
                if (P.Raycast(R, out dist))
                {
                    pos = R.GetPoint(dist);
                    return true;
                }
            }
            else if (usePlaneXY)
            {
                P = (Tools.pivotRotation == PivotRotation.Local) ? new Plane(referenceSpline.transform.forward, tRef.position) : new Plane(Vector3.forward, tRef.position);
                if (P.Raycast(R, out dist))
                {
                    pos = R.GetPoint(dist);
                    return true;
                }
            }
            else if (usePlaneXZ)
            {
                P = (Tools.pivotRotation == PivotRotation.Local) ? new Plane(referenceSpline.transform.up, tRef.position) : new Plane(Vector3.up, tRef.position);
                if (P.Raycast(R, out dist))
                {
                    pos = R.GetPoint(dist);
                    return true;
                }
            }
            else if (usePlaneYZ)
            {
                P = (Tools.pivotRotation == PivotRotation.Local) ? new Plane(referenceSpline.transform.right, tRef.position) : new Plane(Vector3.right, tRef.position);
                if (P.Raycast(R, out dist))
                {
                    pos = R.GetPoint(dist);
                    return true;
                }
            }

            // Fallback: use Camera
            P = new Plane(SceneView.currentDrawingSceneView.camera.transform.forward, tRef.position);
            
            if (P.Raycast(R, out dist))
            {
                pos = R.GetPoint(dist);
                return true;
            }
            else
            {
                pos = Vector3.zero;
                return false;
            }
        }

      

        void addCP(Vector2 cursor, bool castRay, bool connectNew)
        {
            if (selSpline)
            {
                if (!selCP && selSpline.ControlPointCount > 0)
                    selCP = selSpline.ControlPoints[selSpline.ControlPointCount - 1];
            }
            else
            {
                selSpline = CurvySpline.Create();
                var parent = DTSelection.GetAs<Transform>();
                selSpline.transform.SetParent(parent);
                if (parent == null)
                    selSpline.transform.position = HandleUtility.GUIPointToWorldRay(cursor).GetPoint(10);
                Undo.RegisterCreatedObjectUndo(selSpline.gameObject, "Add ControlPoint");
            }
            // Pick a point to add the CP at
            Vector3 pos;
            if (!pickScenePoint(cursor, castRay, selSpline, selCP, out pos))
                return;

            CurvySplineSegment newCP=null;
            
            // Connect by creating a new spline with 2 CP, the first "over" selCP, the second at the desired new position
            // OR connect to existing C
            if (connectNew && selCP)
            {
                CurvySplineSegment conCP=null;
                // if mouse is over an existing CP, connect to this (if possible)
                var existingGO=HandleUtility.PickGameObject(cursor,false);
                if (existingGO)
                {
                    conCP = existingGO.GetComponent<CurvySplineSegment>();
                    // if we picked a target cp, it may be a pick on it's segment, so check distance to CP
                    if (conCP)
                    {
                        var P = new Plane(SceneView.currentDrawingSceneView.camera.transform.forward, conCP.transform.position);
                        var R = HandleUtility.GUIPointToWorldRay(cursor);
                        float dist;
                        if (P.Raycast(R, out dist))
                        {
                            Vector3 hit=R.GetPoint(dist);
                            if ((hit - conCP.transform.position).magnitude <= HandleUtility.GetHandleSize(hit) * CurvyGlobalManager.GizmoControlPointSize)
                            {
                                newCP = insertCP(selSpline, selCP, conCP.transform.position);
                                selCP = newCP;
                            }
                            else
                            {
                                hit=conCP.Interpolate(conCP.GetNearestPointF(hit));
                                conCP=conCP.Spline.InsertAfter(conCP);
                                conCP.transform.position = hit;
                                newCP = insertCP(selSpline, selCP, hit);
                                selCP = newCP;
                            }
                            selSpline.Refresh();
                        }
                    }
                }
                if (!conCP)
                {
                    selSpline = CurvySpline.Create(selSpline);
                    selSpline.Closed = false;
                    conCP = insertCP(selSpline, null, selCP.transform.position);
                    newCP = insertCP(selSpline, conCP, pos);
                    selSpline.Refresh();
                }
                selCP.ConnectTo(conCP);
            }
            else
            {
                newCP = insertCP(selSpline, selCP, pos);
            }

            DTSelection.SetGameObjects(newCP);
        }

        public override void OnSelectionChange()
        {
            Visible = CurvyProject.Instance.ShowGlobalToolbar || DTSelection.HasComponent<CurvySplineBase, CurvySplineSegment, CurvyController, CurvyGenerator>(true);
            // Ensure we have a spline and a CP. If a spline is selected, choose the last CP
            selCP = DTSelection.GetAs<CurvySplineSegment>();
            selSpline = (selCP) ? selCP.Spline : DTSelection.GetAs<CurvySpline>();
            
        }
    }

    [ToolbarItem(35,"Curvy","Import/Export","Import or export objects","importexport_dark,24,24","importexport_light,24,24")]
    public class TBImportExport : DTToolbarButton
    {

        public TBImportExport()
        {
            KeyBindings.Add(new EditorKeyBinding("Import/Export",""));
        }

        public override void OnClick()
        {
            ImportExportWizard.Open();
        }

        public override void OnSelectionChange()
        {
            Visible = CurvyProject.Instance.ShowGlobalToolbar;
        }
    }

    [ToolbarItem(100,"Curvy","Select Parent","","selectparent,24,24")]
    public class TBSelectParent : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Select parent spline(s)"; } }

        public TBSelectParent()
        {
            KeyBindings.Add(new EditorKeyBinding("Select Parent", "", KeyCode.Backslash));
        }
        
        public override void OnClick()
        {
            base.OnClick();
            var cps = DTSelection.GetAllAs<CurvySplineSegment>();
            var parents = new List<CurvySpline>();
            foreach (CurvySplineSegment cp in cps)
                if (!parents.Contains(cp.Spline))
                    parents.Add(cp.Spline);

            DTSelection.SetGameObjects(parents.ToArray());
        }
        
        public override void OnSelectionChange()
        {
            Visible = DTSelection.HasComponent<CurvySplineSegment>(true);
        }
    }

    [ToolbarItem(101,"Curvy","Select Children","","selectchilds,24,24")]
    public class TBSelectAllChildren : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Select Control Points"; } }

        public TBSelectAllChildren()
        {
            KeyBindings.Add(new EditorKeyBinding("Select Children", "", KeyCode.Backslash, true));
        }

        public override void OnClick()
        {
            base.OnClick();
            var splines = DTSelection.GetAllAs<CurvySpline>();
            var cps = DTSelection.GetAllAs<CurvySplineSegment>();
            foreach (var cp in cps)
                if (!splines.Contains(cp.Spline))
                    splines.Add(cp.Spline);
            var res = new List<CurvySplineSegment>();
            foreach (var spl in splines)
                res.AddRange(spl.ControlPoints);
            
            DTSelection.SetGameObjects(res.ToArray());
        }

        public override void OnSelectionChange()
        {
            base.OnSelectionChange();
            Visible = DTSelection.HasComponent<CurvySpline,CurvySplineSegment>(true);
        }

        
    }

    [ToolbarItem(105,"Curvy","Previous","Select Previous","prev,24,24")]
    public class TBCPPrevious : DTToolbarButton 
    {
        public override string StatusBarInfo { get { return "Select previous Control Point"; } }

        public TBCPPrevious()
        {
            KeyBindings.Add(new EditorKeyBinding("Select Previous", "", KeyCode.Tab, true));
        }

        public override void OnClick()
        {
            base.OnClick();
            var cp = DTSelection.GetAs<CurvySplineSegment>();
            if (cp)
                DTSelection.SetGameObjects(cp.Spline.ControlPoints[(int)Mathf.Repeat(cp.ControlPointIndex - 1, cp.Spline.ControlPointCount)]);
            else
            {
                var spl=DTSelection.GetAs<CurvySpline>();
                if (spl && spl.ControlPointCount>0)
                   DTSelection.SetGameObjects(spl.ControlPoints[spl.ControlPointCount-1]);
            }
            
        }

        public override void OnSelectionChange()
        {
            Visible = DTSelection.HasComponent<CurvySplineSegment>() || DTSelection.HasComponent<CurvySpline>();
        }
    }

    [ToolbarItem(106,"Curvy", "Next", "Select Next", "next,24,24")]
    public class TBCPNext : DTToolbarButton 
    {
        public override string StatusBarInfo { get { return "Select next Control Point"; } }

        public TBCPNext()
        {
            KeyBindings.Add(new EditorKeyBinding("Select Next", "", KeyCode.Tab));
        }

        public override void OnClick()
        {
            base.OnClick();
            var cp = DTSelection.GetAs<CurvySplineSegment>(false);
            if (cp)
                DTSelection.SetGameObjects(cp.Spline.ControlPoints[(int)Mathf.Repeat(cp.ControlPointIndex + 1, cp.Spline.ControlPointCount)]);
            else
            {
                var spl = DTSelection.GetAs<CurvySpline>();
                if (spl && spl.ControlPointCount > 0)
                    DTSelection.SetGameObjects(spl.ControlPoints[0]);
            }
            
        }

        public override void OnSelectionChange()
        {
            Visible = DTSelection.HasComponent<CurvySplineSegment>() || DTSelection.HasComponent<CurvySpline>();
        }
    }

    [ToolbarItem(120,"Curvy","Next connected","Toggle between connected CP","nextcon,24,24")]
    public class TBCPNextConnected : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Select next Control Point being part of this connection"; } }

        public TBCPNextConnected()
        {
            KeyBindings.Add(new EditorKeyBinding("Toggle Connection", "",KeyCode.C));
        }

        public override void OnClick()
        {
            base.OnClick();
 	        var cp=DTSelection.GetAs<CurvySplineSegment>();
            if (cp)
            {
                var idx = (int)Mathf.Repeat(cp.Connection.ControlPoints.IndexOf(cp) + 1, cp.Connection.ControlPoints.Count);
                DTSelection.SetGameObjects(cp.Connection.ControlPoints[idx]);
            }
            
        }

        public override void OnSelectionChange()
        {
            var cp=DTSelection.GetAs<CurvySplineSegment>();
            Visible = (cp != null && cp.Connection != null && cp.Connection.ControlPoints.Count > 1);
        }

       
    }

    [ToolbarItem(140, "Curvy", "Sync Dir", "Sync Handles Direction", "beziersyncdir,24,24")]
    public class TBCPBezierModeDirection : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Mirror Bezier Handles Direction"; } }

        public TBCPBezierModeDirection()
        {
            KeyBindings.Add(new EditorKeyBinding("Bezier: Sync Dir", "Sync Handles Direction", KeyCode.B));
        }

        public override bool On
        {
            get
            {
                return ((CurvyProject)Project).BezierMode.HasFlag(CurvyBezierModeEnum.Direction);
            }
            set
            {
                ((CurvyProject)Project).BezierMode = ((CurvyProject)Project).BezierMode.Set<CurvyBezierModeEnum>(CurvyBezierModeEnum.Direction, value);
            }
        }


        public override void OnOtherItemClicked(DTToolbarItem other) { } // IMPORTANT!

        public override void HandleEvents(Event e)
        {
            base.HandleEvents(e);
        }

        public override void OnSelectionChange()
        {
            base.OnSelectionChange();
            var cp = DTSelection.GetAs<CurvySplineSegment>();
            Visible = (cp && cp.Spline.Interpolation == CurvyInterpolation.Bezier);
        }
    }

    [ToolbarItem(141, "Curvy", "Sync Len", "Sync Handles Length", "beziersynclen,24,24")]
    public class TBCPBezierModeLength : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Mirror Bezier Handles Size"; } }

        public TBCPBezierModeLength()
        {
            KeyBindings.Add(new EditorKeyBinding("Bezier: Sync Len", "Sync Handles Length", KeyCode.N));
        }

        public override bool On
        {
            get
            {
                return ((CurvyProject)Project).BezierMode.HasFlag(CurvyBezierModeEnum.Length);
            }
            set
            {
                ((CurvyProject)Project).BezierMode = ((CurvyProject)Project).BezierMode.Set<CurvyBezierModeEnum>(CurvyBezierModeEnum.Length, value);
            }
        }

        public override void OnOtherItemClicked(DTToolbarItem other) { } // IMPORTANT!

        public override void OnSelectionChange()
        {
            base.OnSelectionChange();
            var cp = DTSelection.GetAs<CurvySplineSegment>();
            Visible = (cp && cp.Spline.Interpolation == CurvyInterpolation.Bezier);
        }
    }

    [ToolbarItem(142, "Curvy", "Sync Con", "Sync Handles of connected CP", "beziersynccon,24,24")]
    public class TBCPBezierModeConnections : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Apply Bezier Handle changes to connected Control Points as well"; } }

        public TBCPBezierModeConnections()
        {
            KeyBindings.Add(new EditorKeyBinding("Bezier: Sync Con", "Sync connected CP' handles", KeyCode.M));
        }

        public override bool On
        {
            get
            {
                return ((CurvyProject)Project).BezierMode.HasFlag(CurvyBezierModeEnum.Connections);
            }
            set
            {
                ((CurvyProject)Project).BezierMode = ((CurvyProject)Project).BezierMode.Set<CurvyBezierModeEnum>(CurvyBezierModeEnum.Connections, value);
            }
        }

        public override void OnOtherItemClicked(DTToolbarItem other) { } // IMPORTANT!

        public override void OnSelectionChange()
        {
            base.OnSelectionChange();
            var cp = DTSelection.GetAs<CurvySplineSegment>();
            Visible = (cp && cp.Spline.Interpolation == CurvyInterpolation.Bezier);
        }
    }

    [ToolbarItem(160,"Curvy","Shift","Shift on curve","shiftcp,24,24")]
    public class TBCPShift : DTToolbarToggleButton
    {
        CurvySplineSegment selCP;

        float mMin;
        float mMax;
        float mShift;

        public override string StatusBarInfo
        {
            get
            {
                return "Shifts the Control Point toward the previous or next Control Point";
            }
        }

        Vector3 getLocalPos()
        {
            if (mShift >= 0)
            {
                if (selCP.IsValidSegment)
                    return selCP.Interpolate(mShift);
                else
                    return (selCP.PreviousSegment) ? selCP.PreviousSegment.Interpolate(1) : selCP.localPosition;
            }
            else
                return (selCP.PreviousSegment) ? selCP.PreviousSegment.Interpolate(1+mShift) : selCP.localPosition;
        }

        public override void OnSceneGUI()
        {
            if (On && selCP)
            {
                Vector3 pos = selCP.Spline.transform.TransformPoint(getLocalPos());
                DTHandles.PushHandlesColor(CurvyGlobalManager.DefaultGizmoSelectionColor);
#if UNITY_5_6_OR_NEWER
                Handles.SphereHandleCap(0, pos, Quaternion.identity, HandleUtility.GetHandleSize(pos) * CurvyGlobalManager.GizmoControlPointSize,EventType.Repaint);
#else
                Handles.SphereCap(0, pos, Quaternion.identity, HandleUtility.GetHandleSize(pos)*CurvyGlobalManager.GizmoControlPointSize);
#endif
                DTHandles.PopHandlesColor();
            }
        }

        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            SetElementSize(ref r, 80, 32);
            r.y += 8;
            mShift=GUI.HorizontalSlider(r, mShift, mMin,mMax);
            
            Advance(ref r);
            r.width = 32;
            r.y -= 8;
            if (GUI.Button(r, "Ok"))
            {
                Undo.RecordObject(selCP.transform, "Shift Control Point");
                selCP.localPosition = getLocalPos();
                mShift = 0;
                On = false;
            }
        }

      

        public override void OnSelectionChange()
        {
            base.OnSelectionChange();
            selCP=DTSelection.GetAs<CurvySplineSegment>(false);
            Visible = selCP != null && selCP.IsVisibleControlPoint;
            if (Visible)
            {
                mMin = (selCP.PreviousSegment) ? -0.9f : 0;
                mMax = (selCP.IsValidSegment) ? 0.9f : 0;
                mShift = 0;
            }
        }
    }

    [ToolbarItem(161,"Curvy","Set 1.","Set as first Control Point","setfirstcp,24,24")]
    public class TBCPSetFirst : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Make this Control Point the first of the spline"; } }

        public TBCPSetFirst()
        {
            KeyBindings.Add(new EditorKeyBinding("Set 1. CP",""));
        }

        public override void OnClick()
        {
            base.OnClick();
            var cp = DTSelection.GetAs<CurvySplineSegment>();
            if (cp)
            {
                Undo.RegisterFullObjectHierarchyUndo(cp.Spline, "Set first CP");
                cp.SetAsFirstCP();
            }
        }

        public override void OnSelectionChange()
        {
            base.OnSelectionChange();
            var cp=DTSelection.GetAs<CurvySplineSegment>();
            Visible = (cp != null);
            Enabled= Visible && cp.ControlPointIndex>0;
        }
    }

    [ToolbarItem(162,"Curvy","Join","Join Splines","join,24,24")]
    public class TBCPJoin : DTToolbarButton
    {
        public override string StatusBarInfo 
        { 
            get 
            { 
                return mInfo;
            }
        }

        string mInfo;

        public TBCPJoin()
        {
            KeyBindings.Add(new EditorKeyBinding("Join Spline", "Join two splines"));
        }


        public override void OnClick()
        {
            base.OnClick();
            var source = DTSelection.GetAs<CurvySpline>();
            var destCP = DTSelection.GetAs<CurvySplineSegment>();
            var selIdx = destCP.ControlPointIndex + source.ControlPointCount+1;
            source.JoinWith(destCP);
            DTSelection.SetGameObjects(destCP.Spline.ControlPoints[Mathf.Min(destCP.Spline.ControlPointCount-1,selIdx)]);
        }

        public override void OnSelectionChange()
        {
            var source = DTSelection.GetAs<CurvySpline>();
            var destCP = DTSelection.GetAs<CurvySplineSegment>();
            Visible = source && destCP && source!=destCP.Spline;
            mInfo = (Visible) ? string.Format("Insert all Control Points of <b>{0}</b> after <b>{1}</b>", source.name, destCP.ToString()) : "";
        }

       
    }

    [ToolbarItem(163,"Curvy","Split","Split spline at selection","split,24,24")]
    public class TBCPSplit : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Split current Spline and make this Control Point the first of a new spline"; } }

        public TBCPSplit()
        {
            KeyBindings.Add(new EditorKeyBinding("Split Spline", "Split spline at selection"));
        }
     
        public override void OnClick()
        {
            base.OnClick();
            var cp = DTSelection.GetAs<CurvySplineSegment>();
            DTSelection.SetGameObjects(cp.SplitSpline());
        }

        public override void OnSelectionChange()
        {
            var cp=DTSelection.GetAs<CurvySplineSegment>();
            Visible = cp && cp.IsValidSegment && !cp.IsFirstSegment;
        }
    }

    [ToolbarItem (165, "Curvy", "Connect", "Create a connection", "connectionpos_dark,24,24", "connectionpos_light,24,24")]
    public class TBCPConnect : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Add a connection"; } }

        public TBCPConnect()
        {
            KeyBindings.Add(new EditorKeyBinding("Connect", "Create connection"));
        }

        public override void OnClick()
        {
            var selected=DTSelection.GetAllAs<CurvySplineSegment>();
            var unconnected = (from cp in selected
                       where !cp.Connection
                       select cp).ToArray();
            var con = (from cp in selected
                       where cp.Connection != null
                       select cp.Connection).FirstOrDefault();

            if (unconnected.Length > 0) 
            {
                if (con == null)
                {
                    con = CurvyConnection.Create(unconnected); // Undo inside
                    //con.AddControlPoints(unconnected); // Undo inside
                    //con.AutoSetFollowUp();
                } else
                    con.AddControlPoints(unconnected); // Undo inside
            }
            /*
            if (unconnected.Length == 2)
            {
                var source = unconnected[1];
                var dest = unconnected[0];
                source.ConnectTo(dest, (source.transform.position == dest.transform.position), false);
            }
            else
            {
                if (con == null)
                {
                    con = CurvyConnection.Create(); // Undo inside
                }
                con.AddControlPoints(unconnected); // Undo inside
            }
            */
            foreach (var cp in unconnected)
                EditorUtility.SetDirty(cp);

            CurvyProject.Instance.ScanConnections();
            
            //EditorApplication.RepaintHierarchyWindow();
        }

        public override void OnSelectionChange()
        {
            var selected=DTSelection.GetAllAs<CurvySplineSegment>();
            var unconnected = (from cp in selected
                               where !cp.Connection
                               select cp).ToList();

            Visible = (unconnected.Count > 0);
            /*
                      (unconnected.Count==1 ||
                      unconnected.Count>2 ||
                      (selected.Count == 2 && selected[0].CanConnectTo(selected[1])));
              */      
        }
    }
    /*
    [ToolbarItem(180, "Curvy", "Limit Len", "Constraint max. Spline length", "constraintlength,24,24")]
    public class TBCPLengthConstraint : DTToolbarToggleButton
    {
        public float MaxSplineLength;
        CurvySpline Spline;

        public TBCPLengthConstraint()
        {
            KeyBindings.Add(new EditorKeyBinding("Constraint Length", "Spline: Constraint Length"));
        }
        Vector3[] storedPosPrev = new Vector3[0];
        Vector3[] storedPos = new Vector3[0];


        void StorePos()
        {
            storedPosPrev = storedPos;
            storedPos = new Vector3[Selection.transforms.Length];
            for (int i = 0; i < storedPos.Length; i++)
                storedPos[i] = Selection.transforms[i].position;
        }
        void RestorePos()
        {
            Debug.Log("Restore");
            for (int i = 0; i < storedPosPrev.Length; i++)
                Selection.transforms[i].position = storedPosPrev[i];
        }

        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            SetElementSize(ref r, 84, 22);
            Background(r, 84, 22);
            r.width = 60;
            MaxSplineLength = EditorGUI.FloatField(r, MaxSplineLength);
            r.x += 62;
            r.width = 22;
            if (GUI.Button(r, "<"))
            {
                var cp = DTSelection.GetAs<CurvySplineSegment>();
                if (cp)
                    MaxSplineLength = cp.Spline.Length;
            }
        }

        public override void OnSelectionChange()
        {
            var cp = DTSelection.GetAs<CurvySplineSegment>();
            Visible = cp != null;
            Spline = (cp) ? cp.Spline : null;
        }

        public override void OnSceneGUI()
        {
            base.OnSceneGUI();

            if (On && Spline)
            {
                if (Spline.Length > MaxSplineLength)
                {
                    RestorePos();
                    Spline.SetDirtyAll();
                    Spline.Refresh();
                }
                else
                    StorePos();
            }

        }
    }
    */
    [ToolbarItem(190,"Curvy","Camera Project","Project camera","camproject,24,24")]
    public class TBCPCameraProject : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Raycast and move Control Points"; } }

        List<CurvySplineSegment> mCPSelection;

        public TBCPCameraProject()
        {
        }

        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            SetElementSize(ref r, 32, 32);

            if (GUI.Button(r, "OK"))
            {
                foreach (var cp in mCPSelection)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(new Ray(cp.position, SceneView.currentDrawingSceneView.camera.transform.forward), out hit))
                    {
                        Undo.RecordObject(cp.transform, "Project Control Points");
                        cp.transform.position = hit.point;
                    }
                }

                On = false;
            }
        }

        public override void OnSceneGUI()
        {
            base.OnSceneGUI();
            if (On && SceneView.currentDrawingSceneView!=null)
            {
                DTHandles.PushHandlesColor(Color.red);
                foreach (var cp in mCPSelection)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(new Ray(cp.position, SceneView.currentDrawingSceneView.camera.transform.forward), out hit))
                    {
                        Handles.DrawDottedLine(cp.position, hit.point, 2);
#if UNITY_5_6_OR_NEWER
                        Handles.SphereHandleCap(0, hit.point, Quaternion.identity, HandleUtility.GetHandleSize(hit.point) * 0.1f,EventType.Repaint);
#else
                        Handles.SphereCap(0, hit.point, Quaternion.identity, HandleUtility.GetHandleSize(hit.point)*0.1f);
#endif
                    }
                }
                DTHandles.PopHandlesColor();
            }
        }

        public override void OnSelectionChange()
        {
            mCPSelection = DTSelection.GetAllAs<CurvySplineSegment>();
            Visible = mCPSelection.Count > 0;
            if (!Visible)
                On = false;
        }

        public override void HandleEvents(Event e)
        {
            base.HandleEvents(e);
            if (On)
                _StatusBar.Set("Click <b>OK</b> to apply the preview changes", "CameraProject");
            else
                _StatusBar.Clear("CameraProject");
        }
    }

    [ToolbarItem(200, "Curvy", "CPTools", "Control Point Tools", "tools,24,24")]
    public class TBCPTools : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Open Control Point Tools menu"; } }

        public class SplineRange
        {
            public CurvySpline Spline;
            public CurvySplineSegment Low;
            public CurvySplineSegment High;

            public bool CanSubdivide 
            {
                get { return Low && High && (High.ControlPointIndex - Low.ControlPointIndex > 0); }
            }
            
            public bool CanSimplify 
            {
                get { return Low && High && (High.ControlPointIndex - Low.ControlPointIndex > 1); }
            }

            public SplineRange(CurvySpline spline)
            {
                Spline = spline;
                Low = null;
                High = null;
            }

            public void AddCP(CurvySplineSegment cp)
            {
                if (Low == null || Low.ControlPointIndex > cp.ControlPointIndex)
                    Low = cp;
                if (High == null || High.ControlPointIndex < cp.ControlPointIndex)
                    High = cp;
            }
        }

        List<CurvySplineSegment> mCPSelection;
        Dictionary<CurvySpline, SplineRange> mSplineRanges = new Dictionary<CurvySpline, SplineRange>();

        public bool CanSubdivide
        {
            get
            {
                foreach (var sr in mSplineRanges.Values)
                    if (sr.CanSubdivide)
                        return true;
                return false;
            }
        }

        public bool CanSimplify
        {
            get
            {
                foreach (var sr in mSplineRanges.Values)
                    if (sr.CanSimplify)
                        return true;
                return false;
            }
        }

        

        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            SetElementSize(ref r, 32, 32);
            GUI.enabled = CanSubdivide;
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconSubdivide, "Subdivide")))
                Subdivide();
            Advance(ref r);

            GUI.enabled = CanSimplify;
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconSimplify, "Simplify")))
                Simplify();
            Advance(ref r);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconEqualize, "Equalize")))
                Equalize();
            GUI.enabled = true;
        }

        public override void OnSelectionChange()
        {
            mCPSelection=DTSelection.GetAllAs<CurvySplineSegment>();
            getRange();
            Visible = mCPSelection.Count > 1;
            if (!Visible)
                On = false;
        }

        void Subdivide()
        {
            foreach (var sr in mSplineRanges.Values)
                if (sr.CanSubdivide)
                    sr.Spline.Subdivide(sr.Low,sr.High);
        }

        void Simplify()
        {
            foreach (var sr in mSplineRanges.Values)
                if (sr.CanSimplify)
                    sr.Spline.Simplify(sr.Low, sr.High);
        }

        void Equalize()
        {
            foreach (var sr in mSplineRanges.Values)
                if (sr.CanSimplify)
                    sr.Spline.Equalize(sr.Low, sr.High);
        }

        void getRange()
        {
            mSplineRanges.Clear();
            foreach (var cp in mCPSelection)
            {
                SplineRange sr;
                if (!mSplineRanges.TryGetValue(cp.Spline, out sr))
                {
                    sr = new SplineRange(cp.Spline);
                    mSplineRanges.Add(cp.Spline, sr);
                }

                sr.AddCP(cp);
            }
        }
    }

    [ToolbarItem(120,"Curvy", "Set Pivot", "", "centerpivot,24,24")]
    public class TBSplineSetPivot : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Set center/pivot point"; } }

        bool Is2D;
        float pivotX;
        float pivotY;
        float pivotZ;

        public override void OnClick()
        {
            base.OnClick();
            Is2D = true;
            var splines = DTSelection.GetAllAs<CurvySpline>();
            foreach (var spl in splines)
                if (!spl.RestrictTo2D)
                {
                    Is2D = false;
                    break;
                }
        }

        

        public override void OnSelectionChange()
        {
            Visible = DTSelection.HasComponent<CurvySpline>(true);
        }

        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            if (Is2D)
            {
                Background(r, 182, 102);
                SetElementSize(ref r, 180, 100);
                
            }
            else
            {
                Background(r, 182, 187);
                SetElementSize(ref r, 180, 185);
                
            }
            
            EditorGUIUtility.labelWidth = 20;
            GUILayout.BeginArea(new Rect(r));
            GUILayout.Label("X/Y", EditorStyles.boldLabel);
            for (int y = -1; y <= 1; y++)
            {
                GUILayout.BeginHorizontal();
                for (int x = -1; x <= 1; x++)
                {
                    DTGUI.PushBackgroundColor((x == pivotX && y == pivotY) ? Color.red : GUI.backgroundColor);
                    if (GUILayout.Button("", GUILayout.Width(20)))
                    {
                        pivotX = x;
                        pivotY = y;
                    }
                    DTGUI.PopBackgroundColor();
                }
                if (y == -1)
                {
                    GUILayout.Space(20);
                    pivotX = EditorGUILayout.FloatField("X", pivotX);
                }
                else if (y == 0)
                {
                    GUILayout.Space(20);
                    pivotY = EditorGUILayout.FloatField("Y", pivotY);
                }
                GUILayout.EndVertical();
            }

            if (!Is2D)
            {
                GUILayout.Label("Y/Z", EditorStyles.boldLabel);
                for (int y = -1; y <= 1; y++)
                {
                    GUILayout.BeginHorizontal();
                    for (int z = -1; z <= 1; z++)
                    {
                        DTGUI.PushBackgroundColor((y == pivotY && z == pivotZ) ? Color.red : GUI.backgroundColor);
                        if (GUILayout.Button("", GUILayout.Width(20)))
                        {
                            pivotY = y;
                            pivotZ = z;
                        }
                        DTGUI.PopBackgroundColor();
                    }
                    if (y == -1)
                    {
                        GUILayout.Space(20);
                        pivotZ = EditorGUILayout.FloatField("Z", pivotZ);
                    }

                    GUILayout.EndVertical();
                }
            }
            if (GUILayout.Button("Apply"))
            {
                SetPivot();
                On = false;
            }
            GUILayout.EndArea();
        }



        public override void OnSceneGUI()
        {
            if (On)
            {
                var splines = DTSelection.GetAllAs<CurvySpline>();
                foreach (var spl in splines)
                {
                    
                    var p = spl.SetPivot(pivotX, pivotY, pivotZ, true);
                    DTHandles.PushHandlesColor(new Color(0.3f, 0, 0));
                    DTHandles.BoundsCap(spl.Bounds);
#if UNITY_5_6_OR_NEWER
                    Handles.SphereHandleCap(0, p, Quaternion.identity, HandleUtility.GetHandleSize(p) * .1f,EventType.Repaint);
#else
                    Handles.SphereCap(0, p, Quaternion.identity, HandleUtility.GetHandleSize(p) * .1f);
#endif
                    DTHandles.PopHandlesColor();
                }
            }
        }



        void SetPivot()
        {
            var splines = DTSelection.GetAllAs<CurvySpline>();
            foreach (var spl in splines)
                spl.SetPivot(pivotX, pivotY, pivotZ);
        }

    }

    [ToolbarItem(122,"Curvy","Flip","Flip spline direction","flip,24,24")]
    public class TBSplineFlip : DTToolbarButton 
    {
        public override string StatusBarInfo { get { return "Invert all Control Points, making the spline direction flip"; } }

        public TBSplineFlip()
        {
            KeyBindings.Add(new EditorKeyBinding("Flip", "Flip spline direction"));
        }

        public override void OnClick()
        {
            var splines = DTSelection.GetAllAs<CurvySpline>();
            foreach (var spline in splines)
            {
                spline.Flip();
            }
        }

        public override void OnSelectionChange()
        {
            Visible = DTSelection.HasComponent<CurvySpline>(true);
        }
    }

    [ToolbarItem(124, "Curvy", "Normalize", "Normalize scale", "normalize,24,24")]
    public class TBSplineNormalize : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Apply transform scale to Control Points and reset scale to 1"; } }

        public TBSplineNormalize()
        {
            KeyBindings.Add(new EditorKeyBinding("Normalize", "Normalize spline"));
        }

        public override void OnClick()
        {
            var splines = DTSelection.GetAllAs<CurvySpline>();
            foreach (var spline in splines)
            {
                spline.Normalize();
            }
        }

        public override void OnSelectionChange()
        {
            Visible = DTSelection.HasComponent<CurvySpline>(true);
        }
    }

    [ToolbarItem(124,"Curvy","Shape","Apply a shape","shapewizard,24,24")]
    public class TBSplineSetShape : DTToolbarToggleButton
    {
        public override string StatusBarInfo { get { return "Apply a shape. <b><color=#ff0000>WARNING: THIS CAN'T BE UNDONE!</color></b>"; } }

        Vector2 scroll;
        float winHeight = 120;
        bool hasShape;

        CurvyShape CurrentShape
        {
            get
            {
                return DTSelection.GetAs<CurvyShape>();
            }
        }

        
        void FreeNonPersistent()
        {
            if (CurrentShape != null && !CurrentShape.Persistent)
                CurrentShape.Delete();
        }

        public override void OnClick()
        {
            base.OnClick(); 
            if (!On)
                FreeNonPersistent();
        }

        public override void OnOtherItemClicked(DTToolbarItem other)
        {
            base.OnOtherItemClicked(other);
            FreeNonPersistent();
        }

      
        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);

            SetElementSize(ref r,300,winHeight);
            if (CurrentShape == null)
            {
                if (hasShape)
                {
                    hasShape = false;
                    On = false;
                    return;
                }
                var go = DTSelection.GetGameObject();
                if (go != null)
                {
                    var shp=go.AddComponent<CSCircle>();
                    shp.Dirty = true;
                    shp.Refresh();
                    hasShape = true;
                }
                if (CurrentShape != null)
                    CurrentShape.Persistent = false;
            }
            if (CurrentShape != null)
            {
                CurvyShapeEditor ShapeEditor=Editor.CreateEditor(CurrentShape, typeof(CurvyShapeEditor)) as CurvyShapeEditor;
                if (ShapeEditor != null)
                {
                    FocusedItem = this;
                    ShapeEditor.ShowOnly2DShapes = false;
                    ShapeEditor.ShowPersistent = true;
                    Background(r, 300, winHeight);

                    GUILayout.BeginArea(r);
                    scroll=GUILayout.BeginScrollView(scroll,GUILayout.Height(winHeight-25));
                    

                    if (ShapeEditor.OnEmbeddedGUI())
                        CurrentShape.Persistent = false;

                    GUILayout.EndScrollView();
                    GUILayout.EndArea();
                    
                    r.y += winHeight - 20;
                    r.height = 20;
                    if (!CurrentShape.Persistent)
                    {
                        r.width /= 2;
                        if (GUI.Button(r, "Save"))
                        {
                            CurrentShape.Persistent = true;
                            On = false;
                        }
                        r.x += r.width;
                    }
                    if (GUI.Button(r, "Close"))
                    {
                        FreeNonPersistent();
                        On = false;
                    }



                    Editor.DestroyImmediate(ShapeEditor);
                }
                

            }
        }

        public override void OnSelectionChange()
        {
            FreeNonPersistent();
            Visible = DTSelection.HasComponent<CurvySpline>();
            scroll = Vector2.zero;
        }
    }

    [ToolbarItem(200,"Curvy","Tools","Spline Tools","tools,24,24")]
    public class TBSplineTools : DTToolbarToggleButton
    {

        public override string StatusBarInfo { get { return "Open Spline Tools menu"; } }


        public override void RenderClientArea(Rect r)
        {
            base.RenderClientArea(r);
            SetElementSize(ref r, 32, 32);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconMeshExport, "Export Mesh")))
            {
                CurvySplineExportWizard.Create();
                On = false;
            }
            Advance(ref r);
            if (GUI.Button(r,new GUIContent(CurvyStyles.IconSyncFromHierarchy,"Sync from Hierarchy")))
            {
                var sel = DTSelection.GetAllAs<CurvySpline>();
                foreach (var spl in sel)
                {
                    spl.SyncSplineFromHierarchy();
                    spl.ApplyControlPointsNames();
                    spl.Refresh();
                    On = false;
                }
            }
            Advance(ref r);
            if (GUI.Button(r, new GUIContent(CurvyStyles.IconSelectContainingConnections, "Select containing connections")))
            {
                var sel = DTSelection.GetAllAs<CurvySpline>();
                DTSelection.SetGameObjects(CurvyGlobalManager.Instance.GetContainingConnections(sel.ToArray()));
            }
        }

        public override void OnSelectionChange()
        {
            Visible = DTSelection.HasComponent<CurvySpline>(true);
        }
    }

    [ToolbarItem(190, "Curvy", "Edit", "Open CG Editor", "opengraph_dark,24,24","opengraph_light,24,24")]
    public class TBPCGOpenGraph : DTToolbarButton
    {
        public override string StatusBarInfo { get { return "Open Curvy Generator Editor"; } }

        public override void OnClick()
        {
            base.OnClick();
            var pcg = DTSelection.GetAs<CurvyGenerator>();
            if (pcg)
                FluffyUnderware.CurvyEditor.Generator.CGGraph.Open(pcg);
            
        }

        public override void OnSelectionChange()
        {
            Visible = DTSelection.HasComponent<CurvyGenerator>();
        }
    }

    

}
