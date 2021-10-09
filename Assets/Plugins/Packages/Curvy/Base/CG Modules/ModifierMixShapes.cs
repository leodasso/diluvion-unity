// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Modifier/Mix Shapes", ModuleName="Mix Shapes",Description = "Lerps between two shapes")]
    [HelpURL(CurvySpline.DOCLINK + "cgmixshapes")]
    public class ModifierMixShapes : CGModule, IOnRequestPath
    {
        [HideInInspector]
        [InputSlotInfo(typeof(CGShape), Name = "Shape A")]
        public CGModuleInputSlot InShapeA = new CGModuleInputSlot();

        [HideInInspector]
        [InputSlotInfo(typeof(CGShape), Name = "Shape B")]
        public CGModuleInputSlot InShapeB = new CGModuleInputSlot();

        [HideInInspector]
        [OutputSlotInfo(typeof(CGShape))]
        public CGModuleOutputSlot OutShape = new CGModuleOutputSlot();

        #region ### Serialized Fields ###

        [SerializeField, RangeEx(-1, 1, Tooltip = "Mix between the paths")]
        float m_Mix;

        #endregion

        #region ### Public Properties ###

        public float Mix
        {
            get { return m_Mix; }
            set
            {
                if (m_Mix != value)
                    m_Mix = value;
                Dirty = true;
            }
        }


        public float PathLength
        {
            get
            {
                return (IsConfigured) ? Mathf.Max((InShapeA.SourceSlot().OnRequestPathModule).PathLength,
                                                  (InShapeB.SourceSlot().OnRequestPathModule).PathLength) : 0;
            }
        }

        public bool PathIsClosed
        {
            get
            {
                return (IsConfigured) ? InShapeA.SourceSlot().OnRequestPathModule.PathIsClosed &&
                                        InShapeB.SourceSlot().OnRequestPathModule.PathIsClosed : false;
            }
        }

        #endregion

        #region ### Private Fields & Properties ###
        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

        protected override void OnEnable()
        {
            base.OnEnable();
            Properties.MinWidth = 200;
            Properties.LabelWidth = 50;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Mix = m_Mix;
        }
#endif

        public override void Reset()
        {
            base.Reset();
            Mix = 0;
        }

        /*! \endcond */
        #endregion

        #region ### IOnRequestProcessing ###
        public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
        {
            var raster = GetRequestParameter<CGDataRequestRasterization>(ref requests);
            if (!raster)
                return null;

            var DataA = InShapeA.GetData<CGShape>(requests);
            var DataB = InShapeB.GetData<CGShape>(requests);
            var data = new CGPath();
            CGShape A, B;
            if (DataA.Count > DataB.Count)
            {
                A = DataA;
                B = DataB;
            }
            else
            {
                A = DataB;
                B = DataA;
            }

            Vector3[] pa = new Vector3[A.Count];
            Vector3[] na = new Vector3[A.Count];
            float frag = (Mix + 1) / 2;

            for (int i = 0; i < A.Count; i++)
            {
                pa[i] = Vector3.Lerp(A.Position[i], B.InterpolatePosition(A.F[i]), frag);
                na[i] = Vector3.Lerp(A.Normal[i], B.Normal[i], frag);
            }

            data.F = A.F;
            data.Position = pa;
            data.Normal = na;
            return new CGData[1] { data };
        }
        #endregion

        #region ### Public Methods ###
        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */


        /*! \endcond */
        #endregion

   
        
    }
}
