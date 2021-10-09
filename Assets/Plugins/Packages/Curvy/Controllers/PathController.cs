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


namespace FluffyUnderware.Curvy.Controllers
{
    /// <summary>
    /// Controller working on Curvy Generator Paths
    /// </summary>
    [AddComponentMenu("Curvy/Controller/CG Path Controller",7)]
    [HelpURL(CurvySpline.DOCLINK + "pathcontroller")]
    public class PathController : CurvyController
    {
        
        #region ### Serialized Fields ###

        [Section("General",Sort=0)]
        [SerializeField]
        [CGDataReferenceSelector(typeof(CGPath),Label="Path/Slot")]
        CGDataReference m_Path=new CGDataReference();

        #endregion

        #region ### Public Properties ###

        /// <summary>
        /// Gets or sets the path to use
        /// </summary>
        public CGDataReference Path
        {
            get { return m_Path; }
            set
            {
                if (m_Path != value)
                {
                    if (m_Path != null)
                        UnbindEvents();

                    m_Path = value;
                    if (m_Path != null)
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
        /// Gets the actual CGPath data
        /// </summary>
        public CGPath PathData
        {
            get
            {
                return (Path.HasValue) ? Path.GetData<CGPath>() : null;
            }
        }

        /// <summary>
        /// Gets whether the Controller is configured properly or not
        /// </summary>
        public override bool IsConfigured { get { return Path != null && !Path.IsEmpty; } }

        /// <summary>
        /// Whether the controller dependencies are initialized
        /// </summary>
        public override bool DependenciesInitialized
        {
            get
            {
                return IsConfigured && Path.HasValue;
            }
        }

        /// <summary>
        /// Gets the source's length
        /// </summary>
        public override float Length
        {
            get
            {
                return (PathData != null) ? PathData.Length : 0;
            }
        }

        #endregion

        #region ### Private fields ###

        float mKeepDistanceAt;

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
            Path = null;
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
            return PathData.FToDistance(relativeDistance);
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
            return PathData.DistanceToF(worldUnitDistance);
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
            return (Space==Space.World) ? Path.Module.Generator.transform.TransformPoint(PathData.InterpolatePosition(tf)) : PathData.InterpolatePosition(tf);
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
            PathData.Interpolate(tf,out position, out tangent, out up);
            if (Space == Space.World)
            {
                position = Path.Module.Generator.transform.TransformPoint(position);
                tangent = Path.Module.Generator.transform.TransformDirection(tangent);
                up = Path.Module.Generator.transform.TransformDirection(up);
            }
        }

        /// <summary>
        /// Gets tangent for a given relative position
        /// </summary>
        /// <param name="tf">relative position</param>
        /// <returns></returns>
        protected override Vector3 GetTangent(float tf)
        {
            return (Space == Space.World) ? Path.Module.Generator.transform.TransformDirection(PathData.InterpolateDirection(tf)) : PathData.InterpolateDirection(tf);
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
            return (Space == Space.World) ? Path.Module.Generator.transform.TransformDirection(PathData.InterpolateUp(tf)) : PathData.InterpolateUp(tf);
        }

        /// <summary>
        /// Advances the specified virtual position.
        /// </summary>
        /// <param name="virtualPosition">The virtual position.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="absSpeed">The abs speed.</param>
        /// <param name="clamping">The clamping.</param>
        protected override void Advance(ref float virtualPosition, ref int direction, MoveModeEnum mode, float absSpeed, CurvyClamping clamping)
        {
            switch (mode)
            {
                case MoveModeEnum.Relative:
                    PathData.Move(ref virtualPosition, ref direction, absSpeed, clamping);
                    break;
                default:
                    PathData.MoveBy(ref virtualPosition, ref direction, absSpeed, clamping);
                    break;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:RefreshPath" /> event.
        /// </summary>
        /// <param name="e">The <see cref="CurvyCGEventArgs"/> instance containing the event data.</param>
        protected virtual void OnRefreshPath(CurvyCGEventArgs e)
        {
            if (Path==null || e.Module!=Path.Module)
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

        /// <summary>
        /// Binds any external events
        /// </summary>
        protected override void BindEvents()
        {
            base.BindEvents();
            if (Path!=null && Path.Module!=null)
            {
                UnbindEvents();
                Path.Module.OnRefresh.AddListener(OnRefreshPath);
            }
        }

        /// <summary>
        /// Unbinds any external events
        /// </summary>
        protected override void UnbindEvents()
        {
            if (Path!=null && Path.Module!=null)
            {
                Path.Module.OnRefresh.RemoveListener(OnRefreshPath);
            }
        }
        #endregion
    }
}
