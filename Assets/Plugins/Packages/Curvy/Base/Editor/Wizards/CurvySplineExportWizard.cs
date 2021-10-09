// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Utils;
using System.Collections.Generic;
using FluffyUnderware.DevToolsEditor;
using FluffyUnderware.DevTools;
using FluffyUnderware.Curvy.ThirdParty.LibTessDotNet;

namespace FluffyUnderware.CurvyEditor
{
    /// <summary>
    /// Wizard to export one or more splines to a mesh
    /// </summary>
    public class CurvySplineExportWizard : EditorWindow
    {
        const int CLOSEDSHAPE = 0;
        const int VERTEXLINE = 1;

        // SOURCES
        public List<SplinePolyLine> Curves = new List<SplinePolyLine>();
        public WindingRule Winding = WindingRule.EvenOdd;
        public string TriangulationMessage = string.Empty;

        bool refreshNow = true;
        public int Mode;
        public Material Mat;
        public Vector2 UVOffset = Vector2.zero;
        public Vector2 UVTiling = Vector2.one;
        
        public bool UV2;
        public string MeshName = "CurvyMeshExport";
        public bool[] FoldOuts = new bool[4] { true, true, true, true };

        public CurvySplineGizmos GizmoState;

        public GameObject previewGO;
        public MeshFilter previewMeshFilter;
        public MeshRenderer previewMeshRenderer;
        
        public Vector2 scroll;

        Dictionary<CurvySplineBase, TTransform> splineTransforms = new Dictionary<CurvySplineBase, TTransform>();

        GUIStyle mRedLabel;

        Mesh previewMesh
        {
            get
            {
                return previewMeshFilter.sharedMesh;
            }
            set
            {
                previewMeshFilter.sharedMesh = value;
            }
        }

        static public void Create()
        {
            var win = GetWindow<CurvySplineExportWizard>(true, "Export Curvy Spline", true);
            win.Init(Selection.activeGameObject.GetComponent<CurvySplineBase>());
            win.minSize = new Vector2(500, 390);
            SceneView.onSceneGUIDelegate -= win.Preview;
            SceneView.onSceneGUIDelegate += win.Preview;
        }

        void OnEnable()
        {
            GizmoState = CurvyGlobalManager.Gizmos;
            CurvyGlobalManager.Gizmos = CurvySplineGizmos.Curve;
            
            
           
            if (!previewGO)
            {
                previewGO = new GameObject("ExportPreview");
                previewGO.hideFlags = HideFlags.HideAndDontSave;
                previewMeshRenderer = previewGO.AddComponent<MeshRenderer>();
                previewMeshFilter = previewGO.AddComponent<MeshFilter>();
                if (!Mat)
                {
                    Mat = CurvyUtility.GetDefaultMaterial();
                }
                previewMeshRenderer.material = Mat;
            }
        }

        void OnDisable()
        {
            CurvyGlobalManager.Gizmos = GizmoState;
        }

        void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= Preview;
            foreach (var crv in Curves)
                UnhookSpline(crv.Spline);
            Curves.Clear();
            SceneView.RepaintAll();
            GameObject.DestroyImmediate(previewGO);
        }

        void OnFocus()
        {
            SceneView.onSceneGUIDelegate -= Preview;
            SceneView.onSceneGUIDelegate += Preview;
        }

        void Init(CurvySplineBase spline)
        {
            Curves.Add(new SplinePolyLine(spline));
            HookSpline(spline);
        }


        Mesh clonePreviewMesh()
        {
            Mesh msh = new Mesh();
            msh.vertices = previewMesh.vertices;
            msh.triangles = previewMesh.triangles;
            msh.uv = previewMesh.uv;
            msh.uv2 = previewMesh.uv2;
            msh.RecalculateNormals();
            msh.RecalculateBounds();
            return msh;
        }

        void OnSourceRefresh(CurvySplineEventArgs e)
        {
            refreshNow = true;
        }

        void HookSpline(CurvySplineBase spline)
        {
            if (!spline) return;
            spline.OnRefresh.AddListenerOnce(OnSourceRefresh);
            if (!splineTransforms.ContainsKey(spline))
                splineTransforms.Add(spline,new TTransform(spline.transform));
        }

        void UnhookSpline(CurvySplineBase spline)
        {
            if (!spline) return;
            spline.OnRefresh.RemoveListener(OnSourceRefresh);
            splineTransforms.Remove(spline);
        }

        IDTInspectorNodeRenderer GUIRenderer = new DTInspectorNodeDefaultRenderer();
        DTGroupNode nSplines = new DTGroupNode("Splines") { HelpURL= CurvySpline.DOCLINK + "exportwizard" };
        DTGroupNode nTexture = new DTGroupNode("Texture");
        DTGroupNode nExport = new DTGroupNode("Export");
        bool mNeedRepaint;

        void OnGUI()
        {
           
            DTInspectorNode.IsInsideInspector = false;
            if (Curves.Count == 0)
                return;
            
            
            Mode = GUILayout.SelectionGrid(Mode, new GUIContent[] 
                    {
                        new GUIContent("Closed Shape","Export a closed shape with triangles"),
                        new GUIContent("Vertex Line","Export a vertex line")
                    }, 2);
            
            

            if (!string.IsNullOrEmpty(TriangulationMessage) && !TriangulationMessage.Contains("Angle must be >0"))
                EditorGUILayout.HelpBox(TriangulationMessage, MessageType.Error);

            scroll = EditorGUILayout.BeginScrollView(scroll);

            // OUTLINE
            GUIRenderer.RenderSectionHeader(nSplines);
            if (nSplines.ContentVisible)
            {
                Winding = (WindingRule)EditorGUILayout.EnumPopup("Winding", Winding,GUILayout.Width(285));
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("Spline", "Note: Curves from a SplineGroup needs to be connected!"), EditorStyles.boldLabel, GUILayout.Width(140));
                GUILayout.Label("Vertex Generation", EditorStyles.boldLabel, GUILayout.Width(160));
                GUILayout.Label("Orientation", EditorStyles.boldLabel);
                GUILayout.EndHorizontal();
                CurveGUI(Curves[0]);
                if (Mode == CLOSEDSHAPE)
                {

                    for (int i = 1; i < Curves.Count; i++)
                    {
                        CurveGUI(Curves[i]);
                    }
                    if (GUILayout.Button(CurvyStyles.AddSmallTexture,GUILayout.ExpandWidth(false)))
                        Curves.Add(new SplinePolyLine(null));
                }
            }
        
            mNeedRepaint = mNeedRepaint || nSplines.NeedRepaint;
            GUIRenderer.RenderSectionFooter(nSplines);
        
            // TEXTURING
            GUIRenderer.RenderSectionHeader(nTexture);
            if (nTexture.ContentVisible)
            {
                Mat = (Material)EditorGUILayout.ObjectField("Material", Mat, typeof(Material), true, GUILayout.Width(285));
                UVTiling = EditorGUILayout.Vector2Field("Tiling", UVTiling, GUILayout.Width(285));
                UVOffset = EditorGUILayout.Vector2Field("Offset", UVOffset, GUILayout.Width(285));
                
            }
            GUIRenderer.RenderSectionFooter(nTexture);
            mNeedRepaint = mNeedRepaint || nTexture.NeedRepaint;
            // EXPORT
            GUIRenderer.RenderSectionHeader(nExport);
            if (nExport.ContentVisible)
            {
                EditorGUILayout.HelpBox("Export is 2D (x/y) only!", MessageType.Info);
                MeshName = EditorGUILayout.TextField("Mesh Name", MeshName, GUILayout.Width(285));
                UV2 = EditorGUILayout.Toggle("Add UV2", UV2);

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Save as Asset"))
                {
                    string path = EditorUtility.SaveFilePanelInProject("Save Mesh", MeshName + ".asset", "asset", "Choose a file location");
                    if (!string.IsNullOrEmpty(path))
                    {
                        Mesh msh = clonePreviewMesh();
                        if (msh)
                        {
                            msh.name = MeshName;
                            AssetDatabase.DeleteAsset(path);
                            AssetDatabase.CreateAsset(msh, path);
                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                            DTLog.Log("[Curvy] Export: Mesh Asset saved!");
                        }
                    }
                }

                if (GUILayout.Button("Create GameObject"))
                {
                    Mesh msh = clonePreviewMesh();
                    if (msh)
                    {
                        msh.name = MeshName;
                        var go = new GameObject(MeshName, typeof(MeshRenderer), typeof(MeshFilter));
                        go.GetComponent<MeshFilter>().sharedMesh = msh;
                        go.GetComponent<MeshRenderer>().sharedMaterial = Mat;
                        Selection.activeGameObject = go;
                        DTLog.Log("[Curvy] Export: GameObject created!");
                    }
                    else
                        DTLog.LogWarning("[Curvy] Export: Unable to triangulate spline!");

                }
                GUILayout.EndHorizontal();

            }
            GUIRenderer.RenderSectionFooter(nExport);
            mNeedRepaint = mNeedRepaint || nExport.NeedRepaint;
            EditorGUILayout.EndScrollView();
            refreshNow = refreshNow || GUI.changed;
            if (mNeedRepaint)
            {
                Repaint();
                mNeedRepaint = false;
            }
        }

        void CurveGUI(SplinePolyLine curve)
        {
            GUILayout.BeginHorizontal();
            CurvySplineBase o = curve.Spline;
            curve.Spline = (CurvySplineBase)EditorGUILayout.ObjectField(curve.Spline, typeof(CurvySplineBase), true, GUILayout.Width(140));
            
            if (o != curve.Spline)
            {
                UnhookSpline(o);
            }
            HookSpline(curve.Spline);
            
            curve.VertexMode = (SplinePolyLine.VertexCalculation)EditorGUILayout.EnumPopup(curve.VertexMode, GUILayout.Width(140));
            GUILayout.Space(20);
            curve.Orientation = (ContourOrientation)EditorGUILayout.EnumPopup(curve.Orientation);
            if (GUILayout.Button(new GUIContent(CurvyStyles.DeleteSmallTexture, "Remove"), GUILayout.ExpandWidth(false)))
            {
                if (curve.Spline)
                    UnhookSpline(curve.Spline);
                Curves.Remove(curve);
                refreshNow = true;
                GUIUtility.ExitGUI();
            }
            switch (curve.VertexMode)
            {
                case SplinePolyLine.VertexCalculation.ByAngle:
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(150);
                    float lw = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 40;
                    curve.Angle = Mathf.Max(0, EditorGUILayout.FloatField("Angle",curve.Angle,GUILayout.Width(140)));
                    EditorGUIUtility.labelWidth = 60;
                    GUILayout.Space(20);
                    curve.Distance = EditorGUILayout.FloatField("Min. Dist.",curve.Distance,GUILayout.Width(150));
                    EditorGUIUtility.labelWidth = lw;
                    if (curve.Angle == 0)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(140);
                        EditorGUILayout.HelpBox("Angle must be >0", MessageType.Error);
                    }
                    break;
            }
            GUILayout.EndHorizontal();
            


        }

        void Update()
        {
            if (Curves.Count == 0)
            {
                Close();
                return;
            }

            foreach (var kv in splineTransforms)
            {
                var tt=splineTransforms[kv.Key];
                if (tt != kv.Key.transform)
                {
                    tt.FromTransform(kv.Key.transform);
                    refreshNow = true;
                    break;
                }
            }

            if (refreshNow)
            {
                previewMeshRenderer.sharedMaterial = Mat;
                refreshNow = false;
                Spline2Mesh s2m = new Spline2Mesh();
                foreach (var c in Curves)
                    if (c.Spline!= null)
                        s2m.Lines.Add(c);
                
                s2m.Winding = Winding;
                s2m.VertexLineOnly = (Mode == VERTEXLINE);
                
                s2m.UVOffset = UVOffset;
                s2m.UVTiling = UVTiling;
                s2m.UV2 = UV2;
                s2m.MeshName = MeshName;
                s2m.SetBounds(true, Vector3.one);
                Mesh m;
                s2m.Apply(out m);
                previewMesh = m;
                
                TriangulationMessage = s2m.Error;
                string sTitle;
                if (previewMesh)
                {
                    if (previewMesh.triangles.Length > 0)
                        sTitle = "Export (" + previewMeshFilter.sharedMesh.vertexCount + " Vertices, " + previewMeshFilter.sharedMesh.triangles.Length / 3 + " Triangles)";
                    else
                        sTitle = "Export (" + previewMeshFilter.sharedMesh.vertexCount + " Vertices)";
                }
                else
                    sTitle = "Export";

#if UNITY_5_0 || UNITY_4_6
                title=sTitle;
#else
                titleContent = new GUIContent(sTitle);
#endif
                SceneView.RepaintAll();
            }
        }

        void Preview(SceneView sceneView)
        {
           
            if (!previewMesh)
                return;
            float zOffset = Curves[0].Spline.transform.position.z;
            previewGO.transform.position = new Vector3(previewGO.transform.position.x, previewGO.transform.position.y, zOffset);

            Vector3[] vts = previewMesh.vertices;
            int[] tris=new int[0];
            if (Mode!=VERTEXLINE)
             tris = previewMesh.triangles;
            Handles.color = Color.green;
            Handles.matrix = Matrix4x4.TRS(new Vector3(0, 0, zOffset), Quaternion.identity, Vector3.one);
            for (int i = 0; i < tris.Length; i += 3)
                Handles.DrawPolyLine(vts[tris[i]], vts[tris[i + 1]], vts[tris[i + 2]], vts[tris[i]]);

            Handles.color = Color.gray;
            for (int i = 0; i < vts.Length; i++)
            {
#if UNITY_5_6_OR_NEWER
                Handles.CubeHandleCap(0, vts[i], Quaternion.identity, HandleUtility.GetHandleSize(vts[i]) * 0.07f,EventType.Repaint);
#else
                Handles.CubeCap(0, vts[i], Quaternion.identity, HandleUtility.GetHandleSize(vts[i]) * 0.07f);
#endif
            }

        }

    }
}
