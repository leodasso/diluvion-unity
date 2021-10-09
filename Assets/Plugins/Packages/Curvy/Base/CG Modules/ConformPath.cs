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
    [ModuleInfo("Modifier/Conform Path",ModuleName="Conform Path", Description="Projects a path")]
    [HelpURL(CurvySpline.DOCLINK + "cgconformpath")]
    public class ConformPath : CGModule, IOnRequestPath
    {

        [HideInInspector]
        [InputSlotInfo(typeof(CGPath), Name = "Path", ModifiesData = true)]
        public CGModuleInputSlot InPath = new CGModuleInputSlot();

        [HideInInspector]
        [OutputSlotInfo(typeof(CGPath))]
        public CGModuleOutputSlot OutPath = new CGModuleOutputSlot();

        #region ### Serialized Fields ###

        [SerializeField]
        [VectorEx]
        Vector3 m_Direction = new Vector3(0, -1, 0);
        [SerializeField]
        float m_MaxDistance = 100;
        [SerializeField]
        float m_Offset;
        [SerializeField]
        bool m_Warp;
        [SerializeField]
        LayerMask m_LayerMask;
        
        #endregion

        #region ### Public Properties ###

        public Vector3 Direction
        {
            get
            {
                return m_Direction;
            }
            set
            {
                if (m_Direction != value)
                    m_Direction = value;
                Dirty = true;
            }
        }

        public float MaxDistance
        {
            get { return m_MaxDistance; }
            set
            {
                if (m_MaxDistance != value)
                    m_MaxDistance = value;
                Dirty = true;
            }
        }

        public float Offset
        {
            get { return m_Offset; }
            set
            {
                if (m_Offset != value)
                    m_Offset = value;
                Dirty = true;
            }
        }

        public bool Warp
        {
            get { return m_Warp; }
            set
            {
                if (m_Warp != value)
                    m_Warp = value;
                Dirty = true;
            }
        }

        public LayerMask LayerMask
        {
            get { return m_LayerMask; }
            set
            {
                if (m_LayerMask != value)
                    m_LayerMask = value;
                Dirty = true;
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
            //Properties.MinWidth = 250;
            Properties.LabelWidth = 80;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Direction = m_Direction;
            MaxDistance = m_MaxDistance;
            Offset = m_Offset;
            LayerMask = m_LayerMask;
        }
#endif

        public override void Reset()
        {
            base.Reset();
            Direction = new Vector3(0, -1, 0);
            MaxDistance = 100;
            Offset = 0;
            Warp = false;
            LayerMask = 0;
        }


        /*! \endcond */
        #endregion

        #region ### IOnRequestProcessing ###

        public float PathLength
        {
            get
            {
                if (OutPath.HasData)
                    return OutPath.GetData<CGPath>().Length;
                else
                    return (IsConfigured) ? InPath.SourceSlot().OnRequestPathModule.PathLength : 0;
            }
        }

        public bool PathIsClosed
        {
            get
            {
                return (IsConfigured) ? InPath.SourceSlot().OnRequestPathModule.PathIsClosed : false;
            }
        }

        public CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests)
        {
            var raster = GetRequestParameter<CGDataRequestRasterization>(ref requests);
            if (!raster)
                return null;

            var Data = InPath.GetData<CGPath>(requests);
            return new CGData[1] { Conform(Generator.transform, Data, LayerMask, Direction, Offset, MaxDistance, Warp) };
        }

        public static CGPath Conform(Transform refTransform, CGPath path, LayerMask layers, Vector3 dir, float offset, float rayLength, bool warp)
        {
            if (dir != Vector3.zero && rayLength > 0)
            {
                if (warp)
                {
                    float minDist = float.MaxValue;

                    for (int i = 0; i < path.Count; i++)
                    {
                        RaycastHit hit;
                        Ray R = new Ray(refTransform.TransformPoint(path.Position[i]), dir);
                        if (Physics.Raycast(R, out hit, rayLength, layers))
                        {
                            if (hit.distance < minDist)
                                minDist = hit.distance;
                        }
                    }
                    if (minDist != float.MaxValue)
                    {
                        for (int i = 0; i < path.Count; i++)
                            path.Position[i] += dir * (minDist + offset);
                    }
                    path.Touch();
                    //path.Recalculate();
                }
                else
                {

                    int hi = path.Count;

                    for (int i = 0; i < hi; i++)
                    {
                        RaycastHit hit;
                        Ray R = new Ray(refTransform.TransformPoint(path.Position[i]), dir);
                        if (Physics.Raycast(R, out hit, rayLength, layers))
                        {
                            path.Position[i] += dir * (hit.distance + offset);
                        }
                    }
                    //path.Recalculate();

                }
            }
            return path;
        }

        #endregion

        #region ### Privates ###
        /*! \cond PRIVATE */


        /*! \endcond */
        #endregion

		
    
    }
}
