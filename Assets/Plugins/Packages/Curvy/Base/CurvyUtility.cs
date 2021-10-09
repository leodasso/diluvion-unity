// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using FluffyUnderware.Curvy.ThirdParty.LibTessDotNet;
#if UNITY_EDITOR
using UnityEditor;
#endif
using FluffyUnderware.DevTools;


namespace FluffyUnderware.Curvy.Utils
{

    /// <summary>
    /// Curvy Utility class
    /// </summary>
    public class CurvyUtility
    {
        #region ### Clamping Methods ###

        /// <summary>
        /// Clamps relative position
        /// </summary>
        public static float ClampTF(float tf, CurvyClamping clamping)
        {
            switch (clamping)
            {
                case CurvyClamping.Loop:
                    return Mathf.Repeat(tf, 1);
                case CurvyClamping.PingPong:
                    return Mathf.PingPong(tf, 1);
                default:
                    return Mathf.Clamp01(tf);
            }
        }
        /// <summary>
        /// Clamps a float to a range
        /// </summary>
        public static float ClampValue(float tf, CurvyClamping clamping, float minTF, float maxTF)
        {
            
            switch (clamping)
            {
                case CurvyClamping.Loop:
                    float v1 = DTMath.MapValue(0, 1, tf, minTF, maxTF);
                    return DTMath.MapValue(minTF,maxTF,Mathf.Repeat(v1, 1),0,1);
                case CurvyClamping.PingPong:
                    float v2 = DTMath.MapValue(0, 1, tf, minTF, maxTF);
                    return DTMath.MapValue(minTF,maxTF,Mathf.PingPong(v2, 1),0,1);
                default:
                    return Mathf.Clamp(tf, minTF, maxTF);
            }
        }


        /// <summary>
        /// Clamps relative position and sets new direction
        /// </summary>
        public static float ClampTF(float tf, ref int dir, CurvyClamping clamping)
        {
            switch (clamping)
            {
                case CurvyClamping.Loop:
                    return Mathf.Repeat(tf, 1);
                case CurvyClamping.PingPong:
                    if (Mathf.FloorToInt(tf) % 2 != 0)
                        dir *= -1;
                    return Mathf.PingPong(tf, 1);
                default:
                    return Mathf.Clamp01(tf);
            }
        }

        /// <summary>
        /// Clamps relative position and sets new direction
        /// </summary>
        public static float ClampTF(float tf, ref int dir, CurvyClamping clamping, float minTF, float maxTF)
        {
            minTF = Mathf.Clamp01(minTF);
            maxTF = Mathf.Clamp(maxTF, minTF, 1);

            switch (clamping)
            {
                case CurvyClamping.Loop:
                    return minTF + Mathf.Repeat(tf, maxTF - minTF);
                case CurvyClamping.PingPong:
                    if (Mathf.FloorToInt(tf / (maxTF - minTF)) % 2 != 0)
                        dir *= -1;
                    return minTF + Mathf.PingPong(tf, maxTF - minTF);
                default:
                    return Mathf.Clamp(tf, minTF, maxTF);
            }
        }

        /// <summary>
        /// Clamps absolute position
        /// </summary>
        public static float ClampDistance(float distance, CurvyClamping clamping, float length)
        {
            if (length == 0)
                return 0;
            switch (clamping)
            {
                case CurvyClamping.Loop:
                    return Mathf.Repeat(distance, length);
                case CurvyClamping.PingPong:
                    return Mathf.PingPong(distance, length);
                default:
                    return Mathf.Clamp(distance, 0, length);
            }
        }

        /// <summary>
        /// Clamps absolute position
        /// </summary>
        public static float ClampDistance(float distance, CurvyClamping clamping, float length, float min, float max)
        {
            if (length == 0)
                return 0;
            min = Mathf.Clamp(min, 0, length);
            max = Mathf.Clamp(max, min, length);
            switch (clamping)
            {
                case CurvyClamping.Loop:
                    return min + Mathf.Repeat(distance, max - min);
                case CurvyClamping.PingPong:
                    return min + Mathf.PingPong(distance, max - min);
                default:
                    return Mathf.Clamp(distance, min, max);
            }
        }

        /// <summary>
        /// Clamps absolute position and sets new direction
        /// </summary>
        public static float ClampDistance(float distance, ref int dir, CurvyClamping clamping, float length)
        {
            if (length == 0)
                return 0;
            switch (clamping)
            {
                case CurvyClamping.Loop:
                    return Mathf.Repeat(distance, length);
                case CurvyClamping.PingPong:
                    if (Mathf.FloorToInt(distance / length) % 2 != 0)
                        dir *= -1;
                    return Mathf.PingPong(distance, length);
                default:
                    return Mathf.Clamp(distance, 0, length);
            }
        }

        /// <summary>
        /// Clamps absolute position and sets new direction
        /// </summary>
        public static float ClampDistance(float distance, ref int dir, CurvyClamping clamping, float length, float min, float max)
        {
            if (length == 0)
                return 0;
            min = Mathf.Clamp(min, 0, length);
            max = Mathf.Clamp(max, min, length);
            switch (clamping)
            {
                case CurvyClamping.Loop:
                    return min + Mathf.Repeat(distance, max - min);
                case CurvyClamping.PingPong:
                    if (Mathf.FloorToInt(distance / (max - min)) % 2 != 0)
                        dir *= -1;
                    return min + Mathf.PingPong(distance, max - min);
                default:
                    return Mathf.Clamp(distance, min, max);
            }
        }

        #endregion

        /// <summary>
        /// Gets the default material, i.e. /Packages/Curvy/Resources/CurvyDefaultMaterial
        /// </summary>
        public static Material GetDefaultMaterial()
        {
            var mat = Resources.Load("CurvyDefaultMaterial") as Material;
            if (mat == null)
            {
                mat = new Material(Shader.Find("Diffuse"));
            }
            return mat;
        }

       

    }

    #region ### Spline2Mesh ###

    /// <summary>
    /// Class to create a Mesh from a set of splines
    /// </summary>
    public class Spline2Mesh
    {
        #region ### Public Fields & Properties ###
        /// <summary>
        /// A list of splines (X/Y only) forming the resulting mesh
        /// </summary>
        public List<SplinePolyLine> Lines = new List<SplinePolyLine>();
        /// <summary>
        /// Winding rule used by triangulator
        /// </summary>
        public WindingRule Winding = WindingRule.EvenOdd;
        public Vector2 UVTiling = Vector2.one;
        public Vector2 UVOffset = Vector2.zero;
        public bool SuppressUVMapping;
        /// <summary>
        /// Whether UV2 should be set
        /// </summary>
        public bool UV2;
        /// <summary>
        /// Name of the returned mesh
        /// </summary>
        public string MeshName = string.Empty;
        /// <summary>
        /// Whether only vertices of the outline spline should be created
        /// </summary>
        public bool VertexLineOnly;

        public string Error { get; private set; }

        #endregion

        #region ### Private Fields ###

        bool mUseMeshBounds;
        Vector2 mNewBounds;
        Tess mTess;
        Mesh mMesh;

        #endregion

        #region ### Public Methods ###

        /// <summary>
        /// Create the Mesh using the current settings
        /// </summary>
        /// <param name="result">the resulting Mesh</param>
        /// <returns>true on success. If false, check the Error property!</returns>
        public bool Apply(out Mesh result)
        {
            mTess = null;
            mMesh = null;
            Error = string.Empty;
            if (triangulate())
            {
                if (buildMesh())
                {
                    if (!SuppressUVMapping && !VertexLineOnly)
                        uvmap();
                    result = mMesh;
                    return true;
                }
            }
            if (mMesh)
                mMesh.RecalculateNormals();
            result = mMesh;
            return false;
        }

        /// <summary>
        /// Sets bounds used by UV calculation
        /// </summary>
        /// <param name="useMeshBounds">whether to use mesh bounds</param>
        /// <param name="newSize">the new size</param>
        public void SetBounds(bool useMeshBounds, Vector2 newSize)
        {
            mUseMeshBounds = useMeshBounds;
            mNewBounds = newSize;
        }

        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */

        bool triangulate()
        {
            if (Lines.Count==0)
            {
                Error = "Missing splines to triangulate";
                return false;
            }

            if (VertexLineOnly)
                return true;

            mTess = new Tess();

            for (int i = 0; i < Lines.Count; i++)
            {
                if (Lines[i].Spline == null)
                {
                    Error = "Missing Spline";
                    return false;
                }
                if (!polyLineIsValid(Lines[i]))
                {
                    Error = Lines[i].Spline.name + ": Angle must be >0";
                    return false;
                }
                Vector3[] verts = Lines[i].GetVertices();
                if (verts.Length < 3)
                {
                    Error = Lines[i].Spline.name + ": At least 3 Vertices needed!";
                    return false;
                }
                mTess.AddContour(UnityLibTessUtility.ToContourVertex(verts,true), Lines[i].Orientation);
            }
            try
            {
                mTess.Tessellate(Winding, ElementType.Polygons, 3);
                return true;
            }
            catch (System.Exception e)
            {
                Error = e.Message;
            }

            return false;
        }

        bool polyLineIsValid(SplinePolyLine pl)
        {
            return (pl != null && pl.VertexMode == SplinePolyLine.VertexCalculation.ByApproximation ||
                    !Mathf.Approximately(0, pl.Angle));
        }

        bool buildMesh()
        {
            mMesh = new Mesh();
            mMesh.name = MeshName;

            if (VertexLineOnly && Lines.Count>0 && Lines[0]!=null)
            {
                Vector3[] vts = Lines[0].GetVertices();
                for (int i = 0; i < vts.Length; i++)
                    vts[i].z = 0;
                mMesh.vertices = vts;
            }
            else
            {
                mMesh.vertices = UnityLibTessUtility.FromContourVertex(mTess.Vertices);
                mMesh.triangles = mTess.Elements;
            }
            mMesh.RecalculateBounds();
            return true;
        }

        void uvmap()
        {
            Bounds bounds = mMesh.bounds;

            Vector2 UVBounds = bounds.size;
            if (!mUseMeshBounds)
                UVBounds = mNewBounds;

            Vector3[] vt = mMesh.vertices;

            Vector2[] uv = new Vector2[vt.Length];

            float maxU = 0;
            float maxV = 0;

            for (int i = 0; i < vt.Length; i++)
            {
                float u = UVOffset.x + (vt[i].x - bounds.min.x) / UVBounds.x;
                float v = UVOffset.y + (vt[i].y - bounds.min.y) / UVBounds.y;
                u *= UVTiling.x;
                v *= UVTiling.y;
                maxU = Mathf.Max(u, maxU);
                maxV = Mathf.Max(v, maxV);
                uv[i] = new Vector2(u, v);
            }
            mMesh.uv = uv;
            Vector2[] uv2 = new Vector2[0];
            if (UV2)
            {
                uv2 = new Vector2[uv.Length];
                for (int i = 0; i < vt.Length; i++)
                    uv2[i] = new Vector2(uv[i].x / maxU, uv[i].y / maxV);
            }
            mMesh.uv2 = uv2;
            mMesh.RecalculateNormals();
        }

        /*! \endcond */
        #endregion

        #region ### OBSOLETE ###
        /*! \cond OBSOLETE */

        [System.Obsolete("Use Lines instead!")]
        public SplinePolyLine Outline;
        [System.Obsolete("Use Lines instead!")]
        public List<SplinePolyLine> Holes = new List<SplinePolyLine>();

        /*! \endcond */
        #endregion
    }

    /// <summary>
    /// Spline Triangulation Helper Class
    /// </summary>
    [System.Serializable]
    public class SplinePolyLine
    {
        /// <summary>
        /// How to calculate vertices
        /// </summary>
        public enum VertexCalculation
        {
            /// <summary>
            /// Use Approximation points
            /// </summary>
            ByApproximation,
            /// <summary>
            /// By curvation angle
            /// </summary>
            ByAngle
        }

        /// <summary>
        /// Orientation order
        /// </summary>
        public ContourOrientation Orientation = ContourOrientation.Original;

        /// <summary>
        /// Base Spline
        /// </summary>
        public CurvySplineBase Spline;
        /// <summary>
        /// Vertex Calculation Mode
        /// </summary>
        public VertexCalculation VertexMode;
        /// <summary>
        /// Angle, used by VertexMode.ByAngle only
        /// </summary>
        public float Angle;
        /// <summary>
        /// Minimum distance, used by VertexMode.ByAngle only
        /// </summary>
        public float Distance;
        public Space Space;

        /// <summary>
        /// Creates a Spline2MeshCurve class using Spline2MeshCurve.VertexMode.ByApproximation
        /// </summary>
        public SplinePolyLine(CurvySplineBase spline) : this(spline, VertexCalculation.ByApproximation, 0, 0) { }
        /// <summary>
        /// Creates a Spline2MeshCurve class using Spline2MeshCurve.VertexMode.ByAngle
        /// </summary>
        public SplinePolyLine(CurvySplineBase spline, float angle, float distance) : this(spline, VertexCalculation.ByAngle, angle, distance) { }

        SplinePolyLine(CurvySplineBase spline, VertexCalculation vertexMode, float angle, float distance, Space space=Space.World)
        {
            Spline = spline;
            VertexMode = vertexMode;
            Angle = angle;
            Distance = distance;
            Space = space;
        }
        /// <summary>
        /// Gets whether the spline is closed
        /// </summary>
        public bool IsClosed
        {
            get
            {
                return (Spline && Spline.IsClosed);
            }
        }

        /// <summary>
        /// Gets whether the spline is continuous
        /// </summary>
        public bool IsContinuous
        {
            get
            {
                return (Spline && Spline.IsContinuous);
            }
        }

        /// <summary>
        /// Get vertices calculated using the current VertexMode
        /// </summary>
        /// <returns>an array of vertices</returns>
        public Vector3[] GetVertices()
        {
            Vector3[] points = new Vector3[0];
            switch (VertexMode)
            {
                case VertexCalculation.ByAngle:
                    List<float> tf;
                    List<Vector3> tan;
                    points = Spline.GetPolygon(0, 1, Angle, Distance, -1, out tf, out tan, false);
                    break;
                default:
                    points=Spline.GetApproximation();
                    break;
            }
            if (Space == Space.World)
            {
                for (int i = 0; i < points.Length; i++)
                    points[i] = Spline.transform.TransformPoint(points[i]);
            }
            return points;
        }

        [System.Obsolete("Use SplinePolyLine.GetVertices() instead!")]
        public Vector3[] getVertices() { return GetVertices(); }

    }
    #endregion

    #region ### Import/Export ###

    public static class SerializedCurvyObjectHelper
    {
        public static System.Type GetJsonSerializedType(string json)
        {
            json = json.Substring(0, Mathf.Min(json.Length, 70));
            json = System.Text.RegularExpressions.Regex.Replace(json, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*) (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)", "");
            json = System.Text.RegularExpressions.Regex.Replace(json, "[\f\n\r\t\v]", "");
            
            if (json.StartsWith("{\"ControlPoints\":["))
                return typeof(SerializedCurvySplineSegmentCollection);
            else if (json.StartsWith("{\"Splines\":["))
                return typeof(SerializedCurvySplineCollection);
            else
                return null;
        }
    }

    public enum CurvySerializationSpace
    {
        World=0,
        Self=1,
        WorldSpline=2
    }

    public abstract class SerializedCurvyObject<T>
    {
        public string ToJson()
        {
#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            return string.Empty;
#else
            return JsonUtility.ToJson(this);
#endif
        }

        public static T FromJson(string json)
        {
#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2
            return default(T);
#else
            return JsonUtility.FromJson<T>(json);
#endif
        }

    }

    public class SerializedCurvySplineCollection : SerializedCurvyObject<SerializedCurvySplineCollection>
    {
        public SerializedCurvySpline[] Splines = new SerializedCurvySpline[0];
        public string Data = string.Empty;

        public SerializedCurvySplineCollection(List<CurvySpline> splines, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline)
        {
            Splines = new SerializedCurvySpline[splines.Count];
            for (int i = 0; i < splines.Count; i++)
                Splines[i] = new SerializedCurvySpline(splines[i], space);
        }

        public CurvySpline[] Deserialize(Transform parent=null, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline, Action<CurvySpline, string> onDeserializedSpline = null, Action<CurvySplineSegment, string> onDeserializedCP = null)
        {
            var res = new CurvySpline[Splines.Length];
            for (int i = 0; i < Splines.Length; i++)
            {
                res[i] = Splines[i].Deserialize(parent, space, onDeserializedCP);
                if (onDeserializedSpline!=null)
                    onDeserializedSpline(res[i], Splines[i].Data);
            }
            return res;
        }

        public CurvySpline[] Deserialize(CurvySpline[] splines, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline, Action<CurvySpline, string> onDeserializedSpline = null, Action<CurvySplineSegment, string> onDeserializedCP = null)
        {
            var res = new CurvySpline[Splines.Length];
            for (int i = 0; i < Splines.Length; i++)
            {
                res[i] = Splines[i].Deserialize(splines[i], space, onDeserializedCP);
                if (onDeserializedSpline != null)
                    onDeserializedSpline(res[i], Splines[i].Data);
            }
            return res;
        }

    }

    /// <summary>
    /// Collection of serialized Control Points
    /// </summary>
    public class SerializedCurvySplineSegmentCollection : SerializedCurvyObject<SerializedCurvySplineSegmentCollection>
    {
        public SerializedCurvySplineSegment[] ControlPoints = new SerializedCurvySplineSegment[0];
        public string Data = string.Empty;

        public SerializedCurvySplineSegmentCollection(List<CurvySplineSegment> cps, CurvySerializationSpace space=CurvySerializationSpace.WorldSpline)
        {
            ControlPoints = new SerializedCurvySplineSegment[cps.Count];
            for (int i = 0; i < cps.Count; i++)
                ControlPoints[i] = new SerializedCurvySplineSegment(cps[i], space);
        }

        public CurvySplineSegment[] Deserialize(CurvySpline spline, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline, Action<CurvySplineSegment,string> onDeserializedCP=null)
        {
            var res = new CurvySplineSegment[ControlPoints.Length];
            if (spline != null)
            {
                for (int i = 0; i < ControlPoints.Length; i++)
                {
                    res[i] = ControlPoints[i].Deserialize(spline, space);
                    if (onDeserializedCP!=null)
                        onDeserializedCP(res[i], ControlPoints[i].Data);
                    
                }
            }
            return res;
        }

        public CurvySplineSegment[] Deserialize(CurvySplineSegment controlPoint, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline, Action<CurvySplineSegment, string> onDeserializedCP = null)
        {
            var res = new CurvySplineSegment[ControlPoints.Length];
            if (controlPoint)
            {
                CurvySplineSegment last = controlPoint;
                for (int i=0;i<ControlPoints.Length;i++)
                {
                    last = ControlPoints[i].Deserialize(last, space);
                    res[i] = last;
                    if (onDeserializedCP!=null)
                        onDeserializedCP(last, ControlPoints[i].Data);
                }
            }
            return res;
        }

    }

    /// <summary>
    /// Serialized CurvySpline
    /// </summary>
    [System.Serializable]
    public class SerializedCurvySpline : SerializedCurvyObject<SerializedCurvySpline>
    {
        public string Name = string.Empty;
        public Vector3 P;
        public Vector3 R;
        public CurvyInterpolation Interpolation = CurvyGlobalManager.DefaultInterpolation;
        public bool Keep2D;
        public bool Closed;
        public bool AutoEndTangents = true;
        public CurvyOrientation Orientation = CurvyOrientation.Dynamic;
        public float BzAutoDist=0.39f;
        public int CacheDensity = 50;
        public bool Pooling = true;
        public bool Threading = false;
        public bool CheckTForm = false;
        public CurvyUpdateMethod UpdateIn = CurvyUpdateMethod.Update;
        public SerializedCurvySplineSegment[] ControlPoints = new SerializedCurvySplineSegment[0];
        public string Data = string.Empty;

        public SerializedCurvySpline() { }
        public SerializedCurvySpline(CurvySpline spline, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline)
        {
            if (spline)
            {
                Name = spline.name;
                P = (space == CurvySerializationSpace.Self) ? spline.transform.localPosition : spline.transform.position;
                R = (space == CurvySerializationSpace.Self) ? spline.transform.localRotation.eulerAngles : spline.transform.rotation.eulerAngles;
                Interpolation = spline.Interpolation;
                Keep2D = spline.RestrictTo2D;
                Closed = spline.Closed;
                AutoEndTangents = spline.AutoEndTangents;
                Orientation = spline.Orientation;
                BzAutoDist = spline.AutoHandleDistance;
                CacheDensity = spline.CacheDensity;
                Pooling = spline.UsePooling;
                Threading = spline.UseThreading;
                CheckTForm = spline.CheckTransform;
                UpdateIn = spline.UpdateIn;
                ControlPoints = new SerializedCurvySplineSegment[spline.ControlPointCount];
                for (int i = 0; i < spline.ControlPointCount; i++)
                    ControlPoints[i] = new SerializedCurvySplineSegment(spline.ControlPoints[i]);
            }
        }


        public CurvySpline Deserialize(Transform parent=null, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline, Action<CurvySplineSegment, string> onDeserializedCP = null)
        {
            var spl = CurvySpline.Create();
            if (!string.IsNullOrEmpty(Name))
                spl.name = Name;
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(spl.gameObject, "Deserialize");
#endif
            spl.transform.SetParent(parent);
            DeserializeInto(spl, space, onDeserializedCP);
            return spl;
        }

        public CurvySpline Deserialize(CurvySpline spline, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline, Action<CurvySplineSegment, string> onDeserializedCP = null)
        {
            if (!string.IsNullOrEmpty(Name))
                spline.name = Name;

            DeserializeInto(spline, space, onDeserializedCP);
            return spline;
        }

        public void DeserializeInto(CurvySpline spline, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline, Action<CurvySplineSegment, string> onDeserializedCP = null)
        {
            if (spline)
            {
#if UNITY_EDITOR
                Undo.RegisterCompleteObjectUndo(spline, "Deserialize");
#endif

                if (!string.IsNullOrEmpty(Name))
                    spline.name = Name;
                spline.Clear();
                if (space == CurvySerializationSpace.Self)
                {
                    spline.transform.localPosition = P;
                    spline.transform.localRotation = Quaternion.Euler(R);
                }
                else
                {
                    spline.transform.position = P;
                    spline.transform.rotation = Quaternion.Euler(R);
                }
                spline.Interpolation = Interpolation;
                spline.RestrictTo2D = Keep2D;
                spline.Closed = Closed;
                spline.AutoEndTangents = AutoEndTangents;
                spline.Orientation = Orientation;
                spline.AutoHandleDistance = BzAutoDist;
                spline.CacheDensity = CacheDensity;
                spline.UsePooling = Pooling;
                spline.UseThreading = Threading;
                spline.CheckTransform = CheckTForm;
                spline.UpdateIn = UpdateIn;
                CurvySplineSegment cp;
                foreach (var serCP in ControlPoints)
                {
                    cp=serCP.Deserialize(spline, space);
                    if (onDeserializedCP!=null)
                        onDeserializedCP(cp, serCP.Data);
                }
            }
        }
        
    }

    /// <summary>
    /// Serialized Control Point
    /// </summary>
    [System.Serializable]
    public class SerializedCurvySplineSegment : SerializedCurvyObject<SerializedCurvySplineSegment>
    {
        public Vector3 P;
        public Vector3 R;
        public bool Bake;
        public bool Anchor;
        public CurvyOrientationSwirl Swirl = CurvyOrientationSwirl.None;
        public float SwirlT;
        public bool BzAuto = true;
        public float BzAutoDist = 0.39f;
        public Vector3 BzOut = new Vector3(1, 0, 0);
        public Vector3 BzIn = new Vector3(-1, 0, 0);
        public string Data = string.Empty;

        public SerializedCurvySplineSegment() { }

        public SerializedCurvySplineSegment(CurvySplineSegment segment, CurvySerializationSpace space=CurvySerializationSpace.WorldSpline)
        {
            if (segment)
            {
                P = (space == CurvySerializationSpace.World) ? segment.position : segment.localPosition;
                R = (space == CurvySerializationSpace.World) ? segment.rotation.eulerAngles : segment.localRotation.eulerAngles;
                Bake = segment.AutoBakeOrientation;
                Anchor = segment.OrientationAnchor;
                Swirl = segment.Swirl;
                SwirlT = segment.SwirlTurns;
                BzAuto = segment.AutoHandles;
                BzAutoDist = segment.AutoHandleDistance;
                BzOut = segment.HandleOut;
                BzIn = segment.HandleIn;
            }
        }

        public CurvySplineSegment Deserialize(CurvySpline spline, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline)
        {
            if (spline != null)
            {
                if (spline.ControlPointCount > 0)
                    return Deserialize(spline.ControlPoints[spline.ControlPointCount - 1], space);
                else
                {
                    var cp=spline.Add();
#if UNITY_EDITOR
                    Undo.RegisterCreatedObjectUndo(cp.gameObject, "Deserialize");
#endif
                    DeserializeInto(cp, space);
                    return cp;
                }
            }
            return null;
        }

        public CurvySplineSegment Deserialize(CurvySplineSegment controlPoint, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline)
        {
            if (controlPoint)
            {
                var cp = controlPoint.Spline.InsertAfter(controlPoint);
#if UNITY_EDITOR
                Undo.RegisterCreatedObjectUndo(cp.gameObject, "Deserialize");
#endif
                DeserializeInto(cp, space);
                return cp;
            }
            return null;
        }

        public void DeserializeInto(CurvySplineSegment controlPoint, CurvySerializationSpace space = CurvySerializationSpace.WorldSpline)
        {
            if (controlPoint)
            {
#if UNITY_EDITOR
                Undo.RegisterCompleteObjectUndo(controlPoint, "Deserialize");
#endif
                if (space == CurvySerializationSpace.World)
                {
                    controlPoint.position = P;
                    controlPoint.rotation = Quaternion.Euler(R);
                }
                else
                {
                    controlPoint.localPosition = P;
                    controlPoint.localRotation = Quaternion.Euler(R);
                }
                controlPoint.AutoBakeOrientation = Bake;
                controlPoint.OrientationAnchor = Anchor;
                controlPoint.Swirl = Swirl;
                controlPoint.SwirlTurns = SwirlT;
                controlPoint.AutoHandles = BzAuto;
                controlPoint.AutoHandleDistance = BzAutoDist;
                controlPoint.SetBezierHandleIn(BzIn);
                controlPoint.SetBezierHandleOut(BzOut);
            }
        }

    }


#endregion

}
