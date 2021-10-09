// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FluffyUnderware.DevTools;
using FluffyUnderware.Curvy.Utils;

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Curvy SplineGroup class
    /// </summary>
    [ExecuteInEditMode]
    public class CurvySplineGroup : CurvySplineBase
    {
        #region ### Serialized Fields ###

        
        [SerializeField]
        [ArrayEx(ShowAdd=false)]
        List<CurvySpline> m_Splines = new List<CurvySpline>();

        #endregion

        #region ### Public Properties ###

        /// <summary>
        /// Gets Splines contained in this SplineGroup
        /// </summary>
        public List<CurvySpline> Splines
        {
            get
            {
                return m_Splines;
            }
            set
            {
                m_Splines = Splines;
                SetDirtyAll();
            }
        }

        /// <summary>
        /// Gets Bounds of this SplineGroup
        /// </summary>
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
        /// Distance in world units of each spline in the group, starting from the first spline's segment
        /// </summary>
        public float[] Distances { get; private set; }

        /// <summary>
        /// Number of splines in the SplineGroup
        /// </summary>
        public override int Count { get { return Splines.Count; } }

        /// <summary>
        /// Gets a spline from the group
        /// </summary>
        /// <param name="idx">index of the spline</param>
        /// <returns>a Spline</returns>
        public CurvySpline this[int idx]
        {
            get
            {
                return (idx > -1 && idx < Splines.Count) ? Splines[idx] : null;
            }
        }

        /// <summary>
        /// Gets whether the splines in the group form a continuous curve
        /// </summary>
        public override bool IsContinuous
        {
            get
            {
                return mIsContinuous;
            }
        }

        /// <summary>
        /// Gets whether the splines in the group form a continuous closed curve
        /// </summary>
        public override bool IsClosed
        {
            get
            {
                return mIsContinuous && mIsClosed;
            }
        }

        public override bool Dirty
        {
            get
            {
                return mDirty;
            }
        }


        #endregion

        #region ### Private Fields ###

        bool mIsContinuous;
        bool mIsClosed;
        bool mDirty;
        Bounds? mBounds;

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

        IEnumerator Start()
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i] != null)
                    while (!this[i].IsInitialized)
                        yield return new WaitForEndOfFrame();
            }
            Refresh();
        }

        protected override void OnEnable()
        {
            if (!Application.isPlaying)
                SetDirtyAll();
        }

        void OnDisable()
        {
            mIsInitialized = false;
            for (int i = 0; i < Count; i++)
                if (this[i] != null)
                    this[i].OnRefresh.RemoveListener(OnSplineRefresh);

        }

        void Update()
        {
            if (Dirty)
                Refresh();
        }

        public void Reset()
        {
            Clear();
        }



        /*! \endcond */
        #endregion

        #region ### Public Static Methods ###

        /// <summary>
        /// Creates a new GameObject with a CurvySplineGroup
        /// </summary>
        /// <param name="splines">the splines to add to the group</param>
        /// <returns>the new CurvySplineGroup component</returns>
        public static CurvySplineGroup Create(params CurvySpline[] splines)
        {
            CurvySplineGroup grp = new GameObject("Curvy Spline Group", typeof(CurvySplineGroup)).GetComponent<CurvySplineGroup>();
            grp.Add(splines);
            return grp;
        }

        #endregion

        #region ### Public Methods ###

        public override void SetDirtyAll()
        {
            mDirty = true;
        }

        /// <summary>
        /// Gets the interpolated position for a certain group TF
        /// </summary>
        /// <remarks>TF (Total Fragment) relates to the total length of the group</remarks>
        /// <param name="tf">TF value identifying position in the group (0..1)</param>
        /// <returns>the interpolated position</returns>
        public override Vector3 Interpolate(float tf)
        {
            float localTF;
            CurvySpline spl = TFToSpline(tf, out localTF);

            return (spl) ? spl.Interpolate(localTF) : Vector3.zero;
        }

        /// <summary>
        /// Gets the interpolated position for a certain group TF using a linear approximation
        /// </summary>
        /// <remarks>TF (Total Fragment) relates to the total length of the group</remarks>
        /// <param name="tf">TF value reflecting position in the group (0..1)</param>
        /// <returns>the interpolated position</returns>
        public override Vector3 InterpolateFast(float tf)
        {
            float localTF;
            CurvySpline spl = TFToSpline(tf, out localTF);

            return (spl) ? spl.InterpolateFast(localTF) : Vector3.zero;
        }

        /// <summary>
        /// Gets an interpolated User Value for a certain group TF
        /// </summary>
        /// <remarks>TF (Total Fragment) relates to the total length of the group</remarks>
        /// <param name="tf">TF value reflecting position in the group (0..1)</param>
        /// <param name="index">the UserValue array index</param>
        /// <returns>the interpolated value</returns>
        public override Vector3 InterpolateUserValue(float tf, int index)
        {
            float localTF;
            CurvySpline spl = TFToSpline(tf, out localTF);
            return (spl) ? spl.InterpolateUserValue(localTF, index) : Vector3.zero;
        }

        /// <summary>
        /// Gets an interpolated Scale for a certain group TF
        /// </summary>
        /// <remarks>TF (Total Fragment) relates to the total length of the group</remarks>
        /// <param name="tf">TF value reflecting position in the group(0..1)</param>
        /// <returns>the interpolated value</returns>
        public override Vector3 InterpolateScale(float tf)
        {
            float localTF;
            CurvySpline spl = TFToSpline(tf, out localTF);
            return (spl) ? spl.InterpolateScale(localTF) : Vector3.zero;
        }

        /// <summary>
        /// Gets the Up-Vector for a certain TF based on the splines' Orientation mode
        /// </summary>
        /// <param name="tf">TF value reflecting position in the group (0..1)</param>
        /// <returns>the Up-Vector</returns>
        public override Vector3 GetOrientationUpFast(float tf)
        {
            float localTF;
            CurvySpline spl = TFToSpline(tf, out localTF);

            return (spl) ? spl.GetOrientationUpFast(localTF) : Vector3.zero;
        }

        /// <summary>
        /// Gets a rotation looking to Tangent with the head upwards along the Up-Vector
        /// </summary>
        /// <param name="tf">TF value reflecting position in the group (0..1)</param>
        /// <param name="inverse">whether the orientation should be inversed or not</param>
        /// <returns>a rotation</returns>
        public override Quaternion GetOrientationFast(float tf, bool inverse)
        {
            float localTF;
            CurvySpline spl = TFToSpline(tf, out localTF);
            return (spl) ? spl.GetOrientationFast(localTF, inverse) : Quaternion.identity;
        }

        /// <summary>
        /// Gets the normalized tangent for a certain TF
        /// </summary>
        /// <param name="tf">TF value identifying position in the group (0..1)</param>
        /// <returns>a tangent vector</returns>
        public override Vector3 GetTangent(float tf)
        {
            float localTF;
            CurvySpline spl = TFToSpline(tf, out localTF);
            return (spl) ? spl.GetTangent(localTF) : Vector3.zero;
        }

        /// <summary>
        /// Gets the normalized tangent for a certain TF with a known position for TF
        /// </summary>
        /// <remarks>This saves one interpolation</remarks>
        /// <param name="tf">TF value identifying position in the group (0..1)</param>
        /// <param name="position">The interpolated position for TF</param>
        /// <returns>a tangent vector</returns>
        public override Vector3 GetTangent(float tf, Vector3 position)
        {
            float localTF;
            CurvySpline spl = TFToSpline(tf, out localTF);
            return (spl) ? spl.GetTangent(localTF, position) : Vector3.zero;
        }

        /// <summary>
        /// Gets the normalized tangent for a certain TF using a linear approximation
        /// </summary>
        /// <param name="tf">TF value identifying position in the group (0..1)</param>
        /// <returns>a tangent vector</returns>
        public override Vector3 GetTangentFast(float tf)
        {
            float localTF;
            CurvySpline spl = TFToSpline(tf, out localTF);
            return (spl) ? spl.GetTangentFast(localTF) : Vector3.zero;
        }

        /// <summary>
        /// Alter TF to reflect a movement over a certain distance.
        /// </summary>
        /// <remarks>MoveByLengthFast is used internally, so stepSize is ignored</remarks>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="distance">the distance in world units to move</param>
        /// <param name="clamping">clamping mode</param>
        /// <param name="stepSize">stepSize defines the accuracy</param>
        /// <returns>the interpolated position</returns>
        public override Vector3 MoveBy(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize)
        {
            return MoveByLengthFast(ref tf, ref direction, distance, clamping);
        }

        /// <summary>
        /// Alter TF to reflect a movement over a certain distance.
        /// </summary>
        /// <remarks>MoveByLengthFast is used internally, so stepSize is ignored</remarks>
        /// <param name="tf">the current TF value</param>
        /// <param name="direction">the current direction, 1 or -1</param>
        /// <param name="distance">the distance in world units to move</param>
        /// <param name="clamping">clamping mode</param>
        /// <param name="stepSize">stepSize defines the accuracy</param>
        /// <returns>the interpolated position</returns>
        public override Vector3 MoveByFast(ref float tf, ref int direction, float distance, CurvyClamping clamping, float stepSize)
        {
            return MoveByLengthFast(ref tf, ref direction, distance, clamping);
        }
       
        /// <summary>
        /// Converts a group TF value to a group distance
        /// </summary>
        /// <param name="tf">a TF value in the range 0..1</param>
        /// <param name="clamping">Clamping to use</param>
        /// <returns>distance from the first spline's first segment's Control Point</returns>
        public override float TFToDistance(float tf, CurvyClamping clamping)
        {
            if (Count == 0)
                return 0;
            float localTF;
            int idx = TFToSplineIndex(tf, out localTF);
            return Distances[idx] + this[idx].TFToDistance(localTF);
        }

        /// <summary>
        /// Gets the spline a certan TF lies in
        /// </summary>
        /// <param name="tf">the TF value in the range 0..1</param>
        /// <returns>the spline the given group TF is inside</returns>
        public CurvySpline TFToSpline(float tf)
        {
            float localTF;
            int idx = TFToSplineIndex(tf, out localTF);
            return (idx == -1) ? null : this[idx];
        }

        /// <summary>
        /// Gets the spline and the spline's TF for a certain group TF
        /// </summary>
        /// <param name="tf">the TF value in the range 0..1</param>
        /// <param name="localTF">gets the remaining TF in the range 0..1</param>
        /// <returns>the spline the given group TF is inside</returns>
        public CurvySpline TFToSpline(float tf, out float localTF)
        {
            int idx = TFToSplineIndex(tf, out localTF);
            return (idx == -1) ? null : this[idx];
        }



        /// <summary>
        /// Gets a TF value from a group's spline and a spline's TF
        /// </summary>
        /// <param name="spline">a spline of this group</param>
        /// <param name="splineTF">TF of the spline in the range 0..1</param>
        /// <returns>a group TF value in the range 0..1</returns>
        public float SplineToTF(CurvySpline spline, float splineTF)
        {
            if (Count == 0)
                return 0;
            return ((float)Splines.IndexOf(spline) / Count) + (1f / Count) * splineTF;
        }

        
        /// <summary>
        /// Gets the segment and the local F for a certain TF
        /// </summary>
        /// <param name="tf">the TF value in the range 0..1</param>
        /// <param name="localF">gets the remaining localF in the range 0..1</param>
        /// <param name="clamping">Clamping to use</param>
        /// <returns>the segment the given TF is inside</returns>
        public override CurvySplineSegment TFToSegment(float tf, out float localF, CurvyClamping clamping)
        {
            float localTF;
            localF = 0;
            int idx = TFToSplineIndex(tf, out localTF, clamping);
            return (idx == -1) ? null : this[idx].TFToSegment(localTF, out localF);
        }
        /// <summary>
        /// Gets a TF value from a segment
        /// </summary>
        /// <param name="segment">a segment</param>
        /// <returns>a TF value in the range 0..1</returns>
        public override float SegmentToTF(CurvySplineSegment segment)
        {
            return SplineToTF(segment.Spline, 0);
        }
        /// <summary>
        /// Gets a TF value from a segment and a local F
        /// </summary>
        /// <param name="segment">a segment</param>
        /// <param name="localF">F of this segment in the range 0..1</param>
        /// <returns>a TF value in the range 0..1</returns>
        public override float SegmentToTF(CurvySplineSegment segment, float localF)
        {
            float splineTF = segment.LocalFToTF(localF);
            return SplineToTF(segment.Spline, splineTF);
        }

        /// <summary>
        /// Converts a distance to a TF value
        /// </summary>
        /// <param name="distance">distance in the range 0..Length</param>
        /// <param name="clamping">Clamping to use</param>
        /// <returns>a TF value in the range 0..1</returns>
        public override float DistanceToTF(float distance, CurvyClamping clamping)
        {
            float localDistance;
            CurvySpline spl = DistanceToSpline(distance, out localDistance,clamping);
            return (spl) ? SplineToTF(spl, spl.DistanceToTF(localDistance)) : 0;
        }

        /// <summary>
        /// Gets the spline a certain distance lies within
        /// </summary>
        /// <param name="distance">a distance in the range 0..Length</param>
        /// <param name="clamping">Clamping to use</param>
        /// <returns>a spline or null</returns>
        public CurvySpline DistanceToSpline(float distance, CurvyClamping clamping=CurvyClamping.Clamp)
        {
            float d;
            return DistanceToSpline(distance, out d, clamping);
        }

        /// <summary>
        /// Gets the spline a certain distance lies within
        /// </summary>
        /// <param name="distance">a distance in the range 0..Length</param>
        /// <param name="localDistance">gets the remaining distance inside the spline</param>
        /// <param name="clamping">Clamping to use</param>
        /// <returns>a spline or null</returns>
        public CurvySpline DistanceToSpline(float distance, out float localDistance, CurvyClamping clamping=CurvyClamping.Clamp)
        {
            distance = CurvyUtility.ClampDistance(distance, clamping, Length);
            localDistance = 0;

            for (int i = 1; i < Count; i++)
            {
                if (Distances[i] >= distance)
                {
                    localDistance = distance - Distances[i - 1];
                    return this[i - 1];
                }
            }
            localDistance = distance - Distances[Count - 1];
            return this[Count - 1];
        }

        /// <summary>
        /// Gets an array containing all approximation points
        /// </summary>
        /// <param name="space">whether to use local or global space</param>
        /// <remarks>This can be used to feed meshbuilders etc...</remarks>
        /// <returns>an array of world/local positions</returns>
        public override Vector3[] GetApproximation(Space space=Space.Self)
        {

            List<Vector3[]> ap = new List<Vector3[]>(Count);

            int n = 0;
            bool con;
            for (int i = 0; i < Count; i++)
            {
                Vector3[] p = this[i].GetApproximation(space);
                con = NextSplineConnected(i);
                if (con)
                {
                    System.Array.Resize<Vector3>(ref p, p.Length - 1);
                }
                ap.Add(p);
                n += ap[i].Length;
            }

            Vector3[] apps = new Vector3[n];
            n = 0;
            for (int i = 0; i < Count; i++)
            {
                ap[i].CopyTo(apps, n);
                n += ap[i].Length;
            }
            return apps;
        }
      
        /// <summary>
        /// Gets an array containing all approximation tangent points
        /// </summary>
        /// <returns>an array of normalized tangent directions</returns>
        public override Vector3[] GetApproximationT()
        {
            List<Vector3[]> ap = new List<Vector3[]>(Count);

            int n = 0;
            bool con;
            for (int i = 0; i < Count; i++)
            {
                Vector3[] p = this[i].GetApproximationT();
                con = NextSplineConnected(i);
                if (con)
                {
                    System.Array.Resize<Vector3>(ref p, p.Length - 1);
                }
                ap.Add(p);
                n += ap[i].Length;
            }

            Vector3[] apps = new Vector3[n];
            n = 0;
            for (int i = 0; i < Count; i++)
            {
                ap[i].CopyTo(apps, n);
                n += ap[i].Length;
            }

            return apps;
        }
        /// <summary>
        /// Gets an array containing all approximation Up-Vectors
        /// </summary>
        /// <returns>an array of Up-Vectors</returns>
        public override Vector3[] GetApproximationUpVectors()
        {
            List<Vector3[]> ap = new List<Vector3[]>(Count);

            int n = 0;
            bool con;
            for (int i = 0; i < Count; i++)
            {
                Vector3[] p = this[i].GetApproximationUpVectors();
                con = NextSplineConnected(i);
                if (con)
                {
                    System.Array.Resize<Vector3>(ref p, p.Length - 1);
                }
                ap.Add(p);
                n += ap[i].Length;
            }

            Vector3[] apps = new Vector3[n];
            n = 0;
            for (int i = 0; i < Count; i++)
            {
                ap[i].CopyTo(apps, n);
                n += ap[i].Length;
            }

            return apps;
        }

        /// <summary>
        /// Gets the TF value that is nearest to p for a given set of segments
        /// </summary>
        /// <param name="p">a point in space</param>
        /// <remarks>The speed as well as the accuracy depends on the Granularity</remarks>
        /// <returns>a TF value in the range 0..1 or -1 on error</returns>
        public override float GetNearestPointTF(Vector3 p)
        {
            Vector3 n;
            return GetNearestPointTF(p, out n);
        }

        /// <summary>
        /// Gets the TF value that is nearest to p for a given set of segments
        /// </summary>
        /// <param name="p">a point in space</param>
        /// <param name="nearest">returns the nearest position</param>
        /// <remarks>The speed as well as the accuracy depends on the Granularity</remarks>
        /// <returns>a TF value in the range 0..1 or -1 on error</returns>
        public override float GetNearestPointTF(Vector3 p, out Vector3 nearest)
        {
            nearest = Vector3.zero;
            float resTF = -1;
            float maxDist2 = float.MaxValue;
            int res = -1;
            for (int i = 0; i < Count; i++)
            {
                Vector3 v;
                float tf = this[i].GetNearestPointTF(p,out v);
                if (tf > -1)
                {
                    float dist2 = (p - this[i].Interpolate(tf)).sqrMagnitude;
                    if (dist2 < maxDist2)
                    {
                        res = i;
                        resTF = tf;
                        nearest = v;
                        maxDist2 = dist2;
                    }
                }
            }
            return (res>-1) ? SplineToTF(this[res], resTF) : -1;
        }

              /// <summary>
        /// Gets metadata for a certain TF
        /// </summary>
        /// <param name="type">Metadata type interfacing ICurvyMetadata</param>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the metadata</returns>
        public override Component GetMetadata(System.Type type, float tf)
        {
            float localTF;
            CurvySpline spl = TFToSpline(tf, out localTF);
            return (spl) ? spl.GetMetadata(type, localTF) : null;
        }

        // <summary>
        /// Gets an interpolated Metadata value for a certain TF
        /// </summary>
        /// <typeparam name="T">Metadata type interfacing ICurvyInterpolatableMetadata</typeparam>
        /// <typeparam name="U">Return Value type of T</typeparam>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the interpolated value</returns>
        public override U InterpolateMetadata<T, U>(float tf)
        {
            float localTF;
            CurvySpline spl = TFToSpline(tf, out localTF);
            return (spl) ? spl.InterpolateMetadata<T,U>(localTF) : default(U);
        }

        /// <summary>
        /// Gets an interpolated Metadata value for a certain TF
        /// </summary>
        /// <param name="type">Metadata type interfacing ICurvyInterpolatableMetadata</param>
        /// <param name="tf">TF value reflecting position on spline (0..1)</param>
        /// <returns>the interpolated value</returns>
        public override object InterpolateMetadata(System.Type type, float tf)
        {
            float localTF;
            CurvySpline spl = TFToSpline(tf, out localTF);
            return (spl) ? spl.InterpolateMetadata(type, localTF) : null;
        }

        /// <summary>
        /// Add splines to the group
        /// </summary>
        /// <param name="splines">splines to add</param>
        public void Add(params CurvySpline[] splines)
        {
            Splines.AddRange(splines);
            SetDirtyAll();
        }

        /// <summary>
        /// Remove a spline from the group
        /// </summary>
        /// <param name="spline">the spline to remove</param>
        public void Delete(CurvySpline spline)
        {
            Splines.Remove(spline);
            SetDirtyAll();
        }

        /// <summary>
        /// Remove all splines from the group
        /// </summary>
        public override void Clear()
        {
            Splines.Clear();
            SetDirtyAll();
        }

            /// <summary>
        /// Refreshes the SplineGroup
        /// </summary>
        /// <remarks>This is called automatically during Update, so usually you don't need to call it manually</remarks>
        public override void Refresh()
        {
            RemoveEmptySplines();

            // ensure all splines are initialized
            for (int i = 0; i < Count; i++)
                if (!this[i].IsInitialized)
                    return;

            mIsInitialized = Count > 0;
            if (mIsInitialized)
            {
                for (int i = 0; i < Count; i++)
                    this[i].OnRefresh.AddListenerOnce(OnSplineRefresh);
                doRefreshLength();
                doGetProperties();
                OnRefreshEvent(new CurvySplineEventArgs(this));
            }
        }

        
        /// <summary>
        /// Remove empty entries from the Splines array
        /// </summary>
        public void RemoveEmptySplines()
        {
            if (Splines.Count > 0)
            {
                for (int i = Splines.Count - 1; i > -1; i--)
                    if (Splines[i] == null)
                        Splines.RemoveAt(i);
            }
        }

        #endregion
    
        #region ### Privates & internal Publics ###
        /*! \cond PRIVATE */

        Bounds getBounds()
        {
            Bounds b = new Bounds();
            for (int i = 0; i < Count; i++)
                b.Encapsulate(this[i].Bounds);
            return b;
        }

        //refresh length only
        void doRefreshLength()
        {
            mLength = 0;
            Distances = new float[Count];
            for (int i = 0; i < Count; i++)
            {
                Distances[i] = mLength;
                mLength += this[i].Length;
            }
            
        }

        /// <summary>
        /// Gets whether the next spline's (idx+1) start equals current spline's (idx) end
        /// </summary>
        /// <param name="idx">index of the current spline</param>
        /// <returns>true if end and start share the same position</returns>
        bool NextSplineConnected(int idx)
        {
            idx = Mathf.Clamp(idx, 0, Count - 1);
            int n = (idx == Count - 1) ? 0 : idx + 1;
            var idxn=this[idx].NextSpline;

            return (idxn && idxn == this[n]);
            /*
            Mathf.Clamp(idx, 0, Count - 1);
            int n = (idx == Count - 1) ? 0 : idx + 1;
            return (idx < Count - 1 && Mathf.Abs((this[idx].InterpolateFast(1) - this[n].InterpolateFast(0)).sqrMagnitude) <= float.Epsilon * float.Epsilon);
             */
        }

        void OnSplineRefresh(CurvySplineEventArgs e)
        {
            if (!Splines.Contains(e.Spline))
            {
                e.Spline.OnRefresh.RemoveListener(OnSplineRefresh);
                return;
            }
            if (!mIsInitialized)
                return;
            doRefreshLength();
            doGetProperties();
            
            OnRefreshEvent(new CurvySplineEventArgs(this));
        }

        void doGetProperties() 
        {
            mIsContinuous = true;
            for (int i = 0; i < Count - 2; i++)
                if (!NextSplineConnected(i))
                {
                    mIsContinuous = false;
                    break;
                }
            mIsClosed = (Count > 1 && NextSplineConnected(Count - 1));
        }

        int TFToSplineIndex(float tf, out float localTF, CurvyClamping clamping=CurvyClamping.Clamp)
        {
            tf = CurvyUtility.ClampTF(tf, clamping);
            localTF = 0;
            if (Count == 0) return -1;
            float f = tf * Count;
            int idx = (int)f;
            localTF = f - idx;
            if (idx == Count)
            {
                idx--; localTF = 1;
            }

            return idx;
        }

        /*! \endcond */
        #endregion
    }
}
