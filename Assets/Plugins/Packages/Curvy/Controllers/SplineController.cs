// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy.Controllers
{
    /// <summary>
    /// Controller working with Splines
    /// </summary>
    [AddComponentMenu("Curvy/Controller/Spline Controller",5)]
    [HelpURL(CurvySpline.DOCLINK + "splinecontroller")]
    public class SplineController : CurvyController
    {
        #region ### Serialized Fields ###
        
        [Section("General",Sort=0)]
        [FieldCondition("m_Spline",null,false,ActionAttribute.ActionEnum.ShowWarning,"Missing Source")]
        [SerializeField]
        CurvySpline m_Spline;
        [SerializeField]
        bool m_UseCache;
        [Section("Events", false, false, 1000, HelpURL = CurvySpline.DOCLINK + "splinecontroller_events")]
        [SerializeField]
        CurvySplineMoveEvent m_OnControlPointReached;
        [SerializeField]
        CurvySplineMoveEvent m_OnEndReached;
        [SerializeField]
        CurvySplineMoveEvent m_OnSwitch;
        

        #endregion

        #region ### Public Properties ###

        /// <summary>
        /// Gets or sets the spline to use
        /// </summary>
        public CurvySpline Spline
        {
            get { return m_Spline; }
            set
            {
                if (m_Spline != value)
                {
                    if (m_Spline != null)
                        UnbindEvents();

                    m_Spline = value;
                    if (m_Spline)
                        BindEvents();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether spline's cache data should be used
        /// </summary>
        public bool UseCache
        {
            get
            {
                return m_UseCache;
            }
            set
            {
                if (m_UseCache != value)
                    m_UseCache = value;
            }
        }


        /// <summary>
        /// Gets or sets whether to keep position when the source length changes
        /// </summary>
        public override bool AdaptOnChange
        {
            get
            {
                return base.AdaptOnChange;
            }
            set
            {
                if (base.AdaptOnChange != value)
                {
                    base.AdaptOnChange = value;
                    BindEvents();
                }
            }
        }

        /// <summary>
        /// Event raised when moving over a Control Point
        /// </summary>
        public CurvySplineMoveEvent OnControlPointReached
        {
            get { return m_OnControlPointReached; }
            set
            {
                if (m_OnControlPointReached!=value)
                    m_OnControlPointReached = value;
                if (Spline)
                    BindEvents();
                
            }
        }

        /// <summary>
        /// Event raised when reaching the extends of the source spline
        /// </summary>
        public CurvySplineMoveEvent OnEndReached
        {
            get { return m_OnEndReached; }
            set
            {
                if (m_OnEndReached != value)
                    m_OnEndReached = value;
                if (Spline)
                    BindEvents();

            }
        }

        /// <summary>
        /// Event raised while switching splines
        /// </summary>
        public CurvySplineMoveEvent OnSwitch
        {
            get { return m_OnSwitch; }
            set
            {
                if (m_OnSwitch != value)
                    m_OnSwitch = value;
            }
        }
        

        /// <summary>
        /// Gets whether the Controller is switching splines
        /// </summary>
        public bool IsSwitching
        {
            get { return mSwitchEventArgs != null; }
        }

        /// <summary>
        /// Whether the controller is configured, i.e. all neccessary properties set
        /// </summary>
        public override bool IsConfigured { get { return Spline != null; } }

        /// <summary>
        /// Whether the controller dependencies are initialized
        /// </summary>
        public override bool DependenciesInitialized { get { return Spline.IsInitialized; } }

        /// <summary>
        /// Gets the source's length
        /// </summary>
        public override float Length
        {
            get
            {
                return (Spline) ? Spline.Length : 0;
            }
        }

        /// <summary>
        /// Gets or sets the relative position on the source, respecting Clamping
        /// </summary>
        public override float RelativePosition
        {
            get
            {
                return base.RelativePosition;
            }

            set
            {
                base.RelativePosition = value;
                mKeepDistanceAt = 0;
            }
        }

        /// <summary>
        /// Gets or sets the absolute position on the source, respecting Clamping
        /// </summary>
        public override float AbsolutePosition
        {
            get
            {
                return base.AbsolutePosition;
            }

            set
            {
                base.AbsolutePosition = value;
                mKeepDistanceAt = 0;
            }
        }

        /// <summary>
        /// Gets or sets the position on the source (relative or absolute, depending on MoveMode), respecting Clamping
        /// </summary>
        public override float Position
        {
            get
            {
                return base.Position;
            }

            set
            {
                base.Position = value;
                mKeepDistanceAt = 0;
            }
        }

        #endregion

        #region ### Private fields ###

        static CurvyController _active; // set by advance to filter out events not raised by a certain controller
        
        CurvySpline mInitialSpline;
        
        float mKeepDistanceAt;
        float mSwitchStartTime;
        float mSwitchDuration;
        CurvySplineMoveEventArgs mSwitchEventArgs;

        #endregion

        #region ## Unity Callbacks ###
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
            Spline = m_Spline;
            OnControlPointReached = m_OnControlPointReached;
            OnEndReached = m_OnEndReached;
            base.OnValidate();
        }
#endif

        protected override void Reset()
        {
            base.Reset();
            Spline = null;
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
            base.Prepare();
            mKeepDistanceAt = 0;
        }

        /// <summary>
        /// Refresh the controller and advance position
        /// </summary>
        public override void Refresh()
        {
            if (IsSwitching)
            {
                mSwitchEventArgs.Delta = Mathf.Clamp01((Time.time - mSwitchStartTime) / mSwitchDuration);
                if (OnSwitch.HasListeners())
                    onSwitchEvent(mSwitchEventArgs);
                else
                    mSwitchEventArgs.Data = mSwitchEventArgs.Delta;
            }
            base.Refresh();

            if (IsSwitching)
            {
                Vector3 targetPos;
                Vector3 targetTan;
                Vector3 targetUp;
                getInterpolatedSourcePosition(mSwitchEventArgs.Spline, mSwitchEventArgs.TF, out targetPos, out targetTan, out targetUp);

                if (Space == Space.Self)
                    Transform.localPosition = Vector3.Lerp(Transform.localPosition, targetPos, (float)mSwitchEventArgs.Data);
                else
                    Transform.position = Vector3.Lerp(Transform.localPosition, targetPos, (float)mSwitchEventArgs.Data);

                if (OrientationMode != OrientationModeEnum.None)
                {
                    //transform.localRotation = Quaternion.Lerp(transform.localRotation, GetRotation(tan, up), 1 - OrientationDamping);
                }

                if (mSwitchEventArgs.Delta == 1)
                {
                    Spline = mSwitchEventArgs.Spline;
                    RelativePosition = mSwitchEventArgs.TF;
                    mSwitchEventArgs = null;
                }

            }
        }

        /// <summary>
        /// Start a spline switch
        /// </summary>
        /// <param name="target">the target spline to switch to</param>
        /// <param name="targetTF">the target TF</param>
        /// <param name="duration">duration of the switch phase</param>
        public virtual void SwitchTo(CurvySpline target, float targetTF, float duration)
        {
            mSwitchStartTime = Time.time;
            mSwitchDuration = duration;
            mSwitchEventArgs = new CurvySplineMoveEventArgs(this, target, null, targetTF, 0, Direction);
            mSwitchEventArgs.Data = 0f;
        }

        /// <summary>
        /// Called before starting to move in editor preview
        /// </summary>
        /// <remarks>
        /// Use this to store settings, e.g. the initial VirtualPosition
        /// </remarks>
        public override void BeginPreview()
        {
            mInitialSpline = Spline;
            base.BeginPreview();
        }

        /// <summary>
        /// Called after ending editor preview
        /// </summary>
        /// <remarks>
        /// Use this to restore settings, e.g. the initial VirtualPosition
        /// </remarks>
        public override void EndPreview()
        {
            Spline = mInitialSpline;
            base.EndPreview();
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
            return (Length>0) ?  Spline.TFToDistance(relativeDistance,Clamping) : 0;
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
            return (Length>0) ? Spline.DistanceToTF(worldUnitDistance,Clamping) : 0;
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
            Vector3 p = Spline.transform.localPosition;
            if (Spline.Length>0)
                p = (UseCache) ? Spline.InterpolateFast(tf) : Spline.Interpolate(tf);

            return (Space==Space.World) ? Spline.transform.TransformPoint(p) : p;
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
            getInterpolatedSourcePosition(Spline, tf, out position, out tangent, out up);
        }

        /// <summary>
        /// Gets tangent for a given relative position
        /// </summary>
        /// <param name="tf">relative position</param>
        /// <returns></returns>
        protected override Vector3 GetTangent(float tf)
        {
            if (Spline.Length == 0)
                return Vector3.forward;
            Vector3 t = (UseCache) ? Spline.GetTangentFast(tf) : Spline.GetTangent(tf);
            return (Space == Space.World) ? Spline.transform.TransformDirection(t) : t;
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
            if (Spline.Length == 0)
                return Vector3.zero;
            return (Space == Space.World) ? Spline.transform.TransformDirection(Spline.GetOrientationUpFast(tf)) : Spline.GetOrientationUpFast(tf);
        }


        /// <summary>
        /// Advance the controller and return the new position
        /// </summary>
        /// <param name="tf">the current virtual position (either TF or World Units)</param>
        /// <param name="direction">the current direction</param>
        /// <param name="mode">movement mode</param>
        /// <param name="absSpeed">speed, always positive</param>
        /// <param name="clamping">clamping mode</param>
        protected override void Advance(ref float tf, ref int direction, MoveModeEnum mode, float absSpeed, CurvyClamping clamping)
        {
            _active = this;
            switch (mode)
            {
                case MoveModeEnum.AbsoluteExtrapolate:
                    if (UseCache)
                    {
                        Spline.MoveByFast(ref tf, ref direction, absSpeed, Clamping);
                        if (IsSwitching)
                        {
                            mSwitchEventArgs.Spline.MoveByFast(ref mSwitchEventArgs.TF, ref mSwitchEventArgs.Direction, absSpeed, Clamping);
                            onSwitchEvent(mSwitchEventArgs);
                        }
                    }
                    else
                    {
                        Spline.MoveBy(ref tf, ref direction, absSpeed, Clamping);
                        if (IsSwitching)
                        {
                            mSwitchEventArgs.Spline.MoveBy(ref mSwitchEventArgs.TF, ref mSwitchEventArgs.Direction, absSpeed, Clamping);
                            onSwitchEvent(mSwitchEventArgs);
                        }
                    }
                    break;
                case MoveModeEnum.AbsolutePrecise:
                    Spline.MoveByLengthFast(ref tf, ref direction, absSpeed, Clamping);
                    if (IsSwitching)
                    {
                        mSwitchEventArgs.Spline.MoveByLengthFast(ref mSwitchEventArgs.TF, ref mSwitchEventArgs.Direction, absSpeed, Clamping);
                        onSwitchEvent(mSwitchEventArgs);
                    }
                    break;
                default: // Relative
                    if (UseCache)
                    {
                        Spline.MoveFast(ref tf, ref direction, absSpeed, Clamping);
                        if (IsSwitching)
                        {
                            mSwitchEventArgs.Spline.MoveFast(ref mSwitchEventArgs.TF, ref mSwitchEventArgs.Direction, absSpeed, Clamping);
                            onSwitchEvent(mSwitchEventArgs);
                        }
                    }
                    else
                    {
                        Spline.Move(ref tf, ref direction, absSpeed, Clamping);
                        if (IsSwitching)
                        {
                            mSwitchEventArgs.Spline.Move(ref mSwitchEventArgs.TF, ref mSwitchEventArgs.Direction, absSpeed, Clamping);
                            onSwitchEvent(mSwitchEventArgs);
                        }
                    }
                    break;
            }
            _active = null;

        }

        /// <summary>
        /// Binds any external events
        /// </summary>
        protected override void BindEvents()
        {
            base.BindEvents();
            
            if (Spline)
            {
                UnbindEvents();
                m_OnControlPointReached.CheckForListeners();
                m_OnEndReached.CheckForListeners();
                if (m_OnControlPointReached!=null && m_OnControlPointReached.HasListeners())
                    Spline.OnMoveControlPointReached.AddListenerOnce(onControlPointReachedEvent);
                if (m_OnEndReached!=null && m_OnEndReached.HasListeners())
                    Spline.OnMoveEndReached.AddListenerOnce(onEndReachedEvent);

                Spline.OnRefresh.AddListener(OnRefreshSpline);
                if (AdaptOnChange)
                {
                    Spline.OnBeforeControlPointAdd.AddListener(onBeforeCPChange);
                    Spline.OnBeforeControlPointDelete.AddListener(onBeforeCPChange);
                }
            }
        }

        /// <summary>
        /// Unbinds any external events
        /// </summary>
        protected override void UnbindEvents()
        {
            if (Spline)
            {
                Spline.OnRefresh.RemoveListener(OnRefreshSpline);
                Spline.OnMoveControlPointReached.RemoveListener(onControlPointReachedEvent);
                Spline.OnMoveEndReached.RemoveListener(onEndReachedEvent);
                Spline.OnBeforeControlPointAdd.RemoveListener(onBeforeCPChange);
                Spline.OnBeforeControlPointDelete.RemoveListener(onBeforeCPChange);
            }
        }

        protected virtual void OnRefreshSpline(CurvySplineEventArgs e)
        {
            if (e.Sender is CurvySplineBase && e.Sender != Spline)
                ((CurvySplineBase)e.Sender).OnRefresh.RemoveListener(OnRefreshSpline);
            else
            {
                if (Active)
                {
                    if (Application.isPlaying)
                    {
                        if (mKeepDistanceAt > 0 && IsPlaying)
                            AbsolutePosition = Mathf.Max(0,mKeepDistanceAt); // mKeepDistanceAt=0 implicite
                        // Refresh if static (otherwise done in Update())
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

        #region ### Privates ###
        /*! \cond PRIVATE */

        void getInterpolatedSourcePosition(CurvySplineBase spline, float tf, out Vector3 position, out Vector3 tangent, out Vector3 up)
        {
            if (spline.Length == 0)
            {
                position = spline.transform.localPosition;
                tangent = Vector3.forward;
                up = Vector3.up;
            } else
            {
                if (UseCache)
                {
                    position = spline.InterpolateFast(tf);
                    tangent = spline.GetTangentFast(tf);
                }
                else
                {
                    position = spline.Interpolate(tf);
                    tangent = spline.GetTangent(tf, position);
                }
                up = spline.GetOrientationUpFast(tf);
            }
            if (Space == Space.World)
            {
                position = spline.transform.TransformPoint(position);
                tangent = spline.transform.TransformDirection(tangent);
                up = spline.transform.TransformDirection(up);
            }
        }

        void onBeforeCPChange(CurvyControlPointEventArgs e)
        {
            if (e.Sender is CurvySplineBase && e.Sender != Spline)
                ((CurvySplineBase)e.Sender).OnRefresh.RemoveListener(OnRefreshSpline);
            else
            {
                if (Active)
                {
                    if (mKeepDistanceAt == 0)
                    {
                        switch (e.Mode)
                        {
                            case CurvyControlPointEventArgs.ModeEnum.Delete:
                                mKeepDistanceAt = this.AbsolutePosition - e.ControlPoint.Length;
                                break;
                            case CurvyControlPointEventArgs.ModeEnum.None:
                                break;
                            default:
                                mKeepDistanceAt = this.AbsolutePosition;
                                break;
                        }
                    }
                }
                else
                    UnbindEvents();
            }
        }

        void onControlPointReachedEvent(CurvySplineMoveEventArgs e)
        {
            if (!(_active == this))
                return;
            if (e.Spline && e.Spline != Spline)
                e.Spline.OnMoveControlPointReached.RemoveListener(onControlPointReachedEvent);
            else
            {
                e.Sender = this;
                OnControlPointReached.Invoke(e);
            }
        }

        void onEndReachedEvent(CurvySplineMoveEventArgs e)
        {
            if (!(_active == this))
                return;
            if (e.Spline && e.Spline != Spline)
                e.Spline.OnMoveEndReached.RemoveListener(onEndReachedEvent);
            else
            {
                e.Sender = this;
                OnEndReached.Invoke(e);
            }
        }

        void onSwitchEvent(CurvySplineMoveEventArgs e)
        {
            OnSwitch.Invoke(e);
            if (e.Cancel)
                mSwitchEventArgs = null;
        }

       

        /*! \endcond */
        #endregion
        
    }
}
