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
using FluffyUnderware.DevTools.Extensions;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Utils;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using UnityEngine.Serialization;
using System.Reflection;
using System;



namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Class covering a Curvy Spline Segment / ControlPoint
    /// </summary>
    [ExecuteInEditMode]
    [HelpURL(CurvySpline.DOCLINK + "curvysplinesegment")]
    public class CurvySplineSegment : MonoBehaviour, IComparable, IPoolable
    {

        #region ### Serialized Fields ###

        #region --- General ---

        [Group("General")]
        [FieldAction("CBBakeOrientation", Position = ActionAttribute.ActionPositionEnum.Below)]
        [Label("Bake Orientation", "Automatically apply orientation to CP transforms?")]
        [SerializeField]
        bool m_AutoBakeOrientation = false;

        [Group("General")]
        [Tooltip("Check to use this transform's rotation")]
        [FieldCondition("IsFirstSegment",false,Action=ActionAttribute.ActionEnum.Enable)]
        [FieldCondition("isDynamicOrientation", true)]
        [SerializeField]
        bool m_OrientationAnchor;

        [Label("Swirl", "Add Swirl to orientation?")]
        [Group("General")]
        [FieldCondition("canHaveSwirl", true)]
        [SerializeField]
        CurvyOrientationSwirl m_Swirl = CurvyOrientationSwirl.None;

        [Label("Turns", "Number of swirl turns")]
        [Group("General")]
        [FieldCondition("canHaveSwirl", true, false, ConditionalAttribute.OperatorEnum.AND, "m_Swirl", CurvyOrientationSwirl.None, true)]
        [SerializeField]
        float m_SwirlTurns;
        
        #endregion

        #region --- Bezier ---

        [Section("Bezier Options", Sort = 1, HelpURL = CurvySpline.DOCLINK + "curvysplinesegment_bezier")]
        [GroupCondition("interpolation",CurvyInterpolation.Bezier)]
        [SerializeField]
        bool m_AutoHandles = true;

        [RangeEx(0, 1, "Distance %", "Handle length by distance to neighbours")]
        [FieldCondition("m_AutoHandles",true,Action=ConditionalAttribute.ActionEnum.Enable)]
        [SerializeField]
        float m_AutoHandleDistance = 0.39f;

        [VectorEx(Precision=3,Options=AttributeOptionsFlags.Clipboard | AttributeOptionsFlags.Negate, Color="#FFFF00")]
        [SerializeField, FormerlySerializedAs("HandleIn")]
        Vector3 m_HandleIn = new Vector3(-1, 0, 0);

        [VectorEx(Precision=3,Options = AttributeOptionsFlags.Clipboard | AttributeOptionsFlags.Negate, Color="#00FF00")]
        [SerializeField, FormerlySerializedAs("HandleOut")]
        Vector3 m_HandleOut = new Vector3(1, 0, 0);

        #endregion

        #region --- TCB ---

        [Section("TCB Options", Sort = 1, HelpURL = CurvySpline.DOCLINK + "curvysplinesegment_tcb")]
        [GroupCondition("interpolation", CurvyInterpolation.TCB)]
        [GroupAction("TCBOptionsGUI", Position = ActionAttribute.ActionPositionEnum.Below)]

        [Label("Local Tension","Override Spline Tension?")]
        [SerializeField, FormerlySerializedAs("OverrideGlobalTension")]
        bool m_OverrideGlobalTension;

        [Label("Local Continuity", "Override Spline Continuity?")]
        [SerializeField, FormerlySerializedAs("OverrideGlobalContinuity")]
        bool m_OverrideGlobalContinuity;

        [Label("Local Bias", "Override Spline Bias?")]
        [SerializeField, FormerlySerializedAs("OverrideGlobalBias")]
        bool m_OverrideGlobalBias;
        [Tooltip("Synchronize Start and End Values")]
        [SerializeField, FormerlySerializedAs("SynchronizeTCB")]
        bool m_SynchronizeTCB = true;
        [Label("Tension"),FieldCondition("m_OverrideGlobalTension",true)]
        [SerializeField, FormerlySerializedAs("StartTension")]
        float m_StartTension;

        [Label("Tension (End)"), FieldCondition("m_OverrideGlobalTension", true,false,ConditionalAttribute.OperatorEnum.AND,"m_SynchronizeTCB",false,false)]
        [SerializeField, FormerlySerializedAs("EndTension")]
        float m_EndTension;

        [Label("Continuity"), FieldCondition("m_OverrideGlobalContinuity", true)]
        [SerializeField, FormerlySerializedAs("StartContinuity")]
        float m_StartContinuity;

        [Label("Continuity (End)"), FieldCondition("m_OverrideGlobalContinuity", true, false, ConditionalAttribute.OperatorEnum.AND, "m_SynchronizeTCB", false, false)]
        [SerializeField, FormerlySerializedAs("EndContinuity")]
        float m_EndContinuity;
        
        [Label("Bias"), FieldCondition("m_OverrideGlobalBias", true)]
        [SerializeField, FormerlySerializedAs("StartBias")]
        float m_StartBias;

        [Label("Bias (End)"), FieldCondition("m_OverrideGlobalBias", true, false, ConditionalAttribute.OperatorEnum.AND, "m_SynchronizeTCB", false, false)]
        [SerializeField, FormerlySerializedAs("EndBias")]
        float m_EndBias;

        #endregion
        /*
        #region --- CG Options ---
        
        /// <summary>
        /// Material ID (used by CG)
        /// </summary>
        [Section("Generator Options", true, Sort = 5, HelpURL = CurvySpline.DOCLINK + "curvysplinesegment_cg")]
        [Positive(Label="Material ID")]
        [SerializeField]
        int m_CGMaterialID;

        /// <summary>
        /// Whether to create a hard edge or not (used by PCG)
        /// </summary>
        [Label("Hard Edge")]
        [SerializeField]
        bool m_CGHardEdge;
        /// <summary>
        /// Maximum vertex distance when using optimization (0=infinite)
        /// </summary>
        [Positive(Label="Max Step Size",Tooltip="Max step distance when using optimization")]
        [SerializeField]
        float m_CGMaxStepDistance;
        #endregion
        */
        #region --- Connections ---
        
        [SerializeField,HideInInspector]
        CurvySplineSegment m_FollowUp;
        [SerializeField,HideInInspector]
        ConnectionHeadingEnum m_FollowUpHeading = ConnectionHeadingEnum.Auto;
        [SerializeField,HideInInspector]
        bool m_ConnectionSyncPosition;
        [SerializeField,HideInInspector]
        bool m_ConnectionSyncRotation;

        [SerializeField,HideInInspector]
        CurvyConnection m_Connection;

        #endregion

        #endregion

        #region ### Public Properties ###

        /// <summary>
        /// List of precalculated interpolations
        /// </summary>
        /// <remarks>Based on Spline's CacheDensity</remarks>
        [NonSerialized]        
        public Vector3[] Approximation = new Vector3[0];

        /// <summary>
        /// List of precalculated distances
        /// </summary>
        /// <remarks>Based on Spline's CacheDensity</remarks>
        [NonSerialized]
        public float[] ApproximationDistances = new float[0];

        /// <summary>
        /// List of precalculated Up-Vectors
        /// </summary>
        /// <remarks>Based on Spline's CacheDensity</remarks>
        [NonSerialized]
        public Vector3[] ApproximationUp = new Vector3[0];

        /// <summary>
        /// List of precalculated Tangent-Normals
        /// </summary>
        /// <remarks>Based on Spline's CacheDensity</remarks>
        [NonSerialized]
        public Vector3[] ApproximationT = new Vector3[0];

        /// <summary>
        /// If set, Control Point's rotation will be set to the calculated Up-Vector3
        /// </summary>
        /// <remarks>This is particularly useful when connecting splines</remarks>
        public bool AutoBakeOrientation
        {
            get { return m_AutoBakeOrientation; }
            set
            {
                if (m_AutoBakeOrientation != value)
                {
                    m_AutoBakeOrientation = value;
                    SetDirty(false, true);
                }
            }
        }
        
        /// <summary>
        /// Whether this Control Point acts as an Orientation Anchor
        /// </summary>
        public bool OrientationAnchor
        {
            get { return m_OrientationAnchor; }
            set
            {
                if (m_OrientationAnchor != value)
                {
                    m_OrientationAnchor = value;
                    SetDirty(false,true);
                }
            }
        }

        /// <summary>
        /// Swirling Mode
        /// </summary>
        public CurvyOrientationSwirl Swirl
        {
            get { return m_Swirl; }
            set
            {
                if (m_Swirl != value)
                {
                    m_Swirl = value;
                    SetDirty(false, true);
                }
            }
        }

        /// <summary>
        /// Turns to swirl
        /// </summary>
        public float SwirlTurns
        {
            get { return m_SwirlTurns; }
            set
            {
                float v=Mathf.Max(0, value);
                if (m_SwirlTurns != v)
                {
                    m_SwirlTurns = v;
                    SetDirty(false, true);
                }
            }
        }

        #region --- Bezier ---

        /// <summary>
        /// Left B-Spline Handle in local coordinates
        /// </summary>
        public Vector3 HandleIn
        {
            get
            { return m_HandleIn; }
            set
            {
                if (m_HandleIn != value)
                {
                    m_HandleIn = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Right B-Spline Handle in local coordinates
        /// </summary>
        public Vector3 HandleOut
        {
            get { return m_HandleOut; }
            set
            {
                if (m_HandleOut != value)
                {
                    m_HandleOut = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Left B-Spline Handle in world coordinates
        /// </summary>
        public Vector3 HandleInPosition
        {
            get
            {
                return TTransform.position + Spline.TTransform.rotation * HandleIn;
            }
            set
            {
                HandleIn = Spline.transform.InverseTransformDirection(value - TTransform.position);
            }
        }

        /// <summary>
        /// Right B-Spline Handle in world coordinates
        /// </summary>
        public Vector3 HandleOutPosition
        {
            get
            {
                return TTransform.position + Spline.TTransform.rotation * HandleOut;
            }
            set
            {
                HandleOut = Spline.transform.InverseTransformDirection(value - TTransform.position);
            }
        }
     
        public bool AutoHandles
        {
            get { return m_AutoHandles; }
            set
            {
                if (m_AutoHandles != value)
                {
                    m_AutoHandles = value;
                    var conCP = ConnectedControlPoints;
                    for (int i = 0; i < conCP.Count; i++)
                    {
                        conCP[i].m_AutoHandles = value;
                        conCP[i].SetDirty();
                    }
                }
                SetDirty();
            }
        }

        public float AutoHandleDistance
        {
            get { return m_AutoHandleDistance; }
            set
            {
                if (m_AutoHandleDistance != value)
                    m_AutoHandleDistance = Mathf.Clamp01(value);
                SetDirty();
            }
        }

        #endregion

        #region --- TCB ---

        /// <summary>
        /// Keep Start/End-TCB synchronized
        /// </summary>
        /// <remarks>Applies only to TCB Interpolation</remarks>
        public bool SynchronizeTCB
        {
            get { return m_SynchronizeTCB; }
            set
            {
                if (m_SynchronizeTCB != value)
                {
                    m_SynchronizeTCB = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Whether local Tension should be used
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Tension</remarks>
        public bool OverrideGlobalTension
        {
            get { return m_OverrideGlobalTension; }
            set
            {
                if (m_OverrideGlobalTension != value)
                {
                    m_OverrideGlobalTension = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Whether local Continuity should be used
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Continuity</remarks>
        public bool OverrideGlobalContinuity
        {
            get { return m_OverrideGlobalContinuity; }
            set
            {
                if (m_OverrideGlobalContinuity != value)
                {
                    m_OverrideGlobalContinuity = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Whether local Bias should be used
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Bias</remarks>
        public bool OverrideGlobalBias
        {
            get { return m_OverrideGlobalBias; }
            set
            {
                if (m_OverrideGlobalBias != value)
                {
                    m_OverrideGlobalBias = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Start Tension
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Tension</remarks>
        public float StartTension
        {
            get { return m_StartTension; }
            set
            {
                if (m_StartTension != value)
                {
                    m_StartTension = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Start Continuity
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Continuity</remarks>
        public float StartContinuity
        {
            get { return m_StartContinuity; }
            set
            {
                if (m_StartContinuity != value)
                {
                    m_StartContinuity = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Start Bias
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Bias</remarks>
        public float StartBias
        {
            get { return m_StartBias; }
            set
            {
                if (m_StartBias != value)
                {
                    m_StartBias = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// End Tension
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Tension</remarks>
        public float EndTension
        {
            get { return m_EndTension; }
            set
            {
                if (m_EndTension != value)
                {
                    m_EndTension = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// End Continuity
        /// </summary>
        /// <remarks>This only applies to interpolation methods using Continuity</remarks>
        public float EndContinuity
        {
            get { return m_EndContinuity; }
            set
            {
                if (m_EndContinuity != value)
                {
                    m_EndContinuity = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// End Bias
        /// </summary>
        ///<remarks>This only applies to interpolation methods using Bias</remarks>
        public float EndBias
        {
            get { return m_EndBias; }
            set
            {
                if (m_EndBias != value)
                {
                    m_EndBias = value;
                    SetDirty();
                }
            }
        }


        #endregion
        /*
        #region --- CG ---

        /// <summary>
        /// Material ID (used by PCG)
        /// </summary>
        public int CGMaterialID
        {
            get
            {
                return m_CGMaterialID;
            }
            set
            {
                if (m_CGMaterialID != Mathf.Max(0, value))
                    m_CGMaterialID = Mathf.Max(0, value);
            }
        }

        /// <summary>
        /// Whether to create a hard edge or not (used by PCG)
        /// </summary>
        public bool CGHardEdge
        {
            get { return m_CGHardEdge; }
            set
            {
                if (m_CGHardEdge != value)
                    m_CGHardEdge = value;
            }
        }
        /// <summary>
        /// Maximum vertex distance when using optimization (0=infinite)
        /// </summary>
        public float CGMaxStepDistance
        {
            get
            {
                return m_CGMaxStepDistance;
            }
            set
            {
                if (m_CGMaxStepDistance != Mathf.Max(0, value))
                    m_CGMaxStepDistance = Mathf.Max(0, value);
            }
        }

        #endregion
        */
        #region --- Connections ---
        /// <summary>
        /// Whether this Control Point is allowed to have a FollowUp (i.e. it's the first or last visible Control Point)
        /// </summary>
        public bool CanHaveFollowUp
        {
            get 
            {
                return IsFirstVisibleControlPoint || IsLastVisibleControlPoint;
            }
        }
        /// <summary>
        /// Gets the connected Control Point that is set as "Head To"
        /// </summary>
        public CurvySplineSegment FollowUp
        {
            get
            {
                return m_FollowUp;
            }
            private set
            {
                if (m_FollowUp != value)
                {
                    m_FollowUp = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Gets or sets the heading toward the "Head To" segment
        /// </summary>
        public ConnectionHeadingEnum FollowUpHeading
        {
            get { return m_FollowUpHeading; }
            set
            {
                if (m_FollowUpHeading != value)
                {
                    m_FollowUpHeading = value;
                    SetDirty();
                }
            }
        }

        public bool ConnectionSyncPosition
        {
            get { return m_ConnectionSyncPosition; }
            set
            {
                if (m_ConnectionSyncPosition != value)
                    m_ConnectionSyncPosition = value;
            }
        }

        public bool ConnectionSyncRotation
        {
            get { return m_ConnectionSyncRotation; }
            set
            {
                if (m_ConnectionSyncRotation != value)
                    m_ConnectionSyncRotation = value;
            }
        }

        /// <summary>
        /// Gets the connection handler this Control Point is using (if any)
        /// </summary>
        public CurvyConnection Connection
        {
            get { return m_Connection; }
            internal set 
            {
                if (m_Connection !=value)
                    m_Connection = value;
                if (m_Connection == null)
                    m_FollowUp = null;
            }
        }

        public List<CurvySplineSegment> ConnectedControlPoints
        {
            get
            {
                return (Connection) ? Connection.OtherControlPoints(this) : new List<CurvySplineSegment>();
            }
        }

        #endregion

        /// <summary>
        /// Gets the TTransform (threadsafe)
        /// </summary>
        
        public TTransform TTransform
        {
            get
            {
                return mTTransform;
            }
        }

        /// <summary>
        /// Gets or sets the local position (threadsafe)
        /// </summary>
        public Vector3 localPosition
        {
            get
            {
                return TTransform.localPosition;
            }
            set
            {
                if (TTransform.localPosition!=value || TTransform.localPosition==Vector3.zero)
                {
                    transform.localPosition = value;
                    RefreshTransform();
                }
            }
        }

        /// <summary>
        /// Gets or sets the position (threadsafe)
        /// </summary>
        public Vector3 position
        {
            get
            {
                return TTransform.position;
            }
            set
            {
                if (TTransform.position != value || TTransform.position == Vector3.zero)
                {
                    transform.position = value;
                    TTransform.FromTransform(transform);
                    Spline.SetDirty(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the local rotation (threadsafe)
        /// </summary>
        public Quaternion localRotation
        {
            get { return TTransform.localRotation; }
            set
            {
                if (TTransform.localRotation != value || TTransform.localRotation==Quaternion.identity)
                {
                    transform.localRotation = value;
                    RefreshTransform();
                }
            }
        }

        /// <summary>
        /// Gets or sets the rotation (threadsafe)
        /// </summary>
        public Quaternion rotation
        {
            get { return TTransform.rotation; }
            set
            {
                if (TTransform.rotation != value || TTransform.rotation==Quaternion.identity)
                {
                    transform.rotation = value;
                    SetDirty(false, true);
                }
            }
        }
        
        /// <summary>
        /// Gets the number of individual cache points of this segment
        /// </summary>
        public int CacheSize { get; private set; }

        /// <summary>
        /// Gets this segment's bounds in world space
        /// </summary>
        public Bounds Bounds
        {
            get 
            {
                if (!mBounds.HasValue)
                    mBounds = getBounds();
                return mBounds.Value;
            }
        }

        /// <summary>
        /// Gets the length of this spline segment
        /// </summary>
        public float Length { get; private set; }

        /// <summary>
        /// Gets the distance from spline start to the first control point (localF=0) 
        /// </summary>
        public float Distance { get; internal set; }

        /// <summary>
        /// Gets the TF of this Control Point
        /// </summary>
        /// <remarks>This is a shortcut to LocalFToTF(0)</remarks>
        public float TF
        {
            get { return LocalFToTF(0); }
        }

        /// <summary>
        /// Gets whether this Control Point reflects a valid segment or just an Control Point
        /// </summary>
        public bool IsValidSegment
        {
            get
            {
                switch (Spline.Interpolation)
                {
                    case CurvyInterpolation.Bezier:
                        return (NextControlPoint);
                    case CurvyInterpolation.Linear:
                        return (NextControlPoint);
                    case CurvyInterpolation.CatmullRom:
                    case CurvyInterpolation.TCB:
                        var ncp = GetNextControlPoint(false, false);
                        return (TTransform &&
                                GetPreviousTTransform(true) &&
                                ncp &&
                                ncp.GetNextTTransform(true)
                                );
                }
                return false;
            }
        }

        /// <summary>
        /// Gets whether this segment is the first IGNORING closed splines
        /// </summary>
        public bool IsFirstSegment
        {
            get
            {
                return (!PreviousSegment || (Spline.Closed && PreviousSegment == Spline[Spline.Count - 1]));
            }
        }

        /// <summary>
        /// Gets whether this segment is the last IGNORING closed splines
        /// </summary>
        public bool IsLastSegment
        {
            get
            {
                return (!NextSegment || (Spline.Closed && NextSegment == Spline[0]));
            }
        }
       
        /// <summary>
        /// Gets whether this Control Point is the first IGNORING closed splines
        /// </summary>
        public bool IsFirstControlPoint
        {
            get
            {
                return (ControlPointIndex == 0);
            }
        }

        /// <summary>
        /// Gets whether this Control Point is the first visible (i.e. start of the first segment) Control Point
        /// </summary>
        public bool IsFirstVisibleControlPoint
        {
            get
            {
                return (SegmentIndex == 0);
            }
        }

        /// <summary>
        /// Gets whether this Control Point is the last visible Control Point (i.e. the end of the last segment)
        /// </summary>
        public bool IsLastVisibleControlPoint
        {
            get
            {
                return (this == Spline.LastVisibleControlPoint);
            }
        }

        /// <summary>
        /// Gets whether this Control point is a visible Control Point (i.e. either a segment or the last segment's end CP)
        /// </summary>
        public bool IsVisibleControlPoint
        {
            get
            {
                return SegmentIndex > -1 || IsLastVisibleControlPoint;
            }
        }

        /// <summary>
        /// Gets whether this Control Point is the last IGNORING closed splines
        /// </summary>
        public bool IsLastControlPoint
        {
            get
            {
                return (ControlPointIndex == Spline.ControlPointCount - 1);
            }
        }

        public List<Component> MetaData
        {
            get
            {
                if (mMetaData == null)
                    ReloadMetaData();
                return mMetaData;
            }
        }

        /// <summary>
        /// Gets the next Control Point. FollowUps are ignored
        /// </summary>
        /// <returns>a CurvySplineSegment or Null</returns>
        public CurvySplineSegment NextControlPoint
        {
            get
            {
                return GetNextControlPoint(false, false);
            }
        }
   
        /// <summary>
        /// Gets the previous Control Point, FollowUps are ignored
        /// </summary>
        /// <returns>a CurvySplineSegment or Null</returns>
        public CurvySplineSegment PreviousControlPoint
        {
            get
            {
                return GetPreviousControlPoint(false, false);
            }
        }

        /// <summary>
        /// Gets the previous Control Point or the follow up CP (if any)
        /// </summary>
        /// <returns></returns>
        public CurvySplineSegment PreviousControlPointWithFollowUp
        {
            get
            {
                var cp = PreviousControlPoint;
                if (!cp && IsFirstControlPoint)
                    cp = FollowUp;
                return cp;
            }
        }

        /// <summary>
        /// Gets the next Control Point or the follow up CP (if any)
        /// </summary>
        /// <returns></returns>
        public CurvySplineSegment NextControlPointWithFollowUp
        {
            get
            {
                var cp = NextControlPoint;
                if (!cp && IsLastControlPoint)
                    cp = FollowUp;
                return cp;
            }
        }

        /// <summary>
        /// Gets the next Transform, ignoring FollowUp.
        /// </summary>
        /// <returns>a Transform or Null</returns>
        public Transform NextTransform
        {
            get
            {
                return GetNextTransform(false);
            }
        }

        /// <summary>
        /// Gets the next Transform (threadsafe), ignoring FollowUp.
        /// </summary>
        public TTransform NextTTransform
        {
            get
            {
                return GetNextTTransform(false);
            }
        }

        /// <summary>
        /// Gets the previous Transform, ignoring FollowUp.
        /// </summary>
        /// <returns>a Transform or Null</returns>
        public Transform PreviousTransform
        {
            get
            {
                return GetPreviousTransform(false);
            }
        }

        /// <summary>
        /// Gets the previous Transform (threadsafe), ignoring FollowUp.
        /// </summary>
        public TTransform PreviousTTransform
        {
            get
            {
                return GetPreviousTTransform(false);
            }
        }

        /// <summary>
        /// Gets the next segment, ignoring FollowUp.
        /// </summary>
        /// <returns>a segment or null</returns>
        public CurvySplineSegment NextSegment
        {
            get 
            {
                return GetNextControlPoint(true, false);
            }
        }

        /// <summary>
        /// Gets the previous segment, ignoring FollowUp.
        /// </summary>
        /// <returns>a segment or null</returns>
        public CurvySplineSegment PreviousSegment
        {
            get
            {
                return GetPreviousControlPoint(true, false);
            }
        }

        /// <summary>
        /// Gets the Index of this segment
        /// </summary>
        /// <returns>an index to be used with CurvySpline.Segments or -1 if this Control Point doesn't form an segment</returns>
        public int SegmentIndex
        {
            get
            {
                if (mSegmentIndex == -1)
                    mSegmentIndex = Spline.Segments.IndexOf(this);
                return mSegmentIndex;
            }
        }

        /// <summary>
        /// Gets the Index of this Control Point
        /// </summary>
        /// <returns>an index to be used with CurvySpline.ControlPoints</returns>
        public int ControlPointIndex
        {
            get
            {
                if (mControlPointIndex == -1)
                    mControlPointIndex = Spline.ControlPoints.IndexOf(this);
                return mControlPointIndex;
            }
            
            internal set
            {
                mControlPointIndex = value;
            }
        }

        /// <summary>
        /// Gets the parent spline
        /// </summary>
        public CurvySpline Spline
        {
            get
            {
                if (mSpline == null && transform.parent != null)
                    mSpline = transform.parent.GetComponent<CurvySpline>();

                return mSpline;
            }
        }

        #endregion

        #region ### Private Fields ###

        TTransform mTTransform;
        CurvySpline mSpline;
        float mStepSize;
        int mControlPointIndex = -1;
        int mSegmentIndex = -1;
        Bounds? mBounds;
        int mCacheLastDistanceToLocalFIndex=0;

        List<Component> mMetaData = null;

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */
        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (System.Array.IndexOf(UnityEditor.Selection.gameObjects, gameObject) >= 0)
                return;
#endif
            if (Spline && Spline.ShowGizmos)
                doGizmos(false);
        }

        void OnDrawGizmosSelected()
        {
            if (Spline)
                doGizmos(true);
        }

        void Awake()
        {
            mTTransform = new TTransform(transform);
        }

        void OnEnable()
        {
#if UNITY_EDITOR
            mTTransform = new TTransform(transform);
#endif
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
                Disconnect();
                if (Spline != null)
                {
                    Spline.ControlPoints.Remove(this);
                    Spline.setLengthINTERNAL(Spline.Length - Length);
                }
            }
#if UNITY_EDITOR
            if (!Application.isPlaying && 
                (CurvySpline._newSelectionInstanceIDINTERNAL==0 || CurvySpline._newSelectionInstanceIDINTERNAL==GetInstanceID())
                ){
                if (PreviousControlPoint)
                    CurvySpline._newSelectionInstanceIDINTERNAL=PreviousControlPoint.GetInstanceID();
                else if (NextControlPoint)
                    CurvySpline._newSelectionInstanceIDINTERNAL=NextControlPoint.GetInstanceID();
                else if (Spline)
                    CurvySpline._newSelectionInstanceIDINTERNAL=Spline.GetInstanceID();
            }
#endif
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (mSpline == null)
            {
                var spl = Spline;
                if (spl && !spl.ControlPoints.Contains(this))
                    spl.ControlPoints.Add(this);
            }
            FollowUp = m_FollowUp;
            AutoHandleDistance = m_AutoHandleDistance;
            AutoHandles = m_AutoHandles;
        }

#endif

        public void Reset()
        {
            //mSpline = null;
            mSegmentIndex = -1;
            mControlPointIndex = -1;
            OrientationAnchor = false;
            Swirl = CurvyOrientationSwirl.None;
            SwirlTurns = 0;
            // Bezier
            m_AutoHandles = true;
            m_AutoHandleDistance = 0.39f;
            HandleIn = new Vector3(-1, 0, 0);
            HandleOut = new Vector3(1, 0, 0);
            // TCB
            SynchronizeTCB = true;
            OverrideGlobalTension = false;
            OverrideGlobalContinuity = false;
            OverrideGlobalBias = false;
            StartTension = 0;
            EndTension = 0;
            StartContinuity = 0;
            EndContinuity = 0;
            StartBias = 0;
            EndBias = 0;
            //Connections
            FollowUp = null;
            FollowUpHeading = ConnectionHeadingEnum.Auto;
            ConnectionSyncPosition = false;
            ConnectionSyncRotation = false;
            if (Connection)
                Disconnect();
            
            SetDirty();
        }
        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        /// <summary>
        /// Sets Bezier HandleIn
        /// </summary>
        /// <param name="position">HandleIn position</param>
        /// <param name="space">Local or world space</param>
        /// <param name="mode">Handle synchronization mode</param>
        public void SetBezierHandleIn(Vector3 position, Space space = Space.Self, CurvyBezierModeEnum mode = CurvyBezierModeEnum.None)
        {
            if (space == Space.Self)
                HandleIn = position;
            else
                HandleInPosition = position;

            bool dir = (mode & CurvyBezierModeEnum.Direction)==CurvyBezierModeEnum.Direction;
            bool len = (mode & CurvyBezierModeEnum.Length)==CurvyBezierModeEnum.Length;
            bool con = (mode & CurvyBezierModeEnum.Connections)==CurvyBezierModeEnum.Connections;

            if (dir)
                HandleOut = HandleOut.magnitude * (HandleIn.normalized * -1);
            if (len)
                HandleOut = HandleIn.magnitude * ((HandleOut == Vector3.zero) ? HandleIn.normalized * -1 : HandleOut.normalized);
            if (con)
            {
                var cps = ConnectedControlPoints;
                foreach (var conCP in cps)
                {
                    if ((dir || len) && conCP.HandleIn.magnitude == 0)
                        conCP.HandleIn = HandleIn;
                    bool flipHandles = (Vector3.Angle(HandleIn, conCP.HandleIn) > 90);
                    if (flipHandles)
                    {
                        if (dir)
                            conCP.SetBezierHandleOut(conCP.HandleIn.magnitude * HandleIn.normalized, Space.Self, CurvyBezierModeEnum.Direction);
                        if (len)
                            conCP.SetBezierHandleOut(conCP.HandleIn.normalized * HandleIn.magnitude, Space.Self, CurvyBezierModeEnum.Length);
                    }
                    else
                    {
                        if (dir)
                            conCP.SetBezierHandleIn(conCP.HandleIn.magnitude * HandleIn.normalized, Space.Self, CurvyBezierModeEnum.Direction);
                        if (len)
                            conCP.SetBezierHandleIn(conCP.HandleIn.normalized * HandleIn.magnitude, Space.Self, CurvyBezierModeEnum.Length);
                    }
                }
            }
            var pcp = PreviousControlPoint;
            if (pcp)
                pcp.SetDirty();
        }

        /// <summary>
        /// Sets Bezier HandleOut
        /// </summary>
        /// <param name="position">HandleOut position</param>
        /// <param name="space">Local or world space</param>
        /// <param name="mode">Handle synchronization mode</param>
        public void SetBezierHandleOut(Vector3 position, Space space = Space.Self, CurvyBezierModeEnum mode = CurvyBezierModeEnum.None)
        {
            if (space == Space.Self)
                HandleOut = position;
            else
                HandleOutPosition = position;

            bool dir = (mode & CurvyBezierModeEnum.Direction) == CurvyBezierModeEnum.Direction;
            bool len = (mode & CurvyBezierModeEnum.Length) == CurvyBezierModeEnum.Length;
            bool con = (mode & CurvyBezierModeEnum.Connections) == CurvyBezierModeEnum.Connections;

            if (dir)
                HandleIn = HandleIn.magnitude * (HandleOut.normalized * -1);
            if (len)
                HandleIn = HandleOut.magnitude * ((HandleIn == Vector3.zero) ? HandleOut.normalized * -1 : HandleIn.normalized);
            if (con)
            {
                var cps = ConnectedControlPoints;
                
                foreach (var conCP in cps)
                {
                    if ((dir || len) && conCP.HandleOut.magnitude == 0)
                        conCP.HandleOut = HandleOut;
                    bool flipHandles = (Vector3.Angle(HandleOut, conCP.HandleOut) > 90);
                    if (flipHandles)
                    {
                        if (dir)
                            conCP.SetBezierHandleIn(conCP.HandleOut.magnitude * HandleOut.normalized, Space.Self, CurvyBezierModeEnum.Direction);
                        if (len)
                            conCP.SetBezierHandleIn(conCP.HandleOut.normalized * HandleOut.magnitude, Space.Self, CurvyBezierModeEnum.Length);
                    }
                    else
                    {
                        if (dir)
                            conCP.SetBezierHandleOut(conCP.HandleOut.magnitude * HandleOut.normalized, Space.Self, CurvyBezierModeEnum.Direction);
                        if (len)
                            conCP.SetBezierHandleOut(conCP.HandleOut.normalized * HandleOut.magnitude, Space.Self, CurvyBezierModeEnum.Length);
                    }
                }
            }
            SetDirty();
        }

        /// <summary>
        /// Automatically place Bezier handles relative to neighbour Control Points
        /// </summary>
        /// <param name="distanceFrag">how much % distance between neighbouring CPs are applied to the handle length?</param>
        /// <param name="setIn">Set HandleIn?</param>
        /// <param name="setOut">Set HandleOut?</param>
        public void SetBezierHandles(float distanceFrag=-1, bool setIn=true, bool setOut=true)
        {
            Vector3 pIn = Vector3.zero;
            Vector3 pOut = Vector3.zero;
            if (distanceFrag == -1)
                distanceFrag = AutoHandleDistance;
            if (distanceFrag > 0)
            {
                var ttn = NextTTransform;
                var ttp = PreviousTTransform;
                
                if (ttp || ttn)
                {
                    Vector3 c = TTransform.localPosition;
                    Vector3 p = (ttp) ? ttp.localPosition - c : c - ttn.localPosition;
                    Vector3 n = (ttn) ? ttn.localPosition - c : c - ttp.localPosition;
                    SetBezierHandles(distanceFrag, p, n, setIn, setOut);
                    return;
                }
            }
            // Fallback to zero
            if (setIn)
            {
                HandleIn = pIn;
                var pcp = PreviousControlPoint;
                if (pcp)
                    pcp.SetDirty();
            }
            if (setOut)
            {
                HandleOut = pOut;
                SetDirty();
            }

        }

        /// <summary>
        /// Automatically place Bezier handles
        /// </summary>
        /// <param name="distanceFrag">how much % distance between neighbouring CPs are applied to the handle length?</param>
        /// <param name="p">Position the In-Handle relates to</param>
        /// <param name="n">Position the Out-Handle relates to</param>
        /// <param name="setIn">Set HandleIn?</param>
        /// <param name="setOut">Set HandleOut?</param>
        public void SetBezierHandles(float distanceFrag, Vector3 p, Vector3 n, bool setIn = true, bool setOut = true)
        {
            float pLen = p.magnitude;
            float nLen = n.magnitude;
            Vector3 pIn=Vector3.zero;
            Vector3 pOut=Vector3.zero;

            if (pLen != 0 || nLen != 0)
            {
                Vector3 dir = ((pLen / nLen) * n - p).normalized;
                pIn = -dir * (pLen * distanceFrag);
                pOut = dir * (nLen * distanceFrag);
            }
            if (setIn)
            {
                HandleIn = pIn;
                var pcp = PreviousControlPoint;
                if (pcp)
                    pcp.SetDirty();
            }
            if (setOut)
            {
                HandleOut = pOut;
                SetDirty();
            }
        }

        /// <summary>
        /// Updates TTransform to match the current transform and sets the segment dirty if neccessary
        /// </summary>
        /// <param name="refreshConnection">Whether connected Control Points should refresh as well</param>
        /// <param name="forceCurve">Whether affected Control Points should be marked for curve refresh</param>
        /// <param name="forceOrientation">Whether affected Control Points should be marked for orientation refresh</param>
        public void RefreshTransform(bool refreshConnection=true, bool forceCurve=false, bool forceOrientation=false)
        {
            bool dirtyCurve = TTransform.localPosition != transform.localPosition || forceCurve;
            bool dirtyOrientation = (Spline.Orientation==CurvyOrientation.Static || (OrientationAnchor || forceOrientation)) && (TTransform.localRotation != transform.localRotation || TTransform.rotation != transform.rotation);
            TTransform.FromTransform(transform);
            if (refreshConnection && Connection != null && (dirtyCurve || dirtyOrientation))
                Connection.SynchronizeINTERNAL(transform);

            SetDirty(dirtyCurve,dirtyOrientation);
        }

        /// <summary>
        /// Reloads Metadata components
        /// </summary>
        public void ReloadMetaData()
        {
            mMetaData = new List<Component>();
            GetComponents(typeof(ICurvyMetadata), mMetaData);
            
        }
        
        /// <summary>
        /// Apply Control Point naming convention to this GameObject
        /// </summary>
        public void ApplyName()
        {
            name = "CP" + ControlPointIndex.ToString("D4");
        }

        /// <summary>
        /// Connects this Control Point to another Control Point
        /// </summary>
        /// <param name="targetCP">the Control Point to connect to, or null to release a connection</param>
        /// <param name="syncPosition">Synchronize Position?</param>
        /// <param name="syncRotation">Synchronize Rotation?</param>
        /// <param name="heading">Desired heading mode</param>
        public bool ConnectTo(CurvySplineSegment targetCP, bool syncPosition=true, bool syncRotation=true, ConnectionHeadingEnum heading=ConnectionHeadingEnum.Auto) 
        {
            CurvyConnection con;
            if (!targetCP) // disconnect
            {
                Disconnect();
                return false;
            }
            else if (Connection && targetCP.Connection) // both CPs already connected
                return false;

            ConnectionSyncPosition = syncPosition;
            ConnectionSyncRotation = syncRotation;
            FollowUpHeading = heading;

            if (ConnectionSyncPosition)
                position = targetCP.position;

            if (Connection) {
                con=Connection;
                con.AddControlPoints(targetCP);
                
            }
            else if (targetCP.Connection)
            {
                con = targetCP.Connection;
                con.AddControlPoints(this);
            }
            else
            {
                targetCP.ConnectionSyncPosition = syncPosition;
                targetCP.ConnectionSyncRotation = syncRotation;
                con = CurvyConnection.Create(this, targetCP);
            }
            
            SetDirty();
            if (targetCP)
                targetCP.SetDirty();

            return true;
        }
        
        /// <summary>
        /// Sets Follow Up of this Control Point
        /// </summary>
        /// <param name="target">the Control Point to follow to</param>
        /// <param name="heading">the Heading on the target's spline</param>
        public void SetFollowUp(CurvySplineSegment target, ConnectionHeadingEnum heading = ConnectionHeadingEnum.Auto)
        {
            if (CanHaveFollowUp || target == null)
            {
                FollowUp = target;
                FollowUpHeading = heading;
                SetDirty();
            }
        }
       
        /// <summary>
        /// Removes Connection and Follow Up (if any)
        /// </summary>
        public void Disconnect()
        {
            if (Connection)
                Connection.RemoveControlPoint(this);

            Connection = null;
            FollowUp = null;
            FollowUpHeading = ConnectionHeadingEnum.Auto;
            ConnectionSyncPosition = false;
            ConnectionSyncRotation = false;
            SetDirty();
        }

        /// <summary>
        /// Deletes this Control Point
        /// </summary>
        public void Delete()
        {
            Spline.Delete(this);
        }

        /// <summary>
        /// Gets the next ControlPoint or segment, taking care of AutoEndCP,Closed and FollowUp
        /// </summary>
        /// <param name="segmentsOnly">Whether only segments should be considered</param>
        /// <param name="useFollowUp">Whether FollowUp should be considered</param>
        public CurvySplineSegment GetNextControlPoint(bool segmentsOnly, bool useFollowUp)
        {
            var spl = Spline;
            if (!spl || spl.ControlPoints.Count == 0) return null;

            int i = ControlPointIndex + 1;
            if (i >= spl.ControlPointCount)
            {
                if (spl.Closed)
                    return spl.ControlPoints[0];
                else if (useFollowUp && FollowUp != null)
                    return getFollowUpCP();//FollowUp.getNextControlPointInternal(segmentsOnly, false);
                else
                    return null;
            }
            else if (i < 0)
                return null;
            else
                return (segmentsOnly && spl.ControlPoints[i].SegmentIndex == -1) ? null : spl.ControlPoints[i];
        }

        /// <summary>
        /// Gets the previous ControlPoint or segment, taking care of AutoEndCP,Closed and (optionally) FollowUp
        /// </summary>
        /// <param name="segmentsOnly">Whether only segments should be considered</param>
        /// <param name="useFollowUp">Whether FollowUp should be considered</param>
        public CurvySplineSegment GetPreviousControlPoint(bool segmentsOnly, bool useFollowUp)
        {
            var spl = Spline;
            if (!spl || spl.ControlPoints.Count == 0) return null;
           
            int i = ControlPointIndex - 1;
            if (i < 0)
            {
                if (spl.Closed)
                    return spl.ControlPoints[spl.ControlPointCount - 1];
                else if (useFollowUp && FollowUp != null)
                    return getFollowUpCP();//FollowUp.getPreviousControlPointInternal(segmentsOnly,false);
                else
                    return null;
            }
            else if (i >= spl.ControlPointCount)
                return null;
            else
                return (segmentsOnly && spl.ControlPoints[i].SegmentIndex == -1) ? null : spl.ControlPoints[i];
        }

        /// <summary>
        /// Gets the next Transform to be used by Interpolation, respecting Spline.AutoEndTangents
        /// </summary>
        /// <param name="useFollowUp">Whether FollowUp should be considered</param>
        public Transform GetNextTransform(bool useFollowUp)
        {
            var cp = GetNextControlPoint(false, useFollowUp);
            if (cp)
                return cp.transform;
            else
                return (Spline.AutoEndTangents) ? transform : null;
        }

        /// <summary>
        /// Gets the next Transform (threadsafe) to be used by Interpolation, respecting Spline.AutoEndTangents
        /// </summary>
        /// <param name="useFollowUp">Whether FollowUp should be considered</param>
        public TTransform GetNextTTransform(bool useFollowUp)
        {
            var cp = GetNextControlPoint(false, useFollowUp);
            if (cp)
                return cp.TTransform;
            else
                return (Spline.AutoEndTangents) ? TTransform : null;
        }

        /// <summary>
        /// Gets the previous Transform to be used by Interpolation, respecting Spline.AutoEndTangents
        /// </summary>
        /// <param name="useFollowUp">Whether FollowUp should be considered</param>
        public Transform GetPreviousTransform(bool useFollowUp)
        {
            var cp = GetPreviousControlPoint(false, useFollowUp);
            if (cp)
                return cp.transform;
            else
                return (Spline.AutoEndTangents) ? transform : null;
        }

        /// <summary>
        /// Gets the previous Transform (threadsafe) to be used by Interpolation, respecting Spline.AutoEndTangents
        /// </summary>
        /// <param name="useFollowUp">Whether FollowUp should be considered</param>
        public TTransform GetPreviousTTransform(bool useFollowUp)
        {
            var cp = GetPreviousControlPoint(false, useFollowUp);
            if (cp)
                return cp.TTransform;
            else
                return (Spline.AutoEndTangents) ? TTransform : null;
        }

        /// <summary>
        /// Defines this Control Point to be the first Control Point of the spline
        /// </summary>
        public void SetAsFirstCP()
        {
            if (ControlPointIndex <= 0)
                return;

            CurvySplineSegment[] toMove = new CurvySplineSegment[ControlPointIndex];
            for (int i = 0; i < ControlPointIndex; i++)
                toMove[i] = Spline.ControlPoints[i];

            foreach (CurvySplineSegment seg in toMove)
            {
                Spline.ControlPoints.Remove(seg);
                Spline.ControlPoints.Add(seg);
            }
            Spline.SetDirtyAll();
            Spline.SyncHierarchyFromSpline();
            Spline.Refresh();
        }

        

        /// <summary>
        /// Gets the previous Control Point's position. If this is a FollowUp, it returns the position of the 2nd previous Control Point, respecting Follow Up heading
        /// </summary>
        public Vector3 GetPreviousPosition()
        {
            var cp = PreviousControlPoint;
            if (cp)
                return cp.TTransform.localPosition;
            else if (FollowUp != null)
            {
                var align = FollowUpHeading;
                if (align == ConnectionHeadingEnum.Auto)
                {
                    align = (FollowUp.PreviousControlPoint) ? (FollowUp.NextControlPoint) ? ConnectionHeadingEnum.Sharp : ConnectionHeadingEnum.Minus : ConnectionHeadingEnum.Plus;
                }

                if (align == ConnectionHeadingEnum.Minus)
                {
                    var t = FollowUp.PreviousTransform;
                    if (t)
                        return t.localPosition;
                }
                else if (align == ConnectionHeadingEnum.Plus)
                {
                    var t = FollowUp.NextTransform;
                    if (t)
                        return t.localPosition;
                }
            }
            // Fallback
            if (PreviousTTransform)
                return PreviousTTransform.localPosition;
            else
                return TTransform.localPosition;
        }

        /// <summary>
        /// Gets the next Control Point's position. If this is a FollowUp, it returns the position of the 2nd next Control Point, respecting Follow Up heading
        /// </summary>
        /// <returns></returns>
        public Vector3 GetNextPosition()
        {
            var cp = NextControlPoint;
            if (cp)
                return cp.TTransform.localPosition;
            else if (FollowUp != null)
            {
                var align = FollowUpHeading;
                if (align == ConnectionHeadingEnum.Auto)
                {
                    align = (FollowUp.PreviousControlPoint) ? (FollowUp.NextControlPoint) ? ConnectionHeadingEnum.Sharp : ConnectionHeadingEnum.Minus : ConnectionHeadingEnum.Plus;
                }
                if (align == ConnectionHeadingEnum.Minus)
                {
                    var t = FollowUp.PreviousTTransform;
                    if (t)
                        return t.localPosition;
                }
                else if (align == ConnectionHeadingEnum.Plus)
                {
                    var t = FollowUp.NextTTransform;
                    if (t)
                        return t.localPosition;
                }
            }

            // Fallback
            if (NextTTransform)
                return NextTTransform.localPosition;
            else
                return TTransform.localPosition;
        }

        /// <summary>
        /// Interpolates position for a local F
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <returns>the interpolated position</returns>
        public Vector3 Interpolate(float localF) { return Interpolate(localF, Spline.Interpolation); }

        /// <summary>
        /// Interpolates position for a local F
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <param name="interpolation">the interpolation to use</param>
        /// <returns>the interpolated position</returns>
        public Vector3 Interpolate(float localF, CurvyInterpolation interpolation)
        {
            switch (interpolation)
            {
                case CurvyInterpolation.Bezier:
                    return interpolateBezier(localF);
                case CurvyInterpolation.CatmullRom:
                    return interpolateCatmull(localF);
                case CurvyInterpolation.TCB:
                    return interpolateTCB(localF);
                default: // LINEAR
                    return interpolateLinear(localF);
            }
        }

        /// <summary>
        /// Interpolates position for a local F using a linear approximation
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <returns>the interpolated position</returns>
        public Vector3 InterpolateFast(float localF)
        {
            float frag;
            int idx = getApproximationIndexINTERNAL(localF, out frag);
            int idx2 = Mathf.Min(Approximation.Length - 1, idx + 1);
            return (Vector3.Lerp(Approximation[idx], Approximation[idx2], frag));
        }

        /// <summary>
        /// Gets Metadata
        /// </summary>
        /// <param name="type">type implementing ICurvyMetadata</param>
        /// <param name="autoCreate">whether to create the Metadata component if it's not present</param>
        /// <returns>the Metadata component or null</returns>
        public Component GetMetaData(Type type, bool autoCreate=false)
        {
            var md = MetaData;
#if NETFX_CORE
            if (md != null && type.GetTypeInfo().IsSubclassOf(typeof(Component)) && typeof(ICurvyMetadata).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()))
#else
            if (md != null && type.IsSubclassOf(typeof(Component)) && typeof(ICurvyMetadata).IsAssignableFrom(type))
#endif
            {
                for (int i = 0; i < md.Count; i++)
                    if (md[i]!=null && md[i].GetType() == type)
                        return md[i];
            }
            Component newData = null;
            if (autoCreate)
            {
                newData = gameObject.AddComponent(type);
                MetaData.Add(newData);
            }
            return newData;
        }

        /// <summary>
        /// Gets Metadata of this ControlPoint
        /// </summary>
        /// <typeparam name="T">Metadata type</typeparam>
        /// <param name="autoCreate">whether to create the Metadata component if it's not present</param>
        /// <returns>the Metadata component or null</returns>
        public T GetMetadata<T>(bool autoCreate=false) where T:Component,ICurvyMetadata
        {
            return (T)GetMetaData(typeof(T),autoCreate);
        }

        /// <summary>
        /// Gets interpolated MetaData of this Segment
        /// </summary>
        /// <typeparam name="T">Metadata type implementing ICurvyInterpolatableMetadata</typeparam>
        /// <typeparam name="U">Metadata return value type</typeparam>
        /// <param name="f">a local F in the range 0..1</param>
        /// <returns>interpolated value</returns>
        public U InterpolateMetadata<T,U>(float f) where T : Component, ICurvyInterpolatableMetadata<U>
        {
            var ma = GetMetadata<T>();
            if (ma != null)
            {
                var ncp = GetNextControlPoint(false, true);
                ICurvyInterpolatableMetadata<U> md = null;
                if (ncp)
                    md=ncp.GetMetadata<T>();
                return ma.Interpolate(md, f);
            }
            return default(U);
        }

        /// <summary>
        /// Gets interpolated MetaData of this Segment
        /// </summary>
        /// <param name="type">Metadata type implementing ICurvyInterpolatableMetadata</param>
        /// <param name="f">a local F in the range 0..1</param>
        /// <returns>interpolated value</returns>
        public object InterpolateMetadata(Type type, float f)
        {
            var ma = GetMetaData(type) as ICurvyInterpolatableMetadata;
            if (ma != null)
            {
                var ncp = GetNextControlPoint(false, true);
                ICurvyInterpolatableMetadata md = null;
                if (ncp)
                {
                    md = ncp.GetMetaData(type) as ICurvyInterpolatableMetadata;
                    if (md!=null)
                        return ma.InterpolateObject(md, f);
                }
            }
            return null;
        }

        /// <summary>
        /// Removes all Metadata components of this Control Point
        /// </summary>
        public void DeleteMetadata()
        {
            var md=MetaData;
            for (int i=md.Count-1;i>=0;i--)
                md[i].Destroy();
        }

        /// <summary>
        /// Gets the interpolated Scale
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <returns>the interpolated value</returns>
        public Vector3 InterpolateScale(float localF)
        {
            Transform T = NextTransform;
            return (T) ? Vector3.Lerp(transform.lossyScale, T.lossyScale, localF) : transform.lossyScale;
        }

        /// <summary>
        /// Gets the tangent for a local F
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <remarks>SmoothTangent won't get respected!</remarks>
        /// <returns>the tangent/direction</returns>
        public Vector3 GetTangent(float localF)
        {
            localF = Mathf.Clamp01(localF);
            Vector3 p = Interpolate(localF);
            return GetTangent(localF, ref p);
        }

        /// <summary>
        /// Gets the normalized tangent for a local F with the interpolated position for f known
        /// </summary>
        /// <remarks>This saves one interpolation if you already know the position. SmoothTangent won't get respected!</remarks>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <param name="position">the result of Interpolate(localF)</param>
        /// <returns></returns>
        public Vector3 GetTangent(float localF, ref Vector3 position)
        {
            Vector3 p2;
            int leave = 2;
            CurvySplineSegment nSeg;
            do
            {
                float f2 = localF + 0.01f;
                if (f2 > 1)
                {
                    nSeg = NextSegment;
                    if (nSeg)
                        p2 = nSeg.Interpolate(f2 - 1);//return (NextSegment.Interpolate(f2 - 1) - position).normalized;
                    else
                    {
                        f2 = localF - 0.01f;
                        return (position - Interpolate(f2)).normalized;
                    }
                }
                else
                    p2 = Interpolate(f2); // return (Interpolate(f2) - position).normalized;

                localF += 0.01f;
            } while (p2 == position && --leave > 0);
            //Debug.Log(p2 + "-" + position + "=" + (p2 - position));
            return (p2 - position).normalized;
        }

        /// <summary>
        /// Gets the cached tangent for a certain F
        /// </summary>
        /// <remarks>SmoothTangent option will be respected</remarks>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <returns></returns>
        public Vector3 GetTangentFast(float localF)
        {
            float frag;
            int idx = getApproximationIndexINTERNAL(localF, out frag);
            int idx2 = Mathf.Min(ApproximationT.Length - 1, idx + 1);
            return (Vector3.Lerp(ApproximationT[idx], ApproximationT[idx2], frag));
        }

        /// <summary>
        /// Gets a rotation looking to Tangent with the head upwards along the Up-Vector
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <returns>a rotation</returns>
        public Quaternion GetOrientationFast(float localF) { return GetOrientationFast(localF, false); }

        /// <summary>
        /// Gets a rotation looking to Tangent with the head upwards along the Up-Vector
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <param name="inverse">whether the orientation should be inversed or not</param>
        /// <returns>a rotation</returns>
        public Quaternion GetOrientationFast(float localF, bool inverse)
        {
            Vector3 view = GetTangentFast(localF);

            if (view != Vector3.zero)
            {
                if (inverse)
                    view *= -1;
                return Quaternion.LookRotation(view, GetOrientationUpFast(localF));
            }
            else
                return Quaternion.identity;
        }

        /// <summary>
        /// Gets the Up-Vector for a local F based on the splines' Orientation mode
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <returns>the Up-Vector</returns>
        public Vector3 GetOrientationUpFast(float localF)
        {
            float frag;
            int idx = getApproximationIndexINTERNAL(localF, out frag);
            idx = Mathf.Max(0, Mathf.Min(idx,ApproximationUp.Length - 2));
            int idx2 = Mathf.Min(ApproximationUp.Length - 1, idx + 1);
            return (Vector3.Lerp(ApproximationUp[idx], ApproximationUp[idx2], frag));
        }

        /// <summary>
        /// Gets the f nearest to a certain point
        /// </summary>
        /// <param name="p">a point (in the space of the spline)</param>
        /// <returns>LocalF of the nearest position</returns>
        public float GetNearestPointF(Vector3 p)
        {
            int ui=CacheSize+1;
            float nearestDistSqr=float.MaxValue;
            float distSqr;
            int nearestIndex=0;
            // get the nearest index
            for (int i=0;i<ui;i++){
                distSqr=(Approximation[i] - p).sqrMagnitude;
                if ( distSqr <= nearestDistSqr)
                {
                    nearestDistSqr = distSqr;
                    nearestIndex = i;
                }
            }
            // collide p against the lines build by the index
            int leftIdx = (nearestIndex > 0) ? nearestIndex - 1 : -1;
            int rightIdx = (nearestIndex < CacheSize) ? nearestIndex + 1 : -1;
            
            float lfrag=0;
            float rfrag=0;
            float ldistSqr=float.MaxValue;
            float rdistSqr=float.MaxValue;
            if (leftIdx>-1)
                ldistSqr = DTMath.LinePointDistanceSqr(Approximation[leftIdx], Approximation[nearestIndex], p, out lfrag);
            if (rightIdx>-1)
                rdistSqr = DTMath.LinePointDistanceSqr(Approximation[nearestIndex], Approximation[rightIdx], p, out rfrag);

            
            // return the nearest collision
            if (ldistSqr < rdistSqr)
                return getApproximationLocalF(leftIdx) + lfrag * mStepSize;
            else
                return getApproximationLocalF(nearestIndex) + rfrag * mStepSize;
        }

        /// <summary>
        /// Gets the local F by a distance within this line segment
        /// </summary>
        /// <param name="localDistance">local distance in the range 0..Length</param>
        /// <returns>a local F in the range 0..1</returns>
        public float DistanceToLocalF(float localDistance)
        {
            localDistance = Mathf.Clamp(localDistance, 0, Length);
            if (ApproximationDistances.Length == 0 || localDistance == 0) return 0;
            if (Mathf.Approximately(localDistance, Length)) return 1;



            int lidx = Mathf.Min(ApproximationDistances.Length - 1, mCacheLastDistanceToLocalFIndex);
            
            if (ApproximationDistances[lidx]<localDistance)
                lidx = ApproximationDistances.Length - 1;
            while (ApproximationDistances[lidx] > localDistance)
                lidx--;

            mCacheLastDistanceToLocalFIndex = lidx+1;

            float frag = (localDistance - ApproximationDistances[lidx]) / (ApproximationDistances[lidx + 1] - ApproximationDistances[lidx]);
            float lf = getApproximationLocalF(lidx);
            float uf = getApproximationLocalF(lidx + 1);
            return lf + (uf - lf) * frag;
        }

        /// <summary>
        /// Gets the local distance for a certain localF value
        /// </summary>
        /// <param name="localF">a local F value in the range 0..1</param>
        /// <returns>a distance in the range 0..Length</returns>
        public float LocalFToDistance(float localF)
        {
            localF = Mathf.Clamp01(localF);
            float frag;
            int idx = getApproximationIndexINTERNAL(localF, out frag);
            if (ApproximationDistances.Length > 0)
            {
                if (idx < 0 || idx >= ApproximationDistances.Length - 1)
                    Debug.Log("localF/idx/Arraysize = " + localF + "/" + idx + "/" + ApproximationDistances.Length);
                float d = ApproximationDistances[idx + 1] - ApproximationDistances[idx];
                return ApproximationDistances[idx] + d * frag;
            }
            else
                return 0;
        }

        /// <summary>
        /// Gets TF for a certain local F
        /// </summary>
        /// <param name="localF">a local F in the range 0..1</param>
        /// <returns>a TF value</returns>
        public float LocalFToTF(float localF)
        {
            return Spline.SegmentToTF(this, localF);
        }

        /// <summary>
        /// Moves the Control Point along it's Up-Vector to match a desired Spline length
        /// </summary>
        /// <remarks>When the desired length can't be achieved, the Control Point will stop moving at the nearest possible point</remarks>
        /// <param name="newSplineLength">the desired length of the spline</param>
        /// <param name="stepSize">stepSize used when moving</param>
        /// <returns>false if the length can't be achieved by moving this Control Point.</returns>
        public bool SnapToFitSplineLength(float newSplineLength, float stepSize)
        {
            if (stepSize == 0 || Mathf.Approximately(newSplineLength, Spline.Length)) return true;

            float curLength = Spline.Length;
            Vector3 oldPos = transform.position;
            Vector3 upstep = transform.up * stepSize;

            // Check if increasing by Up-Vector will increase the length
            transform.position += upstep;
            SetDirty();
            Spline.Refresh();
            bool UpGrows = (Spline.Length > curLength);
            int loops = 30000;
            transform.position = oldPos;

            // Need to grow?
            if (newSplineLength > curLength)
            {
                if (!UpGrows)
                    upstep *= -1;
                while (Spline.Length < newSplineLength)
                {
                    loops--;
                    curLength = Spline.Length;
                    transform.position += upstep;
                    SetDirty();
                    Spline.Refresh();
                    if (curLength > Spline.Length)
                    {
                        return false;
                    }
                    if (loops == 0)
                    {
                        Debug.LogError("CurvySplineSegment.SnapToFitSplineLength exceeds 30000 loops, considering this a dead loop! This shouldn't happen, please send a bug report!");
                        return false;
                    }
                }
            }
            else
            { // otherwise shrink
                if (UpGrows)
                    upstep *= -1;
                while (Spline.Length > newSplineLength)
                {
                    loops--;
                    curLength = Spline.Length;
                    transform.position += upstep;
                    SetDirty();
                    Spline.Refresh();
                    if (curLength < Spline.Length)
                    {
                        return false;
                    }
                    if (loops == 0)
                    {
                        Debug.LogError("CurvySplineSegment.SnapToFitSplineLength exceeds 30000 loops, considering this a dead loop! This shouldn't happen, please send a bug report!");
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Marks this Control point for recalculation on next call to CurvySpline.Refresh()
        /// </summary>
        /// <param name="dirtyCurve">whether the curve needs refresh</param>
        /// <param name="dirtyOrientation">whether the orientation needs refresh</param>
        public void SetDirty(bool dirtyCurve = true, bool dirtyOrientation = true)
        {
            
            if (!Spline || (!dirtyCurve && !dirtyOrientation))
                return;
            //Debug.Log(ToString() + ".SetDirty:" + dirtyCurve + "/" + dirtyOrientation);

            var cpp=GetPreviousControlPoint(false,true);
            switch (Spline.Interpolation)
            {
                case CurvyInterpolation.Linear:
                    if (cpp)
                        Spline.SetDirty(cpp, dirtyCurve, dirtyOrientation);
                    Spline.SetDirty(this, dirtyCurve, dirtyOrientation);
                    break;
                case CurvyInterpolation.Bezier:
                    if (cpp)
                            Spline.SetDirty(cpp, dirtyCurve, dirtyOrientation);
                        Spline.SetDirty(this, dirtyCurve, dirtyOrientation);
                    var cpnb = GetNextControlPoint(false, true);
                        
                        if (cpnb)
                            Spline.SetDirty(cpnb, dirtyCurve, dirtyOrientation);
                    break;
                case CurvyInterpolation.CatmullRom:
                case CurvyInterpolation.TCB:
                        if (cpp) {
                            Spline.SetDirty(cpp, dirtyCurve, dirtyOrientation);
                            Spline.SetDirty(cpp.GetPreviousControlPoint(true, true), dirtyCurve, dirtyOrientation);
                        }
                        Spline.SetDirty(this, dirtyCurve, dirtyOrientation);
                        var cpn = GetNextControlPoint(false, true);
                        
                        if (cpn)
                        {
                            Spline.SetDirty(cpn, dirtyCurve, dirtyOrientation);
                            if (cpn.FollowUp)
                                Spline.SetDirty(cpn.FollowUp, dirtyCurve, dirtyOrientation);
                        }
                        
                    break;
            }
            
        }

        public override string ToString()
        {
            if (Spline!=null)
                return Spline.name + "." + name;
            else
                return base.ToString();
        }
       
        /// <summary>
        /// Applies the calculated Orientation to the transform
        /// </summary>
        public void BakeOrientation(bool setDirty=true)
        {
            if (Spline && Spline.IsInitialized)
            {
#if UNITY_EDITOR
                    Undo.RecordObject(this.transform, "Bake Orientation");
#endif
                    transform.localRotation = GetOrientationFast(0);
                    if (setDirty)
                        RefreshTransform(true, false, false);
                    else
                    { // Same as above, but without setting anything dirty
                        bool dirtyCurve = TTransform.localPosition != transform.localPosition;
                        bool dirtyOrientation = (Spline.Orientation == CurvyOrientation.Static || OrientationAnchor) && (TTransform.localRotation != transform.localRotation || TTransform.rotation != transform.rotation);
                        TTransform.FromTransform(transform);
                        if (Connection != null && (dirtyCurve || dirtyOrientation))
                            Connection.SynchronizeINTERNAL(transform);
                    }
            }
        }

        /// <summary>
        /// Splits a spline with this Control Point becoming the first Control Point of the new spline
        /// </summary>
        /// <returns>the new spline</returns>
        public CurvySpline SplitSpline()
        {
            CurvySpline spl = CurvySpline.Create(Spline);

            spl.transform.SetParent(Spline.transform.parent, true);
            spl.name = Spline.name + "_parted";

#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RegisterCreatedObjectUndo(spl.gameObject, "Split Spline");
#endif

            // Move CPs
            var affected = Spline.ControlPoints.GetRange(ControlPointIndex, Spline.ControlPointCount - ControlPointIndex);
            for (int i = 0; i < affected.Count; i++)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.SetTransformParent(affected[i].transform, spl.transform, "Split Spline");
#endif
                if (Application.isPlaying)
                    affected[i].transform.SetParent(spl.transform, true);
                affected[i].reSettleINTERNAL();
                spl.ControlPoints.Add(affected[i]);
            }
            Spline.SetDirtyAll();
            Spline.SyncHierarchyFromSpline();
            spl.SetDirtyAll();
            spl.SyncHierarchyFromSpline();
            Spline.Refresh();
            spl.Refresh();
            return spl;
        }

        #endregion

        #region ### Privates & Internals ###
        /*! \cond PRIVATE */
        
         CurvyInterpolation interpolation { get { return Spline.Interpolation; } }

        
        bool isDynamicOrientation
        {
            get { return Spline && Spline.Orientation == CurvyOrientation.Dynamic; }
        }

        bool canHaveSwirl
        {
            get { return isDynamicOrientation && OrientationAnchor; }
        }

        CurvySplineSegment getFollowUpCP()
        {
            var heading = FollowUpHeading;
            if (heading == ConnectionHeadingEnum.Auto)
            {
                if (FollowUp.IsFirstVisibleControlPoint)
                    heading = ConnectionHeadingEnum.Plus;
                else if (FollowUp.IsLastVisibleControlPoint)
                    heading = ConnectionHeadingEnum.Minus;
                else
                    heading = ConnectionHeadingEnum.Sharp;
            }
            if (heading == ConnectionHeadingEnum.Minus)
                return FollowUp.GetPreviousControlPoint(false, false);
            else if (heading == ConnectionHeadingEnum.Plus)
                return FollowUp.GetNextControlPoint(false, false);
            else
                return FollowUp;
        }

        /// <summary>
        /// Gets the 2nd next Transform to be used by Interpolation, respecting Spline.AutoEndHandles
        /// </summary>
        Transform getNextNextTransform(bool withFollowUp)
        {
            var cp = GetNextControlPoint(false, withFollowUp);
            return (cp) ? cp.GetNextTransform(withFollowUp) : transform;
        }

        TTransform getNextNextTTransform(bool withFollowUp)
        {
            var cp = GetNextControlPoint(false, withFollowUp);
            return (cp) ? cp.GetNextTTransform(withFollowUp) : TTransform;
        }


        /// <summary>
        /// Internal, Gets localF by an index of mApproximation
        /// </summary>
        float getApproximationLocalF(int idx)
        {
            return idx * mStepSize;
        }

        /// <summary>
        /// Internal, gets the index of mApproximation by F and the remaining fragment
        /// </summary>
        public int getApproximationIndexINTERNAL(float localF, out float frag)
        {
            localF = Mathf.Clamp01(localF);
            if (localF == 1)
            {
                frag = 1;
                return Mathf.Max(0,Approximation.Length - 2);
            }
            float f = localF / mStepSize;
            int idx = (int)f;
            frag = f - idx;
            return idx;
        }

        Vector3 interpolateLinear(float localF)
        {
            localF = Mathf.Clamp01(localF);
            return Vector3.Lerp(TTransform.localPosition,
                                GetNextTTransform(true).localPosition, localF);
        }

        Vector3 interpolateBezier(float localF)
        {
            localF = Mathf.Clamp01(localF);
            var ncp = GetNextControlPoint(false, true);
            return CurvySpline.Bezier(TTransform.localPosition + HandleOut,
                                      TTransform.localPosition,
                                      ncp.TTransform.localPosition,
                                      ncp.TTransform.localPosition + ncp.HandleIn,
                                      localF);
        }

        Vector3 interpolateCatmull(float localF)
        {
            localF = Mathf.Clamp01(localF);
            return CurvySpline.CatmullRom(GetPreviousTTransform(true).localPosition,
                                                  TTransform.localPosition,
                                                  GetNextTTransform(true).localPosition,
                                                  getNextNextTTransform(true).localPosition,
                                                  localF);
        }

        Vector3 interpolateTCB(float localF)
        {
            localF = Mathf.Clamp01(localF);
            float t0 = StartTension; float t1 = EndTension;
            float c0 = StartContinuity; float c1 = EndContinuity;
            float b0 = StartBias; float b1 = EndBias;

            if (!OverrideGlobalTension)
                t0 = t1 = Spline.Tension;
            if (!OverrideGlobalContinuity)
                c0 = c1 = Spline.Continuity;
            if (!OverrideGlobalBias)
                b0 = b1 = Spline.Bias;
            return CurvySpline.TCB(GetPreviousTTransform(true).localPosition,
                                   TTransform.localPosition,
                                   GetNextTTransform(true).localPosition,
                                   getNextNextTTransform(true).localPosition,
                                   localF, t0, c0, b0, t1, c1, b1);
        }

        internal void refreshCurveINTERNAL()
        {
            bool isSegment = IsValidSegment;
            TTransform nextTT = NextTTransform;

            if (isSegment)
                CacheSize = CurvySpline.CalculateCacheSize(Spline.CacheDensity,
                                               (nextTT.position - TTransform.position).magnitude,
                                               (Spline is CurvyUISpline) ? 1 : 0);
            else
                CacheSize = 0;
            
            Array.Resize<Vector3>(ref Approximation, CacheSize + 1);
            Array.Resize<Vector3>(ref ApproximationT, CacheSize + 1);
            Array.Resize<float>(ref ApproximationDistances, CacheSize + 1);
            
            if (Spline.Orientation==CurvyOrientation.None)
                Array.Resize<Vector3>(ref ApproximationUp, 2);
            else
                Array.Resize<Vector3>(ref ApproximationUp, CacheSize + 1);

            mBounds=null;
            Length=0;
            mStepSize = 1f / CacheSize;

            Approximation[0] = TTransform.localPosition;
            Approximation[CacheSize] = (nextTT) ? nextTT.localPosition : Approximation[0];

            if (isSegment)
            {
                Vector3 t;
                
                switch (Spline.Interpolation)
                {
                    case CurvyInterpolation.Bezier:
                        for (int i = 1; i < CacheSize; i++)
                        {
                            Approximation[i] = interpolateBezier(i * mStepSize);
                            t = (Approximation[i] - Approximation[i - 1]);
                            Length += t.magnitude;
                            ApproximationDistances[i] = Length;
                            ApproximationT[i - 1] = t.normalized;
                        }
                        break;
                    case CurvyInterpolation.CatmullRom:
                        for (int i = 1; i < CacheSize; i++)
                        {
                            Approximation[i] = interpolateCatmull(i * mStepSize);
                            t = (Approximation[i] - Approximation[i - 1]);
                            Length += t.magnitude;
                            ApproximationDistances[i] = Length;
                            ApproximationT[i - 1] = t.normalized;
                        }
                        break;
                    case CurvyInterpolation.TCB:
                        for (int i = 1; i < CacheSize; i++)
                        {
                            Approximation[i] = interpolateTCB(i * mStepSize);
                            t = (Approximation[i] - Approximation[i - 1]);
                            Length += t.magnitude;
                            ApproximationDistances[i] = Length;
                            ApproximationT[i - 1] = t.normalized;
                        }
                        break;
                    default:
                        for (int i = 1; i < CacheSize; i++)
                        {
                            Approximation[i] = interpolateLinear(i * mStepSize);
                            t = (Approximation[i] - Approximation[i - 1]);
                            Length += t.magnitude;
                            ApproximationDistances[i] = Length;
                            ApproximationT[i - 1] = t.normalized;
                        }
                        break;
                }

                t=Approximation[CacheSize] - Approximation[CacheSize - 1];
                Length += t.magnitude;
                ApproximationDistances[CacheSize] = Length;
                ApproximationT[CacheSize - 1] = t.normalized;
            }
            
        }

        

        internal void refreshOrientationStaticINTERNAL()
        {
            ApproximationUp[0] = getOrthoUp0INTERNAL();
            if (Approximation.Length > 1)
            {
                ApproximationUp[CacheSize] = getOrthoUp1INTERNAL();
                for (int i=1;i<CacheSize;i++)
                {
                    ApproximationUp[i] = Vector3.Lerp(ApproximationUp[0], ApproximationUp[CacheSize], i/(float)CacheSize);
                }
            }
            if (AutoBakeOrientation)
                BakeOrientation(false);
        }

        internal void refreshOrientationPTFINTERNAL(ref Vector3 lastUpVector)
        {
            ApproximationUp[0] = lastUpVector;
            
            if (Approximation.Length > 1)
            {
                int ub = CacheSize + 1;
                for (int i = 1; i < ub; i++)
                {
                    lastUpVector = DTMath.ParallelTransportFrame(ref lastUpVector, ref ApproximationT[i - 1], ref ApproximationT[i]);
                    ApproximationUp[i] = lastUpVector;
                }
            }
            if (AutoBakeOrientation)
                BakeOrientation(false);
        }

        // returns the last T
        internal void smoothOrientationINTERNAL(ref Vector3 lastUpVector, ref float angleaccu, float angle)
        {
            ApproximationUp[0] = lastUpVector;
            int b = ApproximationUp.Length;
            for (int i = 1; i < b; i++)
            {
                ApproximationUp[i] = Quaternion.AngleAxis(angleaccu, ApproximationT[i]) * ApproximationUp[i];
                angleaccu += angle;
            }
            lastUpVector = ApproximationUp[ApproximationUp.Length - 1];
            
            if (AutoBakeOrientation)
                BakeOrientation(false);
        }

        Bounds getBounds()
        {
            if (Approximation.Length == 0)
                return new Bounds(TTransform.position, Vector3.zero);
            var mat = Spline.transform.localToWorldMatrix;
            Bounds b = new Bounds(mat.MultiplyPoint3x4(Approximation[0]), Vector3.zero);
            var u = Approximation.Length;
            for (int i = 1; i < u; i++)
                b.Encapsulate(mat.MultiplyPoint(Approximation[i]));
             
            return b;
        }

        internal void ClearBoundsINTERNAL()
        {
            mBounds = null;
        }

        /// <summary>
        /// Gets Transform.up orthogonal to ApproximationT[0]
        /// </summary>
        internal Vector3 getOrthoUp0INTERNAL()
        {
            var u = Quaternion.Inverse(Spline.TTransform.rotation) * TTransform.up;
            Vector3.OrthoNormalize(ref ApproximationT[0],ref u);
            return u;
        }

        internal Vector3 getOrthoUp1INTERNAL()
        {
            var u = Quaternion.Inverse(Spline.TTransform.rotation) * NextTTransform.up;
            Vector3.OrthoNormalize(ref ApproximationT[CacheSize], ref u);
            return u;
        }

        /// <summary>
        /// Prepare a move by deleting spline reference and resetting ControlPointIndex and SegmentIndex. INTERNAL, DON'T CALL THIS DIRECTLY!
        /// </summary>
        internal void reSettleINTERNAL(bool removeFromCollection = true)
        {

            if (removeFromCollection)
                Spline.ControlPoints.Remove(this);
            else
                SetDirty();
            mSpline = null;
            mControlPointIndex = -1;
            mSegmentIndex = -1;
        }

        void doGizmos(bool selected)
        {

            if (CurvyGlobalManager.Gizmos == CurvySplineGizmos.None) return;
            // Skip if the segment isn't in view
            var cam = Camera.current;
            bool camAware = (cam != null);
            float camDistance = 0;
            if (camAware)
            {
                if (!cam.BoundsInView(Bounds))
                    return;

                camDistance = (cam.transform.position - Bounds.ClosestPoint(cam.transform.position)).magnitude;
            }

            bool viewCurve = (CurvyGlobalManager.Gizmos & CurvySplineGizmos.Curve) == CurvySplineGizmos.Curve;


            CurvyGizmoHelper.Matrix = Spline.transform.localToWorldMatrix;

            // Connections
            if (Connection)
                CurvyGizmoHelper.ConnectionGizmo(this);

            // Control Point
            if (viewCurve)
            {
                CurvyGizmoHelper.ControlPointGizmo(this, selected, (selected) ? Spline.GizmoSelectionColor : Spline.GizmoColor);
            }



            if (!IsValidSegment) return;

            if (viewCurve)
            {
                float steps = 20;
                if (camAware)
                {
                    float df = Mathf.Clamp(camDistance, 1, 3000) / 3000;
                    df = (df < 0.01f) ? DTTween.SineOut(df, 0, 1) : DTTween.QuintOut(df, 0, 1);
                    steps = Mathf.Clamp((Length * CurvyGlobalManager.SceneViewResolution * 0.1f) / df, 1, 10000);
                }
                CurvyGizmoHelper.SegmentCurveGizmo(this, (selected) ? Spline.GizmoSelectionColor : Spline.GizmoColor, 1 / steps);
            }

            if (Approximation.Length > 0 && (CurvyGlobalManager.Gizmos & CurvySplineGizmos.Approximation) == CurvySplineGizmos.Approximation)
                CurvyGizmoHelper.SegmentApproximationGizmo(this, Spline.GizmoColor * 0.8f);

            if (Spline.Orientation != CurvyOrientation.None && ApproximationUp.Length > 0 && (CurvyGlobalManager.Gizmos & CurvySplineGizmos.Orientation) == CurvySplineGizmos.Orientation)
            {
                CurvyGizmoHelper.SegmentOrientationGizmo(this, CurvyGlobalManager.GizmoOrientationColor);
                if (OrientationAnchor && Spline.Orientation == CurvyOrientation.Dynamic)
                    CurvyGizmoHelper.SegmentOrientationAnchorGizmo(this, CurvyGlobalManager.GizmoOrientationColor);
            }

            if (ApproximationT.Length > 0 && (CurvyGlobalManager.Gizmos & CurvySplineGizmos.Tangents) == CurvySplineGizmos.Tangents)
                CurvyGizmoHelper.SegmentTangentGizmo(this, new Color(0, 0.7f, 0));

        }
        
        internal void ClearSegmentIndexINTERNAL()
        {
            mSegmentIndex = -1;
        }
        
        /*! \endcond */
        #endregion

        #region ### Interface Implementations ###

        //IPoolable
        public void OnBeforePush()
        {
            Reset();
            DeleteMetadata();
            mSpline = null;

        }

        //IPoolable
        public void OnAfterPop()
        {
        }

        int IComparable.CompareTo(object obj)
        {
            return ControlPointIndex.CompareTo(((CurvySplineSegment)obj).ControlPointIndex);
        }

        #endregion
    }
}
