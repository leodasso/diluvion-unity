// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;

namespace FluffyUnderware.Curvy
{
    [ExecuteInEditMode]
    public class CurvyMetadataBase : MonoBehaviour
    {

        
        #region ### Serialized Fields ###
        #endregion

        #region ### Public Properties ###

        public CurvySplineSegment ControlPoint
        {
            get { return mCP; }
        }

        public CurvySpline Spline
        {
            get
            {
                return (mCP) ? mCP.Spline : null;
            }
        }

        #endregion

        #region ### Private Fields & Properties ###

        CurvySplineSegment mCP;

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

        protected virtual void Awake()
        {
            mCP = GetComponent<CurvySplineSegment>();
        }

        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        public T GetPreviousData<T>(bool autoCreate=true,bool segmentsOnly = true, bool useFollowUp = false) where T:MonoBehaviour,ICurvyMetadata
        {
            if (ControlPoint)
            {

                var prev = ControlPoint.GetPreviousControlPoint(segmentsOnly, useFollowUp);
                if (prev)
                    return prev.GetMetadata<T>(autoCreate);
            }
            return default(T);
        }

        public T GetNextData<T>(bool autoCreate = true, bool segmentsOnly = true, bool useFollowUp = false) where T:MonoBehaviour,ICurvyMetadata
        {
            if (ControlPoint)
            {

                var next = ControlPoint.GetNextControlPoint(segmentsOnly, useFollowUp);
                if (next)
                    return next.GetMetadata<T>(autoCreate);
            }
            return default(T);
        }

        public void SetDirty()
        {
            if (ControlPoint)
                ControlPoint.SetDirty(true, true);
        }

        #endregion

        #region ### Privates ###
        /*! \cond PRIVATES */


        /*! \endcond */
        #endregion

    }
}
