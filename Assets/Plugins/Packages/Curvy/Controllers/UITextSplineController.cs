// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using FluffyUnderware.DevTools.Extensions;

namespace FluffyUnderware.Curvy.Controllers
{
    /// <summary>
    /// SplineController modifying uGUI text
    /// </summary>
    [RequireComponent(typeof(Text))]
    [AddComponentMenu("Curvy/Controller/UI Text Spline Controller")]
    [HelpURL(CurvySpline.DOCLINK + "uitextsplinecontroller")]
#if UNITY_5_1 || UNITY_5_0 || UNITY_4_6
    public class UITextSplineController : SplineController, IVertexModifier
#else
    public class UITextSplineController : SplineController, IMeshModifier
#endif
    {

        protected class GlyphQuad
        {
            public UIVertex[] V = new UIVertex[4];
            public Rect Rect;
            public Vector3 Center { get { return Rect.center; } }

            public void Load(List<UIVertex> verts, int index)
            {
                V[0] = verts[index];
                V[1] = verts[index + 1];
                V[2] = verts[index + 2];
                V[3] = verts[index + 3];

                calcRect();
            }

            public void LoadTris(List<UIVertex> verts, int index)
            {
                V[0] = verts[index];
                V[1] = verts[index + 1];
                V[2] = verts[index + 2];
                V[3] = verts[index + 4];
                calcRect();
            }

            public void calcRect()
            {
                Rect = new Rect(V[0].position.x,
                              V[2].position.y,
                              V[2].position.x - V[0].position.x,
                              V[0].position.y - V[2].position.y);
            }

            public void Save(List<UIVertex> verts, int index)
            {
                verts[index] = V[0];
                verts[index + 1] = V[1];
                verts[index + 2] = V[2];
                verts[index + 3] = V[3];
            }
#if !UNITY_5_1 && !UNITY_5_0 && !UNITY_4_6
            public void Save(VertexHelper vh)
            {
                vh.AddUIVertexQuad(V);
            }
#endif

            public void Transpose(Vector3 v)
            {
                for (int i = 0; i < 4; i++)
                    V[i].position += v;

            }

            public void Rotate(Quaternion rotation)
            {
                for (int i = 0; i < 4; i++)
                    V[i].position = V[i].position.RotateAround(Center, rotation);
            }

        }

        protected class GlyphPlain
        {
            public Vector3[] V = new Vector3[4];
            public Rect Rect;
            public Vector3 Center { get { return Rect.center; } }

            public void Load(ref Vector3[] verts, int index)
            {
                V[0] = verts[index];
                V[1] = verts[index + 1];
                V[2] = verts[index + 2];
                V[3] = verts[index + 3];

                calcRect();
            }

            public void calcRect()
            {
                Rect = new Rect(V[0].x,
                              V[2].y,
                              V[2].x - V[0].x,
                              V[0].y - V[2].y);
            }

            public void Save(ref Vector3[] verts, int index)
            {
                verts[index] = V[0];
                verts[index + 1] = V[1];
                verts[index + 2] = V[2];
                verts[index + 3] = V[3];
            }

            public void Transpose(Vector3 v)
            {
                for (int i = 0; i < 4; i++)
                    V[i] += v;

            }

            public void Rotate(Quaternion rotation)
            {
                for (int i = 0; i < 4; i++)
                    V[i] = V[i].RotateAround(Center, rotation);
            }

        }

        Graphic m_Graphic;
        RectTransform mRect;
        Text mText;



        protected Text Text
        {
            get
            {
                if (mText == null)
                    mText = GetComponent<Text>();
                return mText;
            }
        }

        protected RectTransform Rect
        {
            get
            {
                if (mRect == null)
                    mRect = GetComponent<RectTransform>();
                return mRect;
            }
        }

        protected Graphic graphic
        {
            get
            {
                if (m_Graphic == null)
                    m_Graphic = GetComponent<Graphic>();

                return m_Graphic;
            }
        }




        public override void Refresh()
        {
            base.Refresh();
            graphic.SetVerticesDirty();
        }

        protected override void OnRefreshSpline(CurvySplineEventArgs e)
        {
            base.OnRefreshSpline(e);
            graphic.SetVerticesDirty();
        }


        //4.6,5.0,5.1
        public virtual void ModifyVertices(List<UIVertex> verts)
        {
            if (enabled && gameObject.activeInHierarchy)
            {
                var glyph = new GlyphQuad();
                for (int c = 0; c < Text.text.Length; c++)
                {
                    glyph.Load(verts, c * 4);

                    float xDistance = AbsolutePosition + glyph.Rect.center.x;
                    float tf = AbsoluteToRelative(xDistance);

                    Vector3 pos = this.GetInterpolatedSourcePosition(tf);
                    Vector3 tan = this.GetTangent(tf);
                    var off = pos - Rect.localPosition - glyph.Center; // position offset to spline

                    glyph.Transpose(new Vector3(0, glyph.Center.y, 0)); // shift to match baseline
                    // Rotate, then offset to real position
                    glyph.Rotate(Quaternion.AngleAxis(Mathf.Atan2(tan.x, -tan.y) * Mathf.Rad2Deg - 90, Vector3.forward));
                    glyph.Transpose(off);

                    glyph.Save(verts, c * 4);
                }
            }
        }

        //5.2.0
        public void ModifyMesh(Mesh verts)
        {
            if (enabled && gameObject.activeInHierarchy && IsInitialized)
            {
                var vtArray = verts.vertices;
                var glyph = new GlyphPlain();
                for (int c = 0; c < Text.text.Length; c++)
                {
                    glyph.Load(ref vtArray, c * 4);

                    float xDistance = AbsolutePosition + glyph.Rect.center.x;
                    float tf = AbsoluteToRelative(xDistance);

                    Vector3 pos = this.GetInterpolatedSourcePosition(tf);
                    Vector3 tan = this.GetTangent(tf);
                    var off = pos - Rect.localPosition - glyph.Center; // position offset to spline

                    glyph.Transpose(new Vector3(0, glyph.Center.y, 0)); // shift to match baseline
                    // Rotate, then offset to real position
                    glyph.Rotate(Quaternion.AngleAxis(Mathf.Atan2(tan.x, -tan.y) * Mathf.Rad2Deg - 90, Vector3.forward));
                    glyph.Transpose(off);

                    glyph.Save(ref vtArray, c * 4);
                }
                verts.vertices = vtArray;
            }
        }

#if !UNITY_5_1 && !UNITY_5_0 && !UNITY_4_6
        //5.2.1p1ff
        public void ModifyMesh(VertexHelper vh)
        {
            
            if (enabled && gameObject.activeInHierarchy && IsInitialized)
            {
                var verts = new List<UIVertex>();
                var glyph = new GlyphQuad();
                
                vh.GetUIVertexStream(verts);
#if UNITY_5_2
                vh.Dispose();
#else
                vh.Clear();
#endif

                for (int c = 0; c < Text.text.Length; c++)
                {

                    glyph.LoadTris(verts, c * 6);

                    float xDistance = AbsolutePosition + glyph.Rect.center.x;
                    float tf = AbsoluteToRelative(xDistance);

                    Vector3 pos = this.GetInterpolatedSourcePosition(tf);
                    Vector3 tan = this.GetTangent(tf);
                    var off = pos - Rect.localPosition - glyph.Center; // position offset to spline

                    glyph.Transpose(new Vector3(0, glyph.Center.y, 0)); // shift to match baseline
                    // Rotate, then offset to real position
                    glyph.Rotate(Quaternion.AngleAxis(Mathf.Atan2(tan.x, -tan.y) * Mathf.Rad2Deg - 90, Vector3.forward));
                    glyph.Transpose(off);
                    glyph.Save(vh);
                }

            }
        }
#endif


        #region ### Unity Callbacks ###

        protected override void OnEnable()
        {
            base.OnEnable();
            if (this.graphic != null)
            {
                this.graphic.SetVerticesDirty();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (this.graphic != null)
            {
                this.graphic.SetVerticesDirty();
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (this.graphic != null)
            {
                this.graphic.SetVerticesDirty();
            }
        }
#endif

        #endregion

    }
}
