// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using System.Collections.Generic;
using FluffyUnderware.Curvy;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Connection component
    /// </summary>
    [ExecuteInEditMode]
    [HelpURL(CurvySpline.DOCLINK + "curvyconnection")]
    public class CurvyConnection : MonoBehaviour
    {
        #region ### Serialized Fields ###

        [SerializeField,Hide]
        List<CurvySplineSegment> m_ControlPoints = new List<CurvySplineSegment>();

        #endregion

        #region ### Public Properties ###

        /// <summary>
        /// Gets all Control Points being part of this connection
        /// </summary>
        public List<CurvySplineSegment> ControlPoints
        {
            get
            {
                return m_ControlPoints;
            }
        }
        /// <summary>
        /// Gets the number of Control Points being part of this connection
        /// </summary>
        public int Count
        {
            get { return m_ControlPoints.Count; }
        }

        /// <summary>
        /// Gets a certain Control Point by index
        /// </summary>
        /// <param name="idx">index of the Control Point</param>
        /// <returns>a Control Point</returns>
        public CurvySplineSegment this[int idx]
        {
            get
            {
                return m_ControlPoints[idx];
            }
        }

        /// <summary>
        /// Gets Transform (Threadsafe)
        /// </summary>
        public TTransform TTransform
        {
            get
            {
                if (!mTTransform)
                    mTTransform = new TTransform(transform);
                return mTTransform;
            }
        }

        #endregion

        #region ### Private Fields & Properties ###

        TTransform mTTransform;

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */
        
        void OnDestroy()
        {
            bool realDestroy = true;
#if UNITY_EDITOR
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                realDestroy = false;
#endif
            if (realDestroy)
            {
                foreach (var cp in ControlPoints)
                {
                    cp.Connection = null;
                    cp.Disconnect();
                }
                ControlPoints.Clear();
            }
        }

        
        public void Update()
        {
            bool syncPos = TTransform.position != transform.position;
            bool syncRot = TTransform.rotation != transform.rotation;
            if (syncPos || syncRot)
            {
                SynchronizeINTERNAL(transform);
            }
        }
        /*! \endcond */
        #endregion

        #region ### Public Methods ###
        
        /// <summary>
        /// Creates a connection and adds Control Points
        /// </summary>
        /// <param name="controlPoints">Control Points to add</param>
        /// <returns>the new connection</returns>
        public static CurvyConnection Create(params CurvySplineSegment[] controlPoints)
        {
            var con = CurvyGlobalManager.Instance.AddChildGameObject<CurvyConnection>("Connection");
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RegisterCreatedObjectUndo(con.gameObject, "Add Connection");
#endif
            if (!con)
                return null;
            if (controlPoints.Length > 0)
            {
                con.transform.position = controlPoints[0].transform.position;
                con.TTransform.FromTransform(con.transform);
                con.AddControlPoints(controlPoints);
            }

            return con;
        }

        /// <summary>
        /// Adds Control Points to this connection
        /// </summary>
        /// <param name="controlPoints">the Control Points to add</param>
        public void AddControlPoints(params CurvySplineSegment[] controlPoints)
        {
            foreach (var cp in controlPoints)
                addControlPoint(cp);
            AutoSetFollowUp();
        }

        public void AutoSetFollowUp()
        {
            if (Count == 2 && ControlPoints[0].position==ControlPoints[1].position && ControlPoints[0].ConnectionSyncPosition && ControlPoints[1].ConnectionSyncPosition)
            {
                if (ControlPoints[0].FollowUp==null && ControlPoints[0].CanHaveFollowUp)
                    ControlPoints[0].SetFollowUp(ControlPoints[1]);
                if (ControlPoints[1].FollowUp == null && ControlPoints[1].CanHaveFollowUp)
                    ControlPoints[1].SetFollowUp(ControlPoints[0]);
            }
        }

        /// <summary>
        /// Removes a Control Point from this connection
        /// </summary>
        /// <param name="controlPoint">the Control Point to remove</param>
        /// <param name="destroySelfIfEmpty">whether the connection should be destroyed when empty afterwards</param>
        public void RemoveControlPoint(CurvySplineSegment controlPoint, bool destroySelfIfEmpty = true)
        {
            controlPoint.Connection = null;
            ControlPoints.Remove(controlPoint);
            if (ControlPoints.Count == 0 && destroySelfIfEmpty)
                Delete();
        }

        /// <summary>
        /// Deletes the connection
        /// </summary>
        public void Delete()
        {
            if (Application.isPlaying)
            {
                GameObject.Destroy(this.gameObject);
            }
#if UNITY_EDITOR
            else
                Undo.DestroyObjectImmediate(this.gameObject);
#endif
        }

        /// <summary>
        /// Gets all Control Points except the one provided
        /// </summary>
        /// <param name="source">the Control Point to filter out</param>
        /// <returns>list of Control Points</returns>
        public List<CurvySplineSegment> OtherControlPoints(CurvySplineSegment source)
        {
            var res = new List<CurvySplineSegment>(ControlPoints);
            res.Remove(source);
            return res;
        }

        #endregion

        #region ### Privates & Internals ###
        /*! \cond PRIVATE */

        void addControlPoint(CurvySplineSegment controlPoint)
        {
            if (!controlPoint.Connection)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.RecordObject(controlPoint, "Add Connection");
#endif
                m_ControlPoints.Add(controlPoint);
                controlPoint.Connection = this;
            }
        }

        void synchronize()
        {
            for (int i = 0; i < ControlPoints.Count; i++)
            {
#if UNITY_EDITOR
                Undo.RecordObject(ControlPoints[i].transform, Undo.GetCurrentGroupName());
#endif
                if (ControlPoints[i].ConnectionSyncPosition)
                    ControlPoints[i].transform.position = transform.position;
                if (ControlPoints[i].ConnectionSyncRotation)
                    ControlPoints[i].transform.rotation = transform.rotation;
                ControlPoints[i].RefreshTransform(false);
            }
            TTransform.FromTransform(transform);
        }

        public void SynchronizeINTERNAL(Transform tform)
        {
            if (tform != transform)
            {
#if UNITY_EDITOR
                Undo.RecordObject(transform, Undo.GetCurrentGroupName());
#endif
                transform.position = tform.position;
                transform.rotation = tform.rotation;

            }

            synchronize();
        }

        /*! \endcond */
        #endregion

 
    }

}
