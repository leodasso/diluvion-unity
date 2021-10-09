// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Shapes;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy.Generator.Modules
{

    [ModuleInfo("Input/Spline Shape", ModuleName="Input Spline Shape",Description = "Spline Shape")]
    [HelpURL(CurvySpline.DOCLINK + "cginputsplineshape")]
    public class InputSplineShape : SplineInputModuleBase, IExternalInput, IOnRequestPath
    {
        [HideInInspector]
        [OutputSlotInfo(typeof(CGShape))]
        public CGModuleOutputSlot OutShape = new CGModuleOutputSlot();

        #region ### Serialized Fields ###

        [Tab("General",Sort=0)]
        [SerializeField, CGResourceManager("Shape")]
        CurvySpline m_Shape;

        #endregion

        #region ### Public Properties ###

        public CurvySpline Shape
        {
            get { return m_Shape; }
            set
            {
                if (m_Shape != value)
                    m_Shape = value;
                if (m_Shape)
                {
                    m_Shape.OnRefresh.AddListenerOnce(m_Shape_OnRefresh);
                    m_Shape.RestrictTo2D = true;
                    if (StartCP && StartCP.Spline != m_Shape)
                    {
                        StartCP = null;
                        EndCP = null;
                    }
                }
                Dirty = true;
            }
        }

        public bool FreeForm
        {
            get
            {
                return (Shape!=null && Shape.GetComponent<CurvyShape>() == null);
            }
            set
            {
                if (Shape != null)
                {
                    CurvyShape sh = Shape.GetComponent<CurvyShape>();
                    if (value && sh != null)
                        sh.Delete();
                    else if (!value && sh == null)
                        Shape.gameObject.AddComponent<CSCircle>();
                }

            }
        }

        public override bool IsInitialized
        {
            get
            {
                return base.IsInitialized && (Shape==null || Shape.IsInitialized);
            }
        }

        public override bool IsConfigured
        {
            get
            {
                return base.IsConfigured && Shape != null;
            }
        }

        public float PathLength
        {
            get { return (IsConfigured) ? getPathLength(m_Shape) : 0; }
        }

        public bool PathIsClosed
        {
            get { return (IsConfigured) ? getPathClosed(m_Shape) : false; }
        }

        public bool SupportsIPE { get { return FreeForm; } }

        #endregion

        #region ### Private Fields & Properties ###
        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

        protected override void OnEnable()
        {
            base.OnEnable();
            Shape = m_Shape;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Shape = m_Shape;
        }
#endif

        public override void Reset()
        {
            base.Reset();
            Shape = null;
        }
        /*! \endcond */
        #endregion

        #region ### IOnRequestPath ###
        public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
        {
            var raster = GetRequestParameter<CGDataRequestRasterization>(ref requests);
            var options = GetRequestParameter<CGDataRequestMetaCGOptions>(ref requests);

            if (!raster || raster.Length == 0)
                return null;
            var data = GetSplineData(Shape, false, raster, options);

            return new CGData[1] { data };

        }

        #endregion

        #region ### Public Methods ###

        public T SetManagedShape<T>() where T:CurvyShape2D
        {
            if (!Shape)
                Shape=(CurvySpline)AddManagedResource("Shape");

            CurvyShape sh = Shape.GetComponent<CurvyShape>();
            
            if (sh != null)
                sh.Delete();
            return Shape.gameObject.AddComponent<T>();
        }

        public void RemoveManagedShape()
        {
            if (Shape)
                DeleteManagedResource("Shape", Shape);
        }

        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */

        void m_Shape_OnRefresh(CurvySplineEventArgs e)
        {
            if (!enabled || !gameObject.activeInHierarchy)
                return;
            if (m_Shape == e.Spline)
            {
                Dirty = true;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (IsManagedResource(Shape))
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
                e.Spline.OnRefresh.RemoveListener(m_Shape_OnRefresh);
        }

        /*! \endcond */
        #endregion

  
        

        

        


        
    }
}
