// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using UnityEngine.Serialization;
using FluffyUnderware.DevTools.Extensions;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Curvy Spline class
    /// </summary>
    [HelpURL(CurvySpline.DOCLINK+"curvyspline")]
    [AddComponentMenu("Curvy/Curvy Spline",1)]
    [ExecuteInEditMode]
    public class CurvySpline : CurvySplineBase
    {
        public const string VERSION = "2.1.3";
        public const string VERSIONSHORT = "210";
        public const string WEBROOT = "http://www.fluffyunderware.com/curvy/";
        public const string DOCROOT = WEBROOT+"documentation/";
        public const string DOCLINK = WEBROOT+"doclink/"+VERSIONSHORT+"/";

        #region ### Serialized fields ###
        
        #region --- General ---
        
        [Section("General",HelpURL=CurvySpline.DOCLINK+"curvyspline_general")]
        [Tooltip("Interpolation Method")]
        [SerializeField,FormerlySerializedAs("Interpolation")]
        CurvyInterpolation m_Interpolation = CurvyGlobalManager.DefaultInterpolation;

        [Tooltip("Restrict Control Points to local X/Y axis")]
        [FieldAction("CBCheck2DPlanar")]
        [SerializeField]
        bool m_RestrictTo2D;

        [SerializeField,FormerlySerializedAs("Closed")]
        bool m_Closed;

        [FieldCondition("canHaveManualEndCP", Action = ActionAttribute.ActionEnum.Enable)]
        [Tooltip("Handle End Control Points automatically?")]
        [SerializeField,FormerlySerializedAs("AutoEndTangents")]
        bool m_AutoEndTangents = true;

        [Tooltip("Orientation Flow")]
        [SerializeField, FormerlySerializedAs("Orientation")]
        CurvyOrientation m_Orientation = CurvyOrientation.Dynamic;

        #endregion

        #region --- Bezier Options ---r

        [Section("Global Bezier Options",HelpURL=CurvySpline.DOCLINK+"curvyspline_bezier")]
        [GroupCondition("m_Interpolation", CurvyInterpolation.Bezier)]
        [RangeEx(0, 1, "Default Distance %", "Handle length by distance to neighbours")]
        [SerializeField]
        float m_AutoHandleDistance = 0.39f;

        #endregion

        #region --- TCB Options ---

        [Section("Global TCB Options",HelpURL=CurvySpline.DOCLINK+"curvyspline_tcb")]
        [GroupCondition("m_Interpolation", CurvyInterpolation.TCB)]
        [GroupAction("TCBOptionsGUI", Position = ActionAttribute.ActionPositionEnum.Below)]
        [SerializeField, FormerlySerializedAs("Tension")]
        float m_Tension;

        [SerializeField, FormerlySerializedAs("Continuity")]
        float m_Continuity;

        [SerializeField, FormerlySerializedAs("Bias")]
        float m_Bias;
        #endregion

        #region --- Advanced Settings ---
        
        [Section("Advanced Settings",HelpURL=CurvySpline.DOCLINK+"curvyspline_advanced")]
        [FieldAction("ShowGizmoGUI",Position=ActionAttribute.ActionPositionEnum.Above)]
        [Label("Color", "Gizmo color")]
        [SerializeField]
        Color m_GizmoColor = CurvyGlobalManager.DefaultGizmoColor;
        
        [Label("Active Color", "Selected Gizmo color")]
        [SerializeField]
        Color m_GizmoSelectionColor = CurvyGlobalManager.DefaultGizmoSelectionColor;

        [RangeEx(1, 100)]
        [SerializeField, FormerlySerializedAs("Granularity")]
        int m_CacheDensity = 50;
        [SerializeField, Tooltip("Use a GameObject pool at runtime")]
        bool m_UsePooling = true;
        [SerializeField, Tooltip("Use threading where applicable")]
        bool m_UseThreading = false;
        [Tooltip("Refresh when Control Point position change?")]
        [SerializeField,FormerlySerializedAs("AutoRefresh")]
        bool m_CheckTransform = false;
        [SerializeField]
        CurvyUpdateMethod m_UpdateIn = CurvyUpdateMethod.Update;
        #endregion
        
        [HideInInspector]
        /// <summary>
        /// Access the list of Control Points
        /// </summary>
        public List<CurvySplineSegment> ControlPoints = new List<CurvySplineSegment>();

        #endregion
       
        #region ### Public Properties ###

        #region --- General ---

        /// <summary>
        /// The interpolation method used by this spline
        /// </summary>
        public CurvyInterpolation Interpolation
        {
            get { return m_Interpolation; }
            set
            {
                if (m_Interpolation != value)
                {
                    m_Interpolation = value;
                    SetDirtyAll();
                }
            }
        }
        
        /// <summary>
        /// Whether to restrict Control Points to the local X/Y plane
        /// </summary>
        public bool RestrictTo2D
        {
            get { return m_RestrictTo2D; }
            set
            {
                if (m_RestrictTo2D != value)
                    m_RestrictTo2D = value;
            }
        }

        /// <summary>
        /// Gets or sets the default Handle distance for Bezier splines
        /// </summary>
        public float AutoHandleDistance
        {
            get { return m_AutoHandleDistance; }
            set
            {
                if (m_AutoHandleDistance != value)
                {
                    m_AutoHandleDistance = Mathf.Clamp01(value);
                    SetDirtyAll();
                }
            }
        }


        /// <summary>
        /// Whether this spline is closed or not
        /// </summary>
        public bool Closed
        {
            get { return m_Closed; }
            set
            {
                if (m_Closed != value)
                {
                    m_Closed = value;
                    SetDirtyAll();
                }
                if (m_Closed)
                    AutoEndTangents = true;
            }
        }
        
        /// <summary>
        /// Whether the first/last Control Point should act as the end tangent, too.
        /// </summary>
        /// <remarks>Ignored by linear splines</remarks>
        public bool AutoEndTangents
        {
            get { return m_AutoEndTangents; }
            set
            {
                bool v = canHaveManualEndCP() ? value : true;
                if (m_AutoEndTangents != v)
                {
                    m_AutoEndTangents = v;
                    SetDirtyAll();
                }
            }
        }

        /// <summary>
        /// Orientation mode
        /// </summary>
        public CurvyOrientation Orientation
        {
            get { return m_Orientation; }
            set
            {
                if (m_Orientation != value)
                {
                    m_Orientation = value;
                    SetDirtyAll();
                }
            }
        }

         public CurvyUpdateMethod UpdateIn
        {
            get { return m_UpdateIn; }
            set
            {
                if (m_UpdateIn != value)
                    m_UpdateIn = value;
            }
        }

        #endregion
        
        #region --- Advanced Settings ---

        /// <summary>
        /// Gets or sets Spline color
        /// </summary>
        public Color GizmoColor
        {
            get { return m_GizmoColor; }
            set
            {
                if (m_GizmoColor != value)
                    m_GizmoColor = value;
            }
        }
        
        /// <summary>
        /// Gets or sets selected segment color
        /// </summary>
        public Color GizmoSelectionColor
        {
            get { return m_GizmoSelectionColor; }
            set
            {
                if (m_GizmoSelectionColor != value)
                    m_GizmoSelectionColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the cache density
        /// </summary>
        public int CacheDensity
        {
            get { return m_CacheDensity; }
            set
            {
                if (m_CacheDensity != value)
                {
                    m_CacheDensity = Mathf.Clamp(value, 1, 100);
                    mCacheSize = 0;
                    SetDirtyAll();
                }
            }
        }

        /// <summary>
        /// Whether to use GameObject pooling for Control Points at runtime
        /// </summary>
        public bool UsePooling
        {
            get { return m_UsePooling; }
            set
            {
                if (m_UsePooling != value)
                    m_UsePooling = value;
            }
        }
        /// <summary>
        /// Whether to use threading where applicable or not
        /// </summary>
        public bool UseThreading
        {
            get
            {
#if UNITY_WSA
                return false;
#else
                return m_UseThreading;
#endif
            }
            set
            {
                if (m_UseThreading != value)
                    m_UseThreading = value;
            }
        }

        /// <summary>
        /// Whether the spline should automatically refresh when a Control Point's position change
        /// </summary>
        /// <remarks>Enable this if you animate a Control Point's transform!</remarks>
        public bool CheckTransform
        {
            get { return m_CheckTransform; }
            set
            {
                if (m_CheckTransform != value)
                    m_CheckTransform= value;
            }
        }

        
#endregion
       
        #region --- TCB Options ---

        /// <summary>
        /// Global Tension
        /// </summary>
        /// <remarks>This only applies to TCB interpolation</remarks>
        public float Tension
        {
            get { return m_Tension; }
            set
            {
                if (m_Tension != value)
                {
                    m_Tension = value;
                    SetDirtyAll();
                }
            }
        }

        /// <summary>
        /// Global Continuity
        /// </summary>
        /// <remarks>This only applies to TCB interpolation</remarks>
        public float Continuity
        {
            get { return m_Continuity; }
            set
            {
                if (m_Continuity != value)
                {
                    m_Continuity = value;
                    SetDirtyAll();
                }
            }
        }
        
        /// <summary>
        /// Global Bias
        /// </summary>
        /// <remarks>This only applies to TCB interpolation</remarks>
        public float Bias
        {
            get { return m_Bias; }
            set
            {
                if (m_Bias != value)
                {
                    m_Bias = value;
                    SetDirtyAll();
                }
            }
        }
#endregion

        #region --- Others ---
        public override Bounds Bounds
        {
            get
            {
                if (!mBounds.HasValue)
                    mBounds = getBounds();
                return mBounds.Value;
            }
        }

        /// <summary>
        /// Gets the number of Segments
        /// </summary>
        public override int Count { get { return mSegments.Count; } }
        /// <summary>
        /// Gets the number of Control Points
        /// </summary>
        public int ControlPointCount { get { return ControlPoints.Count; } }

        /// <summary>
        /// Gets total Cache Size
        /// </summary>
        public int CacheSize
        {
            get
            {
                if (mCacheSize == 0)
                {
                    for (int i = 0; i < mSegments.Count; i++)
                        mCacheSize += mSegments[i].CacheSize;
                }
                return mCacheSize;
            }
        }

        public override bool Dirty
        {
            get
            {
                return (mDirtyControlPoints.Count > 0 || mForceRefresh);
            }
        }

        /// <summary>
        /// Gets the Segment at a certain index
        /// </summary>
        /// <param name="idx">an index in the range 0..Count</param>
        /// <returns>the corresponding spline segment</returns>
        public CurvySplineSegment this[int idx]
        {
            get
            {
                return (idx > -1 && idx < mSegments.Count) ? mSegments[idx] : null;
            }
        }

        /// <summary>
        /// Access the list of Segments
        /// </summary>
        public List<CurvySplineSegment> Segments
        {
            get
            {
                return mSegments;
            }
        }

        

        /// <summary>
        /// Gets the first visible Control Point (equals the first segment or this[0])
        /// </summary>
        public CurvySplineSegment FirstVisibleControlPoint
        {
            get
            {
                // During Start, Segments aren't loaded
                if (!IsInitialized && ControlPointCount > 0)
                    return (AutoEndTangents) ? ControlPoints[0] : ControlPoints[Mathf.Min(ControlPointCount-1,1)];
    
                return (Count > 0 && ControlPoints.Count>0) ? this[0] : null;
            }
        }

        /// <summary>
        /// Gets the last visible Control Point (i.e. the end CP of the last segment)
        /// </summary>
        public CurvySplineSegment LastVisibleControlPoint
        {
            get
            {
                // During Start, Segments aren't loaded
                if (!IsInitialized && ControlPointCount > 0)
                {
                    if (Closed)
                        return ControlPoints[0];
                    else
                        return (AutoEndTangents) ? ControlPoints[ControlPointCount - 1] : ControlPoints[Mathf.Max(0,ControlPointCount - 2)];
                }
                return (Count > 0 && ControlPoints.Count > this[Count - 1].ControlPointIndex + 1) ? 
                    ControlPoints[this[Count - 1].ControlPointIndex + 1] 
                    : (Closed) ? ControlPoints[0] : null;
            }
        }

        /// <summary>
        /// Gets whether the splines in the group form a continuous curve
        /// </summary>
        public override bool IsContinuous
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets whether the splines in the group form a continuous closed curve
        /// </summary>
        public override bool IsClosed
        {
            get
            {
                return Closed;
            }
        }
        /// <summary>
        /// Gets the FollowUp spline of the last Control Point, i.e. the next fully connected spline 
        /// </summary>
        public CurvySpline NextSpline
        {
            get 
            {
                var cp=LastVisibleControlPoint;
                return (cp && cp.FollowUp) ? cp.FollowUp.Spline : null;
            }
        }

        /// <summary>
        /// Gets the FollowUp spline of the first Control Point, i.e. the previous fully connected spline 
        /// </summary>
        public CurvySpline PreviousSpline
        {
            get 
            {
                var cp = FirstVisibleControlPoint;
                return (cp && cp.FollowUp) ? cp.FollowUp.Spline : null;
            }
        }
        
#endregion
        
#endregion

        #region ### Privates Fields ###

        List<CurvySplineSegment> mSegments = new List<CurvySplineSegment>(); // Controlpoints that start a valid spline segment
        int mCacheSize;
        int mLastCPCount;
        Bounds? mBounds;
        bool mDirtyCurve;
        bool mDirtyOrientation;
        List<CurvySplineSegment> mDirtyControlPoints = new List<CurvySplineSegment>();
        bool mForceRefresh;
        ThreadPoolWorker mThreadWorker = new ThreadPoolWorker();
        static CurvySplineMoveEventArgs _moveEventArgs = new CurvySplineMoveEventArgs(); // stored class prevents feeding the GC

        // Internal API speedup: Last returned segment from DistanceToSegment
        CurvySplineSegment _lastDistToSeg;

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */
#if UNITY_EDITOR
        void OnValidate()
        {
            Closed = m_Closed;
            AutoEndTangents = m_AutoEndTangents;
            CacheDensity = m_CacheDensity;
            SetDirtyAll();
        }
#endif

        protected override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            CheckForVersionUpgrade();
#endif
            if (UsePooling && !CurvyGlobalManager.Instance)
                Debug.Log("[Curvy] Unable to create _CurvyGlobalManager!");
        }

        protected override void OnEnable()
        {
            if (ControlPoints.Count == 0)
                SyncSplineFromHierarchy();

            // Preload
            mSegments.Clear();
            if (!Application.isPlaying)
                SetDirtyAll();

          
        }

        void OnDisable()
        {
            mIsInitialized = false;
        }

        void OnDestroy()
        {
            bool realDestroy = true;
#if UNITY_EDITOR
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                realDestroy = false;
#endif
            if (realDestroy)
            {
                
                for (int i = 0; i < ControlPointCount; i++)
                {
                    if (UsePooling && Application.isPlaying)
                    {
                        ControlPoints[i].StripComponents();
                        if (CurvyGlobalManager.Instance!=null)
                            CurvyGlobalManager.Instance.ControlPointPool.Push(ControlPoints[i]);
                    } else
                        ControlPoints[i].Disconnect();
                }
            }
        }

        void Reset()
        {
            Interpolation = CurvyGlobalManager.DefaultInterpolation;
            RestrictTo2D = false;
            AutoHandleDistance = 0.39f;
            Closed = false;
            AutoEndTangents = true;
            // Orientation
            Orientation = CurvyOrientation.Dynamic;
            // Advanced
            GizmoColor = CurvyGlobalManager.DefaultGizmoColor;
            GizmoSelectionColor = CurvyGlobalManager.DefaultGizmoSelectionColor;
            CacheDensity = 50;
            CheckTransform = true;
            // TCB
            Tension = 0;
            Continuity = 0;
            Bias = 0;
            SyncSplineFromHierarchy();
        }
        
        void Update()
        {
            if (UpdateIn == CurvyUpdateMethod.Update || !Application.isPlaying)
                doUpdate();
        }

        void LateUpdate()
        {
            if (UpdateIn == CurvyUpdateMethod.LateUpdate)
                doUpdate();
        }

        void FixedUpdate()
        {
            if (UpdateIn == CurvyUpdateMethod.FixedUpdate)
                doUpdate();
        }
        /*! \endcond */
#endregion

        #region ### Public Static Methods ###

        /// <summary>
        /// Creates an empty spline
        /// </summary>
        public static CurvySpline Create()
        {
            CurvySpline spl = new GameObject("Curvy Spline", typeof(CurvySpline)).GetComponent<CurvySpline>();
            spl.gameObject.layer = CurvyGlobalManager.SplineLayer;
            return spl;
        }

        /// <summary>
        /// Creates an empty spline with the same settings as another spline
        /// </summary>
        /// <param name="takeOptionsFrom">another spline</param>
        public static CurvySpline Create(CurvySpline takeOptionsFrom)
        {
            CurvySpline spl = Create();
            if (takeOptionsFrom)
            {
                spl.RestrictTo2D = takeOptionsFrom.RestrictTo2D;
                spl.GizmoColor = takeOptionsFrom.GizmoColor;
                spl.GizmoSelectionColor = takeOptionsFrom.GizmoSelectionColor;
                spl.Interpolation = takeOptionsFrom.Interpolation;
                spl.Closed = takeOptionsFrom.Closed;
                spl.AutoEndTangents = takeOptionsFrom.AutoEndTangents;
                spl.CacheDensity = takeOptionsFrom.CacheDensity;
                spl.Orientation = takeOptionsFrom.Orientation;
                spl.CheckTransform = takeOptionsFrom.CheckTransform;
            }
            return spl;
        }
        /// <summary>
        /// Gets the number of Cache Points needed for a certain part of a spline
        /// </summary>
        /// <param name="density">the desired CacheDensity</param>
        /// <param name="length">the length of the spline segment</param>
        /// <param name="maxPointsPerUnit">Maximum number of Cache Points per World Unit</param>
        public static int CalculateCacheSize(int density, float length, float maxPointsPerUnit)
        {
            if (maxPointsPerUnit == 0)
                maxPointsPerUnit = CurvyGlobalManager.MaxCachePPU;

            float ppu = DTTween.QuadIn(density - 1, 0, maxPointsPerUnit, 99);
            return Mathf.FloorToInt(length * ppu) + 1;
        }

        /// <summary>
        /// Cubic-Beziere Interpolation
        /// </summary>
        /// <param name="T0">HandleIn</param>
        /// <param name="P0">Pn</param>
        /// <param name="P1">Pn+1</param>
        /// <param name="T1">HandleOut</param>
        /// <param name="f">f in the range 0..1</param>
        /// <returns></returns>
        public static Vector3 Bezier(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
        {
            double Ft2 = 3; double Ft3 = -3;
            double Fu1 = 3; double Fu2 = -6; double Fu3 = 3;
            double Fv1 = -3; double Fv2 = 3;

            double FAX = -P0.x + Ft2 * T0.x + Ft3 * T1.x + P1.x;
            double FBX = Fu1 * P0.x + Fu2 * T0.x + Fu3 * T1.x;
            double FCX = Fv1 * P0.x + Fv2 * T0.x;
            double FDX = P0.x;

            double FAY = -P0.y + Ft2 * T0.y + Ft3 * T1.y + P1.y;
            double FBY = Fu1 * P0.y + Fu2 * T0.y + Fu3 * T1.y;
            double FCY = Fv1 * P0.y + Fv2 * T0.y;
            double FDY = P0.y;

            double FAZ = -P0.z + Ft2 * T0.z + Ft3 * T1.z + P1.z;
            double FBZ = Fu1 * P0.z + Fu2 * T0.z + Fu3 * T1.z;
            double FCZ = Fv1 * P0.z + Fv2 * T0.z;
            double FDZ = P0.z;

            float FX = (float)(((FAX * f + FBX) * f + FCX) * f + FDX);
            float FY = (float)(((FAY * f + FBY) * f + FCY) * f + FDY);
            float FZ = (float)(((FAZ * f + FBZ) * f + FCZ) * f + FDZ);

            return new Vector3(FX, FY, FZ);
        }

        public static float BezierTangent(float T0, float P0, float P1, float T1, float t)
        {
            float C1 = (P1 - (3.0f * T1) + (3.0f * T0) - P0);
            float C2 = ((3.0f * T1) - (6.0f * T0) + (3.0f * P0));
            float C3 = ((3.0f * T0) - (3.0f * P0));
            return ((3.0f * C1 * t * t) + (2.0f * C2 * t) + C3);
        }

        public static Vector3 BezierTangent(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
        {
            Vector3 C1 = (P1 - (3.0f * T1) + (3.0f * T0) - P0);
            Vector3 C2 = ((3.0f * T1) - (6.0f * T0) + (3.0f * P0));
            Vector3 C3 = ((3.0f * T0) - (3.0f * P0));
            return ((3.0f * C1 * f * f) + (2.0f * C2 * f) + C3);
        }

        /// <summary>
        /// Catmull-Rom Interpolation
        /// </summary>
        /// <param name="T0">Pn-1 (In Tangent)</param>
        /// <param name="P0">Pn</param>
        /// <param name="P1">Pn+1</param>
        /// <param name="T1">Pn+2 (Out Tangent)</param>
        /// <param name="f">f in the range 0..1</param>
        /// <returns>the interpolated position</returns>
        public static Vector3 CatmullRom(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f)
        {
            double Ft1 = -0.5; double Ft2 = 1.5; double Ft3 = -1.5; double Ft4 = 0.5;
            double Fu2 = -2.5; double Fu3 = 2; double Fu4 = -0.5;
            double Fv1 = -0.5; double Fv3 = 0.5;

            double FAX = Ft1 * T0.x + Ft2 * P0.x + Ft3 * P1.x + Ft4 * T1.x;
            double FBX = T0.x + Fu2 * P0.x + Fu3 * P1.x + Fu4 * T1.x;
            double FCX = Fv1 * T0.x + Fv3 * P1.x;
            double FDX = P0.x;

            double FAY = Ft1 * T0.y + Ft2 * P0.y + Ft3 * P1.y + Ft4 * T1.y;
            double FBY = T0.y + Fu2 * P0.y + Fu3 * P1.y + Fu4 * T1.y;
            double FCY = Fv1 * T0.y + Fv3 * P1.y;
            double FDY = P0.y;

            double FAZ = Ft1 * T0.z + Ft2 * P0.z + Ft3 * P1.z + Ft4 * T1.z;
            double FBZ = T0.z + Fu2 * P0.z + Fu3 * P1.z + Fu4 * T1.z;
            double FCZ = Fv1 * T0.z + Fv3 * P1.z;
            double FDZ = P0.z;

            float FX = (float)(((FAX * f + FBX) * f + FCX) * f + FDX);
            float FY = (float)(((FAY * f + FBY) * f + FCY) * f + FDY);
            float FZ = (float)(((FAZ * f + FBZ) * f + FCZ) * f + FDZ);

            return new Vector3(FX, FY, FZ);
        }

        /// <summary>
        /// Kochanek-Bartels/TCB-Interpolation
        /// </summary>
        /// <param name="T0">Pn-1 (In Tangent)</param>
        /// <param name="P0">Pn</param>
        /// <param name="P1">Pn+1</param>
        /// <param name="T1">Pn+2 (Out Tangent)</param>
        /// <param name="f">f in the range 0..1</param>
        /// <param name="FT0">Start Tension</param>
        /// <param name="FC0">Start Continuity</param>
        /// <param name="FB0">Start Bias</param>
        /// <param name="FT1">End Tension</param>
        /// <param name="FC1">End Continuity</param>
        /// <param name="FB1">End Bias</param>
        /// <returns>the interpolated position</returns>
        public static Vector3 TCB(Vector3 T0, Vector3 P0, Vector3 P1, Vector3 T1, float f, float FT0, float FC0, float FB0, float FT1, float FC1, float FB1)
        {
            double FFA = (1 - FT0) * (1 + FC0) * (1 + FB0);
            double FFB = (1 - FT0) * (1 - FC0) * (1 - FB0);
            double FFC = (1 - FT1) * (1 - FC1) * (1 + FB1);
            double FFD = (1 - FT1) * (1 + FC1) * (1 - FB1);

            double DD = 2;
            double Ft1 = -FFA / DD; double Ft2 = (+4 + FFA - FFB - FFC) / DD; double Ft3 = (-4 + FFB + FFC - FFD) / DD; double Ft4 = FFD / DD;
            double Fu1 = +2 * FFA / DD; double Fu2 = (-6 - 2 * FFA + 2 * FFB + FFC) / DD; double Fu3 = (+6 - 2 * FFB - FFC + FFD) / DD; double Fu4 = -FFD / DD;
            double Fv1 = -FFA / DD; double Fv2 = (FFA - FFB) / DD; double Fv3 = FFB / DD;
            double Fw2 = +2 / DD;

            double FAX = Ft1 * T0.x + Ft2 * P0.x + Ft3 * P1.x + Ft4 * T1.x;
            double FBX = Fu1 * T0.x + Fu2 * P0.x + Fu3 * P1.x + Fu4 * T1.x;
            double FCX = Fv1 * T0.x + Fv2 * P0.x + Fv3 * P1.x;
            double FDX = Fw2 * P0.x;

            double FAY = Ft1 * T0.y + Ft2 * P0.y + Ft3 * P1.y + Ft4 * T1.y;
            double FBY = Fu1 * T0.y + Fu2 * P0.y + Fu3 * P1.y + Fu4 * T1.y;
            double FCY = Fv1 * T0.y + Fv2 * P0.y + Fv3 * P1.y;
            double FDY = Fw2 * P0.y;

            double FAZ = Ft1 * T0.z + Ft2 * P0.z + Ft3 * P1.z + Ft4 * T1.z;
            double FBZ = Fu1 * T0.z + Fu2 * P0.z + Fu3 * P1.z + Fu4 * T1.z;
            double FCZ = Fv1 * T0.z + Fv2 * P0.z + Fv3 * P1.z;
            double FDZ = Fw2 * P0.z;

            float FX = (float)(((FAX * f + FBX) * f + FCX) * f + FDX);
            float FY = (float)(((FAY * f + FBY) * f + FCY) * f + FDY);
            float FZ = (float)(((FAZ * f + FBZ) * f + FCZ) * f + FDZ);

            return new Vector3(FX, FY, FZ);
        }

        #endregion

        #region ### Public Methods ###

        #region --- Methods based on TF (total fragment) ---

        /// <summary>
        /// Gets the interpolated position for a certain TF
        /// </summary>
        /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <returns>the interpolated position</returns>
        public override Vector3 Interpolate(float tf)
        {
            return Interpolate(tf, Interpolation);
        }

        /// <summary>
        /// Gets the interpolated position for a certain TF
        /// </summary>
        /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <param name="interpolation">the interpolation to use</param>
        /// <returns>the interpolated position</returns>
        public override Vector3 Interpolate(float tf, CurvyInterpolation interpolation)
        {
            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.Interpolate(localF, interpolation) : Vector3.zero;
        }

        /// <summary>
        /// Gets the interpolated position for a certain TF using a linear approximation
        /// </summary>
        /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the interpolated position</returns>
        public override Vector3 InterpolateFast(float tf)
        {
            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);

            return (seg) ? seg.InterpolateFast(localF) : Vector3.zero;
        }

       

        /// <summary>
        /// Gets metadata for a certain TF
        /// </summary>
        /// <param name="type">Metadata type interfacing ICurvyMetadata</param>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the metadata</returns>
        public override Component GetMetadata(System.Type type, float tf) 
        {
            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.GetMetaData(type) : null;
        }
        
        
        /// <summary>
        /// Gets an interpolated Metadata value for a certain TF
        /// </summary>
        /// <typeparam name="T">Metadata type interfacing ICurvyInterpolatableMetadata</typeparam>
        /// <typeparam name="U">Return Value type of T</typeparam>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the interpolated value</returns>
        public override U InterpolateMetadata<T,U>(float tf) 
        {
            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.InterpolateMetadata<T,U>(localF) : default(U);
        }

        /// <summary>
        /// Gets an interpolated Metadata value for a certain TF
        /// </summary>
        /// <param name="type">Metadata type interfacing ICurvyInterpolatableMetadata</param>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the interpolated value</returns>
        public override object InterpolateMetadata(System.Type type, float tf)
        {
            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.InterpolateMetadata(type, localF) : null;
        }

        /// <summary>
        /// Gets an interpolated Scale for a certain TF
        /// </summary>
        /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
        /// <param name="tf">TF value reflecting position on spline(0..1)</param>
        /// <returns>the interpolated value</returns>
        public override Vector3 InterpolateScale(float tf)
        {
            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.InterpolateScale(localF) : Vector3.zero;
        }

        /// <summary>
        /// Gets the Up-Vector for a certain TF based on the splines' Orientation mode
        /// </summary>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the Up-Vector</returns>
        public override Vector3 GetOrientationUpFast(float tf)
        {
            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);

            return (seg) ? seg.GetOrientationUpFast(localF) : Vector3.zero;
        }


        /// <summary>
        /// Gets a rotation looking to Tangent with the head upwards along the Up-Vector
        /// </summary>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <param name="inverse">whether the orientation should be inversed or not</param>
        /// <returns>a rotation</returns>
        public override Quaternion GetOrientationFast(float tf, bool inverse)
        {
            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.GetOrientationFast(localF, inverse) : Quaternion.identity;
        }

        /// <summary>
        /// Gets the normalized tangent for a certain TF
        /// </summary>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <returns>a tangent vector</returns>
        public override Vector3 GetTangent(float tf)
        {
            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.GetTangent(localF) : Vector3.zero;
        }

        /// <summary>
        /// Gets the normalized tangent for a certain TF with a known position for TF
        /// </summary>
        /// <remarks>This saves one interpolation</remarks>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <param name="position">The interpolated position for TF</param>
        /// <returns>a tangent vector</returns>
        public override Vector3 GetTangent(float tf, Vector3 position)
        {
            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            Vector3 v = seg.GetTangent(localF, ref position);
            if (v.x < 0)
                v = seg.GetTangent(localF, ref position);
            return (seg) ? seg.GetTangent(localF, ref position) : Vector3.zero;
        }

        /// <summary>
        /// Gets the normalized tangent for a certain TF using a linear approximation
        /// </summary>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <returns>a tangent vector</returns>
        public override Vector3 GetTangentFast(float tf)
        {
            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF);
            return (seg) ? seg.GetTangentFast(localF) : Vector3.zero;
        }

       

#endregion

        #region --- Conversion Methods ---

        /// <summary>
        /// Converts a TF value to a distance
        /// </summary>
        /// <param name="tf">a TF value</param>
        /// <param name="clamping">Clamping to use</param>
        /// <returns>distance from the first segment's Control Point</returns>
        public override float TFToDistance(float tf, CurvyClamping clamping)
        {
            float localF;
            CurvySplineSegment seg = TFToSegment(tf, out localF, clamping);
            return (seg) ? seg.Distance + seg.LocalFToDistance(localF) : 0;
        }
        
        /// <summary>
        /// Gets the segment and the local F for a certain TF
        /// </summary>
        /// <param name="tf">a TF value</param>
        /// <param name="localF">gets the remaining localF in the range 0..1</param>
        /// <param name="clamping">Clamping to use</param>
        /// <returns>the segment the given TF is inside</returns>
        public override CurvySplineSegment TFToSegment(float tf, out float localF, CurvyClamping clamping)
        {
            tf = CurvyUtility.ClampTF(tf,clamping);
            localF = 0;
            if (Count == 0) return null;
            float f = tf * Count;
            int idx = (int)f;
            localF = f - idx;
            if (idx == Count)
            {
                idx--; localF = 1;
            }

            return this[idx];
        }

        /// <summary>
        /// Gets the segment index for a certain TF or -1 if no Segments exist
        /// </summary>
        /// <param name="tf">a TF value</param>
        /// <param name="clamping">Clamping to use</param>
        /// <returns>-1 or a segment index (0..SegmentCount-1)</returns>
        public int TFToSegmentIndex(float tf, CurvyClamping clamping = CurvyClamping.Clamp)
        {
            if (Count == 0) return -1;
            int idx = (int)(CurvyUtility.ClampTF(tf, clamping) * Count);
            return (idx == Count) ? idx - 1 : idx;
        }

        /// <summary>
        /// Gets a TF value from a segment
        /// </summary>
        /// <param name="segment">a segment</param>
        /// <returns>a TF value in the range 0..1</returns>
        public override float SegmentToTF(CurvySplineSegment segment) { return SegmentToTF(segment, 0); }

        /// <summary>
        /// Gets a TF value from a segment and a local F
        /// </summary>
        /// <param name="segment">a segment</param>
        /// <param name="localF">F of this segment in the range 0..1</param>
        /// <returns>a TF value in the range 0..1</returns>
        public override float SegmentToTF(CurvySplineSegment segment, float localF)
        {
            if (!segment) return 0;
            if (segment.SegmentIndex == -1)
                return (segment.ControlPointIndex > 0) ? 1 : 0;
            return ((float)segment.SegmentIndex / Count) + (1f / Count) * localF;
        }

        /// <summary>
        /// Converts a distance to a TF value
        /// </summary>
        /// <param name="distance">distance in the range 0..Length</param>
        /// <param name="clamping">clamping to use</param>
        /// <returns>a TF value in the range 0..1</returns>
        public override float DistanceToTF(float distance, CurvyClamping clamping)
        {
            if (distance == Length)
                return 1;
            float localDistance;
            // Get the segment the distance lies within
            CurvySplineSegment seg = DistanceToSegment(distance, out localDistance, clamping);
            return (seg) ? SegmentToTF(seg, seg.DistanceToLocalF(localDistance)) : 0;
        }

        /// <summary>
        /// Gets the segment a certain distance lies within
        /// </summary>
        /// <param name="distance">a distance in the range 0..Length</param>
        /// <param name="clamping">clamping to use</param>
        /// <returns>a spline segment or null</returns>
        public CurvySplineSegment DistanceToSegment(float distance, CurvyClamping clamping=CurvyClamping.Clamp)
        {
            float d;
            return DistanceToSegment(distance, out d,clamping);
        }

        

        /// <summary>
        /// Gets the segment a certain distance lies within
        /// </summary>
        /// <param name="distance">a distance in the range 0..Length</param>
        /// <param name="localDistance">gets the remaining distance inside the segment</param>
        /// <param name="clamping">clamping to use</param>
        /// <returns>a spline segment</returns>
        public CurvySplineSegment DistanceToSegment(float distance, out float localDistance, CurvyClamping clamping=CurvyClamping.Clamp)
        {
            distance = CurvyUtility.ClampDistance(distance, clamping, Length);
            CurvySplineSegment seg = null;
            localDistance = -1;
            if (Count > 0)
            {
                distance = Mathf.Clamp(distance, 0, Length);

                if (distance == Length)
                {
                    seg = this[Count - 1];
                    localDistance = seg.Distance + seg.Length;
                    return seg;
                }
                localDistance = 0;
                if (_lastDistToSeg != null && _lastDistToSeg.Distance < distance)
                    seg = _lastDistToSeg;
                else
                    seg =  mSegments[0];
                int dead = Count;
                while (seg && seg.Distance + seg.Length < distance && dead-->0)
                {
                    seg = seg.NextSegment;
                }
                if (dead <= 0)
                    Debug.LogError("[Curvy] CurvySpline.DistanceToSegment() caused a deadloop! This shouldn't happen at all. Please raise a bug report!");
                if (seg == null)
                    seg = this[Count - 1];
                localDistance = distance - seg.Distance;
                _lastDistToSeg = seg;
            }
            return seg;
        }


#endregion

        #region --- Movement ---

        public override Vector3 Move(ref float tf, ref int direction, float fDistance, CurvyClamping clamping)
        {
            // Simple case
            if (!OnMoveControlPointReached.HasListeners() && !OnMoveEndReached.HasListeners())
                return base.Move(ref tf, ref direction, fDistance, clamping);
            else
                return eventAwareMove(ref tf, ref direction, fDistance, clamping, false);
        }

        public override Vector3 MoveFast(ref float tf, ref int direction, float fDistance, CurvyClamping clamping)
        {
            if (!OnMoveControlPointReached.HasListeners() && !OnMoveEndReached.HasListeners())
                return base.MoveFast(ref tf, ref direction, fDistance, clamping);
            else
                return eventAwareMove(ref tf, ref direction, fDistance, clamping, true);
        }

        public override Vector3 MoveByLengthFast(ref float tf, ref int direction, float distance, CurvyClamping clamping)
        {
            if (!OnMoveControlPointReached.HasListeners() && !OnMoveEndReached.HasListeners())
                return base.MoveByLengthFast(ref tf, ref direction, distance, clamping);
            else
                return eventAwareMoveDistance(ref tf, ref direction, distance, clamping,false);
        }
        
#endregion

        #region --- General ---

        /// <summary>
        /// Adds a Control Point and refreshes the spline
        /// </summary>
        /// <returns>a Control Point</returns>
        public CurvySplineSegment Add() { return InsertAfter(null); }

        /// <summary>
        /// Adds several Control Points at once and refresh the spline
        /// </summary>
        /// <param name="controlPoints">one or more positions</param>
        /// <returns>an array containing the new Control Points</returns>
        public CurvySplineSegment[] Add(params Vector3[] controlPoints)
        {
            if (!OnBeforeControlPointAddEvent(new CurvyControlPointEventArgs(this)).Cancel)
            {
                CurvySplineSegment[] cps = new CurvySplineSegment[controlPoints.Length];
                for (int i = 0; i < controlPoints.Length; i++)
                {
                    cps[i] = Add();
                    cps[i].transform.localPosition = controlPoints[i];
                    cps[i].TTransform.localPosition = controlPoints[i];
                    cps[i].AutoHandleDistance = AutoHandleDistance;
                }

                for (int i = 0; i < controlPoints.Length; i++)
                    OnAfterControlPointAddEvent(new CurvyControlPointEventArgs(this,this,cps[i],CurvyControlPointEventArgs.ModeEnum.AddAfter));

                OnAfterControlPointChangesEvent(new CurvySplineEventArgs(this,this));

                return cps;
            }
            else
                return new CurvySplineSegment[0];
        }

        /// <summary>
        /// Inserts a Control Point before a given Control Point
        /// </summary>
        /// <remarks>If you add several Control Points in a row, just refresh the last one!</remarks>
        /// <param name="controlPoint">an descendant Control Point</param>
        /// <returns>a Control Point</returns>
        public CurvySplineSegment InsertBefore(CurvySplineSegment controlPoint)
        {
            if (!OnBeforeControlPointAddEvent(new CurvyControlPointEventArgs(this,this,controlPoint,CurvyControlPointEventArgs.ModeEnum.AddBefore)).Cancel)
            {
                GameObject go;
                CurvySplineSegment cp;

                if (UsePooling && Application.isPlaying)
                {
                    cp = CurvyGlobalManager.Instance.ControlPointPool.Pop<CurvySplineSegment>(transform);
                    go = cp.gameObject;
                }
                else
                {
                    go = new GameObject("NewCP", typeof(CurvySplineSegment));
                    cp = go.GetComponent<CurvySplineSegment>();
                }
                go.layer = gameObject.layer;
                go.transform.parent = transform;
                int idx = 0;
                if (controlPoint)
                {
                    if (controlPoint.PreviousSegment)
                        cp.transform.localPosition=controlPoint.PreviousSegment.Interpolate(0.5f);
                    else
                        if (controlPoint.PreviousTransform)
                            go.transform.position = Vector3.Lerp(controlPoint.PreviousTransform.position, controlPoint.transform.position, 0.5f);

                    idx = Mathf.Max(0, controlPoint.ControlPointIndex);
                }

                ControlPoints.Insert(idx, cp);
                SyncHierarchyFromSpline();
                cp.AutoHandleDistance = AutoHandleDistance;

                if (controlPoint)
                    controlPoint.SetDirty();
                cp.SetDirty();

                mLastCPCount++;

                var e = new CurvyControlPointEventArgs(this,this, cp);
                OnAfterControlPointAddEvent(e);
                OnAfterControlPointChangesEvent(e);

                return cp;
            }
            else
                return null;
        }

        /// <summary>
        /// Inserts a Control Point after a given Control Point
        /// </summary>
        /// <remarks>If you add several Control Points in a row, just refresh the last one!</remarks>
        /// <param name="controlPoint">an ancestor Control Point. If null, the CP will added at the end</param>
        /// <returns>the new Control Point</returns>
        public CurvySplineSegment InsertAfter(CurvySplineSegment controlPoint)
        {
            if (!OnBeforeControlPointAddEvent(new CurvyControlPointEventArgs(this,this,controlPoint,CurvyControlPointEventArgs.ModeEnum.AddAfter)).Cancel)
            {
                GameObject go;
                CurvySplineSegment cp;

                if (UsePooling && Application.isPlaying)
                {
                    cp = CurvyGlobalManager.Instance.ControlPointPool.Pop<CurvySplineSegment>(transform);
                    go = cp.gameObject;
                }
                else
                {
                    go = new GameObject("NewCP", typeof(CurvySplineSegment));
                    cp = go.GetComponent<CurvySplineSegment>();
                }
                go.layer = gameObject.layer;
                go.transform.SetParent(transform);
                int idx = ControlPoints.Count;
                
                if (controlPoint)
                {
                    if (controlPoint.IsValidSegment)
                        cp.transform.localPosition=controlPoint.Interpolate(0.5f);
                    else if (controlPoint.NextTransform)
                        go.transform.position = Vector3.Lerp(controlPoint.NextTransform.position, controlPoint.transform.position, 0.5f);

                    idx = controlPoint.ControlPointIndex + 1;

                }

                ControlPoints.Insert(idx, cp);
                SyncHierarchyFromSpline();

                cp.AutoHandleDistance = AutoHandleDistance;
                
                if (controlPoint)
                    controlPoint.SetDirty();
                cp.SetDirty();
                SetDirty(cp.PreviousControlPoint);

                mLastCPCount++;

                var e = new CurvyControlPointEventArgs(this,this, cp,CurvyControlPointEventArgs.ModeEnum.AddAfter);
                OnAfterControlPointAddEvent(e);
                OnAfterControlPointChangesEvent(e);
                return cp;
            }
            else
                return null;
        }

        /// <summary>
        /// Removes all control points.
        /// </summary>
        public override void Clear()
        {
            // Fire Event. Canceling the event cancels the operation
            foreach (CurvySplineSegment seg in ControlPoints)
            {
                if (OnBeforeControlPointDeleteEvent(new CurvyControlPointEventArgs(this,this, seg,CurvyControlPointEventArgs.ModeEnum.Delete)).Cancel)
                    return;
            }
            for (int i=ControlPointCount-1;i>=0;i--)
                if (UsePooling && Application.isPlaying)
                    CurvyGlobalManager.Instance.ControlPointPool.Push(ControlPoints[i]);
                else
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        _newSelectionInstanceIDINTERNAL = 42; // Prevent selection of another CP/Spline
                        Undo.DestroyObjectImmediate(ControlPoints[i].gameObject);
                    } else
                        DestroyImmediate(ControlPoints[i].gameObject);
#else
                        Destroy(ControlPoints[i].gameObject);
#endif
                }
            
            ControlPoints.Clear();
            mDirtyControlPoints.Clear();
            mSegments.Clear();
            mCacheSize = 0;
            mLength = 0;
            SetDirtyAll();
            Refresh();
            OnAfterControlPointChangesEvent(new CurvySplineEventArgs(this, null));
#if UNITY_EDITOR
            _newSelectionInstanceIDINTERNAL = 0;
#endif
        }

        /// <summary>
        /// Deletes a Control Point
        /// </summary>
        /// <param name="controlPoint">a Control Point</param>
        public void Delete(CurvySplineSegment controlPoint)
        {
            if (!controlPoint)
                return;
            if (!OnBeforeControlPointDeleteEvent(new CurvyControlPointEventArgs(this,this,controlPoint,CurvyControlPointEventArgs.ModeEnum.Delete)).Cancel)
            {
                var pcp = controlPoint.PreviousControlPoint;
                var ncp = controlPoint.NextControlPoint;
                if (pcp)
                    pcp.SetDirty();
                if (ncp)
                    ncp.SetDirty();
                ControlPoints.Remove(controlPoint);
                mDirtyControlPoints.Remove(controlPoint);
                controlPoint.transform.SetAsLastSibling();// IMPORTANT! Runtime Delete is delayed, so we need to make sure it got sorted to the end 
                if (UsePooling && Application.isPlaying)
                {
                    controlPoint.StripComponents();
                    CurvyGlobalManager.Instance.ControlPointPool.Push(controlPoint);
                    mDirtyControlPoints.Remove(controlPoint);
                }
                else
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        _newSelectionInstanceIDINTERNAL = 42; // Prevent selection of another CP/Spline
                        Undo.DestroyObjectImmediate(controlPoint.gameObject);
                    }
                    else
                        DestroyImmediate(controlPoint.gameObject);
#else
                        Destroy(controlPoint.gameObject);
#endif
                }

                SyncHierarchyFromSpline();
                mLastCPCount--;
                OnAfterControlPointChangesEvent(new CurvyControlPointEventArgs(this,this,null));
            }
        }

        /// <summary>
        /// Gets an array containing all approximation points
        /// </summary>
        /// <param name="space">whether to use local or global space</param>
        /// <remarks>This can be used to feed meshbuilders etc...</remarks>
        /// <returns>an array of world/local positions</returns>
        public override Vector3[] GetApproximation(Space space=Space.Self)
        {
            Vector3[] apps = new Vector3[CacheSize + 1];
            int idx = 0;
            for (int si = 0; si < Count; si++)
            {
                this[si].Approximation.CopyTo(apps, idx);
                idx += Mathf.Max(0, this[si].Approximation.Length - 1);
            }

            if (space==Space.World)
            {
                Matrix4x4 m = TTransform.localToWorldMatrix;
                for (int i = 0; i < apps.Length; i++)
                    apps[i] = m.MultiplyPoint3x4(apps[i]);
            }

            return apps;
        }

        /// <summary>
        /// Gets an array containing all approximation tangents
        /// </summary>
        /// <remarks>This can be used to feed meshbuilders etc...</remarks>
        /// <returns>an array of tangents</returns>
        public override Vector3[] GetApproximationT()
        {
            Vector3[] apps = new Vector3[CacheSize + 1];
            int idx = 0;
            for (int si = 0; si < Count; si++)
            {
                this[si].ApproximationT.CopyTo(apps, idx);
                idx += Mathf.Max(0, this[si].ApproximationT.Length - 1);
            }
            return apps;
        }

        /// <summary>
        /// Gets an array containing all approximation Up-Vectors
        /// </summary>
        /// <remarks>This can be used to feed meshbuilders etc...</remarks>
        /// <returns>an array of Up-Vectors</returns>
        public override Vector3[] GetApproximationUpVectors()
        {
            Vector3[] apps = new Vector3[CacheSize + 1];
            int idx = 0;
            for (int si = 0; si < Count; si++)
            {
                this[si].ApproximationUp.CopyTo(apps, idx);
                idx += Mathf.Max(0, this[si].ApproximationUp.Length - 1);
            }
            return apps;
        }
       
        /// <summary>
        /// Gets the TF value that is nearest to p
        /// </summary>
        /// <param name="p">a point in spline's local space</param>
        /// <remarks>The speed as well as the accuracy depends on the Granularity</remarks>
        /// <returns>a TF value in the range 0..1 or -1 on error</returns>
        public override float GetNearestPointTF(Vector3 p)
        {
            Vector3 v;
            return GetNearestPointTF(p,out v,0,-1);
        }

        /// <summary>
        /// Gets the TF value that is nearest to p
        /// </summary>
        /// <param name="p">a point in spline's local space</param>
        /// <param name="nearest">returns the nearest position</param>
        /// <remarks>The speed as well as the accuracy depends on the Granularity</remarks>
        /// <returns>a TF value in the range 0..1 or -1 on error</returns>
        public override float GetNearestPointTF(Vector3 p, out Vector3 nearest)
        {
            return GetNearestPointTF(p, out nearest, 0, -1);
        }

        /// <summary>
        /// Gets the TF value that is nearest to p
        /// </summary>
        /// <param name="p">a point in spline's local space</param>
        /// <param name="startSegmentIndex">the segment index to start searching</param>
        /// <param name="stopSegmentIndex">the segment index to stop searching or -1 to search until end</param>
        /// <remarks>The speed as well as the accuracy depends on the Granularity</remarks>
        /// <returns>a TF value in the range 0..1 or -1 on error</returns>
        public float GetNearestPointTF(Vector3 p, int startSegmentIndex = 0, int stopSegmentIndex = -1)
        {
            Vector3 v;
            return GetNearestPointTF(p, out v, startSegmentIndex, stopSegmentIndex);
        }

        /// <summary>
        /// Gets the TF value that is nearest to p
        /// </summary>
        /// <param name="p">a point in spline's local space</param>
        /// <param name="nearest">returns the nearest position</param>
        /// <param name="startSegmentIndex">the segment index to start searching</param>
        /// <param name="stopSegmentIndex">the segment index to stop searching or -1 to search until end</param>
        /// <remarks>The speed as well as the accuracy depends on the Granularity</remarks>
        /// <returns>a TF value in the range 0..1 or -1 on error</returns>
        public float GetNearestPointTF(Vector3 p, out Vector3 nearest, int startSegmentIndex = 0, int stopSegmentIndex = -1)
        {
            nearest = Vector3.zero;
            if (Count == 0)
                return -1;
            // for each segment, get the distance to it's approximation points
            float distSqr = float.MaxValue;
            float f;
            float resF = 0;

            CurvySplineSegment resSeg = null;
            if (stopSegmentIndex == -1)
                stopSegmentIndex = Count - 1;
            startSegmentIndex = Mathf.Clamp(startSegmentIndex, 0, Count - 1);
            stopSegmentIndex = Mathf.Clamp(stopSegmentIndex + 1, startSegmentIndex + 1, Count);
            for (int i = startSegmentIndex; i < stopSegmentIndex; i++)
            {
                f = this[i].GetNearestPointF(p);
                Vector3 v = this[i].Interpolate(f);
                float magSqr = (v - p).sqrMagnitude;
                if (magSqr <= distSqr)
                {
                    resSeg = this[i];
                    resF = f;
                    nearest = v;
                    distSqr = magSqr;
                }
            }
            // return the nearest
            return resSeg.LocalFToTF(resF);
        }
      
        /// <summary>
        /// Refreshes the spline
        /// </summary>
        /// <remarks>This is called automatically during Update, so usually you don't need to call it manually</remarks>
        public override void Refresh()
        {
            // clear API speedup caches
            _lastDistToSeg = null;

            if (mForceRefresh)
            {
                mDirtyControlPoints.Clear();
                mDirtyControlPoints.AddRange(ControlPoints);
            }
            
            if (mDirtyControlPoints.Count > 0)
            {
                mDirtyControlPoints.Sort();
                
                //string r = "";
                //mDirtyControlPoints.ForEach((x) => r += x.ToString() + ", ");
                //Debug.Log(name+"."+"Crv/Ori: " + mDirtyCurve + "/" + mDirtyOrientation + " => " + r);

                if (mDirtyCurve)
                {
#region --- Curve ---

                    mCacheSize = 0;
                    mLength = 0;
                    mSegments.Clear();

                    // Iterate from first changed up to end
                    for (int i = mDirtyControlPoints[0].ControlPointIndex; i < ControlPointCount; i++)
                    {
                        ControlPoints[i].TTransform.FromTransform(ControlPoints[i].transform);
                        ControlPoints[i].ClearSegmentIndexINTERNAL();
                    }

                    // Update Bezier Handles
                    if (Interpolation == CurvyInterpolation.Bezier)
                        for (int i = 0; i < mDirtyControlPoints.Count; i++)
                        {
                            
                            if (mDirtyControlPoints[i].AutoHandles)
                                mDirtyControlPoints[i].SetBezierHandles();
                        }

                    mBounds = null;

                    CurvySplineSegment cp;
                    
                    
                    // Iterate through all changed for threadable tasks (cache Approximation, ApproximationT, ApproximationDistance)
                    
                    if (UseThreading)
                    {
                        for (int i = 0; i < mDirtyControlPoints.Count; i++)
                        {
                            var dcp = mDirtyControlPoints[i];
                            mThreadWorker.QueueWorkItem(dcp.refreshCurveINTERNAL);
                        }

                        mThreadWorker.WaitAll();
                    }
                    else
                    {
                        for (int i = 0; i < mDirtyControlPoints.Count; i++)
                            mDirtyControlPoints[i].refreshCurveINTERNAL();
                    }
                    // Iterate through all changed to finalize tangents etc.
                    CurvySplineSegment cpp;
                    for (int i = 0; i < mDirtyControlPoints.Count; i++)
                    {
                        cp = mDirtyControlPoints[i];
                        cpp = cp.PreviousControlPoint;
                        if (cpp)
                        {
                                cpp.ApproximationT[cpp.CacheSize] = cp.ApproximationT[0];
                        }
                    }

                    // Iterate through all ControlPoints for some basic actions
                    if (ControlPointCount > 0)
                    {
                        if (ControlPoints[0].IsValidSegment)
                            mSegments.Add(ControlPoints[0]);
                        mCacheSize += ControlPoints[0].CacheSize;
                        ControlPoints[0].Distance = 0;
                        // Calc Length & Distances, CacheSize
                        for (int i = 1; i < ControlPoints.Count; i++)
                        {
                            ControlPoints[i].Distance = ControlPoints[i - 1].Distance + ControlPoints[i - 1].Length;
                            if (ControlPoints[i].IsValidSegment)
                                mSegments.Add(ControlPoints[i]);
                            mCacheSize += ControlPoints[i].CacheSize;
                        }
                        // Actions that needs proper segment infos
                        for (int i = 1; i < ControlPoints.Count; i++)
                        {
                            if (!ControlPoints[i].CanHaveFollowUp)
                                ControlPoints[i].SetFollowUp(null);
                        }
                        if (Count > 0)
                        {
                            // Handle closed spline
                            mLength = (Closed) ? this[Count-1].Distance+this[Count-1].Length : LastVisibleControlPoint.Distance;
                            // Handle very first/last Tangent
                            var lastSeg = this[Count - 1];
                            if (Closed)
                            {
                                    lastSeg.ApproximationT[lastSeg.CacheSize] = this[0].ApproximationT[0];
                            }
                            else
                            {
                                lastSeg.ApproximationT[lastSeg.CacheSize] = lastSeg.ApproximationT[lastSeg.CacheSize - 1];
                                var lvp = LastVisibleControlPoint;
                                if (lvp)
                                {
                                    lvp.Approximation[0] = lastSeg.Approximation[lastSeg.CacheSize];
                                    lvp.ApproximationT[0] = lastSeg.ApproximationT[lastSeg.CacheSize];
                                }
                            }
                        }
                    }
#endregion
                }

                if (mDirtyOrientation && Count>0)
                {
#region --- Orientation ---
                    if (Orientation == CurvyOrientation.Static)
                    {
#region --- Static ---
                        if (UseThreading)
                        {
                            for (int i = 0; i < mDirtyControlPoints.Count; i++)
                            {
                                var dcp = mDirtyControlPoints[i];
                                mThreadWorker.QueueWorkItem(dcp.refreshOrientationStaticINTERNAL);
                            }
                            mThreadWorker.WaitAll();
                        }
                        else
                        {
                            for (int i = 0; i < mDirtyControlPoints.Count; i++)
                            {
                                mDirtyControlPoints[i].refreshOrientationStaticINTERNAL(); 
                            }
                        }
#endregion
                    }
                    else if (Orientation == CurvyOrientation.Dynamic)
                    {
#region --- Dynamic ---
                        // Ensure that the very first segment always sets orientation anchor
                        this[0].OrientationAnchor = true;

                        CurvySplineSegment startCP;
                        Vector3 lastUp;
                        int sampleCount;
                        CurvySplineSegment seg;
                        bool isSeg;
                        int anchorgroupSegmentCount;
                        float anchorgroupLength;
                        if (!AutoEndTangents && ControlPointCount>1)
                        {
                            mDirtyControlPoints.Remove(ControlPoints[0]);
                            mDirtyControlPoints.Remove(ControlPoints[ControlPointCount - 1]);
                        }
                        // process PTF and smoothing for all anchor groups inside dirty CPs
                        int dead = ControlPointCount+1;
                        do
                        {
                            // Pass I: calculate frame up to the next anchor (or end)
                            startCP = getCurrentAnchorGroup(mDirtyControlPoints[0]);
                            lastUp = startCP.getOrthoUp0INTERNAL();
                            seg = startCP;
                            sampleCount = 0;
                            anchorgroupSegmentCount = 0;
                            anchorgroupLength = 0;
                            do
                            {
                                sampleCount += seg.CacheSize;
                                anchorgroupSegmentCount++;
                                anchorgroupLength += seg.Length;
                                seg.refreshOrientationPTFINTERNAL(ref lastUp);
                                mDirtyControlPoints.Remove(seg);
                                seg = seg.NextControlPoint;
                                isSeg = seg.IsValidSegment;
                            } while (seg && !seg.OrientationAnchor && isSeg);
                            if (!isSeg)
                            {
                                //seg.ApproximationUp[0] = seg.PreviousControlPoint.ApproximationUp[seg.PreviousControlPoint.CacheSize];
                                mDirtyControlPoints.Remove(seg);
                            }
                            
                            // Pass II: Smooth out to match anchor rotation (if present) and/or add swirl
                            if (seg && (isSeg || seg.OrientationAnchor || seg.IsLastVisibleControlPoint))
                            {
                                float angleSmoothStep = lastUp.AngleSigned(seg.getOrthoUp0INTERNAL(), seg.ApproximationT[0]) / sampleCount;
                                float angleSegStep;
                                float angleAccu = angleSmoothStep;
                                seg = startCP;    
                                lastUp = seg.ApproximationUp[0];
                                do
                                {

                                    angleSegStep = angleSmoothStep;
                                    // Apply swirl
                                    switch (startCP.Swirl)
                                    {
                                        case CurvyOrientationSwirl.Segment:
                                            angleSegStep += startCP.SwirlTurns * 360 / seg.CacheSize;
                                            break;
                                        case CurvyOrientationSwirl.AnchorGroup:
                                            angleSegStep += (startCP.SwirlTurns * 360 / (float)anchorgroupSegmentCount) / seg.CacheSize;
                                            break;
                                        case CurvyOrientationSwirl.AnchorGroupAbs:
                                            angleSegStep += (startCP.SwirlTurns * 360) * (seg.Length / anchorgroupLength) / seg.CacheSize;
                                            break;
                                    }
                                    seg.smoothOrientationINTERNAL(ref lastUp, ref angleAccu, angleSegStep);
                                    seg = seg.NextSegment;
                                } while (seg && !seg.OrientationAnchor);
                            }


                        } while (mDirtyControlPoints.Count > 0 && dead-->0);
                        if (dead <= 0)
                            Debug.Log("[Curvy] Deadloop in CurvySpline.Refresh! Please raise a bugreport!");
#endregion
                    }
                    
                    // Handle very last CP
                    if (!Closed && Orientation!=CurvyOrientation.None)
                    {
                        var lcp = LastVisibleControlPoint;
                        var pp=lcp.PreviousControlPoint;
                        lcp.ApproximationUp[0] = pp.ApproximationUp[pp.CacheSize];
                        if (lcp.AutoBakeOrientation)
                            lcp.BakeOrientation(false);
                    }
#endregion
                }
            }
            mDirtyControlPoints.Clear();
            mDirtyCurve = false;
            mDirtyOrientation = false;
            mForceRefresh = false;
            mIsInitialized = true;
            OnRefreshEvent(new CurvySplineEventArgs(this, this, null));
        }
        
        /// <summary>
        /// Ensures the whole spline (curve & orientation) will be recalculated on next call to Refresh()
        /// </summary>
        public override void SetDirtyAll() 
        {
            SetDirtyAll(true, true); 
        }

        /// <summary>
        /// Ensure the whole spline will be recalculated on next call to Refresh()
        /// </summary>
        /// <param name="dirtyCurve">whether to refresh the curve</param>
        /// <param name="dirtyOrientation">whether to refresh orientation</param>
        public void SetDirtyAll(bool dirtyCurve = true, bool dirtyOrientation = true)
        {
            mForceRefresh = true;
            mDirtyCurve = dirtyCurve;
            mDirtyOrientation = dirtyOrientation;
        }

        /// <summary>
        /// Marks a Control Point to get recalculated on next call to Refresh()
        /// </summary>
        /// <param name="controlPoint">the Control Point to refresh</param>
        /// <param name="dirtyCurve">whether the curve should be recalculated</param>
        /// <param name="dirtyOrientation">whether the orientation should be recalculated</param>
        public void SetDirty(CurvySplineSegment controlPoint, bool dirtyCurve=true, bool dirtyOrientation=true)
        {
            if (controlPoint)
            {
                if (controlPoint.Spline != this && controlPoint.Spline != null)
                {
                    controlPoint.Spline.SetDirty(controlPoint, dirtyCurve, dirtyOrientation);
                    return;
                }
                    
                if (!mDirtyControlPoints.Contains(controlPoint))
                {
                    mDirtyControlPoints.Add(controlPoint);
                    if (controlPoint.FollowUp)
                        controlPoint.FollowUp.SetDirty();
                }
            }
            mDirtyCurve =mDirtyCurve || dirtyCurve;
            mDirtyOrientation = mDirtyCurve || dirtyOrientation;
        }

        /// <summary>
        /// Rebuilds the hierarchy from the ControlPoints list
        /// </summary>
        public void SyncHierarchyFromSpline(bool renameControlPoints=true)
        {
            // First clear non-existent Control Points (may be caused by e.g. deleting manually in the hierarchy window)
            for (int i = ControlPoints.Count - 1; i >= 0; i--)
                if (ControlPoints[i] == null)
                    ControlPoints.RemoveAt(i);
            // rename them and set their order based on ControlPoint list
            for (int i = 0; i < ControlPoints.Count; i++)
            {
                ControlPoints[i].ControlPointIndex = i;
                ControlPoints[i].transform.SetSiblingIndex(i);
                if (renameControlPoints)
                    ControlPoints[i].ApplyName();
                //ControlPoints[i].InitializeControlPointINTERNAL();
            }
        }

        /// <summary>
        /// Shortcut to transform.TransformPoint(localPosition)
        /// </summary>
        public Vector3 ToWorldPosition(Vector3 localPosition)
        {
            return transform.TransformPoint(localPosition);
        }

        /// <summary>
        /// Apply proper names to all Control Points
        /// </summary>
        public void ApplyControlPointsNames()
        {
            // rename them and set their order based on ControlPoint list
            for (int i = 0; i < ControlPoints.Count; i++)
            {
                ControlPoints[i].ApplyName();
            }
        }

        /// <summary>
        /// Rebuilds the ControlPoints list from the hierarchy
        /// </summary>
        public void SyncSplineFromHierarchy()
        {
            Segments.Clear();
            ControlPoints.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                var cp = transform.GetChild(i).GetComponent<CurvySplineSegment>();
                if (cp)
                {
                    cp.reSettleINTERNAL(false);
                    ControlPoints.Add(cp);
                    cp.TTransform.FromTransform(cp.transform);
                }
            }
        }


        #endregion

        #region --- Utilities ---

        /// <summary>
        /// Checks if the curve is planar
        /// </summary>
        /// <param name="ignoreAxis">returns the axis that can be ignored (0=x,1=y,2=z)</param>
        /// <returns>true if a planar axis was found</returns>
        public bool IsPlanar(out int ignoreAxis)
        {
            bool xp, yp, zp;
            bool res = IsPlanar(out xp, out yp, out zp);
            if (xp)
                ignoreAxis = 0;
            else if (yp)
                ignoreAxis = 1;
            else
                ignoreAxis = 2;
            return res;
        }
        
        /// <summary>
        /// Checks if the curve is planar
        /// </summary>
        /// <param name="xplanar">whether the x-axis is planar</param>
        /// <param name="yplanar">whether the y-axis is planar</param>
        /// <param name="zplanar">whether the z-axis is planar</param>
        /// <returns>true if at least on axis is planar</returns>
        public bool IsPlanar(out bool xplanar, out bool yplanar, out bool zplanar)
        {
            xplanar = true;
            yplanar = true;
            zplanar = true;
            if (ControlPointCount == 0) return true;
            Vector3 p = ControlPoints[0].TTransform.localPosition;
            for (int i = 1; i < ControlPointCount; i++)
            {
                if (!Mathf.Approximately(ControlPoints[i].TTransform.localPosition.x, p.x))
                    xplanar = false;
                if (!Mathf.Approximately(ControlPoints[i].TTransform.localPosition.y, p.y))
                    yplanar = false;
                if (!Mathf.Approximately(ControlPoints[i].TTransform.localPosition.z, p.z))
                    zplanar = false;

                if (xplanar == false && yplanar == false && zplanar == false)
                    return false;

            }
            return true;
        }

        /// <summary>
        /// Determines if the spline is at zero position on a certain plane
        /// </summary>
        /// <param name="plane">the plane the spline should be tested against</param>
        /// <returns>true if the spline is on the plane</returns>
        public bool IsPlanar(CurvyPlane plane)
        {
            switch (plane)
            {
                case CurvyPlane.XY:
                    for (int i = 0; i < ControlPointCount; i++)
                        if (ControlPoints[i].localPosition.z != 0)
                            return false;
                    break;
                case CurvyPlane.XZ:
                    for (int i = 0; i < ControlPointCount; i++)
                        if (ControlPoints[i].localPosition.y != 0)
                            return false;
                    break;
                case CurvyPlane.YZ:
                    for (int i = 0; i < ControlPointCount; i++)
                        if (ControlPoints[i].localPosition.x != 0)
                            return false;
                    break;
            }
            return true;
        }

        /// <summary>
        /// Forces the spline to be at zero position on a certain plane
        /// </summary>
        /// <param name="plane">the plane the should be on</param>
        public void MakePlanar(CurvyPlane plane)
        {
            switch (plane)
            {
                case CurvyPlane.XY:
                    for (int i = 0; i < ControlPointCount; i++)
                        if (ControlPoints[i].localPosition.z != 0)
                            ControlPoints[i].localPosition = new Vector3(ControlPoints[i].localPosition.x, ControlPoints[i].localPosition.y, 0);
                    break;
                case CurvyPlane.XZ:
                    for (int i = 0; i < ControlPointCount; i++)
                        if (ControlPoints[i].localPosition.y != 0)
                            ControlPoints[i].localPosition = new Vector3(ControlPoints[i].localPosition.x, 0,ControlPoints[i].localPosition.z);
                    break;
                case CurvyPlane.YZ:
                    for (int i = 0; i < ControlPointCount; i++)
                        if (ControlPoints[i].localPosition.x != 0)
                            ControlPoints[i].localPosition = new Vector3(0,ControlPoints[i].localPosition.y, ControlPoints[i].localPosition.z);
                    break;
            }
        }


        /// <summary>
        /// Subdivides the spline, i.e. adds additional segments to a certain range
        /// </summary>
        /// <param name="fromCP">starting ControlPoint</param>
        /// <param name="toCP">ending ControlPoint</param>
        public void Subdivide(CurvySplineSegment fromCP=null, CurvySplineSegment toCP=null)
        {
            if (!fromCP)
                fromCP = FirstVisibleControlPoint;
            if (!toCP)
                toCP = LastVisibleControlPoint;

            if (fromCP == null || toCP == null || fromCP.Spline != this || toCP.Spline != this)
            {
                Debug.Log("CurvySpline.Subdivide: Not a valid range selection!");
                return;
            }

            int startCPIndex = Mathf.Clamp(fromCP.ControlPointIndex,0, ControlPointCount - 2);
            int endCPIndex = Mathf.Clamp(toCP.ControlPointIndex,startCPIndex + 1, ControlPointCount - 1);

            if (endCPIndex - startCPIndex < 1)
            {
                Debug.Log("CurvySpline.Subdivide: Not a valid range selection!");
                return;
            }
            Vector3 newPos;
            for (int i = endCPIndex-1; i >= startCPIndex; i--)
            {
                newPos=ControlPoints[i].Interpolate(0.5f);
                var newCP = InsertAfter(ControlPoints[i]);
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.RegisterCreatedObjectUndo(newCP.gameObject, "Subdivide");
#endif
                newCP.localPosition = newPos;
            }
        }

        /// <summary>
        /// Simplifies the spline, i.e. remove segments from a certain range
        /// </summary>
        /// <param name="fromCP">starting ControlPoint</param>
        /// <param name="toCP">ending ControlPoint</param>
        public void Simplify(CurvySplineSegment fromCP = null, CurvySplineSegment toCP = null)
        {
            if (!fromCP)
                fromCP = FirstVisibleControlPoint;
            if (!toCP)
                toCP = LastVisibleControlPoint;

            if (fromCP == null || toCP == null || fromCP.Spline != this || toCP.Spline != this)
            {
                Debug.Log("CurvySpline.Simplify: Not a valid range selection!");
                return;
            }
            int startCPIndex = Mathf.Clamp(fromCP.ControlPointIndex, 0, ControlPointCount - 2);
            int endCPIndex = Mathf.Clamp(toCP.ControlPointIndex, startCPIndex + 2, ControlPointCount - 1);
            if (endCPIndex - startCPIndex < 2)
            {
                Debug.Log("CurvySpline.Simplify: Not a valid range selection!");
                return;
            }
            
            for (int i = endCPIndex - 2; i >= startCPIndex; i -= 2)
            {
#if UNITY_EDITOR
                _newSelectionInstanceIDINTERNAL = 42;
                if (!Application.isPlaying)
                {
                    Undo.SetCurrentGroupName("Simplify");
                    Undo.DestroyObjectImmediate(ControlPoints[i + 1].gameObject);
                }
                else
#endif
                    ControlPoints[i + 1].Delete();
#if UNITY_EDITOR
                _newSelectionInstanceIDINTERNAL = 0;
#endif
            }
            
        }

        /// <summary>
        /// Equalizes the segment length of a certain range
        /// </summary>
        /// <param name="fromCP">starting ControlPoint</param>
        /// <param name="toCP">ending ControlPoint</param>
        public void Equalize(CurvySplineSegment fromCP = null, CurvySplineSegment toCP = null)
        {
            if (!fromCP)
                fromCP = FirstVisibleControlPoint;
            if (!toCP)
                toCP = LastVisibleControlPoint;

            if (fromCP == null || toCP == null || fromCP.Spline != this || toCP.Spline != this)
            {
                Debug.Log("CurvySpline.Equalize: Not a valid range selection!");
                return;
            }
            int startCPIndex = Mathf.Clamp(fromCP.ControlPointIndex, 0, ControlPointCount - 2);
            int endCPIndex = Mathf.Clamp(toCP.ControlPointIndex, startCPIndex + 2, ControlPointCount - 1);
            if (endCPIndex - startCPIndex < 2)
            {
                Debug.Log("CurvySpline.Equalize: Not a valid range selection!");
                return;
            }
            float length = ControlPoints[endCPIndex].Distance - ControlPoints[startCPIndex].Distance;
            float equal = length / (endCPIndex - startCPIndex);
            float dist=ControlPoints[startCPIndex].Distance;
            for (int i = startCPIndex + 1; i < endCPIndex; i++)
            {
                dist+=equal;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.RecordObject(ControlPoints[i].transform, "Equalize");
#endif
                ControlPoints[i].localPosition = InterpolateByDistance(dist);
            }
        }

        /// <summary>
        /// Applies a spline's scale to it's Control Points and resets scale
        /// </summary>
        public void Normalize()
        {
            Vector3 scl = transform.localScale;

            if (scl != Vector3.one)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.RecordObject(transform, "Normalize Spline");
#endif
                transform.localScale = Vector3.one;
                for (int i = 0; i < ControlPointCount; i++)
                {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.RecordObject(ControlPoints[i].transform, "Normalize Spline");
#endif
                    ControlPoints[i].localPosition = Vector3.Scale(ControlPoints[i].localPosition, scl);

                }


            }
        }

        /// <summary>
        /// Equalize one axis of the spline to match the first control points's value
        /// </summary>
        /// <param name="axis">the axis to equalize (0=x,1=y,2=z)</param>
        public void MakePlanar(int axis)
        {
            Vector3 p = ControlPoints[0].transform.localPosition;
            for (int i = 1; i < ControlPointCount; i++)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.RecordObject(ControlPoints[i].transform, "MakePlanar");
#endif
                Vector3 pi = ControlPoints[i].transform.localPosition;
                switch (axis)
                {
                    case 0: pi.x = p.x; break;
                    case 1: pi.y = p.y; break;
                    case 2: pi.z = p.z; break;
                }
                ControlPoints[i].transform.localPosition = pi;
            }
            SetDirtyAll();
        }

        /// <summary>
        /// Sets the pivot of the spline
        /// </summary>
        /// <param name="xRel">-1 to 1</param>
        /// <param name="yRel">-1 to 1</param>
        /// <param name="zRel">-1 to 1</param>
        /// <param name="preview">if true, only return the new pivot position</param>
        /// <returns>the new pivot position</returns>
        public Vector3 SetPivot(float xRel = 0, float yRel = 0, float zRel = 0, bool preview = false)
        {
            Bounds b = Bounds;
            Vector3 v = new Vector3(b.min.x + b.size.x * ((xRel + 1) / 2),
                                    b.max.y - b.size.y * ((yRel + 1) / 2),
                                    b.min.z + b.size.z * ((zRel + 1) / 2));

            Vector3 off = transform.position - v;
            if (preview)
                return transform.position - off;

            foreach (CurvySplineSegment cp in ControlPoints)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.RecordObject(cp.transform, "SetPivot");
#endif
                cp.transform.position += off;
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RecordObject(transform, "SetPivot");
#endif
            transform.position -= off;
            SetDirtyAll();
            return transform.position;
        }

        /// <summary>
        /// Flips the direction of the spline, i.e. the first Control Point will become the last and vice versa.
        /// </summary>
        public void Flip()
        {
            if (ControlPointCount <= 1)
                return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RegisterFullObjectHierarchyUndo(this,"Flip Spline");
#endif
            switch (Interpolation)
            {
                case CurvyInterpolation.TCB:
                    Bias *= -1;
                    for (int i = ControlPointCount - 1; i >= 0; i--)
                    {
                        CurvySplineSegment cur = ControlPoints[i];

                        int j = i - 1;
                        if (j >= 0)
                        {
                            CurvySplineSegment prev = ControlPoints[j];

                            cur.EndBias = prev.StartBias * -1;
                            cur.EndContinuity = prev.StartContinuity;
                            cur.EndTension = prev.StartTension;

                            cur.StartBias = prev.EndBias * -1;
                            cur.StartContinuity = prev.EndContinuity;
                            cur.StartTension = prev.EndTension;

                            cur.OverrideGlobalBias = prev.OverrideGlobalBias;
                            cur.OverrideGlobalContinuity = prev.OverrideGlobalContinuity;
                            cur.OverrideGlobalTension = prev.OverrideGlobalTension;

                            cur.SynchronizeTCB = prev.SynchronizeTCB;
                        }
                    }
                    break;
                case CurvyInterpolation.Bezier:
                    for (int i = ControlPointCount - 1; i >= 0; i--)
                    {
                        CurvySplineSegment cur = ControlPoints[i];

                        Vector3 h = cur.HandleIn;
                        cur.HandleIn = cur.HandleOut;
                        cur.HandleOut = h;


                    }
                    break;
            }
            ControlPoints.Reverse();
            SetDirtyAll();
            SyncHierarchyFromSpline();
            Refresh();
        }

        /// <summary>
        /// Moves ControlPoints from this spline, inserting them after a destination ControlPoint of another spline
        /// </summary>
        /// <param name="startIndex">ControlPointIndex of the first CP to move</param>
        /// <param name="count">number of ControlPoints to move</param>
        /// <param name="destCP">ControlPoint at the destination spline to insert after</param>
        public void MoveControlPoints(int startIndex, int count, CurvySplineSegment destCP)
        {
            if (!destCP || this == destCP.Spline || destCP.ControlPointIndex == -1)
                return;
            startIndex = Mathf.Clamp(startIndex, 0, ControlPointCount - 1);
            count = Mathf.Clamp(count, startIndex, ControlPointCount - startIndex);

            CurvySplineSegment cp;
            for (int i = 0; i < count; i++)
            {
                cp = ControlPoints[startIndex];
                cp.reSettleINTERNAL();
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.SetTransformParent(cp.transform, destCP.Spline.transform, "Move ControlPoints");
                else
#endif
                    cp.transform.SetParent(destCP.Spline.transform, true);

                destCP.Spline.ControlPoints.Insert(destCP.ControlPointIndex + i + 1, cp);
            }
            destCP.SetDirty();
            Refresh();
            destCP.Spline.Refresh();
        }

        /// <summary>
        /// Insert this spline after another spline's destination Control Point and delete this spline
        /// </summary>
        /// <param name="destCP">the Control Point of the destination spline</param>
        public void JoinWith(CurvySplineSegment destCP)
        {
            if (destCP.Spline == this)
                return;
            MoveControlPoints(0, ControlPointCount, destCP);
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.DestroyObjectImmediate(gameObject);
            else
#endif
                GameObject.Destroy(gameObject);
        }

        #endregion

        /// <summary>
        /// Event-friedly helper that sets a field or property value
        /// </summary>
        /// <param name="fieldAndValue">e.g. "MyValue=42"</param>
        public void SetFromString(string fieldAndValue)
        {
            string[] f = fieldAndValue.Split('=');
            if (f.Length != 2)
                return;

            var fi = this.GetType().FieldByName(f[0], true, false);
            if (fi != null)
            {
                try
                {
#if NETFX_CORE
                    if (fi.FieldType.GetTypeInfo().IsEnum)
#else
                    if (fi.FieldType.IsEnum)
#endif
                        fi.SetValue(this, System.Enum.Parse(fi.FieldType, f[1]));
                    else
                        fi.SetValue(this, System.Convert.ChangeType(f[1], fi.FieldType));
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(this.name + ".SetFromString(): " + e.ToString());
                }
            }
            else
            {
                var pi = this.GetType().PropertyByName(f[0], true, false);
                if (pi != null)
                {
                    try
                    {
#if NETFX_CORE
                        if (pi.PropertyType.GetTypeInfo().IsEnum)
#else
                        if (pi.PropertyType.IsEnum)
#endif

                            pi.SetValue(this, System.Enum.Parse(pi.PropertyType, f[1]), null);
                        else
                            pi.SetValue(this, System.Convert.ChangeType(f[1], pi.PropertyType), null);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning(this.name + ".SetFromString(): " + e.ToString());
                    }
                }
            }
        }

        #endregion

        #region ### Privates & Internals ###
        /*! \cond PRIVATE */

        protected override bool UpgradeVersion(string oldVersion, string newVersion)
        {
            if (string.IsNullOrEmpty(oldVersion))
            {
                ControlPoints.Sort((x, y) => (x.name.CompareTo(y.name)));
            }
            return true;
        }

#if UNITY_EDITOR
        public static int _newSelectionInstanceIDINTERNAL; // Editor Bridge helper to determine new selection after object deletion
#endif

        void doUpdate()
        {
            if (TTransform != transform)
            {
                TTransform.FromTransform(transform);
                clearBounds();
            }


            if (!IsInitialized)
            {
                SetDirtyAll();
            }
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (mLastCPCount != ControlPointCount)
                {
                    SyncHierarchyFromSpline();
                    SetDirtyAll();
                }
                mLastCPCount = ControlPointCount;


            }
#endif

            if (CheckTransform || !Application.isPlaying)
                for (int i = 0; i < ControlPointCount; i++)
                    ControlPoints[i].RefreshTransform();

            if (Dirty)
                Refresh();
            
        }

        Bounds getBounds()
        {
            if (Count > 0)
            {
                Bounds b = this[0].Bounds;

                for (int i = 1; i < Count; i++)
                    b.Encapsulate(this[i].Bounds);
                return b;
            } else
                return new Bounds(TTransform.position, Vector3.zero);
        }

        void clearBounds()
        {
            mBounds = null;
            for (int i = 0; i < ControlPointCount; i++)
                ControlPoints[i].ClearBoundsINTERNAL();
        }

        /// <summary>
        /// are manual start/end CP's allowed?
        /// </summary>
        bool canHaveManualEndCP()
        {
            return !Closed && (Interpolation == CurvyInterpolation.CatmullRom || Interpolation == CurvyInterpolation.TCB);
        }

        internal void setLengthINTERNAL(float length)
        {
            mLength = length;
        }

        bool getPreviousApproximationPoint(CurvySplineSegment seg, int idx, out CurvySplineSegment res, out int residx, ref CurvySplineSegment[] validSegments)
        {
            residx = idx - 1;
            res = seg;
            if (residx < 0)
            {
                res = seg.PreviousSegment;
                if (res)
                {
                    residx = res.Approximation.Length - 2;
                    for (int i = 0; i < validSegments.Length; i++)
                        if (validSegments[i] == res)
                            return true;
                    return false;
                }
                return (false);
            }
            return true;
        }

        

        bool getNextApproximationPoint(CurvySplineSegment seg, int idx, out CurvySplineSegment res, out int residx, ref CurvySplineSegment[] validSegments)
        {
            residx = idx + 1;
            res = seg;
            if (residx == seg.Approximation.Length)
            {
                res = seg.NextSegment;
                if (res)
                {
                    residx = 1;
                    for (int i = 0; i < validSegments.Length; i++)
                        if (validSegments[i] == res)
                            return true;
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// gets the next segment within a certain range. If tf is at boundaries of direction (0 or 1), null is returned
        /// </summary>
        /// <param name="seg">current segment</param>
        /// <param name="tf">current tf</param>
        /// <param name="fDistance">distance*direction</param>
        /// <param name="segTF">TF of a found segment</param>
        /// <returns>the next (if forward) or the same segment (if backwards) or null if nothing in range</returns>
        CurvySplineSegment getNextSegmentWithinRange(CurvySplineSegment seg, float tf, float fDistance, out float segTF)
        {
            segTF = -1;
            if (seg)
            {
                if (fDistance > 0 && tf < 1)
                {
                    var next = seg.NextControlPoint;
                    segTF = SegmentToTF(next);
                    if (segTF == 0 && Closed)
                        segTF = 1;
                    return (segTF - tf <= fDistance) ? next : null;
                }
                else if (fDistance < 0 && tf > 0)
                {
                    if (tf == 1 && Closed)
                        tf = 0;
                    var next = (SegmentToTF(seg) == tf) ? seg.PreviousControlPoint : seg;
                    segTF = SegmentToTF(next);
                    return (Mathf.Abs(tf - segTF) <= -fDistance) ? next : null;
                }
            }
            return null;
        }

        /// <summary>
        /// gets the next segment within a certain range. If distance is at boundaries of direction (0 or length), null is returned
        /// </summary>
        /// <param name="seg">current segment</param>
        /// <param name="tf">current tf</param>
        /// <param name="tfDist">current distance of tf</param>
        /// <param name="distance">distance*direction</param>
        /// <param name="segDistance">Distance of a found segment</param>
        /// <returns>the next (if forward) or the same segment (if backwards) or null if nothing in range</returns>
        CurvySplineSegment getNextSegmentWithinRangeDistance(CurvySplineSegment seg, float tf, float tfDist, float distance, out float segDistance)
        {
            segDistance = -1;
            if (seg)
            {
                if (distance > 0 && tf < 1)
                {
                    var next = seg.NextControlPoint;
                    segDistance = (next.Distance == 0 && Closed) ? Length : next.Distance;
                    return (next && segDistance - tfDist <= distance) ? next : null;
                }
                else if (distance < 0 && tf > 0)
                {
                    if (tf == 1 && Closed)
                        tf = 0;
                    var next = (SegmentToTF(seg) == tf) ? seg.PreviousControlPoint : seg;
                    if (!next)
                        return null;
                    segDistance = next.Distance;
                    return (next && (Mathf.Abs(tfDist-segDistance)<=-distance)) ? next : null;
                }
            }
            return null;
        }



        /// <summary>
        /// Same like Move(), but fires OnMoveControlPointReached event and checks for markers
        /// </summary>
        Vector3 eventAwareMove(ref float tf, ref int direction, float fDistance, CurvyClamping clamping, bool fastMode)
        {
            var seg = TFToSegment(tf);
            var e = _moveEventArgs;
            e.Sender = this;
            e.Spline = this;
            e.ControlPoint = seg;
            e.TF = tf;
            e.Delta = fDistance;
            e.Direction = direction;
            e.WorldUnits = false;
            e.Cancel = false;
            CurvySplineSegment nextSegInRange;
            float nextSegInRangeTF;
            bool fireEvent;
            int dead = 2000;

            while (!e.Cancel && e.Delta > 0 && dead-- > 0)
            {
                nextSegInRange = e.Spline.getNextSegmentWithinRange(e.ControlPoint, e.TF, e.Delta * e.Direction, out nextSegInRangeTF); // if dir=-1, this gets the same segment

                if (nextSegInRange)
                {
                    e.ControlPoint = nextSegInRange;

                    e.Delta -= Mathf.Abs(nextSegInRangeTF - e.TF);
                    fireEvent = e.TF != nextSegInRangeTF;
                    e.TF = nextSegInRangeTF;
                    // Fire event (prevent firing for same segment <== This is causing issues!)
                    if (fireEvent)
                        e.Spline.OnMoveControlPointReachedEvent(e);

                    if (e.Direction < 0)
                    {
#region --- Reverse Movement ---
                        if (e.TF == 0)
                        {
                            switch (clamping)
                            {
                                case CurvyClamping.Clamp:
                                    e.Delta = 0;
                                    break;
                                case CurvyClamping.Loop:
                                    e.TF = 1;
                                    e.ControlPoint = LastVisibleControlPoint;
                                    if (!Closed)
                                        OnMoveControlPointReachedEvent(e);
                                    break;
                                case CurvyClamping.PingPong:
                                    e.Direction *= -1;
                                    break;
                            }
                        }
                        if (e.ControlPoint == nextSegInRange || e.ControlPoint.LocalFToTF(0) == e.TF)
                            e.ControlPoint = e.ControlPoint.PreviousSegment;
#endregion
                    }

                    else if (e.Direction >= 0 && e.TF == 1)
                    {
                        switch (clamping)
                        {
                            case CurvyClamping.Clamp:
                                e.Delta = 0;
                                break;
                            case CurvyClamping.Loop:
                                e.TF = 0;
                                e.ControlPoint = e.Spline.FirstVisibleControlPoint;
                                if (!Closed)
                                    e.Spline.OnMoveControlPointReachedEvent(e);
                                break;
                            case CurvyClamping.PingPong:
                                e.Direction *= -1;
                                break;
                        }
                    }

                }
                else
                {
                    e.TF += e.Delta * e.Direction;
                    e.Delta = 0;
                }

            }
            if (dead <= 0)
                Debug.Log("[Curvy] HE'S DEAD, JIM! Infinite loops shouldn't happen! Please raise a Bug Report!");

            tf = e.TF;

            direction = e.Direction;
            return (fastMode) ? e.Spline.InterpolateFast(tf) : e.Spline.Interpolate(tf);
        }

        Vector3 eventAwareMoveDistance(ref float tf, ref int direction, float distance, CurvyClamping clamping, bool fastMode)
        {
            var seg = TFToSegment(tf);
            float tfDist;
            var e = _moveEventArgs;
            e.Sender = this;
            e.Spline = this;
            e.ControlPoint = seg;
            e.TF = tf;
            e.Delta = distance;
            e.Direction = direction;
            e.WorldUnits = true;
            e.Cancel = false;

            CurvySplineSegment nextSegInRange;
            float nextSegInRangeDistance;
            bool fireEvent;
            int dead = 2000;

            while (!e.Cancel && e.Delta > 0 && dead-- > 0)
            {
                tfDist = e.Spline.TFToDistance(e.TF);
                nextSegInRange = e.Spline.getNextSegmentWithinRangeDistance(e.ControlPoint, e.TF, tfDist, e.Delta * e.Direction, out nextSegInRangeDistance); // if dir=-1, this gets the same segment

                if (nextSegInRange)
                {
                    e.ControlPoint = nextSegInRange;
                    e.Delta -= Mathf.Abs(nextSegInRangeDistance - tfDist);
                    fireEvent = e.TF != nextSegInRangeDistance;
                    e.TF = e.Spline.SegmentToTF(nextSegInRange);
                    // Fire event (prevent firing for same segment <== This is causing issues!)
                    if (fireEvent)
                        OnMoveControlPointReachedEvent(e);
                    if (e.Spline != this)
                        tfDist = e.Spline.TFToDistance(e.TF);

                    if (e.Direction < 0)
                    {
#region --- Reverse Movement ---
                        if (e.TF == 0)
                        {
                            switch (clamping)
                            {
                                case CurvyClamping.Clamp:
                                    e.Delta = 0;
                                    break;
                                case CurvyClamping.Loop:
                                    e.TF = 1;
                                    e.ControlPoint = LastVisibleControlPoint;
                                    if (!Closed)
                                        OnMoveControlPointReachedEvent(e);
                                    break;
                                case CurvyClamping.PingPong:
                                    e.Direction *= -1;
                                    break;
                            }
                        }
                        if (e.ControlPoint == nextSegInRange || e.ControlPoint.LocalFToTF(0) == e.TF)
                            e.ControlPoint = e.ControlPoint.PreviousSegment;
#endregion
                    }

                    else if (e.Direction >= 0 && e.TF == 1)
                    {
                        switch (clamping)
                        {
                            case CurvyClamping.Clamp:
                                e.Delta = 0;
                                break;
                            case CurvyClamping.Loop:
                                e.TF = 0;
                                e.ControlPoint = FirstVisibleControlPoint;
                                if (!Closed)
                                    OnMoveControlPointReachedEvent(e);
                                break;
                            case CurvyClamping.PingPong:
                                e.Direction *= -1;
                                break;
                        }
                    }

                }
                else
                {
                    e.TF = e.Spline.DistanceToTF(tfDist + e.Delta * e.Direction);
                    e.Delta = 0;
                }

            }
            if (dead <= 0)
                Debug.Log("[Curvy] HE'S DEAD, JIM! Infinite loops shouldn't happen! Please raise a Bug Report!");

            tf = e.TF;

            direction = e.Direction;
            return (fastMode) ? e.Spline.InterpolateFast(tf) : e.Spline.Interpolate(tf);
        }

        internal bool MoveByAngleExtINTERNAL(ref float tf, float minDistance, float maxDistance, float maxAngle, out Vector3 pos, out Vector3 tan, out float movedDistance, float stopTF = float.MaxValue, bool loop = true, float stepDist = -1)
        {
            if (stepDist == -1)
                stepDist = 1f / CacheSize;
            minDistance = Mathf.Max(0, minDistance);
            maxDistance = Mathf.Max(minDistance, maxDistance);
            if (!loop)
                tf = Mathf.Clamp01(tf);
            float tn = (loop) ? tf % 1 : tf;
            pos = Interpolate(tn);
            tan = GetTangent(tn, pos);
            Vector3 lastPos = pos;
            Vector3 lastTan = tan;

            movedDistance = 0;
            float angleAccu = 0;

            int linearSteps = 0;
            if (stopTF < tf && loop)
                stopTF++;
            while (tf < stopTF)
            {
                tf = Mathf.Min(stopTF, tf + stepDist);
                tn = (loop) ? tf % 1 : tf;
                // Get pos & tan
                pos = Interpolate(tn);
                tan = GetTangent(tn, pos);
                movedDistance += (pos - lastPos).magnitude;
                angleAccu += Vector3.Angle(lastTan, tan);
                // increase linear segment counter?
                if (tan == lastTan)
                    linearSteps++;
                // Check if conditions are met
                if (movedDistance >= minDistance)
                {
                    // Max Distance or Angle
                    if (movedDistance >= maxDistance) // max distance reached
                        break;
                    
                    if (angleAccu >= maxAngle)
                    {
                        angleAccu = 0;
                        break;
                    }
                    else if (linearSteps > 0 && angleAccu > 0)
                        break;
                }
                lastPos = pos;
                lastTan = tan;
            }

            return Mathf.Approximately(tf, stopTF);
        }

        CurvySplineSegment getCurrentAnchorGroup(CurvySplineSegment seg)
        {
            if (!AutoEndTangents && seg.IsFirstControlPoint)
                return FirstVisibleControlPoint;
            while (seg && !seg.OrientationAnchor)
                seg = seg.PreviousSegment;
            return seg;
        }

        /*! \endcond */
#endregion
        
    }

    
}
