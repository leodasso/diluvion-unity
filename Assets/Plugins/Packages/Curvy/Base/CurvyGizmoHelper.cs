// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Curvy Gizmo Helper Class
    /// </summary>
    public static class CurvyGizmoHelper
    {
        public static Matrix4x4 Matrix = Matrix4x4.identity;

        public static void SegmentCurveGizmo(CurvySplineSegment seg, Color col, float stepSize=0.05f)
        {
            
            Matrix4x4 mat = Gizmos.matrix;
            Gizmos.matrix = Matrix;
            Gizmos.color = col;
            
            if (seg.Spline.Interpolation == CurvyInterpolation.Linear)
            {
                Gizmos.DrawLine(seg.Interpolate(0), seg.Interpolate(1));
                return;
            }
            Vector3 p = seg.Interpolate(0);
            for (float f = stepSize; f < 1; f += stepSize)
            {
                Vector3 p1 = seg.Interpolate(f);
                Gizmos.DrawLine(p, p1);
                p = p1;
            }
            Gizmos.DrawLine(p, seg.Interpolate(1));
            
            Gizmos.matrix = mat;
        }

        public static void SegmentApproximationGizmo(CurvySplineSegment seg, Color col)
        {
            Matrix4x4 mat = Gizmos.matrix;
            Gizmos.matrix = Matrix;
            Gizmos.color = col;
            Vector3 size = new Vector3(0.1f/seg.Spline.transform.localScale.x,
                                       0.1f/seg.Spline.transform.localScale.y,
                                       0.1f/seg.Spline.transform.localScale.z);
            
            for (int i=0;i<seg.Approximation.Length;i++){
                Vector3 p=seg.Approximation[i];
                Gizmos.DrawCube(p, DTUtility.GetHandleSize(p)*size);
            }
            Gizmos.matrix = mat;
        }

        public static void SegmentOrientationAnchorGizmo(CurvySplineSegment seg, Color col)
        {
            if (seg.ApproximationUp.Length == 0)
                return;
            Matrix4x4 mat = Gizmos.matrix;
            Gizmos.matrix = Matrix;
            Gizmos.color = col;
            Vector3 scl = new Vector3(1 / seg.Spline.transform.localScale.x,
                                      1 / seg.Spline.transform.localScale.y,
                                      1 / seg.Spline.transform.localScale.z);

            Vector3 u = seg.ApproximationUp[0];
            u.Set(u.x * scl.x, u.y * scl.y, u.z * scl.z);
            Gizmos.DrawRay(seg.Approximation[0], u * CurvyGlobalManager.GizmoOrientationLength * 1.75f);
            Gizmos.matrix = mat;
        }

        public static void SegmentOrientationGizmo(CurvySplineSegment seg, Color col)
        {
            Matrix4x4 mat = Gizmos.matrix;
            Gizmos.matrix = Matrix;
            Gizmos.color = col;
            Vector3 scl = new Vector3(1 / seg.Spline.transform.localScale.x,
                                      1 / seg.Spline.transform.localScale.y,
                                      1 / seg.Spline.transform.localScale.z);
            
            Vector3 u;
            for (int i = 0; i < seg.ApproximationUp.Length; i++)
            {
                u = seg.ApproximationUp[i];
                u.Set(u.x * scl.x, u.y * scl.y, u.z * scl.z);
                Gizmos.DrawRay(seg.Approximation[i], u *  CurvyGlobalManager.GizmoOrientationLength);
            }
            Gizmos.matrix = mat;
        }

        public static void SegmentTangentGizmo(CurvySplineSegment seg, Color col)
        {
            Matrix4x4 mat = Gizmos.matrix;
            Gizmos.matrix = Matrix;
            Gizmos.color = col;
            
            for (int i = 0; i < seg.ApproximationT.Length; i++)
            {
                Gizmos.color = (i==seg.CacheSize) ? Color.black : (i==0) ? Color.blue : col;
                Vector3 p = seg.Approximation[i];
                Vector3 t = seg.ApproximationT[i].normalized;
                Gizmos.DrawRay(p, t * CurvyGlobalManager.GizmoOrientationLength);
            }
            Gizmos.matrix = mat;
        }
       
        public static void ControlPointGizmo (CurvySplineSegment cp, bool selected, Color col)
        {
            Matrix4x4 mat = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.identity;
            
            Gizmos.color = col;
            Vector3 p = Matrix.MultiplyPoint(cp.transform.localPosition);
            float size = (selected) ? 1 : 0.7f;

            if (cp.Spline.RestrictTo2D)
                Gizmos.DrawCube(p, Vector3.one * DTUtility.GetHandleSize(p) * size * CurvyGlobalManager.GizmoControlPointSize);
            else
                Gizmos.DrawSphere(p, DTUtility.GetHandleSize(p) * size * CurvyGlobalManager.GizmoControlPointSize);

            Gizmos.matrix = mat;
        }
        
        public static void ConnectionGizmo(CurvySplineSegment cp)
        {
            Matrix4x4 mat = Gizmos.matrix;
            Gizmos.matrix = Matrix;
            
            Color c = Color.black;
            if (cp.ConnectionSyncPosition)
            {
                if (cp.ConnectionSyncRotation)
                    c = Color.white;
                else
                    c = Color.red;
            }
            else
                if (cp.ConnectionSyncRotation)
                    c = new Color(1, 1, 0);

            Gizmos.color = c;
            Vector3 p = cp.transform.localPosition;
            Gizmos.DrawWireSphere(p, DTUtility.GetHandleSize(p) * CurvyGlobalManager.GizmoControlPointSize*1.3f);
            Gizmos.matrix = mat;
        }

        public static void BoundsGizmo(CurvySplineSegment cp, Color col)
        {
            Matrix4x4 mat = Gizmos.matrix;
            Gizmos.matrix = Matrix;

            Gizmos.color = col;
            Gizmos.DrawWireCube(cp.Bounds.center, cp.Bounds.size);
            Gizmos.matrix = mat;
        }

    }
}
