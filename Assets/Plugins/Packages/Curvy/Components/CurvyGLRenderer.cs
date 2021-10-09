// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using FluffyUnderware.DevTools;
using System.Collections.Generic;
// =====================================================================
// Copyright 2013-2014 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
/* Renders curvy spline(s) approximation using GL.Draw
 * 
 * Add this script to a camera
 */

namespace FluffyUnderware.Curvy.Components
{
    
    /// <summary>
    /// Class to render a spline using GL.Draw
    /// </summary>
    /// <remarks>Useful for debugging</remarks>
    [HelpURL(CurvySpline.DOCLINK + "curvyglrenderer")]
    [AddComponentMenu("Curvy/Misc/Curvy GL Renderer")]
    public class CurvyGLRenderer : MonoBehaviour
    {
        [ArrayEx(ShowAdd=false,Draggable=false)]
        public List<GLSlotData> Splines = new List<GLSlotData>();

        Material lineMaterial;

        void CreateLineMaterial()
        {
            if (!lineMaterial)
            {
#if UNITY_5_0 || UNITY_4_6
                lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
                    "SubShader { Pass { " +
                    "    Blend SrcAlpha OneMinusSrcAlpha " +
                    "    ZWrite Off Cull Off Fog { Mode Off } " +
                    "    BindChannels {" +
                    "      Bind \"vertex\", vertex Bind \"color\", color }" +
                    "} } }");
					
#else
                lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
#endif
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            sanitize();
        }
#endif

        void OnPostRender()
        {
            sanitize();
            CreateLineMaterial();
            for (int i = Splines.Count-1; i >=0;i--)
            {
                Splines[i].Spline.OnRefresh.AddListenerOnce(OnSplineRefresh);
                if (Splines[i].VertexData.Count==0)
                    Splines[i].GetVertexData();

                Splines[i].Render(lineMaterial);
            }

        }

        void sanitize()
        {
            for (int i = Splines.Count-1; i >=0;i--)
                if (Splines[i]==null || Splines[i].Spline==null)
                    Splines.RemoveAt(i);
        }

        void OnSplineRefresh(CurvySplineEventArgs e)
        {
            var slot=getSlot((CurvySplineBase)e.Sender);
            if (slot == null)
                ((CurvySplineBase)e.Sender).OnRefresh.RemoveListener(OnSplineRefresh);
            else
                slot.VertexData.Clear();
        }

        GLSlotData getSlot(CurvySplineBase spline)
        {
            if (spline)
            {
                foreach (var slot in Splines)
                    if (slot.Spline == spline)
                        return slot;
            }
            return null;
        }

        public void Add(CurvySplineBase spline)
        {
            if (spline!=null)
                Splines.Add(new GLSlotData() { Spline = spline });
        }

        public void Remove(CurvySplineBase spline)
        {
            for (int i = Splines.Count - 1; i >= 0; i--)
                if (Splines[i].Spline == spline)
                    Splines.RemoveAt(i);
        }
     
    }

    /// <summary>
    /// Helper class used by CurvyGLRenderer
    /// </summary>
    [System.Serializable]
    public class GLSlotData
    {
        [SerializeField]
        public CurvySplineBase Spline;
        public Color LineColor = CurvyGlobalManager.DefaultGizmoColor;
        public List<Vector3[]> VertexData = new List<Vector3[]>();

        public void GetVertexData()
        {
            VertexData.Clear();
            var splines=new List<CurvySpline>();

            if (Spline is CurvySplineGroup)
                splines.AddRange(((CurvySplineGroup)Spline).Splines);
            else
                splines.Add((CurvySpline)Spline);

            for (int i = 0; i < splines.Count; i++)
                if (splines[i].IsInitialized)
                    VertexData.Add(splines[i].GetApproximation(Space.World));
        }

        public void Render(Material mat)
        {
            for (int i=0;i<VertexData.Count;i++)
                if (VertexData[i].Length > 0)
                {
                    mat.SetPass(0);
                    GL.Begin(GL.LINES);
                    GL.Color(LineColor);
                    for (int v = 1; v < VertexData[i].Length; v++)
                    {
                        GL.Vertex(VertexData[i][v - 1]);
                        GL.Vertex(VertexData[i][v]);
                    }
                    GL.End();
                }
        }
    }

    
}
