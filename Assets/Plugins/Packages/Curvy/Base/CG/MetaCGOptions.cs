// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy
{

    /// <summary>
    /// Curvy Generator options Metadata class
    /// </summary>
    [HelpURL(CurvySpline.DOCLINK + "metacgoptions")]
    public class MetaCGOptions : CurvyMetadataBase, ICurvyMetadata
    {

        #region ### Serialized Fields ###
        
        [Positive]
        [SerializeField]
        int m_MaterialID;

        
        [SerializeField]
        bool m_HardEdge;
        [Positive(Tooltip = "Max step distance when using optimization")]
        [SerializeField]
        float m_MaxStepDistance;
        [Section("Extended UV",HelpURL = CurvySpline.DOCLINK + "metacgoptions_extendeduv")]
        [FieldCondition("showUVEdge",true)]
        [SerializeField]
        bool m_UVEdge;
        
        [Positive]
        [FieldCondition("showExplicitU", true)]
        [SerializeField]
        bool m_ExplicitU;
        [FieldCondition("showFirstU", true)]
        [FieldAction("CBSetFirstU")]
        [Positive]
        [SerializeField]
        float m_FirstU;
        [FieldCondition("showSecondU", true)]
        [Positive]
        [SerializeField]
        float m_SecondU;
        
        

        #endregion

        #region ### Public Properties ###

        /// <summary>
        /// Gets or sets Material ID
        /// </summary>
        public int MaterialID
        {
            get
            {
                return m_MaterialID;
            }
            set
            {
                int v=Mathf.Max(0, value);
                if (m_MaterialID != v)
                {
                    m_MaterialID = v;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to create a hard edge or not
        /// </summary>
        public bool HardEdge
        {
            get { return m_HardEdge; }
            set
            {
                if (m_HardEdge != value)
                {
                    m_HardEdge = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to create an UV edge or not
        /// </summary>
        public bool UVEdge
        {
            get { return m_UVEdge; }
            set
            {
                if (m_UVEdge != value)
                {
                    m_UVEdge = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to define explicit U values
        /// </summary>
        public bool ExplicitU
        {
            get { return m_ExplicitU; }
            set
            {
                if (m_ExplicitU != value)
                {
                    m_ExplicitU = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Gets or sets UV0
        /// </summary>
        public float FirstU
        {
            get { return m_FirstU; }
            set
            {
                if (m_FirstU != value)
                {
                    m_FirstU = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Gets or sets UV0
        /// </summary>
        public float SecondU
        {
            get { return m_SecondU; }
            set
            {
                if (m_SecondU != value)
                {
                    m_SecondU = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Gets or sets maximum vertex distance when using optimization (0=infinite)
        /// </summary>
        public float MaxStepDistance
        {
            get
            {
                return m_MaxStepDistance;
            }
            set
            {
                float v=Mathf.Max(0, value);
                if (m_MaxStepDistance != v)
                {
                    m_MaxStepDistance = v;
                    SetDirty();
                }
            }
        }

        public bool HasDifferentMaterial
        {
            get
            {
                return (ControlPoint.PreviousControlPoint && GetPreviousData<MetaCGOptions>(true).MaterialID != MaterialID);
            }
        }

        #endregion

        #region ### Private Fields & Properties ###

        

        bool showUVEdge
        {
            get
            {
                return (ControlPoint && (Spline.Closed || (!ControlPoint.IsFirstVisibleControlPoint && !ControlPoint.IsLastVisibleControlPoint)) && !HasDifferentMaterial);
            }
        }

        bool showExplicitU
        {
            get
            {
                return (ControlPoint && !UVEdge && !HasDifferentMaterial);
            }
        }

        bool showFirstU
        {
            get
            {
                var res = false;
                if (ControlPoint)
                    res = UVEdge || ExplicitU || HasDifferentMaterial;

                return res;
            }
        }

        bool showSecondU
        {
            get
            {
                return UVEdge || HasDifferentMaterial;
            }
        }

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

        

#if UNITY_EDITOR
        void OnValidate()
        {
            SetDirty();
        }
#endif
        
        public void Reset()
        {
            MaterialID = 0;
            HardEdge = false;
            MaxStepDistance = 0;
            UVEdge = false;
            ExplicitU = false;
            FirstU = 0;
            SecondU = 0;
        }

        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        public float GetDefinedFirstU(float defaultValue)
        {
            return (UVEdge || ExplicitU || HasDifferentMaterial) ? FirstU : defaultValue;
        }

        public float GetDefinedSecondU(float defaultValue)
        {
            return (UVEdge || HasDifferentMaterial) ? SecondU : GetDefinedFirstU(defaultValue);
        }

        public MetaCGOptions GetPreviousDefined(out CurvySplineSegment cp)
        {
            if (Spline.Closed && ControlPoint.IsFirstVisibleControlPoint)
            {
                cp = ControlPoint;
                return this;
            }
            cp = ControlPoint.PreviousControlPoint;
            MetaCGOptions res;
            while (cp && !cp.IsLastVisibleControlPoint)
            {
                res = cp.GetMetadata<MetaCGOptions>(true);
                if (res.UVEdge || res.ExplicitU || res.HasDifferentMaterial)
                    return res;
                cp = cp.PreviousControlPoint;
            }
            return null;
        }

        public MetaCGOptions GetNextDefined(out CurvySplineSegment cp)
        {
            cp = ControlPoint.NextControlPoint;
            MetaCGOptions res;
            while (cp)
            {
                res = cp.GetMetadata<MetaCGOptions>(true);
                
                if (res.UVEdge || res.ExplicitU || res.HasDifferentMaterial)
                    return res;
                if (Spline.Closed && cp.IsFirstVisibleControlPoint)
                    return null;
                cp = cp.NextControlPoint;
            }
            return null;
        }

        
        #endregion

        #region ### Privates ###
        /*! \cond PRIVATES */

       

        /*! \endcond */
        #endregion
    }
}
