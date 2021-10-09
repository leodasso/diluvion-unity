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
    /// <summary>
    /// Spline component that fits perfectly to uGUI Canvas
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Curvy/Curvy UI Spline",2)]
    public class CurvyUISpline : CurvySpline
    {
        /// <summary>
        /// Creates a GameObject with a CurvyUISpline attached
        /// </summary>
        /// <returns>the component</returns>
        public static CurvyUISpline CreateUISpline()
        {
            CurvyUISpline spl = new GameObject("Curvy UI Spline", typeof(CurvyUISpline)).GetComponent<CurvyUISpline>();
            spl.RestrictTo2D = true;
            spl.Orientation = CurvyOrientation.None;
            return spl;
        }
    
    }
}
