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

namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// Base class for spline input modules
    /// </summary>
    public class SplineInputModuleBase : CGModule
    {
        #region ### Serialized Fields ###
        [Tab("General")]
        [SerializeField]
        bool m_UseCache;
        [Tab("Range")]
        [SerializeField]
        CurvySplineSegment m_StartCP;
        [FieldCondition("m_StartCP",null,true,Action=ActionAttribute.ActionEnum.Enable)]
        [SerializeField]
        CurvySplineSegment m_EndCP;
        #endregion

        #region ### Public Properties ###

        public bool UseCache
        {
            get { return m_UseCache; }
            set
            {
                if (m_UseCache != value)
                    m_UseCache = value;
                Dirty = true;
            }
        }

        public CurvySplineSegment StartCP
        {
            get { return m_StartCP; }
            set
            {
                if (m_StartCP != value)
                    m_StartCP = value;
                Dirty = true;
            }
        }

        public CurvySplineSegment EndCP
        {
            get { return m_EndCP; }
            set
            {
                var cp = value;
                if (cp && StartCP && cp.ControlPointIndex <= StartCP.ControlPointIndex)
                    cp = null;
                if (m_EndCP != cp)
                {
                    m_EndCP = cp;
                }
                Dirty = true;
            }
        }

        #endregion

        protected override void OnEnable()
        {
            base.OnEnable();
            Properties.MinWidth = 250;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            EndCP = m_EndCP;
            UseCache = m_UseCache;
        }
#endif

        void getRange(CurvySpline spline, CGDataRequestRasterization raster, out float startDist, out float endDist)
        {
            if (StartCP)
            {
                float l = getPathLength(spline);
                startDist = StartCP.Distance + l * raster.Start;
                endDist = StartCP.Distance + l * (raster.Start + raster.Length);
            }
            else
            {
                startDist = spline.Length * raster.Start;
                endDist = spline.Length * (raster.Start + raster.Length);
            }

        }

        protected float getPathLength(CurvySpline spline)
        {
            if (!spline)
                return 0;
            if (StartCP)
            {
                if (EndCP)
                {
                    return EndCP.Distance - StartCP.Distance;
                }
            }
            return spline.Length;
        }

        protected bool getPathClosed(CurvySpline spline)
        {
            if (!spline || !spline.Closed)
                return false;
            return EndCP == null;
        }

        protected CGData GetSplineData(CurvySpline spline, bool fullPath, CGDataRequestRasterization raster, CGDataRequestMetaCGOptions options)
        {
            if (spline == null || spline.Count == 0)
                return null;
            List<ControlPointOption> optionsSegs = new List<ControlPointOption>();
            int materialID = 0;
            float maxStep = float.MaxValue;

            var data = (fullPath) ? new CGPath() : new CGShape();
            // calc start & end point (distance)
            float startDist;
            float endDist;
            getRange(spline, raster, out startDist, out endDist);
            raster.Resolution = Mathf.Max(raster.Resolution, 2);
            float stepDist = (endDist - startDist) / (raster.Resolution - 1);
            data.Length = endDist - startDist;

            // initialize with start TF
            float tf = spline.DistanceToTF(startDist);
            float startTF = tf;
            float endTF = (endDist > spline.Length && spline.Closed) ? spline.DistanceToTF(endDist - spline.Length) + 1 : spline.DistanceToTF(endDist);

            // Set properties
            data.SourceIsManaged = IsManagedResource(spline);
            data.Closed = spline.Closed;
            data.Seamless = spline.Closed && raster.Length == 1;


            if (data.Length == 0)
                return data;

            // Scan input spline and fetch a list of control points that provide special options (Hard Edge, MaterialID etc...)
            if (options)
                optionsSegs = CGUtility.GetControlPointsWithOptions(options, 
                                                                    spline, 
                                                                    startDist, 
                                                                    endDist, 
                                                                    raster.Mode == CGDataRequestRasterization.ModeEnum.Optimized, 
                                                                    out materialID, 
                                                                    out maxStep);

            // Setup vars
            List<SamplePointUData> extendedUVData = new List<SamplePointUData>();
            List<Vector3> pos = new List<Vector3>();
            List<float> relF = new List<float>();
            List<float> sourceF = new List<float>();
            List<Vector3> tan = new List<Vector3>();
            List<Vector3> up = new List<Vector3>();
            float curDist = startDist;
            Vector3 curPos;
            float curF;
            Vector3 curTan = Vector3.zero;
            Vector3 curUp = Vector3.zero;
            List<int> softEdges = new List<int>();




            int dead = 100000;
            
            switch (raster.Mode)
            {
                case CGDataRequestRasterization.ModeEnum.Even:
                    #region --- Even ---
                    // we advance the spline using a fixed distance
                    
                    bool dupe = false;
                    // we have at least one Material Group
                    SamplePointsMaterialGroup grp = new SamplePointsMaterialGroup(materialID);
                    // and at least one patch within that group
                    SamplePointsPatch patch = new SamplePointsPatch(0);
                    var clampMode=(data.Closed) ? CurvyClamping.Loop : CurvyClamping.Clamp;
                    
                    while (curDist <= endDist && --dead>0)
                    {
                        
                        tf = spline.DistanceToTF(spline.ClampDistance(curDist, clampMode));
                        curPos = (UseCache) ? spline.InterpolateFast(tf) : spline.Interpolate(tf);
                        curF = (curDist-startDist) / data.Length;//curDist / endDist;
                        if (Mathf.Approximately(1, curF))
                            curF = 1;
                        
                        pos.Add(curPos);
                        relF.Add(curF);
                        sourceF.Add(curDist / spline.Length);
                        if (fullPath) // add path values
                        {
                            curTan = (UseCache) ? spline.GetTangentFast(tf) : spline.GetTangent(tf,curPos);
                            curUp = spline.GetOrientationUpFast(tf);
                            tan.Add(curTan);
                            up.Add(curUp);
                        }
                        if (dupe) // HardEdge, IncludeCP, MaterialID changes etc. need an extra vertex
                        {
                            pos.Add(curPos);
                            relF.Add(curF);
                            sourceF.Add(curDist / spline.Length);
                            if (fullPath)
                            {
                                tan.Add(curTan);
                                up.Add(curUp);
                            }
                            dupe = false;
                        }
                        // Advance
                        curDist += stepDist;

                        // Check next Sample Point's options. If the next point would be past a CP with options
                        if (optionsSegs.Count > 0 && curDist >= optionsSegs[0].Distance)
                        {
                            if (optionsSegs[0].UVEdge || optionsSegs[0].UVShift)
                                extendedUVData.Add(new SamplePointUData(pos.Count, optionsSegs[0].UVEdge,optionsSegs[0].FirstU, optionsSegs[0].SecondU));
                            // clamp point at CP and maybe duplicate the next sample point
                            curDist = optionsSegs[0].Distance;
                            dupe = optionsSegs[0].HardEdge || optionsSegs[0].MaterialID != grp.MaterialID || (options.CheckExtendedUV && optionsSegs[0].UVEdge);
                            // end the current patch...
                            if (dupe)
                            {
                                patch.End = pos.Count;
                                grp.Patches.Add(patch);
                                // if MaterialID changes, we start a new MaterialGroup
                                if (grp.MaterialID != optionsSegs[0].MaterialID)
                                {
                                    data.MaterialGroups.Add(grp);
                                    grp = new SamplePointsMaterialGroup(optionsSegs[0].MaterialID);
                                }
                                // in any case we start a new patch
                                patch = new SamplePointsPatch(pos.Count + 1);
                                if (!optionsSegs[0].HardEdge)
                                    softEdges.Add(pos.Count + 1);
                                // Extended UV
                                if (optionsSegs[0].UVEdge || optionsSegs[0].UVShift)
                                    extendedUVData.Add(new SamplePointUData(pos.Count + 1, optionsSegs[0].UVEdge, optionsSegs[0].FirstU, optionsSegs[0].SecondU));
                            }
                            // and remove the CP from the options
                            optionsSegs.RemoveAt(0);
                        }

                        // Ensure last sample point position is at the desired end distance
                        if (curDist > endDist && curF < 1) // next loop curF will be 1
                            curDist = endDist;
                    }
                    if (dead<= 0)
                        Debug.LogError("[Curvy] He's dead, Jim! Deadloop in SplineInputModuleBase.GetSplineData (Even)! Please send a bug report!");
                    // store the last open patch
                    patch.End = pos.Count - 1;
                    grp.Patches.Add(patch);
                    // ExplicitU on last Vertex?
                    //if (optionsSegs.Count > 0 && optionsSegs[0].UVShift)
                    //    extendedUVData.Add(new SamplePointUData(pos.Count - 1, optionsSegs[0].UVEdge, optionsSegs[0].FirstU, optionsSegs[0].SecondU));
                    // if path is closed and no hard edges involved, we need to smooth first normal
                    if (data.Closed && !spline[0].GetMetadata<MetaCGOptions>(true).HardEdge)
                        softEdges.Add(0);
                    data.MaterialGroups.Add(grp);
                    // fill data
                    data.SourceF = sourceF.ToArray();
                    data.F = relF.ToArray();
                    data.Position = pos.ToArray();
                    if (fullPath)
                    {
                        ((CGPath)data).Direction = tan.ToArray();
                        data.Normal = up.ToArray();
                    }
                    #endregion
                    break;
                case CGDataRequestRasterization.ModeEnum.Optimized:
                    #region --- Optimized ---
                    dupe = false;
                    // we have at least one Material Group
                    grp = new SamplePointsMaterialGroup(materialID);
                    // and at least one patch within that group
                    patch = new SamplePointsPatch(0);
                    float stepSizeTF = stepDist / spline.Length;

                    float maxAngle = raster.AngleThreshold;
                    float stopAt;
                    bool atStopPoint;
                    curPos = spline.Interpolate(tf);
                    curTan = spline.GetTangent(tf, curPos);

                    var addPoint = new System.Action<float>((float f) =>
                    {
                        sourceF.Add(curDist/spline.Length);
                        pos.Add(curPos);
                        relF.Add((curDist -startDist) / data.Length);
                        if (fullPath)
                        {
                            tan.Add(curTan);
                            up.Add(spline.GetOrientationUpFast(f));
                        }
                    });
                    
                    while (tf < endTF && dead-- > 0)
                    {
                        addPoint(tf%1);

                        // Advance
                        stopAt = (optionsSegs.Count > 0) ? optionsSegs[0].TF : endTF;
                        
                        atStopPoint = spline.MoveByAngleExtINTERNAL(ref tf, Generator.MinDistance, maxStep, maxAngle, out curPos, out curTan, out stepDist, stopAt, data.Closed,stepSizeTF);
                        curDist += stepDist;
                        if (Mathf.Approximately(tf, endTF) || tf > endTF)
                        {
                            curDist = endDist;
                            endTF = (data.Closed) ? DTMath.Repeat(endTF,1) : Mathf.Clamp01(endTF);
                            curPos = spline.Interpolate(endTF);
                            if (fullPath)
                                curTan = spline.GetTangent(endTF, curPos);
                            addPoint(endTF);
                            break;
                        }
                        if (atStopPoint)
                        {
                            if (optionsSegs.Count > 0)
                            {
                                if (optionsSegs[0].UVEdge || optionsSegs[0].UVShift)
                                    extendedUVData.Add(new SamplePointUData(pos.Count, optionsSegs[0].UVEdge,optionsSegs[0].FirstU, optionsSegs[0].SecondU));
                                // clamp point at CP and maybe duplicate the next sample point
                                curDist = optionsSegs[0].Distance;
                                maxStep = (optionsSegs[0].MaxStepDistance);
                                dupe = optionsSegs[0].HardEdge || optionsSegs[0].MaterialID != grp.MaterialID || (options.CheckExtendedUV && optionsSegs[0].UVEdge);
                                if (dupe)
                                {
                                    // end the current patch...
                                    patch.End = pos.Count;
                                    grp.Patches.Add(patch);
                                    // if MaterialID changes, we start a new MaterialGroup
                                    if (grp.MaterialID != optionsSegs[0].MaterialID)
                                    {
                                        data.MaterialGroups.Add(grp);
                                        grp = new SamplePointsMaterialGroup(optionsSegs[0].MaterialID);
                                    }
                                    
                                    
                                    // in any case we start a new patch
                                    patch = new SamplePointsPatch(pos.Count + 1);
                                    if (!optionsSegs[0].HardEdge)
                                        softEdges.Add(pos.Count + 1);
                                    // Extended UV
                                    if (optionsSegs[0].UVEdge || optionsSegs[0].UVShift)
                                        extendedUVData.Add(new SamplePointUData(pos.Count+1, optionsSegs[0].UVEdge, optionsSegs[0].FirstU, optionsSegs[0].SecondU));
                                    addPoint(tf);
                                }
                                // and remove the CP from the options
                                optionsSegs.RemoveAt(0);
                                
                            }
                            else
                            {
                                addPoint(tf);
                                break;
                            }
                        }

                    }
                    if (dead <= 0)
                        Debug.LogError("[Curvy] He's dead, Jim! Deadloop in SplineInputModuleBase.GetSplineData (Optimized)! Please send a bug report!");
                    // store the last open patch
                    patch.End = pos.Count - 1;
                    grp.Patches.Add(patch);
                    // ExplicitU on last Vertex?
                    if (optionsSegs.Count > 0 && optionsSegs[0].UVShift)
                        extendedUVData.Add(new SamplePointUData(pos.Count - 1, optionsSegs[0].UVEdge, optionsSegs[0].FirstU, optionsSegs[0].SecondU));
                    // if path is closed and no hard edges involved, we need to smooth first normal
                    if (data.Closed && !spline[0].GetMetadata<MetaCGOptions>(true).HardEdge)
                        softEdges.Add(0);
                    data.MaterialGroups.Add(grp);
                    // fill data
                    data.SourceF = sourceF.ToArray();
                    data.F = relF.ToArray();
                    data.Position = pos.ToArray();
                    data.Bounds = spline.Bounds;
                    
                    if (fullPath)
                    {
                        ((CGPath)data).Direction = tan.ToArray();
                        data.Normal = up.ToArray();
                    }
                    #endregion
                    break;
            }
            data.Map = (float[])data.F.Clone();
            if (!fullPath)
            {
                data.RecalculateNormals(softEdges);
                if (options && options.CheckExtendedUV)
                    CalculateExtendedUV(spline,startTF,endTF,extendedUVData, data);
            }
            return data;
        }

        void CalculateExtendedUV(CurvySpline spline, float startTF, float endTF, List<SamplePointUData> ext, CGShape data)
        {
            // we have a list of data, either UV Edge (double then) or Explicit
            // unlike easy mode, U is bound to Shape's SourceF, not F!

            // for the first vertex, find the reference CP and calculate starting U (first vertex never has matching Udata, even if it's over a reference CP!!!)
            
            CurvySplineSegment refCCW,refCW;
            var optCCW=findPreviousReferenceCPOptions(spline, startTF, out refCCW);
            var optCW=findNextReferenceCPOptions(spline, startTF, out refCW);
            // we now know the U range the first vertex is in, so let's calculate it's actual U value
            // get the distance delta within that range
            float frag;
            // Special case: CW is first CP (implies closed spline)
            if (refCW.IsFirstVisibleControlPoint)
                frag = ((data.SourceF[0] * spline.Length) - refCCW.Distance) / (spline.Length - refCCW.Distance);
            else
                frag = ((data.SourceF[0] * spline.Length)-refCCW.Distance) / (refCW.Distance - refCCW.Distance);
            ext.Insert(0, new SamplePointUData(0,(startTF==0 && optCCW.UVEdge), frag*(optCW.FirstU-optCCW.GetDefinedFirstU(0))+optCCW.GetDefinedFirstU(0),(startTF==0 && optCCW.UVEdge)?optCCW.SecondU:0));

            // Do the same for the last vertex, find the reference CP and calculate starting U (first vertex never has matching Udata, even if it's over a reference CP!!!)
            if (ext[ext.Count - 1].Vertex < data.Count - 1)
            {
                optCCW = findPreviousReferenceCPOptions(spline, endTF, out refCCW);
                optCW = findNextReferenceCPOptions(spline, endTF, out refCW);
                float cwU = optCW.FirstU;
                float ccwU = optCCW.GetDefinedSecondU(0);
                // Special case: CW is first CP (implies closed spline)
                if (refCW.IsFirstVisibleControlPoint)
                {
                    frag = ((data.SourceF[data.Count - 1] * spline.Length) - refCCW.Distance) / (spline.Length - refCCW.Distance);
                    // either take the ending U from 2nd U of first CP or raise last U to next int

                    if (optCW.UVEdge)
                        cwU = optCW.FirstU;
                    else if (ext.Count > 1)
                        cwU = Mathf.FloorToInt((ext[ext.Count - 1].UVEdge) ? ext[ext.Count - 1].SecondU : ext[ext.Count - 1].FirstU) + 1;
                    else
                        cwU = 1;
                }
                else
                {
                    frag = ((data.SourceF[data.Count - 1] * spline.Length) - refCCW.Distance) / (refCW.Distance - refCCW.Distance);
                }
                ext.Add(new SamplePointUData(data.Count - 1, false, frag * (cwU - ccwU) + ccwU, 0));
            }
            float startF = 0;
            float curF;
            float lo = (ext[0].UVEdge) ? ext[0].SecondU : ext[0].FirstU;
            float hi = ext[1].FirstU;
            float lenF = data.F[ext[1].Vertex] - data.F[ext[0].Vertex];
            int current=1;
            //Debug.Log("lo=" + lo + ", hi=" + hi + ", length=" + lenF);
            for (int vt = 0; vt < data.Count-1; vt++)
            {
                curF = (data.F[vt] - startF) / lenF;
                //Debug.Log(vt + ":" + curF);
                data.Map[vt] = (hi - lo) * curF + lo;
                
                if (ext[current].Vertex == vt)
                {
                    // UVEdge?
                    if (ext[current].FirstU==ext[current+1].FirstU)
                    {
                        lo = (ext[current].UVEdge) ? ext[current].SecondU : ext[current].FirstU; 
                        current++; 
                    } else
                        lo = ext[current].FirstU;

                    hi = ext[current + 1].FirstU;
                    lenF = data.F[ext[current+1].Vertex] - data.F[ext[current].Vertex];
                    startF = data.F[vt];
                    //Debug.Log("lo=" + lo + ", hi=" + hi + ", length=" + lenF);
                    current++;
                }
            }
            data.Map[data.Count - 1] = ext[ext.Count - 1].FirstU;
        }

        MetaCGOptions findPreviousReferenceCPOptions(CurvySpline spline, float tf,  out CurvySplineSegment cp)
        {
            MetaCGOptions options;
            cp = spline.TFToSegment(tf);
            do
            {
                options = cp.GetMetadata<MetaCGOptions>(true);
                if (cp.IsFirstVisibleControlPoint)
                    return options;
                cp = cp.PreviousSegment;
            }
            while (cp && !options.UVEdge && !options.ExplicitU &&!options.HasDifferentMaterial);
            return options;
        }

        MetaCGOptions findNextReferenceCPOptions(CurvySpline spline, float tf, out CurvySplineSegment cp)
        {
            MetaCGOptions options;
            float localF;
            cp = spline.TFToSegment(tf, out localF);

            do
            {
                cp = cp.NextControlPoint;
                options = cp.GetMetadata<MetaCGOptions>(true);
                if (!spline.Closed && cp.IsLastVisibleControlPoint)
                    return options;
            }
            while (!options.UVEdge && !options.ExplicitU && !options.HasDifferentMaterial && !cp.IsFirstSegment);
            return options;
        }
     
    }
}
