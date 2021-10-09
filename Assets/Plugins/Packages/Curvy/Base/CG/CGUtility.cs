// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// Curvy Generator Utility class
    /// </summary>
    public static class CGUtility
    {
        /// <summary>
        /// Given a number of lengths, returns the maximum CacheDensity
        /// </summary>
        /// <param name="density">density value</param>
        /// <param name="pathlengths"></param>
        public static int CalculateSamplePointsCacheSize(int density, params float[] pathlengths)
        {
            density = Mathf.Clamp(density, 0, 100);
            int size = 0;
            for (int i = 0; i < pathlengths.Length; i++)
                size = Mathf.Max(size, CurvySpline.CalculateCacheSize(density, pathlengths[i], CurvyGlobalManager.MaxCachePPU));

            return size;
        }

        

        /// <summary>
        /// Calculates lightmap UV's
        /// </summary>
        /// <param name="uv">the UV to create UV2 for</param>
        /// <returns>UV2</returns>
        public static Vector2[] CalculateUV2(Vector2[] uv)
        {
            var UV2 = new Vector2[uv.Length];
            float sx = 1;
            float sy = 1;
            for (int i = 0; i < uv.Length; i++)
            {
                sx = Mathf.Max(sx, uv[i].x);
                sx = Mathf.Max(sy, uv[i].y);
            }
            for (int i = 0; i < uv.Length; i++)
                UV2[i] = new Vector2(uv[i].x / sx, uv[i].y / sy);
            
            return UV2;
        }

       

        #region ### Rasterization Helpers ###

        /// <summary>
        /// Rasterization Helper class
        /// </summary>
        public static List<ControlPointOption> GetControlPointsWithOptions(CGDataRequestMetaCGOptions options, CurvySpline shape, float startDist, float endDist, bool optimize, out int initialMaterialID, out float initialMaxStep)
        {
            var res = new List<ControlPointOption>();
            initialMaterialID = 0;
            initialMaxStep = float.MaxValue;
            CurvySplineSegment startSeg = shape.DistanceToSegment(startDist);
            
            float clampedEndDist = shape.ClampDistance(endDist, shape.IsClosed ? CurvyClamping.Loop : CurvyClamping.Clamp);
            if (clampedEndDist == 0)
                clampedEndDist = endDist;
            CurvySplineSegment finishSeg = (clampedEndDist==shape.Length) ? shape.LastVisibleControlPoint : shape.DistanceToSegment(clampedEndDist);
            if (endDist!=shape.Length && endDist > finishSeg.Distance)
                finishSeg = finishSeg.NextControlPoint;
            MetaCGOptions cgOptions;

            float loopOffset = 0;
            if (startSeg)
            {
                cgOptions = startSeg.GetMetadata<MetaCGOptions>(true);
                initialMaxStep = (cgOptions.MaxStepDistance == 0) ? float.MaxValue : cgOptions.MaxStepDistance;
                if (options.CheckMaterialID)
                    initialMaterialID = cgOptions.MaterialID;
                int currentMaterialID = initialMaterialID;
                
                float maxDist = cgOptions.MaxStepDistance;
                /*
                if ((options.CheckMaterialID && cgOptions.MaterialID != 0) ||
                       (optimize && cgOptions.MaxStepDistance != 0))
                    res.Add(new ControlPointOption(startSeg.LocalFToTF(0),
                                                   startSeg.Distance,
                                                   true,
                                                   cgOptions.MaterialID,
                                                   options.CheckHardEdges && cgOptions.HardEdge,
                                                   initialMaxStep,
                                                   (options.CheckExtendedUV && cgOptions.UVEdge),
                                                   options.CheckExtendedUV && cgOptions.ExplicitU,
                                                   cgOptions.FirstU,
                                                   cgOptions.SecondU));
                */


                var seg = startSeg.NextSegment ?? startSeg.NextControlPoint;
                do
                {
                    cgOptions = seg.GetMetadata<MetaCGOptions>(true);
                    if (seg.ControlPointIndex < startSeg.ControlPointIndex)
                        loopOffset = shape.Length;
                    if (options.IncludeControlPoints ||
                       (options.CheckHardEdges && cgOptions.HardEdge) ||
                       (options.CheckMaterialID && cgOptions.MaterialID != currentMaterialID) ||
                       (optimize && cgOptions.MaxStepDistance != maxDist) ||
                       (options.CheckExtendedUV && (cgOptions.UVEdge || cgOptions.ExplicitU))
                        )
                    {
                        bool matDiff = cgOptions.MaterialID != currentMaterialID;
                        maxDist = (cgOptions.MaxStepDistance == 0) ? float.MaxValue : cgOptions.MaxStepDistance;
                        currentMaterialID = options.CheckMaterialID ? cgOptions.MaterialID : initialMaterialID;
                        res.Add(new ControlPointOption(seg.LocalFToTF(0) + Mathf.FloorToInt(loopOffset / shape.Length),
                                                       seg.Distance + loopOffset, 
                                                       options.IncludeControlPoints,
                                                       currentMaterialID,
                                                       options.CheckHardEdges && cgOptions.HardEdge,
                                                       cgOptions.MaxStepDistance,
                                                       (options.CheckExtendedUV && cgOptions.UVEdge) || matDiff,
                                                       options.CheckExtendedUV && cgOptions.ExplicitU,
                                                       cgOptions.FirstU,
                                                       cgOptions.SecondU));

                    }
                    seg = seg.NextSegment;
                } while (seg && seg != finishSeg);
                // Check UV settings of last cp (not a segment if open spline!)
                if (options.CheckExtendedUV && !seg && finishSeg.IsLastVisibleControlPoint) 
                {
                    cgOptions = finishSeg.GetMetadata<MetaCGOptions>(true);
                    if (cgOptions.ExplicitU)
                        res.Add(new ControlPointOption(1,
                                                       finishSeg.Distance + loopOffset,
                                                       options.IncludeControlPoints,
                                                       currentMaterialID,
                                                       options.CheckHardEdges && cgOptions.HardEdge,
                                                       cgOptions.MaxStepDistance,
                                                       (options.CheckExtendedUV && cgOptions.UVEdge) || (options.CheckMaterialID && cgOptions.MaterialID != currentMaterialID),
                                                       options.CheckExtendedUV && cgOptions.ExplicitU,
                                                       cgOptions.FirstU,
                                                       cgOptions.SecondU));
                }
            }
            
            return res;
        }

        #endregion

    }
}
