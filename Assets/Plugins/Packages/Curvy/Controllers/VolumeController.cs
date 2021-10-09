// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.Curvy.Generator;
using FluffyUnderware.DevTools;
using FluffyUnderware.Curvy.Utils;


namespace FluffyUnderware.Curvy.Controllers
{
    /// <summary>
    /// Controller using a Curvy Generator Volume
    /// </summary>
    [AddComponentMenu("Curvy/Controller/CG Volume Controller",8)]
    [HelpURL(CurvySpline.DOCLINK + "volumecontroller")]
    public class VolumeController : CurvyController
    {

        #region ### Serialized Fields ###
        
        [Section("General")]
        [CGDataReferenceSelector(typeof(CGVolume), Label = "Volume/Slot")]
        [SerializeField]
        CGDataReference m_Volume = new CGDataReference();

        [Section("Cross Position", Sort = 1, HelpURL = CurvySpline.DOCLINK + "volumecontroller_crossposition")]
        [SerializeField]
        [FloatRegion(UseSlider = true, Precision = 4,RegionOptionsPropertyName="CrossRangeOptions",Options=AttributeOptionsFlags.Full)]
        FloatRegion m_CrossRange = new FloatRegion(-0.5f, 0.5f);
        
        [SerializeField]
        [RangeEx(-0.5f,0.5f)]
        float m_CrossInitialPosition;
        [SerializeField]
        CurvyClamping m_CrossClamping;

        #endregion

        #region ### Public Properties ###

        /// <summary>
        /// Gets or sets the volume to use
        /// </summary>
        public CGDataReference Volume
        {
            get { return m_Volume; }
            set
            {
                if (m_Volume != value)
                {
                    if (m_Volume != null)
                        UnbindEvents();

                    m_Volume = value;
                    if (m_Volume != null)
                    {
                        if (!mIsPrepared)
                            Prepare();
                        else
                            BindEvents();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the actual volume data
        /// </summary>
        public CGVolume VolumeData
        {
            get
            {
                return (Volume.HasValue) ? Volume.GetData<CGVolume>() : null;
            }
        }

        public float CrossFrom
        {
            get { return m_CrossRange.From; }
            set
            {
                float v = Mathf.Max(-0.5f, value);
                if (m_CrossRange.From != v)
                    m_CrossRange.From = v;
                if (!Application.isPlaying)
                    Prepare();
            }
        }

        public float CrossTo
        {
            get { return m_CrossRange.To; }
            set
            {
                float v = Mathf.Clamp(value,CrossFrom, 0.5f);
                
                if (m_CrossRange.To != v)
                    m_CrossRange.To = v;

                if (!Application.isPlaying)
                    Prepare();
            }
        }

        public float CrossLength
        {
            get { return m_CrossRange.Length; }
        }

        

        /// <summary>
        /// Gets or sets the initial lateral position
        /// </summary>
        public float CrossInitialPosition
        {
            get { return m_CrossInitialPosition; }
            set
            {
                float v = Mathf.Clamp01(value);

                if (m_CrossInitialPosition != v)
                {
                    m_CrossInitialPosition = v;
                    if (!Application.isPlaying)
                        Prepare();
                }
            }
        }

        /// <summary>
        /// Gets or sets the clamping mode for lateral movement
        /// </summary>
        public CurvyClamping CrossClamping
        {
            get { return m_CrossClamping; }
            set
            {
                if (m_CrossClamping != value)
                    m_CrossClamping = value;
            }
        }

        /// <summary>
        /// Gets or sets the current relative lateral position, respecting clamping
        /// </summary>
        public float CrossPosition
        {
            get
            {
                return getUnrangedCross(mCrossTF);
            }
            set
            {
                float v = getRangedCross(CurvyUtility.ClampValue(value, CrossClamping, -0.5f,0.5f));
                if (mCrossTF != v)
                {
                    mCrossTF = v;
                    if (Speed == 0)
                        Refresh();
                }
            }
        }

       

        /// <summary>
        /// Whether the controller is configured, i.e. all neccessary properties set
        /// </summary>
        public override bool IsConfigured { get { return Volume != null && !Volume.IsEmpty; } }

        /// <summary>
        /// Whether the controller dependencies are initialized
        /// </summary>
        public override bool DependenciesInitialized
        {
            get
            {
                return IsConfigured && Volume.HasValue;
            }
        }

        /// <summary>
        /// Gets the source's length
        /// </summary>
        public override float Length
        {
            get
            {
                return (VolumeData != null) ? VolumeData.Length : 0;
            }
        }

        #endregion

        #region ### Private fields ###

        float mKeepDistanceAt;
        float mCrossTF;

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */

        protected override void OnEnable()
        {
            base.OnEnable();
            BindEvents();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UnbindEvents();
        }

        IEnumerator Start()
        {
            if (IsConfigured)
            {
                while (!DependenciesInitialized)
                    yield return 0;

                Prepare();
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
        }
#endif

        protected override void Reset()
        {
            base.Reset();
            Volume = null;
        }

        /*! \endcond */
        #endregion

        #region ### Public Methods ###

        /// <summary>
        /// Initialize the controller and set starting position
        /// </summary>
        public override void Prepare()
        {
            BindEvents();
            if (IsInitialized)
            {
                mCrossTF = getRangedCross(CrossInitialPosition);
            }
            
            base.Prepare();
        }

        /// <summary>
        /// Converts relative lateral to absolute position, respecting clamping, ignoring CrossRange
        /// </summary>
        /// <param name="relativeDistance">the relative position</param>
        /// <returns>the absolute position</returns>
        public float CrossRelativeToAbsolute(float relativeDistance)
        {
            return (VolumeData != null) ? VolumeData.CrossFToDistance(GetTF(Position), relativeDistance, CrossClamping) : 0;
        }

        /// <summary>
        /// Converts absolute lateral to relative position, respecting clamping, ignoring CrossRange
        /// </summary>
        /// <param name="worldUnitDistance">the absolute position</param>
        /// <returns>the relative position</returns>
        public float CrossAbsoluteToRelative(float worldUnitDistance)
        {
            return (VolumeData != null) ? VolumeData.CrossDistanceToF(GetTF(Position), worldUnitDistance, CrossClamping) : 0;
        }

        #endregion

        #region ### Protected Methods ###

        /// <summary>
        /// Converts distance on source from relative to absolute position, respecting Clamping
        /// </summary>
        /// <param name="relativeDistance">relative distance (TF) from the source start</param>
        /// <returns>
        /// distance in world units from the source start
        /// </returns>
        protected override float RelativeToAbsolute(float relativeDistance)
        {
            return (VolumeData != null) ? VolumeData.FToDistance(relativeDistance) : 0;
        }

        /// <summary>
        /// Converts distance on source from absolute to relative position, respecting Clamping
        /// </summary>
        /// <param name="worldUnitDistance">distance in world units from the source start</param>
        /// <returns>
        /// relative distance (TF) in the range 0..1
        /// </returns>
        protected override float AbsoluteToRelative(float worldUnitDistance)
        {
            return (VolumeData != null) ? VolumeData.DistanceToF(worldUnitDistance) : 0;
        }

        /// <summary>
        /// Retrieve the source position for a given relative position (TF)
        /// </summary>
        /// <param name="tf">relative position</param>
        /// <returns>
        /// a world position
        /// </returns>
        protected override Vector3 GetInterpolatedSourcePosition(float tf)
        {
            return (Space == Space.World) ? Volume.Module.Generator.transform.TransformPoint(VolumeData.InterpolateVolumePosition(tf, mCrossTF)) : VolumeData.InterpolateVolumePosition(tf, mCrossTF);
        }

        /// <summary>
        /// Gets the interpolated source position.
        /// </summary>
        /// <param name="tf">The tf.</param>
        /// <param name="position">The position.</param>
        /// <param name="tangent">The tangent.</param>
        /// <param name="up">Up.</param>
        protected override void GetInterpolatedSourcePosition(float tf, out Vector3 position, out Vector3 tangent, out Vector3 up)
        {
            VolumeData.InterpolateVolume(tf, mCrossTF, out position, out tangent, out up);
            if (Space == Space.World)
            {
                position = Volume.Module.Generator.transform.TransformPoint(position);
                tangent = Volume.Module.Generator.transform.TransformDirection(tangent);
                up = Volume.Module.Generator.transform.TransformDirection(up);
            }
        }

        /// <summary>
        /// Gets tangent for a given relative position
        /// </summary>
        /// <param name="tf">relative position</param>
        /// <returns></returns>
        protected override Vector3 GetTangent(float tf)
        {
            return (Space == Space.World) ? Volume.Module.Generator.transform.TransformDirection(VolumeData.InterpolateVolumeDirection(tf, mCrossTF)) : VolumeData.InterpolateVolumeDirection(tf, mCrossTF);
        }

        /// <summary>
        /// Retrieve the source Orientation/Up-Vector for a given relative position
        /// </summary>
        /// <param name="tf">relative position</param>
        /// <returns>
        /// the Up-Vector
        /// </returns>
        protected override Vector3 GetOrientation(float tf)
        {
            return (Space == Space.World) ? Volume.Module.Generator.transform.TransformDirection(VolumeData.InterpolateVolumeUp(tf, mCrossTF)) : VolumeData.InterpolateVolumeUp(tf, mCrossTF);
        }

        /// <summary>
        /// Advance the controller and return the new position
        /// </summary>
        /// <param name="virtualPosition">the current virtual position (either TF or World Units) </param>
        /// <param name="direction">the current direction</param>
        /// <param name="mode">movement mode</param>
        /// <param name="absSpeed">speed, always positive</param>
        /// <param name="clamping">clamping mode</param>
        protected override void Advance(ref float virtualPosition, ref int direction, MoveModeEnum mode, float absSpeed, CurvyClamping clamping)
        {
            switch (mode)
            {
                case MoveModeEnum.Relative:
                    VolumeData.Move(ref virtualPosition, ref direction, absSpeed, clamping);
                    break;
                default:
                    VolumeData.MoveBy(ref virtualPosition, ref direction, absSpeed, clamping);
                    break;
            }
        }

        /// <summary>
        /// Binds any external events
        /// </summary>
        protected override void BindEvents()
        {
            base.BindEvents();
            if (Volume != null && Volume.Module != null)
            {
                UnbindEvents();
                Volume.Module.OnRefresh.AddListener(OnRefreshPath);
            }
        }

        /// <summary>
        /// Unbinds any external events
        /// </summary>
        protected override void UnbindEvents()
        {
            if (Volume != null && Volume.Module != null)
            {
                Volume.Module.OnRefresh.RemoveListener(OnRefreshPath);
            }
        }

        protected virtual void OnRefreshPath(CurvyCGEventArgs e)
        {
            if (Volume == null || e.Module != Volume.Module)
                e.Module.OnRefresh.RemoveListener(OnRefreshPath);
            else
            {
                if (Active)
                {
                    if (Application.isPlaying)
                    {
                        if (mKeepDistanceAt > 0 && IsInitialized && IsPlaying)
                        {
                            AbsolutePosition = mKeepDistanceAt;
                            mKeepDistanceAt = 0;
                        }
                        if (Speed == 0)
                            Refresh();
                    }
                    else if (IsInitialized) // Align if not moving
                        Prepare();
                }
                else
                    UnbindEvents();
            }
        }

        #endregion

        #region ### Privates & Internals ###
        /*! \cond PRIVATE */

        RegionOptions<float> CrossRangeOptions
        {
            get
            {
                return RegionOptions<float>.MinMax(-0.5f,0.5f);
            }
        }

        float getRangedCross(float f)
        {
            return DTMath.MapValue(CrossFrom, CrossTo, f, -0.5f, 0.5f);
        }

        float getUnrangedCross(float f)
        {
            return DTMath.MapValue(-0.5f, 0.5f, f, CrossFrom, CrossTo);
        }
        

        /*! \endcond */
        #endregion
    }
}
