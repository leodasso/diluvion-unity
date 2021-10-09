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
    [ModuleInfo("Modifier/Mix Paths", ModuleName="Mix Paths", Description = "Lerps between two paths")]
    [HelpURL(CurvySpline.DOCLINK + "cgmixpaths")]
    public class ModifierMixPaths : CGModule, IOnRequestPath
    {
        [HideInInspector]
        [InputSlotInfo(typeof(CGPath), Name = "Path A")]
        public CGModuleInputSlot InPathA = new CGModuleInputSlot();

        [HideInInspector]
        [InputSlotInfo(typeof(CGPath), Name = "Path B")]
        public CGModuleInputSlot InPathB = new CGModuleInputSlot();

        [HideInInspector]
        [OutputSlotInfo(typeof(CGPath))]
        public CGModuleOutputSlot OutPath = new CGModuleOutputSlot();

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
                return (IsConfigured) ? Mathf.Max((InPathA.SourceSlot().OnRequestPathModule).PathLength,
                                                  (InPathB.SourceSlot().OnRequestPathModule).PathLength) : 0;
            }
        }

        public bool PathIsClosed
        {
            get
            {
                return (IsConfigured) ? InPathA.SourceSlot().OnRequestPathModule.PathIsClosed &&
                                        InPathB.SourceSlot().OnRequestPathModule.PathIsClosed : false;
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

            var DataA = InPathA.GetData<CGPath>(requests);
            var DataB = InPathB.GetData<CGPath>(requests);

            return new CGData[1] { MixPath(DataA, DataB, Mix) };
        }
        #endregion

        #region ### Public Static Methods ###

        public static CGPath MixPath(CGPath pathA, CGPath pathB, float mix)
        {
            var data = new CGPath();

            CGPath A, B;
            if (pathA.Count > pathB.Count)
            {
                A = pathA;
                B = pathB;
            }
            else
            {
                A = pathB;
                B = pathA;
            }

            Vector3[] pa = new Vector3[A.Count];
            Vector3[] ta = new Vector3[A.Count];
            Vector3[] upa = new Vector3[A.Count];
            Vector3 p, t, u;
            float frag = (mix + 1) / 2;

            for (int i = 0; i < A.Count; i++)
            {
                B.Interpolate(A.F[i], out p, out t, out u);
                pa[i] = Vector3.Lerp(A.Position[i], p, frag);
                ta[i] = Vector3.Lerp(A.Direction[i], t, frag);
                upa[i] = Vector3.Lerp(A.Normal[i], u, frag);
            }
            data.F = A.F;
            data.Position = pa;
            data.Direction = ta;
            data.Normal = upa;
            data.Length = Mathf.Lerp(A.Length, B.Length, frag);

            return data;
        }

        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */


        /*! \endcond */
        #endregion



      

        
    }
}
