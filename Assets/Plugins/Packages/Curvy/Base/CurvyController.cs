// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.Curvy.Utils;
using FluffyUnderware.DevTools;
using FluffyUnderware.DevTools.Extensions;
using System.Reflection;

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Controller base class
    /// </summary>
    [ExecuteInEditMode]
    public class CurvyController : DTVersionedMonoBehaviour
    {
        #region ### Enums ###
        /// <summary>
        /// Movement method options
        /// </summary>
        public enum MoveModeEnum
        {
            /// <summary>
            /// Move by Percentage or TF (SplineController only)
            /// </summary>
            Relative,
            /// <summary>
            /// Move by extrapolated distance
            /// </summary>
            AbsoluteExtrapolate,
            /// <summary>
            /// Move by calculated distance
            /// </summary>
            AbsolutePrecise
        }

        #endregion

        #region ### Events ###

        /// <summary>
        /// Event raised when animation sequence has ended
        /// </summary>
        public CurvyControllerEvent OnAnimationEnd
        {
            get { return m_OnAnimationEnd; }
            set
            {
                if (m_OnAnimationEnd != value)
                    m_OnAnimationEnd = value;
            }
        }

        #endregion

        #region ### Serialized Fields ###

        [Section("General", Sort = 0, HelpURL = CurvySpline.DOCLINK + "curvycontroller_general")]
        [Label(Tooltip = "Determines when to update")]
        public CurvyUpdateMethod UpdateIn = CurvyUpdateMethod.Update; // when to update?
        [SerializeField]
        Space m_Space;
        [Section("Position", HelpURL = CurvySpline.DOCLINK + "curvycontroller_position")]
        [SerializeField]
        CurvyPositionMode m_PositionMode;
        [RangeEx(0,"maxPosition")]
        [SerializeField]
        float m_InitialPosition;
        
        [Section("Move", HelpURL = CurvySpline.DOCLINK + "curvycontroller_move")]
        
        [SerializeField]
        MoveModeEnum m_MoveMode = MoveModeEnum.AbsolutePrecise;
        [SerializeField]
        float m_Speed=0;
        [SerializeField]
        CurvyClamping m_Clamping = CurvyClamping.Loop;
        [SerializeField]
        bool m_PlayAutomatically = true;
        [SerializeField]
        bool m_AdaptOnChange = true;
        [SerializeField]
        bool m_Animate=false;
        [FieldCondition("m_Animate",true)]
        [SerializeField]
        AnimationCurve m_Animation=AnimationCurve.EaseInOut(0,0,1,1);
        [FieldCondition("m_Animate", true)]
        [SerializeField]
        float m_TimeScale = 1;
        [FieldCondition("m_Animate", true)]
        [SerializeField]
        bool m_SingleShot;
        [FieldCondition("m_Animate", true)]
        [SerializeField]
        bool m_ResetOnStop = false;
        

        [Section("Orientation & Offset",HelpURL=CurvySpline.DOCLINK+"curvycontroller_orientation")]
        [Label("Source","Source Vector")]
        [SerializeField]
        OrientationModeEnum m_OrientationMode = OrientationModeEnum.Orientation;
        [Label("Target", "Target Vector3")]
        [SerializeField]
        OrientationAxisEnum m_OrientationAxis = OrientationAxisEnum.Up;
        [Positive]
        [SerializeField]
        float m_DampingDirection;
        [Positive]
        [SerializeField]
        float m_DampingUp;
        [Tooltip("Ignore direction when moving backwards?")]
        [SerializeField]
        bool m_IgnoreDirection;
        [FieldCondition("m_OrientationMode",OrientationModeEnum.None,Action=ActionAttribute.ActionEnum.Hide)]
        [RangeEx(-180f, 180f)]
        [SerializeField]
        float m_OffsetAngle;
        [FieldCondition("m_OrientationMode", OrientationModeEnum.None, Action = ActionAttribute.ActionEnum.Hide)]
        [SerializeField]
        float m_OffsetRadius;
        [FieldCondition("m_OrientationMode", OrientationModeEnum.None, Action = ActionAttribute.ActionEnum.Hide)]
        [Label("Compensate Offset")]
        [SerializeField]
        bool m_OffsetCompensation = true;

        [Section("Events")]
        [SerializeField]
        protected CurvyControllerEvent m_OnAnimationEnd = new CurvyControllerEvent();

        #endregion

        #region ### Public Properties ###

        /// <summary>
        /// Gets or sets whether to use local or global space
        /// </summary>
        public Space Space
        {
            get { return m_Space; }
            set
            {
                if (m_Space != value)
                    m_Space = value;
            }
        }

        /// <summary>
        /// Gets or sets the position mode to use
        /// </summary>
        public CurvyPositionMode PositionMode
        {
            get { return m_PositionMode; }
            set
            {
                if (m_PositionMode != value)
                {
                    m_PositionMode = value;
                    if (!Application.isPlaying)
                        Prepare();
                }
            }
        }

        /// <summary>
        /// Gets or sets the movement mode to use
        /// </summary>
        public MoveModeEnum MoveMode
        {
            get { return m_MoveMode; }
            set
            {
                if (m_MoveMode != value)
                    m_MoveMode = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to start playing automatically
        /// </summary>
        public bool PlayAutomatically
        {
            get { return m_PlayAutomatically; }
            set
            {
                if (m_PlayAutomatically != value)
                    m_PlayAutomatically = value;
            }
        }

       
        /// <summary>
        /// Gets or sets whether to keep position when the source length changes
        /// </summary>
        public virtual bool AdaptOnChange
        {
            get { return m_AdaptOnChange; }
            set
            {
                if (m_AdaptOnChange != value)
                    m_AdaptOnChange = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to animate the movement
        /// </summary>
        public bool Animate
        {
            get { return m_Animate; }
            set
            {
                if (m_Animate != value)
                    m_Animate = value;
            }
        }

        /// <summary>
        /// Gets or sets the movement animation curve to apply
        /// </summary>
        public AnimationCurve Animation
        {
            get { return m_Animation; }
            set
            {
                if (m_Animation != value)
                    m_Animation = value;
            }
        }

        /// <summary>
        /// Gets or sets the duration the animation curve refers to
        /// </summary>
        public float TimeScale
        {
            get { return m_TimeScale; }
            set
            {
                if (m_TimeScale != value)
                    m_TimeScale = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to play the animation just once
        /// </summary>
        public bool SingleShot
        {
            get { return m_SingleShot; }
            set
            {
                if (m_SingleShot != value)
                    m_SingleShot = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to reset to the initial position on stop
        /// </summary>
        public bool ResetOnStop
        {
            get { return m_ResetOnStop; }
            set
            {
                if (m_ResetOnStop != value)
                    m_ResetOnStop = value;
            }
        }

        /// <summary>
        /// Gets or sets what to do when the source's end is reached
        /// </summary>
        public CurvyClamping Clamping
        {
            get { return m_Clamping; }
            set
            {
                if (m_Clamping != value)
                    m_Clamping = value;
            }
        }

        /// <summary>
        /// Gets or sets how to apply rotation
        /// </summary>
        public OrientationModeEnum OrientationMode
        {
            get { return m_OrientationMode; }
            set
            {
                if (m_OrientationMode != value)
                    m_OrientationMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the axis to apply the rotation to
        /// </summary>
        public OrientationAxisEnum OrientationAxis
        {
            get { return m_OrientationAxis; }
            set
            {
                if (m_OrientationAxis != value)
                    m_OrientationAxis = value;
                
            }
        }

        /// <summary>
        /// Gets or sets the time direction change is applied over
        /// </summary>
        public float DampingDirection
        {
            get { return m_DampingDirection;}
            set
            {
                float v = Mathf.Max(0, value);
                if (m_DampingDirection != v)
                    m_DampingDirection = v;
            }
        }

        /// <summary>
        /// Gets or sets the time orientation change is applied over
        /// </summary>
        public float DampingUp
        {
            get { return m_DampingUp; }
            set
            {
                float v = Mathf.Max(0, value);
                if (m_DampingUp != v)
                    m_DampingUp = v;
            }
        }

        

        /// <summary>
        /// Gets or sets whether to ignore direction when moving backwards or to rotate 180�
        /// </summary>
        public bool IgnoreDirection
        {
            get { return m_IgnoreDirection; }
            set
            {
                if (m_IgnoreDirection != value)
                    m_IgnoreDirection = value;
            }
        }

        /// <summary>
        /// Gets or sets the angle to offset (-180° to 180° off Orientation)
        /// </summary>
        public float OffsetAngle
        {
            get { return m_OffsetAngle; }
            set
            {
                if (m_OffsetAngle != value)
                    m_OffsetAngle = value;
            }
        }
        /// <summary>
        /// Gets or sets the offset radius
        /// </summary>
        public float OffsetRadius
        {
            get { return m_OffsetRadius; }
            set
            {
                if (m_OffsetRadius != value)
                    m_OffsetRadius = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to compensate offset distances in curvy paths
        /// </summary>
        public bool OffsetCompensation
        {
            get { return m_OffsetCompensation; }
            set
            {
                if (m_OffsetCompensation != value)
                    m_OffsetCompensation = value;
                if (!m_OffsetCompensation)
                    mOffsetCompensation = 1f;
            }
        }


        /// <summary>
        /// Gets or sets the initial/starting position
        /// </summary>
        public virtual float InitialPosition
        {
            get { return m_InitialPosition; }
            set
            {
                float v = m_InitialPosition;
                switch (PositionMode)
                {
                    case CurvyPositionMode.Relative:
                        v = CurvyUtility.ClampTF(value,Clamping);//Mathf.Clamp01(value);
                        break;
                    case CurvyPositionMode.WorldUnits:
                        if (IsInitialized)
                            v = CurvyUtility.ClampDistance(value,Clamping,Length);//Mathf.Clamp(value, 0, Length);
                        break;
                }

                if (m_InitialPosition != v)
                {
                    m_InitialPosition = v;
                    
                    if (!IsPlaying)
                        Prepare();
                }
                mInitialVirtualPos = v;

                
            }
        }

        /// <summary>
        /// Gets or sets the speed either in world units or relative, depending on MoveMode
        /// </summary>
        public virtual float Speed
        {
            get { return m_Speed; }
            set
            {
                if (m_Speed==0 && value!=0)
                    mDirection = (int)Mathf.Sign(value);
                if (m_Speed != value)
                    m_Speed = value;

                
                mForceUpdate = true;
            }
        }

        /// <summary>
        /// Gets or sets the relative position on the source, respecting Clamping
        /// </summary>
        public virtual float RelativePosition
        {
            get { return mTF; }
            set
            {
                if (mTF != value)
                {
                    mTF = CurvyUtility.ClampTF(value, Clamping);
                    mForceUpdate = true;
                    if (Animate && IsPlaying)
                        Stop();
                }
            }
        }

        /// <summary>
        /// Gets or sets the absolute position on the source, respecting Clamping
        /// </summary>
        public virtual float AbsolutePosition
        {
            get
            {
                return RelativeToAbsolute(mTF);
            }
            set
            {
                float v = AbsoluteToRelative(value);
                if (mTF != v)
                {
                    
                    mTF = v;
                    mForceUpdate = true;
                    if (Animate && IsPlaying)
                        Stop();
                }
            }
        }

        /// <summary>
        /// Gets or sets the position on the source (relative or absolute, depending on MoveMode), respecting Clamping
        /// </summary>
        public virtual float Position
        {
            get
            {
                switch (MoveMode)
                {
                    case MoveModeEnum.Relative:
                        return mTF;
                    default:
                        return RelativeToAbsolute(mTF);
                }
            }
            set
            {
                float v;
                switch (MoveMode)
                {
                    case MoveModeEnum.Relative:
                        v = CurvyUtility.ClampTF(value, Clamping);
                        break;
                    default:
                        v = AbsoluteToRelative(value);
                        break;
                }
                if (mTF != v)
                {
                    mTF = v;
                    mForceUpdate = true;
                    if (Animate && IsPlaying)
                        Stop();
                }
            }
        }
        /// <summary>
        /// Gets whether the GameObject is active (Shortcut to activeInHierarchy)
        /// </summary>
        public bool Active
        {
            get { return (mGameObject != null && mGameObject.activeInHierarchy); }
        }

        /// <summary>
        /// Gets Time.deltaTime - even in the editor!
        /// </summary>
        public float DeltaTime
        {
            get { return DTTime.deltaTime; }
        }

        /// <summary>
        /// Shortcut to Mathf.Abs(Speed)
        /// </summary>
        public float AbsSpeed
        {
            get
            {
                return Mathf.Abs(Speed)*mOffsetCompensation;
            }
        }

        /// <summary>
        /// Gets whether the controller is playing
        /// </summary>
        public bool IsPlaying
        {
            get { return mIsPlaying; }
        }
        /// <summary>
        /// Gets whether the controller is paused
        /// </summary>
        public bool IsPaused
        {
            get { return mIsPausing; }
        }

        /// <summary>
        /// Gets or sets the movement direction
        /// </summary>
        public int Direction
        {
            get { return mDirection; }
            set
            {
                int v=(int)Mathf.Sign(value);
                if (mDirection != v)
                    mDirection = v;
            }
        }

       

        #endregion

        #region ### Private Fields ###

        /// <summary>
        /// Relative Position while moving
        /// </summary>
        float mTF;
        /// <summary>
        /// Direction while moving, may change for some clamping modes
        /// </summary>
        /// <remarks>Initially this is setup with Mathf.Sign(Speed)</remarks>
        int mDirection;

        bool mForceUpdate;

        float mInitialVirtualPos;
        protected bool mIsPrepared;
        bool mIsPlaying;
        bool mIsPausing;
        float mShotTime;
        float mShotStartTF;
        float mShotStartDistance;
        GameObject mGameObject;
        Vector3 mDampingDirVelocity;
        Vector3 mDampingUpVelocity;

        protected float mOffsetCompensation = 1;
        Vector3 oldPos;
        Vector3 oldOff;

        #endregion

        #region ### Unity Callbacks ###
        /*! \cond UNITY */
        protected virtual void OnEnable()
        {
            mGameObject = gameObject;
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void Update()
        {
            if (UpdateIn == CurvyUpdateMethod.Update && Application.isPlaying && (Speed!=0 || mForceUpdate))
                Refresh();
        }

        protected virtual void LateUpdate()
        {
            if (UpdateIn == CurvyUpdateMethod.LateUpdate && (Speed != 0 || mForceUpdate))
                Refresh();
        }

        protected virtual void FixedUpdate()
        {
            if (UpdateIn == CurvyUpdateMethod.FixedUpdate && (Speed != 0 || mForceUpdate))
                Refresh();
        }

        protected virtual void OnTransformParentChanged()
        {
            Prepare();
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            PositionMode = m_PositionMode;
            InitialPosition = m_InitialPosition;
            Speed = m_Speed;
            OffsetCompensation = m_OffsetCompensation;
            if (!Application.isPlaying)
                Prepare();
        }
#endif

        protected virtual void Reset()
        {
            UpdateIn = CurvyUpdateMethod.Update;
            PositionMode = CurvyPositionMode.Relative;
            InitialPosition = 0;
            PlayAutomatically = true;
            MoveMode = MoveModeEnum.AbsolutePrecise;
            Speed = 0;
            Animate = false;
            Animation = AnimationCurve.Linear(0, 1, 1, 1);
            TimeScale = 1;
            SingleShot = false;
            ResetOnStop = false;
            Clamping = CurvyClamping.Loop;
            OrientationMode = OrientationModeEnum.Orientation;
            OrientationAxis = OrientationAxisEnum.Up;
            IgnoreDirection = false;
        }
        /*! \endcond */
        #endregion

        #region ### Virtual Properties & Methods every controller MAY override  ###
        
        /// <summary>
        /// Gets the transform being controlled by this controller.
        /// </summary>
        public virtual Transform Transform
        {
            get
            {
                return transform;
            }
        }

        /// <summary>
        /// Whether the controller is configured, i.e. all neccessary properties set
        /// </summary>
        public virtual bool IsConfigured { get { return true; } }

        /// <summary>
        /// Whether the controller dependencies are initialized
        /// </summary>
        public virtual bool DependenciesInitialized { get { return true; } }

        /// <summary>
        /// Whether the controller is initialized, i.e. everything configured and dependencies loaded
        /// </summary>
        public virtual bool IsInitialized { get { return IsConfigured && mIsPrepared && (MoveMode==MoveModeEnum.Relative || Length>0); } }


        /// <summary>
        /// Called about 100 times a second when the component is selected
        /// </summary>
        public virtual void EditorUpdate()
        {
            Refresh();
        }
        
        /// <summary>
        /// Initialize the controller and set starting position
        /// </summary>
        public virtual void Prepare()
        {
            if (IsPlaying)
                Stop();
            
            if (IsConfigured)
            {
                if (DependenciesInitialized)
                {
                    mDirection = (int)Mathf.Sign(Speed);
                    mTF = GetTF(InitialPosition);
                    applyPositionAndRotation(mTF);
                    mIsPrepared = true;
                    UserAfterInit();
                    if (PlayAutomatically && Application.isPlaying)
                        Play();
                }
            }
        }

        public virtual void Play()
        {
            if (IsInitialized)
            {
                if (IsPlaying && IsPaused)
                {
                    mIsPausing = false;
                    return;
                }

                if (IsPlaying)
                    Stop();
                mShotStartTF = mTF;
                if (Animate && MoveMode != MoveModeEnum.Relative)
                    mShotStartDistance = RelativeToAbsolute(mShotStartTF);
                mShotTime = 0;
                mIsPlaying = true;
                mIsPausing = false;
            }
        }

        public virtual void Stop()
        {
            mShotTime = 0;
            mIsPlaying = false;
            mIsPausing = false;
            if (IsInitialized)
            {
                if (Animate)
                {
                    if (ResetOnStop)
                        mTF = mShotStartTF;

                    if (SingleShot)
                    {
                        applyPositionAndRotation(mTF);
                        if (OnAnimationEnd.HasListeners())
                            OnAnimationEndEvent(new CurvyControllerEventArgs(this, this));
                    }
                    else
                    {
                        if (OnAnimationEnd.HasListeners())
                            OnAnimationEndEvent(new CurvyControllerEventArgs(this, this));
                        Play();
                    }
                }
            }
        }

        public virtual void Pause()
        {
            mIsPausing = true;
        }

        /// <summary>
        /// Applies the current position to the transform
        /// </summary>
        /// <remarks>Use this to immediately set the transform after changing AbsolutePosition or RelativePosition!</remarks>
        public virtual void Apply()
        {
            applyPositionAndRotation(mTF);
        }


        public virtual void Warp(float delta)
        {
            int dir = (int)Mathf.Sign(delta);
            Advance(ref mTF, ref dir, MoveMode, Mathf.Abs(delta), Clamping);

            Vector3 pos;
            Vector3 tan;
            Vector3 up;

            if (OrientationMode != OrientationModeEnum.None)
            {
                GetInterpolatedSourcePosition(mTF, out pos, out tan, out up);
                ApplyTransformRotation(GetRotation(tan,up,false));
            }
            else
                pos = GetInterpolatedSourcePosition(mTF);

            ApplyTransformPosition(pos);
        }

        
        

        /// <summary>
        /// Refresh the controller and advance position
        /// </summary>
        public virtual void Refresh()
        {
            
            if (IsPaused)
                return;
            
            if (IsInitialized && IsPlaying)
            {
                
                mShotTime += DeltaTime;

                Vector3 pos;
                Vector3 posOff;
                Vector3 tan;
                Vector3 up;

                if (Animate)
                {
                    mTF = getAnimationTF();

                    if (OrientationMode != OrientationModeEnum.None)
                    {
                        GetInterpolatedSourcePosition(mTF, out pos, out tan, out up);
                        if (OffsetRadius != 0)
                        {
                            posOff = offsetPosition(pos, tan, up, OffsetAngle, OffsetRadius);
                            if (OffsetCompensation)
                            {
                                float d = (pos - oldPos).magnitude;
                                oldPos = pos;
                                float dp = (oldOff - posOff).magnitude;
                                oldOff = posOff;
                                mOffsetCompensation = d / dp;
                            }
                            pos = posOff;
                        }
                        ApplyTransformRotation(GetRotation(tan, up));
                    }
                    else
                        pos = GetInterpolatedSourcePosition(mTF);

                    ApplyTransformPosition(pos);
                }
                else
                {
                    Advance(ref mTF, ref mDirection, MoveMode, AbsSpeed * DeltaTime, Clamping);

                    if (OrientationMode != OrientationModeEnum.None)
                    {
                        GetInterpolatedSourcePosition(mTF, out pos, out tan, out up);
                        
                        if (OffsetRadius != 0)
                        {
                            posOff = offsetPosition(pos, tan, up, OffsetAngle, OffsetRadius);
                            if (OffsetCompensation)
                            {
                                float d = (pos - oldPos).magnitude;
                                oldPos = pos;
                                float dp = (oldOff - posOff).magnitude;
                                oldOff = posOff;
                                mOffsetCompensation = d / dp;
                            }
                            pos = posOff;
                        }
                        ApplyTransformRotation(GetRotation(tan, up));
                    }
                    else
                        pos=GetInterpolatedSourcePosition(mTF);
                    
                    ApplyTransformPosition(pos);
                }
                
                UserAfterUpdate();

                // Handle Playing state
                if (Animate && mShotTime >= TimeScale)
                    Stop();
            }
            mForceUpdate = false;
        }

        

        /// <summary>
        /// Called before starting to move in editor preview
        /// </summary>
        /// <remarks>Use this to store settings, e.g. the initial VirtualPosition</remarks>
        public virtual void BeginPreview()
        {
            mInitialVirtualPos = InitialPosition;
            Play();
        }
        /// <summary>
        /// Called after ending editor preview
        /// </summary>
        /// <remarks>Use this to restore settings, e.g. the initial VirtualPosition</remarks>
        public virtual void EndPreview()
        {
            mIsPlaying = false;
            InitialPosition = mInitialVirtualPos;
            Speed = m_Speed;
            Prepare();
            
        }

        #endregion

        #region ### Virtual Properties & Methods every Controller SHOULD override ###

        /// <summary>
        /// Gets the source's length
        /// </summary>
        public virtual float Length { get { return 0; } }

        /// <summary>
        /// Advance the controller and return the new position
        /// </summary>
        /// <param name="tf">the current virtual position (either TF or World Units) </param>
        /// <param name="direction">the current direction</param>
        /// <param name="mode">movement mode</param>
        /// <param name="absSpeed">speed, always positive</param>
        /// <param name="clamping">clamping mode</param>
        protected virtual void Advance(ref float tf, ref int direction, MoveModeEnum mode, float absSpeed, CurvyClamping clamping)
        {
        }

        /// <summary>
        /// Set the transform's localRotation
        /// </summary>
        /// <param name="rotation">the new rotation</param>
        protected virtual void ApplyTransformRotation(Quaternion rotation)
        {
            Transform.localRotation = rotation;
        }

        /// <summary>
        /// Set the transform's position
        /// </summary>
        /// <remarks>Depending on Space, either localPosition or position needs to be set</remarks>
        /// <param name="position">the new position</param>
        protected virtual void ApplyTransformPosition(Vector3 position) 
        {
            if (Space == Space.Self)
                Transform.localPosition = position;
            else
                Transform.position = position;
        }

        /// <summary>
        /// Converts distance on source from absolute to relative position, respecting Clamping
        /// </summary>
        /// <param name="worldUnitDistance">distance in world units from the source start</param>
        /// <returns>relative distance (TF) in the range 0..1</returns>
        protected virtual float AbsoluteToRelative(float worldUnitDistance)
        {
            return CurvyUtility.ClampTF(worldUnitDistance / Length,Clamping);
        }

        /// <summary>
        /// Converts distance on source from relative to absolute position, respecting Clamping
        /// </summary>
        /// <param name="relativeDistance">relative distance (TF) from the source start</param>
        /// <returns>distance in world units from the source start</returns>
        protected virtual float RelativeToAbsolute(float relativeDistance)
        {
            return CurvyUtility.ClampDistance(relativeDistance * Length, Clamping, Length);
        }

        /// <summary>
        /// Retrieve the source' relative position, i.e. TF
        /// </summary>
        /// <param name="virtualPosition">either TF or World Units, depending on mode</param>
        /// <returns>a relative position 0..1</returns>
        protected float GetTF(float virtualPosition)
        {
            switch (PositionMode)
            {
                case CurvyPositionMode.WorldUnits:
                    return AbsoluteToRelative(virtualPosition);
                default:
                    return virtualPosition;
            }
        }

        /// <summary>
        /// Retrieve the source position for a given relative position (TF)
        /// </summary>
        /// <param name="tf">relative position</param>
        /// <returns>a world position</returns>
        protected virtual Vector3 GetInterpolatedSourcePosition(float tf)
        {
            return Vector3.zero;
        }

        protected virtual void GetInterpolatedSourcePosition(float tf, out Vector3 position, out Vector3 tangent, out Vector3 up)
        {
            position = Vector3.zero;
            tangent = Vector3.forward;
            up = Vector3.up;
        }

        
        /// <summary>
        /// Retrieve the source Orientation/Up-Vector for a given relative position
        /// </summary>
        /// <param name="tf">relative position</param>
        /// <returns>the Up-Vector</returns>
        protected virtual Vector3 GetOrientation(float tf)
        {
            return Transform.up;
        }
        
        /// <summary>
        /// Gets the resulting rotation
        /// </summary>
        /// <param name="tangent">direction vector</param>
        /// <param name="up">up vector</param>
        /// <param name="damp">Whether the result should be smoothdamped</param>
        /// <returns>the rotation the transform will be set to</returns>
        protected virtual Quaternion GetRotation(Vector3 tangent, Vector3 up, bool damp=true)
        {
            if (!IgnoreDirection && mDirection<0)
                    tangent *= -1;

            if (damp)
            {
                tangent = Vector3.SmoothDamp(Transform.forward, tangent, ref mDampingDirVelocity, DampingDirection);
                up = Vector3.SmoothDamp(Transform.up, up, ref mDampingUpVelocity, DampingUp);
            }
            

            if (OrientationMode == OrientationModeEnum.Tangent)
            {
                up = tangent;
                tangent = Vector3.forward;
            }

            switch (OrientationAxis)
            {
                case OrientationAxisEnum.Up:
                    return Quaternion.LookRotation(tangent, up);
                case OrientationAxisEnum.Down:
                    return Quaternion.LookRotation(tangent, up * -1);
                case OrientationAxisEnum.Forward:
                    return Quaternion.LookRotation(up, tangent * -1);
                case OrientationAxisEnum.Backward:
                    return Quaternion.LookRotation(up, tangent);
                case OrientationAxisEnum.Left:
                    return Quaternion.LookRotation(tangent, Vector3.Cross(up, tangent));
                case OrientationAxisEnum.Right:
                    return Quaternion.LookRotation(tangent, Vector3.Cross(tangent, up));
                default:
                    return Quaternion.identity;
            }

        }
        /// <summary>
        /// Gets tangent for a given relative position
        /// </summary>
        /// <param name="tf">relative position</param>
        /// <returns></returns>
        protected virtual Vector3 GetTangent(float tf)
        {
            return Vector3.forward;
        }

        /// <summary>
        /// Binds any external events
        /// </summary>
        protected virtual void BindEvents()
        {
            m_OnAnimationEnd.CheckForListeners();
        }
        /// <summary>
        /// Unbinds any external events
        /// </summary>
        protected virtual void UnbindEvents()
        {
        }

        #endregion

        #region ### Virtual Methods for inherited custom controllers (Easy mode) ###

        /// <summary>
        /// Called after the controller needs to set initial position
        /// </summary>
        protected virtual void UserAfterInit() { }
        /// <summary>
        /// Called after the controller has updated it's position
        /// </summary>
        protected virtual void UserAfterUpdate() { }



        #endregion

        #region ### Events ###

        protected CurvyControllerEventArgs OnAnimationEndEvent(CurvyControllerEventArgs e)
        {
            if (OnAnimationEnd != null)
                OnAnimationEnd.Invoke(e);
            return e;
        }

        #endregion

        /// <summary>
        /// Event-friedly helper that sets a field or property value
        /// </summary>
        /// <param name="fieldAndValue">e.g. "MyValue=42"</param>
        public void SetFromString(string fieldAndValue)
        {
            string[] f = fieldAndValue.Split('=');
            if (f.Length != 2)
                return;

            var fi = this.GetType().FieldByName(f[0], true, false);
            if (fi != null)
            {
                try
                {
#if NETFX_CORE
                    if (fi.FieldType.GetTypeInfo().IsEnum)
#else
                    if (fi.FieldType.IsEnum)
#endif
                        fi.SetValue(this, System.Enum.Parse(fi.FieldType, f[1]));
                    else
                        fi.SetValue(this, System.Convert.ChangeType(f[1], fi.FieldType));
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(this.name + ".SetFromString(): " + e.ToString());
                }
            }
            else
            {
                var pi = this.GetType().PropertyByName(f[0], true, false);
                if (pi != null)
                {
                    try
                    {
#if NETFX_CORE
                        if (pi.PropertyType.GetTypeInfo().IsEnum)
#else
                        if (pi.PropertyType.IsEnum)
#endif

                            pi.SetValue(this, System.Enum.Parse(pi.PropertyType, f[1]), null);
                        else
                            pi.SetValue(this, System.Convert.ChangeType(f[1], pi.PropertyType), null);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning(this.name + ".SetFromString(): " + e.ToString());
                    }
                }
            }
        }


        #region ### Privates & Internals ###
        /*! \cond PRIVATE */

        protected Vector3 offsetPosition(Vector3 pos, Vector3 tan, Vector3 up, float angle, float radius)
        {
            Quaternion R = Quaternion.AngleAxis(angle, tan);
            return pos + (R * up) * radius;
        }

        void applyPositionAndRotation(float tf)
        {
            Vector3 pos;
            Vector3 tan;
            Vector3 up;
            if (OrientationMode != OrientationModeEnum.None)
            {
                GetInterpolatedSourcePosition(tf, out pos, out tan, out up);
                if (OffsetRadius != 0)
                    pos = offsetPosition(pos, tan, up, OffsetAngle, OffsetRadius);

                ApplyTransformRotation(GetRotation(tan, up,false));
            }
            else
                pos = GetInterpolatedSourcePosition(tf);

            ApplyTransformPosition(pos);
            
        }

        
        float getAnimationTF()
        {
            float step = AbsSpeed * Animation.Evaluate(Mathf.Clamp01(mShotTime / TimeScale));
            mDirection = (int)Mathf.Sign(step);

            return (MoveMode == MoveModeEnum.Relative) ? mShotStartTF + step : AbsoluteToRelative(mShotStartDistance + step);
        }

        float maxPosition
        {
            get {return (PositionMode==CurvyPositionMode.Relative) ? 1 : Length;}
        }

        /*! \endcond */
        #endregion

        #region ### OBSOLETE ###
        /*! \cond OBSOLETE */

        /*! \endcond */
        #endregion
    }
}
