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
using FluffyUnderware.DevTools.Extensions;

namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Build/Shape Extrusion", ModuleName="Shape Extrusion", Description = "Simple Shape Extrusion")]
    [HelpURL(CurvySpline.DOCLINK + "cgbuildshapeextrusion")]
    public class BuildShapeExtrusion : CGModule
    {
        #region ### Enums ###

        public enum ScaleModeEnum
        {
            Simple,
            Advanced
        }

        public enum CrossShiftModeEnum
        {
            None,
            ByOrientation,
            Custom
        }

        #endregion

        [HideInInspector]
        [InputSlotInfo(typeof(CGPath),RequestDataOnly=true)]
        public CGModuleInputSlot InPath = new CGModuleInputSlot();

        [HideInInspector]
        [InputSlotInfo(typeof(CGShape), RequestDataOnly = true)]
        public CGModuleInputSlot InCross = new CGModuleInputSlot();

        [HideInInspector]
        [OutputSlotInfo(typeof(CGVolume))]
        public CGModuleOutputSlot OutVolume = new CGModuleOutputSlot();

        [HideInInspector]
        [OutputSlotInfo(typeof(CGVolume))]
        public CGModuleOutputSlot OutVolumeHollow = new CGModuleOutputSlot();

        #region ### Serialized Fields ###

         #region TAB: Path
        [Tab("Path")]
        [FloatRegion(UseSlider = true, RegionOptionsPropertyName = "RangeOptions", Precision = 4)]
        [SerializeField]
        FloatRegion m_Range = FloatRegion.ZeroOne;

        [SerializeField, RangeEx(0, 100, Tooltip="Vertex distance")]
        int m_Resolution=50;
        [SerializeField]
        bool m_Optimize=true;
        [FieldCondition("m_Optimize", true)]
        [SerializeField, RangeEx(0.1f, 120, Tooltip = "Max angle")]
        float m_AngleThreshold = 10;
        
        #endregion

        #region TAB: Cross
        [Tab("Cross")]
        [FieldAction("CBEditCrossButton",Position=ActionAttribute.ActionPositionEnum.Above)]
        [FloatRegion(UseSlider = true, RegionOptionsPropertyName = "CrossRangeOptions", Precision = 4)]
        [SerializeField]
        FloatRegion m_CrossRange = FloatRegion.ZeroOne;
        
        [SerializeField, RangeEx(0, 100,"Resolution",Tooltip="Vertex distance")]
        int m_CrossResolution=50;
        [SerializeField,Label("Optimize")]
        bool m_CrossOptimize=true;
        [FieldCondition("m_CrossOptimize",true)]
        [SerializeField, RangeEx(0.1f, 120, "Angle Threshold", Tooltip = "Max angle")]
        float m_CrossAngleThreshold=10;
        
        //[Header("Options")]
        [SerializeField,Label("Include CP")]
        bool m_CrossIncludeControlpoints;
        [SerializeField,Label("Hard Edges")]
        bool m_CrossHardEdges;
        [SerializeField,Label("Materials")]
        bool m_CrossMaterials;
        [SerializeField, Label("Extended UV")]
        bool m_CrossExtendedUV;
        [SerializeField, Label("Shift",Tooltip="Shift Cross Start")]
        CrossShiftModeEnum m_CrossShiftMode = CrossShiftModeEnum.ByOrientation;
        [SerializeField]
        [RangeEx(0,1,"Value","Shift By",Slider=true)]
        [FieldCondition("m_CrossShiftMode",CrossShiftModeEnum.Custom)]
        float m_CrossShiftValue;
        [Label("Reverse Normal", "Reverse Vertex Normals?")]
        [SerializeField]
        bool m_CrossReverseNormals;
        #endregion

        #region TAB: Scale

        [Tab("Scale")]
        [Label("Mode")]
        [SerializeField]
        ScaleModeEnum m_ScaleMode = ScaleModeEnum.Simple;
        
        [FieldCondition("m_ScaleMode", ScaleModeEnum.Advanced)]
        [Label("Reference")]
        [SerializeField]
        CGReferenceMode m_ScaleReference = CGReferenceMode.Self;

        [FieldCondition("m_ScaleMode", ScaleModeEnum.Advanced)]
        [Label("Offset")]
        [SerializeField]
        float m_ScaleOffset;

        [SerializeField,Label("Uniform",Tooltip="Use a single curve")]
        bool m_ScaleUniform=true;
        
        [SerializeField]
        float m_ScaleX = 1;
        
        [SerializeField]
        [FieldCondition("m_ScaleUniform", false)]
        float m_ScaleY = 1;

        [SerializeField]
        [FieldCondition("m_ScaleMode", ScaleModeEnum.Advanced)]
        [AnimationCurveEx("Multiplier X")]
        public AnimationCurve m_ScaleCurveX = AnimationCurve.Linear(0, 1, 1, 1);
        [SerializeField]
        [FieldCondition("m_ScaleUniform", false,false,ConditionalAttribute.OperatorEnum.AND,"m_ScaleMode",ScaleModeEnum.Advanced,false)]
        [AnimationCurveEx("Multiplier Y")]
        public AnimationCurve m_ScaleCurveY = AnimationCurve.Linear(0, 1, 1, 1);

        #endregion

        #region TAB: Hollow
        [Tab("Hollow")]
        [RangeEx(0,1,Slider=true,Label="Inset")]
        [SerializeField]
        float m_HollowInset;
        [Label("Reverse Normal","Reverse Vertex Normals?")]
        [SerializeField]
        bool m_HollowReverseNormals;
        #endregion


        #endregion

        #region ### Public Properties ###

        #region TAB: Path


        public float From
        {
            get { return m_Range.From; }
            set
            {
                float v = Mathf.Repeat(value, 1);
                if (m_Range.From != v)
                    m_Range.From = v;

                Dirty = true;
            }
        }

        public float To
        {
            get { return m_Range.To; }
            set
            {
                float v = Mathf.Max(From, value);
                if (ClampPath)
                    v = DTMath.Repeat(value, 1);
                if (m_Range.To != v)
                    m_Range.To = v;

                Dirty = true;
            }
        }

        public float Length
        {
            get
            {
                return (ClampPath) ? m_Range.To - m_Range.From : m_Range.To;
            }
            set
            {
                float v = (ClampPath) ? value - m_Range.To : value;
                if (m_Range.To != v)
                    m_Range.To = v;
                Dirty = true;
            }
        }

        public int Resolution
        {
            get { return m_Resolution; }
            set
            {
                int v = Mathf.Clamp(value, 0, 100);
                if (m_Resolution != v)
                    m_Resolution = v;
                Dirty = true;
            }
        }

        public bool Optimize
        {
            get { return m_Optimize; }
            set
            {
                if (m_Optimize != value)
                    m_Optimize = value;
                Dirty = true;
            }
        }

        public float AngleThreshold
        {
            get { return m_AngleThreshold; }
            set
            {
                float v = Mathf.Clamp(value, 0.1f, 120);
                if (m_AngleThreshold != v)
                    m_AngleThreshold = v;
                Dirty = true;
            }
        }


        #endregion
        #region TAB: Cross
        public float CrossFrom
        {
            get { return m_CrossRange.From; }
            set
            {
                float v = Mathf.Repeat(value, 1);
                if (m_CrossRange.From != v)
                    m_CrossRange.From = v;

                Dirty = true;
            }
        }

        public float CrossTo
        {
            get { return m_CrossRange.To; }
            set
            {
                float v = Mathf.Max(CrossFrom, value);
                if (ClampCross)
                    v = DTMath.Repeat(value, 1);
                if (m_CrossRange.To != v)
                    m_CrossRange.To = v;

                Dirty = true;
            }
        }

        public float CrossLength
        {
            get
            {
                return (ClampCross) ? m_CrossRange.To - m_CrossRange.From : m_CrossRange.To;
            }
            set
            {
                float v = (ClampCross) ? value - m_CrossRange.To : value;
                if (m_CrossRange.To != v)
                    m_CrossRange.To = v;
                Dirty = true;
            }
        }

        public int CrossResolution
        {
            get { return m_CrossResolution; }
            set
            {
                int v = Mathf.Clamp(value, 0, 100);
                if (m_CrossResolution != v)
                    m_CrossResolution = v;
                Dirty = true;
            }
        }

        public bool CrossOptimize
        {
            get { return m_CrossOptimize; }
            set
            {
                if (m_CrossOptimize != value)
                    m_CrossOptimize = value;
                Dirty = true;
            }
        }

        public float CrossAngleThreshold
        {
            get { return m_CrossAngleThreshold; }
            set
            {
                float v = Mathf.Clamp(value, 0.1f, 120);
                if (m_CrossAngleThreshold != v)
                    m_CrossAngleThreshold = v;
                Dirty = true;
            }
        }


        public bool CrossIncludeControlPoints
        {
            get { return m_CrossIncludeControlpoints; }
            set
            {
                if (m_CrossIncludeControlpoints != value)
                    m_CrossIncludeControlpoints = value;
                Dirty = true;
            }
        }

        public bool CrossHardEdges
        {
            get { return m_CrossHardEdges; }
            set
            {
                if (m_CrossHardEdges != value)
                    m_CrossHardEdges = value;
                Dirty = true;
            }
        }

        public bool CrossMaterials
        {
            get { return m_CrossMaterials; }
            set
            {
                if (m_CrossMaterials != value)
                    m_CrossMaterials = value;
                Dirty = true;
            }
        }

        public bool CrossExtendedUV
        {
            get { return m_CrossExtendedUV; }
            set
            {
                if (m_CrossExtendedUV != value)
                    m_CrossExtendedUV = value;
                Dirty = true;
            }
        }

        public CrossShiftModeEnum CrossShiftMode
        {
            get { return m_CrossShiftMode; }
            set
            {
                if (m_CrossShiftMode != value)
                    m_CrossShiftMode = value;
                Dirty = true;
            }
        }

        public float CrossShiftValue
        {
            get { return m_CrossShiftValue; }
            set
            {
                float v = Mathf.Repeat(m_CrossShiftValue, 1);
                if (m_CrossShiftValue != v)
                    m_CrossShiftValue = v;
                Dirty = true;
            }
        }

        bool CrossReverseNormals
        {
            get { return m_CrossReverseNormals; }
            set
            {
                if (m_CrossReverseNormals != value)
                    m_CrossReverseNormals = value;
                Dirty = true;
            }
        }


        #endregion
        #region TAB: Scale

        public ScaleModeEnum ScaleMode
        {
            get { return m_ScaleMode; }
            set
            {
                if (m_ScaleMode != value)
                    m_ScaleMode = value;
                Dirty = true;
            }
        }

        public CGReferenceMode ScaleReference
        {
            get { return m_ScaleReference; }
            set
            {
                if (m_ScaleReference != value)
                    m_ScaleReference = value;
                Dirty = true;
            }
        }

        public bool ScaleUniform
        {
            get { return m_ScaleUniform; }
            set
            {
                if (m_ScaleUniform != value)
                    m_ScaleUniform = value;
                Dirty = true;
            }
        }

        public float ScaleOffset
        {
            get { return m_ScaleOffset; }
            set
            {
                if (m_ScaleOffset != value)
                    m_ScaleOffset = value;
                Dirty = true;
            }
        }

        public float ScaleX
        {
            get { return m_ScaleX; }
            set
            {
                if (m_ScaleX != value)
                    m_ScaleX = value;
                Dirty = true;
            }
        }

        public float ScaleY
        {
            get { return m_ScaleY; }
            set
            {
                if (m_ScaleY != value)
                    m_ScaleY = value;
                Dirty = true;
            }
        }



        #endregion
        #region TAB: Hollow

        float HollowInset
        {
            get { return m_HollowInset; }
            set
            {
                float v = Mathf.Clamp01(value);
                if (m_HollowInset != v)
                    m_HollowInset = v;
                Dirty = true;
            }
        }

        bool HollowReverseNormals
        {
            get { return m_HollowReverseNormals; }
            set
            {
                if (m_HollowReverseNormals != value)
                    m_HollowReverseNormals = value;
                Dirty = true;
            }
        }

        #endregion

        public int PathSamples
        {
            get;
            private set;
        }

        public int CrossSamples
        {
            get;
            private set;
        }

        public int CrossGroups { get; private set; }

        public IExternalInput Cross
        {
            get
            {
                return (IsConfigured) ? InCross.SourceSlot().ExternalInput : null;
            }
        }

        public Vector3 CrossPosition { get; protected set; }

        public Quaternion CrossRotation { get; protected set; }

        #endregion

        #region ### Private Fields & Properties ###

        bool ClampPath { get { return (InPath.IsLinked) ? !InPath.SourceSlot().OnRequestPathModule.PathIsClosed : true; } }
        bool ClampCross { get { return (InCross.IsLinked) ? !InCross.SourceSlot().OnRequestPathModule.PathIsClosed : true; } }
        RegionOptions<float> RangeOptions
        {
            get
            {

                if (ClampPath)
                {
                    return RegionOptions<float>.MinMax(0, 1);
                }
                else
                {
                    return new RegionOptions<float>()
                    {
                        LabelFrom = "Start",
                        ClampFrom = DTValueClamping.Min,
                        FromMin = 0,
                        LabelTo = "Length",
                        ClampTo = DTValueClamping.Range,
                        ToMin = 0,
                        ToMax = 1
                    };
                }
            }
        }

        RegionOptions<float> CrossRangeOptions
        {
            get
            {

                if (ClampCross)
                {
                    return RegionOptions<float>.MinMax(0, 1);
                }
                else
                {
                    return new RegionOptions<float>()
                    {
                        LabelFrom = "Start",
                        ClampFrom = DTValueClamping.Min,
                        FromMin = 0,
                        LabelTo = "Length",
                        ClampTo = DTValueClamping.Range,
                        ToMin = 0,
                        ToMax = 1
                    };
                }
            }
        }

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

        protected override void OnEnable()
        {
            base.OnEnable();
            Properties.MinWidth = 270;
            Properties.LabelWidth = 100;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            Resolution = m_Resolution;
            Optimize = m_Optimize;

            CrossResolution = m_CrossResolution;
            CrossOptimize = m_CrossOptimize;

            CrossIncludeControlPoints = m_CrossIncludeControlpoints;
            CrossHardEdges = m_CrossHardEdges;
            ScaleMode = m_ScaleMode;
            ScaleReference = m_ScaleReference;
            ScaleUniform = m_ScaleUniform;
            ScaleOffset = m_ScaleOffset;
            ScaleX = m_ScaleX;
            ScaleY = m_ScaleY;

        }
#endif

        public override void Reset()
        {
            base.Reset();
            From = 0;
            To = 1;
            Resolution = 50;
            AngleThreshold = 10;
            Optimize = true;
            CrossFrom = 0;
            CrossTo = 1;
            CrossResolution = 50;
            CrossAngleThreshold = 10;
            CrossOptimize = true;
            CrossIncludeControlPoints = false;
            CrossHardEdges = false;
            CrossMaterials = false;
            CrossShiftMode = CrossShiftModeEnum.ByOrientation;
            ScaleMode = ScaleModeEnum.Simple;
            ScaleUniform = true;
            ScaleX = 1;
            ScaleY = 1;
            HollowInset = 0;
        }


        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        public override void Refresh()
        {
            base.Refresh();
            if (Length == 0)
            {
                OutVolume.SetData(null);
                OutVolumeHollow.SetData(null);
            }
            else
            {
                var req = new List<CGDataRequestParameter>();
                req.Add(new CGDataRequestRasterization(this.From, this.Length, CGUtility.CalculateSamplePointsCacheSize(Resolution, InPath.SourceSlot().OnRequestPathModule.PathLength * this.Length), AngleThreshold, (Optimize) ? CGDataRequestRasterization.ModeEnum.Optimized : CGDataRequestRasterization.ModeEnum.Even));
                var path = InPath.GetData<CGPath>(req.ToArray());
                req.Clear();
                req.Add(new CGDataRequestRasterization(this.CrossFrom, this.CrossLength, CGUtility.CalculateSamplePointsCacheSize(CrossResolution, InCross.SourceSlot().OnRequestPathModule.PathLength * this.CrossLength), CrossAngleThreshold,(CrossOptimize) ? CGDataRequestRasterization.ModeEnum.Optimized : CGDataRequestRasterization.ModeEnum.Even));
                
                if (CrossIncludeControlPoints || CrossHardEdges || CrossMaterials)
                    req.Add(new CGDataRequestMetaCGOptions(CrossHardEdges, CrossMaterials, CrossIncludeControlPoints, CrossExtendedUV));

                var cross = InCross.GetData<CGShape>(req.ToArray());
                if (!path || !cross || path.Count == 0 || cross.Count==0)
                {
                    OutVolume.ClearData();
                    OutVolumeHollow.ClearData();
                    return;
                }


                var vol = CGVolume.Get(OutVolume.GetData<CGVolume>(), path, cross);
                var volHollow = (OutVolumeHollow.IsLinked) ? CGVolume.Get(OutVolumeHollow.GetData<CGVolume>(), path, cross) : null;

                PathSamples = path.Count;
                CrossSamples = cross.Count;
                CrossGroups = cross.MaterialGroups.Count;
                CrossPosition = vol.Position[0];
                CrossRotation = Quaternion.LookRotation(vol.Direction[0], vol.Normal[0]);



                Vector3 baseScale = (ScaleUniform) ? new Vector3(ScaleX, ScaleX, 1) : new Vector3(ScaleX, ScaleY, 1);
                Vector3 scl = baseScale;
                int vtIdx = 0;

                float[] scaleFArray = (ScaleReference == CGReferenceMode.Source) ? path.SourceF : path.F;

                Matrix4x4 mat;
                Matrix4x4 matHollow;

                Quaternion R;
                int crossNormalMul = (CrossReverseNormals) ? -1 : 1;
                int hollowNormalMul=(HollowReverseNormals) ? -1 : 1;


                    
                for (int sample = 0; sample < path.Count; sample++)
                {
                    R = Quaternion.LookRotation(path.Direction[sample], path.Normal[sample]);

                    getScaleInternal(scaleFArray[sample], baseScale, ref scl);
                    mat = Matrix4x4.TRS(path.Position[sample], R, scl);

                    if (volHollow == null)
                    {
                        for (int c = 0; c < cross.Count; c++)
                        {
                            vol.Vertex[vtIdx] = mat.MultiplyPoint(cross.Position[c]);
                            vol.VertexNormal[vtIdx++] = R * cross.Normal[c]*crossNormalMul;
                        }
                    }
                    else
                    {
                        matHollow = Matrix4x4.TRS(path.Position[sample], R, scl * (1 - HollowInset));
                        for (int c = 0; c < cross.Count; c++)
                        {
                            vol.Vertex[vtIdx] = mat.MultiplyPoint(cross.Position[c]);
                            vol.VertexNormal[vtIdx] = R * cross.Normal[c];
                            volHollow.Vertex[vtIdx] = matHollow.MultiplyPoint(cross.Position[c]);
                            volHollow.VertexNormal[vtIdx] = vol.VertexNormal[vtIdx++]*hollowNormalMul;
                        }
                    }
                }

                switch (CrossShiftMode)
                {
                    case CrossShiftModeEnum.ByOrientation:
                        // shift CrossF to match Path Orientation
                        Vector2 hit;
                        float frag;
                        for (int i = 0; i < cross.Count - 1; i++)
                        {
                            if (DTMath.RayLineSegmentIntersection(vol.Position[0], vol.VertexNormal[0], vol.Vertex[i], vol.Vertex[i + 1], out hit, out frag))
                            {
                                vol.CrossFShift = DTMath.SnapPrecision(vol.CrossF[i] + (vol.CrossF[i + 1] - vol.CrossF[i]) * frag, 2);
                                break;
                            }
                        }
                        if (vol.CrossClosed && DTMath.RayLineSegmentIntersection(vol.Position[0], vol.VertexNormal[0], vol.Vertex[cross.Count - 1], vol.Vertex[0], out hit, out frag))
                            vol.CrossFShift = DTMath.SnapPrecision(vol.CrossF[cross.Count - 1] + (vol.CrossF[0] - vol.CrossF[cross.Count - 1]) * frag, 2);
                        break;
                    case CrossShiftModeEnum.Custom:
                        vol.CrossFShift = CrossShiftValue;
                        break;
                    default:
                        vol.CrossFShift = 0;
                        break;
                }

                if (volHollow != null)
                    volHollow.CrossFShift = vol.CrossFShift;

                OutVolume.SetData(vol);
                OutVolumeHollow.SetData(volHollow);
            }
        }

        public Vector3 GetScale(float f)
        {
            Vector3 baseScale = (ScaleUniform) ? new Vector3(ScaleX, ScaleX, 1) : new Vector3(ScaleX, ScaleY, 1);
            Vector3 res = Vector3.zero;
            getScaleInternal(f, baseScale, ref res);
            return res;
        }

        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */

        void getScaleInternal(float f, Vector3 baseScale, ref Vector3 scale)
        {
            if (ScaleMode == ScaleModeEnum.Advanced)
            {
                float scf = DTMath.Repeat(f - ScaleOffset, 1);
                float scx = baseScale.x * m_ScaleCurveX.Evaluate(scf);
                scale.Set(scx,
                              (m_ScaleUniform) ? scx : baseScale.y * m_ScaleCurveY.Evaluate(scf),
                              1);
            }
            else
                scale = baseScale;

        }

        /*! \endcond */
        #endregion

    }
    
}
