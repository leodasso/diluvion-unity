// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// Base class for TRS Modules
    /// </summary>
    public class TRSModuleBase : CGModule
    {

        #region ### Serialized Fields ###

        [SerializeField]
        [VectorEx]
        Vector3 m_Transpose;
        [SerializeField]
        [VectorEx]
        Vector3 m_Rotation;
        [SerializeField]
        [VectorEx]
        Vector3 m_Scale = Vector3.one;

        #endregion

        #region ### Public Properties ###

        public Vector3 Transpose
        {
            get { return m_Transpose; }
            set
            {
                if (m_Transpose != value)
                    m_Transpose = value;
                Dirty = true;
            }
        }

        public Vector3 Rotation
        {
            get { return m_Rotation; }
            set
            {
                if (m_Rotation != value)
                    m_Rotation = value;
                Dirty = true;
            }
        }

        public Vector3 Scale
        {
            get { return m_Scale; }
            set
            {
                if (m_Scale != value)
                    m_Scale = value;
                Dirty = true;
            }
        }

        public Matrix4x4 Matrix
        {
            get { return Matrix4x4.TRS(Transpose, Quaternion.Euler(Rotation), Scale); }
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
            Transpose = m_Transpose;
            Rotation = m_Rotation;
            Scale = m_Scale;
        }
#endif

        /*! \endcond */
        #endregion

      
        

    }

}
