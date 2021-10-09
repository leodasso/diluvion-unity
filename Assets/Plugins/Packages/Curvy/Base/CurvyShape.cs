// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using System;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools.Extensions;


namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Base class for CurvyShape components
    /// </summary>
    [RequireComponent(typeof(CurvySpline))]
    //[DisallowMultipleComponent]
    [ExecuteInEditMode]
    [HelpURL(CurvySpline.DOCLINK + "curvyshape")]
    public class CurvyShape : DTVersionedMonoBehaviour
    {
        #region ### Serialized Fields ###

        [SerializeField, Label("Plane")]
        CurvyPlane m_Plane = CurvyPlane.XY;

        [SerializeField, HideInInspector]
        bool m_Persistent = true;

        #endregion

        #region ### Public Properties ###

        /// <summary>
        /// Gets or sets the plane to create the shape in
        /// </summary>
        public CurvyPlane Plane
        {
            get { return m_Plane; }
            set
            {
                if (m_Plane != value)
                {
                    m_Plane = value;
                    Dirty = true;
                }
            }
        }

        /// <summary>
        /// Whether this component is persistent or not
        /// </summary>
        public bool Persistent
        {
            get { return m_Persistent; }
            set
            {
                if (m_Persistent != value)
                {
                    m_Persistent = value;
                    hideFlags = (value) ? HideFlags.None : HideFlags.HideInInspector;
                }
            }
        }

        /// <summary>
        /// Gets the attached spline
        /// </summary>
        public CurvySpline Spline
        {
            get
            {
                if (!mSpline)
                    mSpline = GetComponent<CurvySpline>();
                return mSpline;
            }
        }

        #endregion

        #region ### Private fields ###

        static Dictionary<CurvyShapeInfo, Type> mShapeDefs = new Dictionary<CurvyShapeInfo, System.Type>();

        CurvySpline mSpline;
        [System.NonSerialized]
        public bool Dirty;

        bool mLoadingInEditor=true;

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */
        void Update()
        {
            hideFlags = (Persistent) ? HideFlags.None : HideFlags.HideInInspector;
            
            // Prevent updating while dragging prefab
#if UNITY_EDITOR
            //if (Selection.activeGameObject==gameObject)
#endif
          
                Refresh();
        }

#if UNITY_EDITOR

        void OnEnable()
        {
            hideFlags = (Persistent) ? HideFlags.None : HideFlags.HideInInspector;
            mLoadingInEditor = false;
        }

        protected virtual void OnValidate()
        {
            if (!mLoadingInEditor)
                Dirty = true;
        }
#endif

        protected virtual void Reset()
        {
            Plane = CurvyPlane.XY;
        }
        /*! \endcond */
        
        #endregion

        #region ### Public Methods ###

        /// <summary>
        /// Remove the CurvyShape component from it's GameObject
        /// </summary>
        public void Delete()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.DestroyObjectImmediate(this);
            else
#endif
                Destroy(this);
        }

        /// <summary>
        /// Called to refresh the shape. Please call base.Refresh() or RefreshSpline() after your custom code!
        /// </summary>
        /// <remarks>Warning: Only work with ControlPoints, not with segments</remarks>
        public void Refresh()
        {
            if (Spline && Spline.IsInitialized && Dirty)
            {
                ApplyShape();
                applyPlane();
                Spline.Refresh();
            }
            Dirty = false;
        }


        /// <summary>
        /// Replace the current script with another shape's script
        /// </summary>
        /// <returns>the new shape script</returns>
        public CurvyShape Replace(string menuName)
        {
            bool isPersistent = Persistent;
            var shapeType = GetShapeType(menuName);
            if (shapeType != null)
            {
                var go = this.gameObject;

                Delete();
                var shp = (CurvyShape)go.AddComponent(shapeType);
                shp.Persistent = isPersistent;
                shp.Dirty = true;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Undo.RegisterCreatedObjectUndo(shp, "Apply Shape");
                }
#endif
                return shp;
            }
            return null;
        }


        #endregion

        #region ### Protected Methods ###

        /// <summary>
        /// Sets basic spline parameters
        /// </summary>
        protected void PrepareSpline(CurvyInterpolation interpolation, CurvyOrientation orientation = CurvyOrientation.Dynamic, int cachedensity = 50, bool closed = true)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RecordObject(Spline, "Apply Shape");
#endif
            Spline.Interpolation = interpolation;
            Spline.Orientation = orientation;
            Spline.CacheDensity = cachedensity;
            Spline.Closed = closed;
            Spline.RestrictTo2D = this is CurvyShape2D;
        }

        /// <summary>
        /// Sets a Control Point's position by index
        /// </summary>
        /// <param name="no">Control point index</param>
        /// <param name="position">local position</param>
        protected void SetPosition(int no, Vector3 position)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RecordObject(Spline.ControlPoints[no].transform, "Apply Shape");
#endif
            Spline.ControlPoints[no].localPosition = position;
        }

        /// <summary>
        /// Sets a Control Point's rotation by index
        /// </summary>
        /// <param name="no">Control point index</param>
        /// <param name="rotation">local rotation</param>
        protected void SetRotation(int no, Quaternion rotation)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RecordObject(Spline.ControlPoints[no].transform, "Apply Shape");
#endif
            Spline.ControlPoints[no].localRotation = rotation;
        }

        /// <summary>
        /// Sets a Control Point's Bezier HandleIn position by index
        /// </summary>
        /// <param name="no">Control point index</param>
        /// <param name="inHandle">local position of HandleIn</param>
        protected void SetBezierHandleIn(int no, Vector3 inHandle)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RecordObject(Spline.ControlPoints[no], "Apply Shape");
#endif
            Spline.ControlPoints[no].HandleIn = inHandle;
        }

        /// <summary>
        /// Sets a Control Point's Bezier HandleOut position by index
        /// </summary>
        /// <param name="no">Control point index</param>
        /// <param name="outHandle">local position of HandleOut</param>
        protected void SetBezierHandleOut(int no, Vector3 outHandle)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RecordObject(Spline.ControlPoints[no], "Apply Shape");
#endif
            Spline.ControlPoints[no].HandleOut = outHandle;
        }

        /// <summary>
        /// Sets a Control Point's Bezier Handles by index
        /// </summary>
        /// <param name="no">Control point index</param>
        /// <param name="distanceFrag">distance in percent</param>
        protected void SetBezierHandles(int no, float distanceFrag)
        {
            SetBezierHandles(no, distanceFrag, distanceFrag);
        }

        /// <summary>
        /// Sets a Control Point's Bezier Handles by index
        /// </summary>
        /// <param name="no">Control point index</param>
        /// <param name="inDistanceFrag">distance in percent for HandleIn</param>
        /// /// <param name="outDistanceFrag">distance in percent for HandleOut</param>
        protected void SetBezierHandles(int no, float inDistanceFrag, float outDistanceFrag)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RecordObject(Spline.ControlPoints[no], "Apply Shape");
#endif
            if (no >= 0 && no < Spline.ControlPointCount)
            {
                if (inDistanceFrag == outDistanceFrag)
                {
                    Spline.ControlPoints[no].AutoHandles = true;
                    Spline.ControlPoints[no].AutoHandleDistance = inDistanceFrag;
                }
                else
                {
                    Spline.ControlPoints[no].AutoHandles = false;
                    Spline.ControlPoints[no].AutoHandleDistance = (inDistanceFrag + outDistanceFrag) / 2f;
                    SetBezierHandles(inDistanceFrag, true, false, Spline.ControlPoints[no]);
                    SetBezierHandles(outDistanceFrag, false, true, Spline.ControlPoints[no]);
                }


            }
        }

        /// <summary>
        /// Sets a Control Point's Bezier Handles position
        /// </summary>
        /// <param name="no">the ControlPoint</param>
        /// <param name="i">HandlInPosition</param>
        /// <param name="o">HandleOutPosition</param>
        /// <param name="space">World or local space</param>
        protected void SetBezierHandles(int no, Vector3 i, Vector3 o, Space space = Space.World)
        {
            if (no >= 0 && no < Spline.ControlPointCount)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.RecordObject(Spline.ControlPoints[no], "Apply Shape");
#endif
                Spline.ControlPoints[no].AutoHandles = false;
                if (space == Space.World)
                {
                    Spline.ControlPoints[no].HandleInPosition = i;
                    Spline.ControlPoints[no].HandleOutPosition = o;
                }
                else
                {
                    Spline.ControlPoints[no].HandleIn = i;
                    Spline.ControlPoints[no].HandleOut = o;
                }
            }
        }

        /// <summary>
        /// Automatically place Bezier handles for a set of Control Points
        /// </summary>
        /// <param name="distanceFrag">how much % distance between neighbouring CPs are applied to the handle length?</param>
        /// <param name="setIn">Set HandleIn?</param>
        /// <param name="setOut">Set HandleOut?</param>
        /// <param name="controlPoints">one or more Control Points to set</param>
        public static void SetBezierHandles(float distanceFrag, bool setIn, bool setOut, params CurvySplineSegment[] controlPoints)
        {
            if (controlPoints.Length == 0)
                return;

            foreach (CurvySplineSegment cp in controlPoints)
                cp.SetBezierHandles(distanceFrag, setIn, setOut);
        }

        /// <summary>
        /// Enables CGHardEdge for a set of Control Points
        /// </summary>
        /// <param name="controlPoints">list of Control Point indices</param>
        protected void SetCGHardEdges(params int[] controlPoints)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                foreach (int idx in controlPoints)
                    Undo.RecordObject(Spline.ControlPoints[idx], "Apply Shape");
#endif
            if (controlPoints.Length == 0)
            {
                for (int i = 0; i < Spline.ControlPointCount; i++)
                    Spline.ControlPoints[i].GetMetadata<MetaCGOptions>(true).HardEdge = true;
            }
            else
            {
                for (int i = 0; i < controlPoints.Length; i++)
                    if (i >= 0 && i < Spline.ControlPointCount)
                        Spline.ControlPoints[i].GetMetadata<MetaCGOptions>(true).HardEdge = true;
            }
        }

        /// <summary>
        /// Override this to add custom code
        /// </summary>
        protected virtual void ApplyShape()
        {
        }

        /// <summary>
        /// Resizes the spline to have a certain number of Control Points
        /// </summary>
        /// <param name="count">number of Control Points</param>
        protected void PrepareControlPoints(int count)
        {
            /*
            Spline.Clear();
            while (count-- > 0)
                Spline.Add();
            Spline.Refresh();
            */
            int delta = count - Spline.ControlPointCount;
            bool upd = delta != 0;

            while (delta > 0)
            {
                Spline.Add();
                delta--;
            }

            while (delta < 0)
            {
                Spline.Delete(Spline.LastVisibleControlPoint);
                delta++;
            }
            // Revert to default settings
            for (int i = 0; i < Spline.ControlPoints.Count; i++)
            {
                Spline.ControlPoints[i].Reset();
                var mcg = Spline.ControlPoints[i].GetMetadata<MetaCGOptions>();
                if (mcg)
                    mcg.Reset();
            }

            if (upd)
            {
                Spline.SetDirtyAll();
                Spline.Refresh();
            }

        }

        
        

        #endregion
       
        #region ### Public Static Methods & Properties ###
        
        /// <summary>
        /// Dictionary of Shape definitions and their types
        /// </summary>
        public static Dictionary<CurvyShapeInfo, System.Type> ShapeDefinitions
        {
            get
            {
                if (mShapeDefs.Count == 0)
                    mShapeDefs = typeof(CurvyShape).GetAllTypesWithAttribute<CurvyShapeInfo>();

                return mShapeDefs;
            }
        }

        /// <summary>
        /// Gets a list of Menu Names of available shapes
        /// </summary>
        /// <param name="only2D">whether to skip 3D shapes or not</param>
        /// <returns>a list of Menu Names</returns>
        public static List<string> GetShapesMenuNames(bool only2D = false)
        {
            var res = new List<string>();
            foreach (var shapeInfo in ShapeDefinitions.Keys)
                if (!only2D || shapeInfo.Is2D)
                    res.Add(shapeInfo.Name);

            return res;
        }

        /// <summary>
        /// Gets a list of Menu Names of available shapes
        /// </summary>
        /// <param name="currentShapeType">the current shape type</param>
        /// <param name="currentIndex">returns the index of the current shape type</param>
        /// <param name="only2D">whether only to show 2D shapes</param>
        /// <returns>a list of Menu Names</returns>
        public static List<string> GetShapesMenuNames(System.Type currentShapeType, out int currentIndex, bool only2D = false)
        {
            currentIndex = 0;
            if (currentShapeType == null)
                return GetShapesMenuNames(only2D);
            var lst = new List<string>();
            foreach (var kv in ShapeDefinitions)
            {
                if (!only2D || kv.Key.Is2D)
                    lst.Add(kv.Key.Name);
                if (kv.Value == currentShapeType)
                    currentIndex = lst.Count - 1;
                
            }
            return lst;
        }

        /// <summary>
        /// Gets Shape Menu Name from a CurvyShape subclass type
        /// </summary>
        /// <param name="shapeType"></param>
        /// <returns></returns>
        public static string GetShapeName(System.Type shapeType)
        {
            foreach (var kv in ShapeDefinitions)
                if (kv.Value == shapeType)
                    return kv.Key.Name;
            return null;
        }

        /// <summary>
        /// Gets a CurvyShape subclass type from a Shape's MenuName
        /// </summary>
        /// <param name="menuName"></param>
        /// <returns></returns>
        public static Type GetShapeType(string menuName)
        {
            foreach (var shapeInfo in ShapeDefinitions.Keys)
                if (shapeInfo.Name == menuName)
                    return ShapeDefinitions[shapeInfo];

            return null;
        }

        #endregion

        #region ### Privates ###

        void applyPlane()
        {
            switch (Plane)
            {
                case CurvyPlane.XZ:
                    applyRotation(Quaternion.Euler(90, 0, 0));
                    break;
                case CurvyPlane.YZ:
                    applyRotation(Quaternion.Euler(0, 90, 0));
                    break;
                default:
                    applyRotation(Quaternion.Euler(0, 0, 0));
                    break;
            }
        }

        void applyRotation(Quaternion q)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RecordObject(Spline.transform, "Apply Shape");
#endif
            Spline.transform.localRotation = Quaternion.identity;

            if (Spline.Interpolation == CurvyInterpolation.Bezier)
                for (int i = 0; i < Spline.ControlPointCount; i++)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        Undo.RecordObject(Spline.ControlPoints[i], "Apply Shape");
                        Undo.RecordObject(Spline.ControlPoints[i].transform, "Apply Shape");
                    }
#endif
                    //Spline.ControlPoints[i].localRotation = Quaternion.identity;
                    Spline.ControlPoints[i].localPosition = q * Spline.ControlPoints[i].localPosition;
                    Spline.ControlPoints[i].HandleIn = q * Spline.ControlPoints[i].HandleIn;
                    Spline.ControlPoints[i].HandleOut = q * Spline.ControlPoints[i].HandleOut;
                }
            else
                for (int i = 0; i < Spline.ControlPointCount; i++)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        Undo.RecordObject(Spline.ControlPoints[i].transform, "Apply Shape");
                    }
#endif
                    Spline.ControlPoints[i].localRotation = Quaternion.identity;
                    Spline.ControlPoints[i].localPosition = q * Spline.ControlPoints[i].localPosition;
                }
            Spline.ControlPoints[0].localRotation = q;
        }

        #endregion

       
    }

    /// <summary>
    /// CurvyShape Info Attribute
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class,AllowMultiple=false,Inherited=true)]
    public class CurvyShapeInfo : System.Attribute
    {
        public readonly string Name;
        public readonly bool Is2D;

        public CurvyShapeInfo(string name, bool is2D=true)
        {
            Name = name;
            Is2D = is2D;
        }
    }
}
