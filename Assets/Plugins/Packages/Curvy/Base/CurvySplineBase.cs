// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools.Extensions;

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Base class both CurvySpline and CurvySplineGroup derive from
    /// </summary>
    [DTVersion(CurvySpline.VERSION)]
    public class CurvySplineBase : DTVersionedMonoBehaviour
    {

        #region ### Events ###
        [Group("Events",Expanded=false,Sort=1000,HelpURL=CurvySpline.DOCLINK+"curvyspline_events")]
        [SortOrder(0)]
        [SerializeField]
        CurvySplineEvent m_OnRefresh=new CurvySplineEvent();

        public CurvySplineEvent OnRefresh
        {
            get { return m_OnRefresh; }
            set
            {
                if (m_OnRefresh!=value)
                    m_OnRefresh = value;
            
            }
            
        }

        [Group("Events", Sort = 1000)]
        [SortOrder(1)]
        [SerializeField]
        CurvySplineEvent m_OnAfterControlPointChanges = new CurvySplineEvent();

        /// <summary>
        /// Callback after one or more Control Points have been added or deleted
        /// </summary>
        /// <remarks>This executes last, after individual add/delete events and OnRefresh </remarks>
        public CurvySplineEvent OnAfterControlPointChanges
        {
            get { return m_OnAfterControlPointChanges; }
            set
            {
                if (m_OnAfterControlPointChanges != value)
                    m_OnAfterControlPointChanges = value;
            }
        }



        [Group("Events", Sort = 1000)]
        [SortOrder(2)]
        [SerializeField]
        CurvyControlPointEvent m_OnBeforeControlPointAdd = new CurvyControlPointEvent();

        /// <summary>
        /// Callback before a Control Point is about to be added. Return false to cancel the execution
        /// </summary>
        public CurvyControlPointEvent OnBeforeControlPointAdd 
        {
            get { return m_OnBeforeControlPointAdd; }
            set
            {
                if (m_OnBeforeControlPointAdd != value)
                    m_OnBeforeControlPointAdd = value;
            }
        }
        [Group("Events", Sort = 1000)]
        [SortOrder(3)]
        [SerializeField]
        CurvyControlPointEvent m_OnAfterControlPointAdd = new CurvyControlPointEvent();

        /// <summary>
        /// Callback after a Control Point has been added and the spline was refreshed
        /// </summary>
        public CurvyControlPointEvent OnAfterControlPointAdd
        {
            get { return m_OnAfterControlPointAdd; }
            set
            {
                if (m_OnAfterControlPointAdd != value)
                    m_OnAfterControlPointAdd = value;
            }
            
        }
        [Group("Events", Sort = 1000)]
        [SortOrder(4)]
        [SerializeField]
        CurvyControlPointEvent m_OnBeforeControlPointDelete=new CurvyControlPointEvent();

        /// <summary>
        /// Callback before a Control Point is about to be deleted. Return false to cancel the execution
        /// </summary>
        public CurvyControlPointEvent OnBeforeControlPointDelete
        {
            get { return m_OnBeforeControlPointDelete; }
            set
            {
                if (m_OnBeforeControlPointDelete != value)
                    m_OnBeforeControlPointDelete = value;
            }
        }
        

        [SerializeField,HideInInspector]
        CurvySplineMoveEvent m_OnMoveControlPointReached=new CurvySplineMoveEvent();

        /// <summary>
        /// Callback raised when a CP is reached in one of the move methods
        /// </summary>
        public CurvySplineMoveEvent OnMoveControlPointReached
        {
            get { return m_OnMoveControlPointReached; }
            set
            {
                if (m_OnMoveControlPointReached != value)
                    m_OnMoveControlPointReached = value;
            }
        }

        [SerializeField, HideInInspector]
        CurvySplineMoveEvent m_OnMoveEndReached=new CurvySplineMoveEvent();

        /// <summary>
        /// Callback raised when the first or last CP is reached in one of the move methods
        /// </summary>
        public CurvySplineMoveEvent OnMoveEndReached
        {
            get { return m_OnMoveEndReached; }
            set
            {
                if (m_OnMoveEndReached != value)
                    m_OnMoveEndReached = value;
            }
        }

        

        protected CurvySplineEventArgs OnRefreshEvent(CurvySplineEventArgs e)
        {
            if (OnRefresh != null)
                OnRefresh.Invoke(e);
            return e;
        }

        protected CurvyControlPointEventArgs OnBeforeControlPointAddEvent(CurvyControlPointEventArgs e)
        {
            if (OnBeforeControlPointAdd != null)
                OnBeforeControlPointAdd.Invoke(e);
            return e;
        }

        protected CurvyControlPointEventArgs OnAfterControlPointAddEvent(CurvyControlPointEventArgs e)
        {
            if (OnAfterControlPointAdd != null)
                OnAfterControlPointAdd.Invoke(e);
            return e;
        }

        protected CurvyControlPointEventArgs OnBeforeControlPointDeleteEvent(CurvyControlPointEventArgs e)
        {
            if (OnBeforeControlPointDelete != null)
                OnBeforeControlPointDelete.Invoke(e);
            return e;
        }

        protected CurvySplineEventArgs OnAfterControlPointChangesEvent(CurvySplineEventArgs e)
        {
            if (OnAfterControlPointChanges != null)
                OnAfterControlPointChanges.Invoke(e);
            return e;
        }

        protected CurvySplineMoveEventArgs OnMoveControlPointReachedEvent(CurvySplineMoveEventArgs e)
        {
            if (OnMoveControlPointReached != null)
                OnMoveControlPointReached.Invoke(e);
            if (OnMoveEndReached != null && (e.ControlPoint.IsFirstVisibleControlPoint || e.ControlPoint.IsLastVisibleControlPoint))
                OnMoveEndReached.Invoke(e);
            return e;
        }

        #endregion

        #region ### Serialized Fields ###

        /// <summary>
        /// Whether to show the Gizmos enabled in the view settings or not at all 
        /// </summary>
        [HideInInspector]
        public bool ShowGizmos = true;

        #endregion

        #region ### Public Properties ###
        
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
        /// Whether the spline is fully initialized and all segments loaded
        /// </summary>
        public bool IsInitialized { get { return mIsInitialized; } }
        /// <summary>
        /// Gets the total length of the Spline or SplineGroup
        /// </summary>
        /// <remarks>The accuracy depends on the current Granularity (higher Granularity means more exact values)</remarks>
        public float Length { get { return mLength; } }

        /// <summary>
        /// Gets whether the splines in a spline group form a continuous curve
        /// </summary>
        /// <remarks>This is always true for single splines</remarks>
        public virtual bool IsContinuous
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether the splines in a spline group form a continuous closed curve
        /// </summary>
        /// <remarks>For single splines, this is equal to Closed</remarks>
        public virtual bool IsClosed
        {
            get
            {
                return false;
            }
        }

        public virtual bool Dirty
        {
            get { return false; }
        }

        public virtual int Count { get { return 0; } }
        public virtual Bounds Bounds { get { return new Bounds(); } }

        #endregion

        #region ### Private Fields ###

        protected float mLength;
        TTransform mTTransform;
        Bounds mBounds;
        protected bool mIsInitialized;

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

        protected virtual void Awake()
        {
            mTTransform = new TTransform(transform);
        }

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            mTTransform = new TTransform(transform);
#endif
        }

        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        /// <summary>
        /// Gets the interpolated position for a certain TF
        /// </summary>
        /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <returns>the interpolated position</returns>
        public virtual Vector3 Interpolate(float tf) { return Vector3.zero; }
        /// <summary>
        /// Gets the interpolated position for a certain TF
        /// </summary>
        /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <param name="interpolation">the interpolation to use</param>
        /// <returns>the interpolated position</returns>
        public virtual Vector3 Interpolate(float tf, CurvyInterpolation interpolation) { return Vector3.zero; }
        /// <summary>
        /// Gets the interpolated position for a certain TF using a linear approximation
        /// </summary>
        /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the interpolated position</returns>
        public virtual Vector3 InterpolateFast(float tf) { return Vector3.zero; }
        /// <summary>
        /// Gets an interpolated User Value for a certain TF
        /// </summary>
        /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
        /// <param name="tf">TF value reflecting position on spline(0..1)</param>
        /// <param name="index">the UserValue array index</param>
        /// <returns>the interpolated value</returns>
        public virtual Vector3 InterpolateUserValue(float tf, int index) { return Vector3.zero; }

        /// <summary>
        /// Gets metadata for a certain TF
        /// </summary>
        /// <typeparam name="T">Metadata type interfacing ICurvyMetadata</typeparam>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the metadata</returns>
        public T GetMetadata<T>(float tf) where T : Component, ICurvyMetadata { return (T)GetMetadata(typeof(T), tf); }

        /// <summary>
        /// Gets metadata for a certain TF
        /// </summary>
        /// <param name="type">Metadata type interfacing ICurvyMetadata</typeparam>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the metadata</returns>
        public virtual Component GetMetadata(System.Type type, float tf) { return null;}
        /// <summary>
        /// Gets an interpolated Metadata value for a certain TF
        /// </summary>
        /// <typeparam name="T">Metadata type interfacing ICurvyInterpolatableMetadata</typeparam>
        /// <typeparam name="U">Return Value type of T</typeparam>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the interpolated value</returns>
        public virtual U InterpolateMetadata<T,U>(float tf) where T : Component, ICurvyInterpolatableMetadata<U> { return default(U); }
        /// <summary>
        /// Gets an interpolated Metadata value for a certain TF
        /// </summary>
        /// <param name="type">Metadata type interfacing ICurvyInterpolatableMetadata</param>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the interpolated value</returns>
        public virtual object InterpolateMetadata(System.Type type, float tf) { return null; }
        /// <summary>
        /// Gets an interpolated Scale for a certain TF
        /// </summary>
        /// <remarks>TF (Total Fragment) relates to the total length of the spline</remarks>
        /// <param name="tf">TF value reflecting position on spline(0..1)</param>
        /// <returns>the interpolated value</returns>
        public virtual Vector3 InterpolateScale(float tf) { return Vector3.zero; }
        /// <summary>
        /// Gets the Up-Vector for a certain TF based on the splines' Orientation mode
        /// </summary>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the Up-Vector</returns>
        public virtual Vector3 GetOrientationUpFast(float tf) { return Vector3.zero; }
        /// <summary>
        /// Gets a rotation looking to Tangent with the head upwards along the Up-Vector
        /// </summary>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>a rotation</returns>
        public virtual Quaternion GetOrientationFast(float tf) { return GetOrientationFast(tf, false); }
        /// <summary>
        /// Gets a rotation looking to Tangent with the head upwards along the Up-Vector
        /// </summary>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <param name="inverse">whether the orientation should be inversed or not</param>
        /// <returns>a rotation</returns>
        public virtual Quaternion GetOrientationFast(float tf, bool inverse) { return Quaternion.identity; }
        /// <summary>
        /// Gets the normalized tangent for a certain TF
        /// </summary>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <returns>a tangent vector</returns>
        public virtual Vector3 GetTangent(float tf) { return Vector3.zero; }
        /// <summary>
        /// Gets the normalized tangent for a certain TF with a known position for TF
        /// </summary>
        /// <remarks>This saves one interpolation</remarks>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <param name="position">The interpolated position for TF</param>
        /// <returns>a tangent vector</returns>
        public virtual Vector3 GetTangent(float tf, Vector3 position) { return Vector3.zero; }
        /// <summary>
        /// Gets the normalized tangent for a certain TF using a linear approximation
        /// </summary>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <returns>a tangent vector</returns>
        public virtual Vector3 GetTangentFast(float tf) { return Vector3.zero; }
        /// <summary>
        /// Converts a TF value to a distance
        /// </summary>
        /// <param name="tf">a TF value</param>
        /// <param name="clamping">Clamping to use</param>
        /// <returns>distance from the first segment's Control Point</returns>
        public virtual float TFToDistance(float tf, CurvyClamping clamping) { return 0; }
        public float TFToDistance(float tf) { return TFToDistance(tf, CurvyClamping.Clamp); }
        /// <summary>
        /// Converts a distance to a TF value
        /// </summary>
        /// <param name="distance">distance</param>
        /// <param name="clamping">Clamping to use</param>
        /// <returns>a TF value in the range 0..1</returns>
        public virtual float DistanceToTF(float distance, CurvyClamping clamping) { return 0; }
        public float DistanceToTF(float distance) { return DistanceToTF(distance, CurvyClamping.Clamp); }
        
        /// <summary>
        /// Gets the segment and the local F for a certain TF
        /// </summary>
        /// <param name="tf">the TF value</param>
        /// <param name="localF">gets the remaining localF in the range 0..1</param>
        /// <param name="clamping">Clamping to use</param>
        /// <returns>the segment the given TF is inside</returns>
        public virtual CurvySplineSegment TFToSegment(float tf, out float localF, CurvyClamping clamping) { localF = 0; return null; }

        /// <summary>
        /// Gets the segment for a certain TF
        /// </summary>
        /// <param name="tf">the TF value</param>
        /// <param name="clamping">Clamping to use</param>
        /// <returns>the segment the given TF is inside</returns>
        public CurvySplineSegment TFToSegment(float tf, CurvyClamping clamping)
        {
            float f;
            return TFToSegment(tf, out f, clamping);
        }

        /// <summary>
        /// Gets the segment for a certain TF clamped to 0..1
        /// </summary>
        /// <param name="tf">the TF value</param>
        /// <returns>the segment the given TF is inside</returns>
        public CurvySplineSegment TFToSegment(float tf)
        {
            float f;
            return TFToSegment(tf, out f, CurvyClamping.Clamp);
        }
        /// <summary>
        /// Gets the segment and the local F for a certain TF clamped to 0..1
        /// </summary>
        /// <param name="tf">the TF value</param>
        /// <param name="localF">gets the remaining localF in the range 0..1</param>
        /// <returns>the segment the given TF is inside</returns>
        public CurvySplineSegment TFToSegment(float tf, out float localF)
        {
            return TFToSegment(tf, out localF, CurvyClamping.Clamp);
        }
        /// <summary>
        /// Gets a TF value from a segment
        /// </summary>
        /// <param name="segment">a segment</param>
        /// <returns>a TF value in the range 0..1</returns>
        public virtual float SegmentToTF(CurvySplineSegment segment) { return SegmentToTF(segment, 0); }
        /// <summary>
        /// Gets a TF value from a segment and a local F
        /// </summary>
        /// <param name="segment">a segment</param>
        /// <param name="localF">F of this segment in the range 0..1</param>
        /// <returns>a TF value in the range 0..1</returns>
        public virtual float SegmentToTF(CurvySplineSegment segment, float localF) { return 0; }
        /// <summary>
        /// Gets the TF value that is nearest to p
        /// </summary>
        /// <param name="p">a point in space</param>
        /// <remarks>The speed as well as the accuracy depends on the Granularity</remarks>
        /// <returns>a TF value in the range 0..1 or -1 on error</returns>
        public virtual float GetNearestPointTF(Vector3 p) { return 0; }
        /// <summary>
        /// Gets the TF value that is nearest to p
        /// </summary>
        /// <param name="p">a point in spline's local space</param>
        /// <param name="nearest">returns the nearest position</param>
        /// <remarks>The speed as well as the accuracy depends on the Granularity</remarks>
        /// <returns>a TF value in the range 0..1 or -1 on error</returns>
        public virtual float GetNearestPointTF(Vector3 p, out Vector3 nearest) { nearest = Vector3.zero; return 0; }
        /// <summary>
        /// Removes all control points
        /// </summary>
        public virtual void Clear() { }
        /// <summary>
        /// Gets an array containing all approximation points
        /// </summary>
        /// <param name="space">Whether to use local or global space</param>
        /// <remarks>This can be used to feed meshbuilders etc...</remarks>
        /// <returns>an array of world/local positions</returns>
        public virtual Vector3[] GetApproximation(Space space=Space.Self) { return new Vector3[0]; }
        /// <summary>
        /// Gets an array containing all approximation tangents
        /// </summary>
        /// <remarks>This can be used to feed meshbuilders etc...</remarks>
        /// <returns>an array of tangents</returns>
        public virtual Vector3[] GetApproximationT() { return new Vector3[0]; }
        /// <summary>
        /// Gets an array containing all approximation Up-Vectors
        /// </summary>
        /// <remarks>This can be used to feed meshbuilders etc...</remarks>
        /// <returns>an array of Up-Vectors</returns>
        public virtual Vector3[] GetApproximationUpVectors() { return new Vector3[0]; }
        /// <summary>
        /// Gets an array containing spline positions by angle difference
        /// </summary>
        /// <param name="angle">maximum angle in degrees</param>
        /// <param name="minDistance">minimum distance between two points</param>
        /// <returns>an array of interpolated positions</returns>
        public Vector3[] GetPolygonByAngle(float angle, float minDistance)
        {
            if (Mathf.Approximately(angle, 0))
            {
                Debug.LogError("CurvySplineBase.GetPolygonByAngle: angle must be greater than 0!");
                return new Vector3[0];
            }
            List<Vector3> points = new List<Vector3>();

            float tf = 0;
            int dir = 1;
            float min2 = minDistance * minDistance;
            points.Add(Interpolate(0));
            while (tf < 1)
            {
                Vector3 p = MoveByAngle(ref tf, ref dir, angle, CurvyClamping.Clamp);
                if ((p - points[points.Count - 1]).sqrMagnitude >= min2)
                    points.Add(p);
            }

            return points.ToArray();
        }
        /// <summary>
        /// Gets an array containing spline positions
        /// </summary>
        /// <param name="fromTF">start TF</param>
        /// <param name="toTF">end TF</param>
        /// <param name="maxAngle">maximum angle in degrees</param>
        /// <param name="minDistance">minimum distance between two points</param>
        /// <param name="maxDistance">maximum distance between two points</param>
        /// <param name="vertexTF">Stores the TF of the resulting points</param>
        /// <param name="vertexTangents">Stores the Tangents of the resulting points</param>
        /// <param name="includeEndPoint">Whether the end position should be included</param>
        /// <param name="stepSize">the stepsize to use</param>
        /// <returns>an array of interpolated positions</returns>
        public Vector3[] GetPolygon(float fromTF, float toTF, float maxAngle, float minDistance, float maxDistance, out List<float> vertexTF, out List<Vector3> vertexTangents, bool includeEndPoint=true, float stepSize=0.01f)
        {
            stepSize = Mathf.Clamp(stepSize, 0.002f, 1);
            maxDistance = (maxDistance == -1) ? Length : Mathf.Clamp(maxDistance, 0, Length);
            minDistance = Mathf.Clamp(minDistance, 0, maxDistance);
            if (!IsClosed)
            {
                toTF = Mathf.Clamp01(toTF);
                fromTF = Mathf.Clamp(fromTF, 0, toTF);
            }
            List<Vector3> vPos = new List<Vector3>();
            List<Vector3> vTan = new List<Vector3>();
            List<float>vTF= new List<float>();

            int linearSteps = 0;
            float angleFromLast = 0;
            float distAccu = 0;
            Vector3 curPos = Interpolate(fromTF);
            Vector3 curTangent = GetTangent(fromTF);
            Vector3 lastPos = curPos;
            Vector3 lastTangent = curTangent;

            var addPoint = new System.Action<float>((f) =>
            {
                vPos.Add(curPos);
                vTan.Add(curTangent);
                vTF.Add(f);
                angleFromLast = 0;
                distAccu = 0;
                
                linearSteps = 0;
            });

            addPoint(fromTF);

            float tf = fromTF + stepSize;
            float t;
            while (tf < toTF)
            {
                t = tf % 1;
                // Get Point Pos & Tangent
                curPos = Interpolate(t);
                curTangent = GetTangent(t);
                if (curTangent == Vector3.zero)
                {
                    Debug.Log("zero Tangent! Oh no!");
                }
                distAccu += (curPos - lastPos).magnitude;
                if (curTangent == lastTangent)
                    linearSteps++;
                if (distAccu >= minDistance)
                {
                    // Exceeding distance?
                    if (distAccu >= maxDistance)
                        addPoint(t);
                    else // Check angle
                    {
                        angleFromLast += Vector3.Angle(lastTangent, curTangent);
                        // Max angle reached or entering/leaving a linear zone
                        if (angleFromLast >= maxAngle || (linearSteps > 0 && angleFromLast > 0))
                            addPoint(t);
                    }
                }
                tf += stepSize;
                lastPos = curPos;
                lastTangent = curTangent;
            }
            if (includeEndPoint)
            {
                vTF.Add(toTF % 1);
                curPos = Interpolate(toTF % 1);
                vPos.Add(curPos);
                vTan.Add(GetTangent(toTF % 1, curPos));
            }

            vertexTF=vTF;
            vertexTangents=vTan;
            return vPos.ToArray();
        }

        /// <summary>
        /// Gets all Approximation points for a given spline part
        /// </summary>
        /// <param name="fromTF">start TF</param>
        /// <param name="toTF">end TF</param>
        /// <param name="includeEndPoint">Whether the end position should be included</param>
        /// <returns>an array of Approximation points</returns>
        public Vector3[] GetApproximationPoints(float fromTF, float toTF, bool includeEndPoint=true)
        {
            float startLF;
            float startFrag;
            float endLF;
            float endFrag;
            var startSeg = TFToSegment(fromTF, out startLF);
            int startIdx = startSeg.getApproximationIndexINTERNAL(startLF, out startFrag);
            var endSeg = TFToSegment(toTF, out endLF);
            int endIdx = endSeg.getApproximationIndexINTERNAL(endLF, out endFrag);

            var seg = startSeg;
            Vector3[] res = new Vector3[1] { Vector3.Lerp(seg.Approximation[startIdx], seg.Approximation[startIdx + 1], startFrag)};
            //if (startFrag == 1)
            //    seg = seg.NextSegment;
            while (seg && seg != endSeg)
            {
                res=res.AddRange<Vector3>(seg.Approximation.SubArray<Vector3>(startIdx + 1, seg.Approximation.Length-1));
                startIdx = 1;
                seg = seg.NextSegment;
            }
            if (seg)
            {
                int i = (startSeg == seg) ? startIdx + 1 : 1;
                res = res.AddRange<Vector3>(seg.Approximation.SubArray<Vector3>(i, endIdx-i));
                if (includeEndPoint && (endFrag>0 || endFrag<1))
                    return res.Add<Vector3>(Vector3.Lerp(seg.Approximation[endIdx], seg.Approximation[endIdx + 1], endFrag));
            }
            return res;
        }


        /// <summary>
        /// Alter TF to reflect a movement over a certain portion of the spline/group
        /// </summary>
        /// <remarks>fDistance relates to the total spline, so longer splines will result in faster movement for constant fDistance</remarks>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="fDistance">the percentage of the spline/group to move</param>
        /// <param name="clamping">clamping mode</param>
        /// <returns>the interpolated position</returns>
        public virtual Vector3 Move(ref float tf, ref int direction, float fDistance, CurvyClamping clamping)
        {
            tf = CurvyUtility.ClampTF(tf + fDistance * direction, ref direction, clamping);
            return Interpolate(tf);
        }

        /// <summary>
        /// Alter TF to reflect a movement over a certain portion of the spline/group, respecting Clamping. Unlike Move() a linear approximation will be used
        /// </summary>
        /// <remarks>fDistance relates to the total spline, so longer splines will result in faster movement for constant fDistance</remarks>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="fDistance">the percentage of the spline/group to move</param>
        /// <param name="clamping">clamping mode</param>
        /// <returns>the interpolated position</returns>
        public virtual Vector3 MoveFast(ref float tf, ref int direction, float fDistance, CurvyClamping clamping)
        {
            tf = CurvyUtility.ClampTF(tf + fDistance * direction, ref direction, clamping);
            return InterpolateFast(tf);
        }

        /// <summary>
        /// Alter TF to reflect a movement over a certain distance using a default stepSize of 0.002
        /// </summary>
        /// <remarks>MoveBy works by extrapolating current curvation, so results may be inaccurate for large distances</remarks>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="distance">the distance in world units to move</param>
        /// <param name="clamping">clamping mode</param>
        /// <returns>the interpolated position</returns>
        public virtual Vector3 MoveBy(ref float tf, ref int direction, float distance, CurvyClamping clamping)
        {
            return MoveBy(ref tf, ref direction, distance, clamping, 0.002f);
        }

        /// <summary>
        /// Alter TF to reflect a movement over a certain distance
        /// </summary>
        /// <remarks>MoveBy works by extrapolating current curvation, so results may be inaccurate for large distances</remarks>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="distance">the distance in world units to move</param>
        /// <param name="clamping">clamping mode</param>
        /// <param name="stepSize">stepSize defines the accuracy</param>
        /// <returns>the interpolated position</returns>
        public virtual Vector3 MoveBy(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize)
        {
            return Move(ref tf, ref direction, ExtrapolateDistanceToTF(tf, distance, stepSize), clamping);
        }

        /// <summary>
        /// Alter TF to reflect a movement over a certain distance. Unlike MoveBy, a linear approximation will be used
        /// </summary>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="distance">the distance in world units to move</param>
        /// <param name="clamping">clamping mode</param>
        /// <returns>the interpolated position</returns>
        public virtual Vector3 MoveByFast(ref float tf, ref int direction, float distance, CurvyClamping clamping)
        {
            return MoveByFast(ref tf, ref direction, distance, clamping, 0.002f);
        }

        /// <summary>
        /// Alter TF to reflect a movement over a certain distance. Unlike MoveBy, a linear approximation will be used
        /// </summary>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="distance">the distance in world units to move</param>
        /// <param name="clamping">clamping mode</param>
        /// <param name="stepSize">stepSize defines the accuracy</param>
        /// <returns>the interpolated position</returns>
        public virtual Vector3 MoveByFast(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize)
        {
            return MoveFast(ref tf, ref direction, ExtrapolateDistanceToTFFast(tf, distance, stepSize), clamping);
        }

        /// <summary>
        /// Alter TF to reflect a movement over a certain distance.
        /// </summary>
        /// <remarks>MoveByLengthFast works by using actual lengths</remarks>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="distance">the distance in world units to move</param>
        /// <param name="clamping">clamping mode</param>
        /// <returns>the interpolated position</returns>
        public virtual Vector3 MoveByLengthFast(ref float tf, ref int direction, float distance, CurvyClamping clamping)
        {
            float dist = ClampDistance(TFToDistance(tf) + distance * direction, ref direction, clamping);
            tf = DistanceToTF(dist);
            return InterpolateFast(tf);
        }

        /// <summary>
        /// Alter TF to move until the curvation reached angle, using a default stepSize of 0.002f
        /// </summary>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="angle">the angle in degrees</param>
        /// <param name="clamping">the clamping mode. CurvyClamping.PingPong isn't supported!</param>
        /// <returns>the interpolated position</returns>
        public virtual Vector3 MoveByAngle(ref float tf, ref int direction, float angle, CurvyClamping clamping)
        {
            return MoveByAngle(ref tf, ref direction, angle, clamping, 0.002f);
        }

        /// <summary>
        /// Alter TF to move until the curvation reached angle.
        /// </summary>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="angle">the angle in degrees</param>
        /// <param name="clamping">the clamping mode. CurvyClamping.PingPong isn't supported!</param>
        /// <param name="stepSize">stepSize defines the accuracy</param>
        /// <returns>the interpolated position</returns>
        public virtual Vector3 MoveByAngle(ref float tf, ref int direction, float angle, CurvyClamping clamping, float stepSize)
        {
            if (clamping == CurvyClamping.PingPong)
            {
                Debug.LogError("CurvySplineBase.MoveByAngle: PingPong clamping isn't supported!");
                return Vector3.zero;
            }
            stepSize = Mathf.Max(0.0001f, stepSize);
            float initialTF = tf;
            Vector3 initialP = Interpolate(tf);
            Vector3 initialT = GetTangent(tf, initialP);
            Vector3 P2 = Vector3.zero;
            Vector3 T2;
            int deadlock = 10000;
            while (deadlock-- > 0)
            {
                tf += stepSize * direction;
                if (tf > 1)
                {
                    if (clamping == CurvyClamping.Loop)
                        tf -= 1;
                    else
                    {
                        tf = 1;
                        return Interpolate(1);
                    }
                }
                else if (tf < 0)
                {
                    if (clamping == CurvyClamping.Loop)
                        tf += 1;
                    else
                    {
                        tf = 0;
                        return Interpolate(0);
                    }
                }
                P2 = Interpolate(tf);
                T2 = P2 - initialP;
                float accAngle = Vector3.Angle(initialT, T2);
                if (accAngle >= angle)
                {
                    tf = initialTF + (tf - initialTF) * angle / accAngle;
                    return Interpolate(tf);
                }
            }
            return P2;

        }

        /// <summary>
        /// Alter TF to move until the curvation reached angle. Unlike MoveByAngle, a linear approximation will be used
        /// </summary>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="angle">the angle in degrees</param>
        /// <param name="clamping">the clamping mode. CurvyClamping.PingPong isn't supported!</param>
        /// <param name="stepSize">stepSize defines the accuracy</param>
        /// <returns>the interpolated position</returns>
        public virtual Vector3 MoveByAngleFast(ref float tf, ref int direction, float angle, CurvyClamping clamping, float stepSize)
        {
            if (clamping == CurvyClamping.PingPong)
            {
                Debug.LogError("CurvySplineBase.MoveByAngle: PingPong clamping isn't supported!");
                return Vector3.zero;
            }

            stepSize = Mathf.Max(0.0001f, stepSize);
            float initialTF = tf;
            Vector3 initialP = InterpolateFast(tf);
            Vector3 initialT = GetTangentFast(tf);
            Vector3 P2 = Vector3.zero;
            Vector3 T2;
            int deadlock = 10000;
            while (deadlock-- > 0)
            {
                tf += stepSize * direction;
                if (tf > 1)
                {
                    if (clamping == CurvyClamping.Loop)
                        tf -= 1;
                    else
                    {
                        tf = 1;
                        return InterpolateFast(1);
                    }
                }
                else if (tf < 0)
                {
                    if (clamping == CurvyClamping.Loop)
                        tf += 1;
                    else
                    {
                        tf = 0;
                        return InterpolateFast(0);
                    }
                }
                P2 = InterpolateFast(tf);
                T2 = P2 - initialP;
                float accAngle = Vector3.Angle(initialT, T2);
                if (accAngle >= angle)
                {
                    tf = initialTF + (tf - initialTF) * angle / accAngle;
                    return InterpolateFast(tf);
                }
            }
            return P2;
        }

        /// <summary>
        /// Gets the point at a certain radius and angle on the plane defined by P and it's Tangent
        /// </summary>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <param name="radius">radius in world units</param>
        /// <param name="angle">angle in degrees</param>
        /// <returns>the extruded point</returns>
        public virtual Vector3 GetExtrusionPoint(float tf, float radius, float angle)
        {
            Vector3 pos = Interpolate(tf);
            Vector3 tan = GetTangent(tf, pos);
            Quaternion R = Quaternion.AngleAxis(angle, tan);
            return pos + (R * GetOrientationUpFast(tf)) * radius;
        }

        /// <summary>
        /// Gets the point at a certain radius and angle on the plane defined by P and it's Tangent, using a linear interpolation
        /// </summary>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <param name="radius">radius in world units</param>
        /// <param name="angle">angle in degrees</param>
        /// <returns>the extruded point</returns>
        public virtual Vector3 GetExtrusionPointFast(float tf, float radius, float angle)
        {
            Vector3 pos = InterpolateFast(tf);
            Vector3 tan = GetTangentFast(tf);
            Quaternion R = Quaternion.AngleAxis(angle, tan);
            return pos + (R * GetOrientationUpFast(tf)) * radius;
        }
        /// <summary>
        /// Gets a rotated Up-Vector
        /// </summary>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <param name="angle">angle in degrees</param>
        /// <returns>the rotated Up-Vector</returns>
        public Vector3 GetRotatedUp(float tf, float angle)
        {
            Vector3 tan = GetTangent(tf);
            Quaternion R = Quaternion.AngleAxis(angle, tan);
            return (R * GetOrientationUpFast(tf));
        }

        /// <summary>
        /// Gets an rotated Up-Vector using cached values
        /// </summary>
        /// <param name="tf">TF value identifying position on spline (0..1)</param>
        /// <param name="angle">angle in degrees</param>
        /// <returns>the rotated Up-Vector</returns>
        public Vector3 GetRotatedUpFast(float tf, float angle)
        {
            Vector3 tan = GetTangentFast(tf);
            Quaternion R = Quaternion.AngleAxis(angle, tan);
            return (R * GetOrientationUpFast(tf));
        }
       
        /// <summary>
        /// Gets the normalized tangent by distance from the spline/group's start.
        /// </summary>
        /// <param name="distance">distance in the range of 0..Length</param>
        /// <returns>the tangent/direction</returns>
        public virtual Vector3 GetTangentByDistance(float distance)
        {
            return GetTangent(DistanceToTF(distance));
        }

        /// <summary>
        /// Gets the normalized tangent by distance from the spline/group's start. Unlike TangentByDistance() this uses a linear approximation
        /// </summary>
        /// <param name="distance">distance in the range of 0..Length</param>
        /// <returns>the tangent/direction</returns>
        public virtual Vector3 GetTangentByDistanceFast(float distance)
        {
            return GetTangentFast(DistanceToTF(distance));
        }
        /// <summary>
        /// Gets the interpolated position by distance from the spline/group's start
        /// </summary>
        /// <param name="distance">distance in the range of 0..Length</param>
        /// <returns>the interpolated position</returns>
        public virtual Vector3 InterpolateByDistance(float distance)
        {
            return Interpolate(DistanceToTF(distance));
        }

        /// <summary>
        /// Gets the interpolated position by distance from the spline/group's start. Unlike InterpolateByDistance() this uses a linear approximation
        /// </summary>
        /// <param name="distance">distance in the range of 0..Length</param>
        /// <returns>the interpolated position</returns>
        public virtual Vector3 InterpolateByDistanceFast(float distance)
        {
            return InterpolateFast(DistanceToTF(distance));
        }

        /// <summary>
        /// Converts a unit distance into TF-distance by extrapolating the curvation at a given point on the curve
        /// </summary>
        /// <param name="tf">the current TF value</param>
        /// <param name="distance">the distance in world units</param>
        /// <param name="stepSize">stepSize defines the accuracy</param>
        /// <returns>the distance in TF</returns>
        public float ExtrapolateDistanceToTF(float tf, float distance, float stepSize)
        {
            stepSize = Mathf.Max(0.0001f, stepSize);
            Vector3 p = Interpolate(tf);
            float tf1;
            if (tf == 1)
                tf1 = Mathf.Min(1f, tf - stepSize);
            else
                tf1 = Mathf.Min(1f, tf + stepSize);

            stepSize = Mathf.Abs(tf1 - tf);
            Vector3 p1 = Interpolate(tf1);
            float mag = (p1 - p).magnitude;

            if (mag != 0)
            {
                return (1 / mag) * stepSize * distance;
            }
            else return 0;
        }

        /// <summary>
        /// Converts a unit distance into TF-distance by extrapolating the curvation at a given point on the curve using a linear approximation
        /// </summary>
        /// <param name="tf">the current TF value</param>
        /// <param name="distance">the distance in world units</param>
        /// <param name="stepSize">stepSize defines the accuracy</param>
        /// <returns>the distance in TF</returns>
        public float ExtrapolateDistanceToTFFast(float tf, float distance, float stepSize)
        {
            stepSize = Mathf.Max(0.0001f, stepSize);
            Vector3 p = InterpolateFast(tf);
            float tf1;
            if (tf == 1)
                tf1 = Mathf.Min(1f, tf - stepSize);
            else
                tf1 = Mathf.Min(1f, tf + stepSize);

            stepSize = Mathf.Abs(tf1 - tf);
            Vector3 p1 = InterpolateFast(tf1);
            float mag = (p1 - p).magnitude;

            if (mag != 0)
            {
                return (1 / mag) * stepSize * distance;
            }
            else return 0;
        }

        /// <summary>
        /// Destroys the spline/spline group
        /// </summary>
        public void Destroy()
        {
            if (Application.isPlaying)
                GameObject.Destroy(gameObject);
            else
                GameObject.DestroyImmediate(gameObject);
        }

        /// <summary>
        /// Refreshs the spline/group
        /// </summary>
        /// <remarks>This is called automatically on the next Update() if any changes are pending</remarks>
        public virtual void Refresh(){}

        /// <summary>
        /// Ensures the whole spline (curve & orientation) will be recalculated on next call to Refresh()
        /// </summary>
        public virtual void SetDirtyAll()
        {
        }
        
        /// <summary>
        /// Clamps absolute position
        /// </summary>
        public float ClampDistance(float distance, CurvyClamping clamping)
        {
            return CurvyUtility.ClampDistance(distance, clamping, Length);
        }

        /// <summary>
        /// Clamps absolute position
        /// </summary>
        public float ClampDistance(float distance, CurvyClamping clamping, float min, float max)
        {
            return CurvyUtility.ClampDistance(distance, clamping, Length, min, max);
        }

        /// <summary>
        /// Clamps absolute position and sets new direction
        /// </summary>
        public float ClampDistance(float distance, ref int dir, CurvyClamping clamping)
        {
            return CurvyUtility.ClampDistance(distance, ref dir, clamping, Length);
        }

        /// <summary>
        /// Clamps absolute position and sets new direction
        /// </summary>
        public float ClampDistance(float distance, ref int dir, CurvyClamping clamping, float min, float max)
        {
            return CurvyUtility.ClampDistance(distance, ref dir, clamping, Length, min, max);
        }


        #endregion

        #region ### Privates & internal Publics ###
        /*! \cond PRIVATE */

        /*! \endcond */
        #endregion

        #region ### Obsolete ###
        /*! \cond OBSOLETE */

        /*! \endcond */
        #endregion
    }
}
