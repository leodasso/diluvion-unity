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

namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Build/Rasterized Path", ModuleName="Rasterize Path",Description = "Rasterizes a virtual path")]
    [HelpURL(CurvySpline.DOCLINK + "cgbuildrasterizedpath")]
    public class BuildRasterizedPath : CGModule
    {
        [HideInInspector]
        [InputSlotInfo(typeof(CGPath), Name = "Path", RequestDataOnly = true)]
        public CGModuleInputSlot InPath = new CGModuleInputSlot();
        [HideInInspector]
        [OutputSlotInfo(typeof(CGPath),Name="Path")]
        public CGModuleOutputSlot OutPath = new CGModuleOutputSlot();

        #region ### Serialized Fields ###

        [FloatRegion(UseSlider=true,RegionOptionsPropertyName="RangeOptions",Precision=4)]
        [SerializeField]
        FloatRegion m_Range = FloatRegion.ZeroOne;

        [SerializeField, RangeEx(0, 100)]
        int m_Resolution = 50;
        [SerializeField]
        bool m_Optimize;
        [FieldCondition("m_Optimize",true)]
        [SerializeField, RangeEx(0.1f, 120)]
        float m_AngleTreshold = 10;

        #endregion

        #region ### Public Properties ###

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
                if (PathIsClosed)
                    v = Mathf.Repeat(value, 1);
                if (m_Range.To != v)
                    m_Range.To = v;

                Dirty = true;
            }
        }

        public float Length
        {
            get
            {
                return (PathIsClosed) ? m_Range.To - m_Range.From : m_Range.To;
            }
            set
            {
                float v = (PathIsClosed) ? value - m_Range.To : value;
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
            get { return m_AngleTreshold; }
            set
            {
                float v = Mathf.Clamp(value, 0.1f, 120);
                if (m_AngleTreshold != v)
                    m_AngleTreshold = v;
                Dirty = true;
            }
        }

        public CGPath Path
        {
            get
            {
                return OutPath.GetData<CGPath>();
            }
        }

        public bool PathIsClosed
        {
            get
            {
                return (IsConfigured) ? InPath.SourceSlot().OnRequestPathModule.PathIsClosed : true;
            }
        }

        #endregion

        #region ### Private Fields & Properties ###

        RegionOptions<float> RangeOptions
        {
            get
            {

                if (!PathIsClosed)
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
            Properties.MinWidth = 250;
            Properties.LabelWidth = 100;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            //From = m_Range.From;
            //To = m_Range.To;
            Resolution = m_Resolution;
            Optimize = m_Optimize;
        }
#endif

        public override void Reset()
        {
            base.Reset();
            m_Range = FloatRegion.ZeroOne;
            Resolution = 50;
            AngleThreshold = 10;
            OutPath.ClearData();
        }

        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        public override void Refresh()
        {
            base.Refresh();
            if (Length == 0)
            {
                Reset();
            }
            else
            {
                var req = new List<CGDataRequestParameter>();
                req.Add(new CGDataRequestRasterization(From, Length, CGUtility.CalculateSamplePointsCacheSize(Resolution, InPath.SourceSlot().OnRequestPathModule.PathLength), AngleThreshold,(Optimize) ? CGDataRequestRasterization.ModeEnum.Optimized:CGDataRequestRasterization.ModeEnum.Even));
                var path = InPath.GetData<CGPath>(req.ToArray());

                OutPath.SetData(path);
            }
        }

        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */


        /*! \endcond */
        #endregion
     
       
    }
}
