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
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FluffyUnderware.Curvy.Generator.Modules
{
    [ModuleInfo("Input/Spline Path", ModuleName="Input Spline Path",Description = "Spline Path")]
    [HelpURL(CurvySpline.DOCLINK + "cginputsplinepath")]
    public class InputSplinePath : SplineInputModuleBase, IExternalInput,IOnRequestPath
    {
        [HideInInspector]
        [OutputSlotInfo(typeof(CGPath))]
        public CGModuleOutputSlot Path = new CGModuleOutputSlot();

        #region ### Serialized Fields ###

        [Tab("General",Sort=0)]
        [SerializeField]
        [CGResourceManager("Spline")]
        [FieldCondition("m_Spline", null, false, ActionAttribute.ActionEnum.ShowWarning, "Create or assign spline")]
        CurvySpline m_Spline;
        
        #endregion

        #region ### Public Properties ###

        public CurvySpline Spline
        {
            get { return m_Spline; }
            set
            {
                if (m_Spline != value)
                    m_Spline = value;

                if (m_Spline)
                {
                    m_Spline.OnRefresh.AddListenerOnce(m_Spline_OnRefresh);
                    if (StartCP && StartCP.Spline != m_Spline)
                    {
                        StartCP = null;
                        EndCP = null;
                    }
                }

                Dirty = true;
            }
        }

        public override bool IsInitialized
        {
            get
            {
                return base.IsInitialized && (Spline==null || Spline.IsInitialized);
            }
        }

        public override bool IsConfigured
        {
            get
            {
                return base.IsConfigured && Spline != null;
            }
        }

        public bool SupportsIPE { get { return false; } }

        public float PathLength
        {
            get { return (IsConfigured) ? getPathLength(m_Spline) : 0; }
        }

        public bool PathIsClosed
        {
            get { return (IsConfigured) ? getPathClosed(m_Spline) : false; }
        }

        #endregion

        #region ### Private Fields & Properties ###
        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */
        
        protected override void OnEnable()
        {
            base.OnEnable();
            if (Spline)
                Spline.OnRefresh.AddListenerOnce(m_Spline_OnRefresh);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (Spline)
                Spline.OnRefresh.RemoveListener(m_Spline_OnRefresh);
        }
        

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Spline = m_Spline;
        }
#endif

        public override void Reset()
        {
            base.Reset();
            Spline = null;
        }

        /*! \endcond */
        #endregion

        #region ### IOnRequestModule ###

        public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
        {
            var raster = GetRequestParameter<CGDataRequestRasterization>(ref requests);
            var options = GetRequestParameter<CGDataRequestMetaCGOptions>(ref requests);
            if (options)
            {
                if (options.CheckMaterialID)
                {
                    options.CheckMaterialID = false;
                    UIMessages.Add("MaterialID option not supported!");
                }
                if (options.IncludeControlPoints)
                {
                    options.IncludeControlPoints = false;
                    UIMessages.Add("IncludeCP option not supported!");
                }
            }
            if (!raster || raster.Length == 0)
                return null;

            var data = GetSplineData(Spline, true, raster, options);

            return new CGData[1] { data };
        }

        #endregion

        #region ### Public Methods ###

        public override void Refresh()
        {
            base.Refresh();
        }

        public override void OnTemplateCreated()
        {
            base.OnTemplateCreated();
            if (Spline && !IsManagedResource(Spline))
            {
                Spline = null;
            }
        }

        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */

        void m_Spline_OnRefresh(CurvySplineEventArgs e)
        {
            if (!enabled || !gameObject.activeInHierarchy)
                return;
            if (m_Spline == e.Spline)
            {
                Dirty = true;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (IsManagedResource(Spline))
                    {
                        Generator.CancelInvoke("Update");
                        Generator.Invoke("Update", 0);
                    }
                    else
                        Generator.Refresh();
                }
#endif
            }
            else
                e.Spline.OnRefresh.RemoveListener(m_Spline_OnRefresh);
        }

        /*! \endcond */
        #endregion
   



        

        

     



        

       

       
        
    }
}
