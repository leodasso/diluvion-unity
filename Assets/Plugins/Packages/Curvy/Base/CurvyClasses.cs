// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FluffyUnderware.Curvy.Utils;
using UnityEngine.Events;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy
{

    /// <summary>
    /// Base Interface for Metadata components
    /// </summary>
    public interface ICurvyMetadata
    {
    }
    /// <summary>
    /// Interface for Metadata components that interpolate values
    /// </summary>
    public interface ICurvyInterpolatableMetadata : ICurvyMetadata
    {
        object Value { get; }
        object InterpolateObject(ICurvyMetadata b, float f);
    }

    /// <summary>
    /// Generic Interface for Metadata components that interpolate values
    /// </summary>
    /// <typeparam name="U">return type</typeparam>
    public interface ICurvyInterpolatableMetadata<U> : ICurvyInterpolatableMetadata
    {
        U Interpolate(ICurvyMetadata b, float f);
    }

    /// <summary>
    /// Orientation options
    /// </summary>
    public enum OrientationModeEnum
    {
        /// <summary>
        /// No Orientation
        /// </summary>
        None,
        /// <summary>
        /// Use Orientation/Up-Vector
        /// </summary>
        Orientation,
        /// <summary>
        /// Use Direction/Tangent
        /// </summary>
        Tangent
    }

    /// <summary>
    /// Orientation axis to use
    /// </summary>
    public enum OrientationAxisEnum
    {
        Up,
        Down,
        Forward,
        Backward,
        Left,
        Right
    }

    /// <summary>
    /// Connection Heading mode
    /// </summary>
    public enum ConnectionHeadingEnum
    {
        /// <summary>
        /// Align toward the targets start (negative F)
        /// </summary>
        Minus = -1,
        /// <summary>
        /// No Alignment
        /// </summary>
        Sharp = 0,
        /// <summary>
        /// Align toward the targets end (positive F)
        /// </summary>
        Plus = 1,
        /// <summary>
        /// Automatically tries to avoid sharp cuts
        /// </summary>
        Auto = 2
    }
    
    /// <summary>
    /// Used by components to determine when updates should occur
    /// </summary>
    public enum CurvyUpdateMethod
    {
        Update,
        LateUpdate,
        FixedUpdate
    }

    public enum CurvyRepeatingOrderEnum
    {
        Random = 0,
        Row = 1
    }

    /// <summary>
    /// Plane definition
    /// </summary>
    public enum CurvyPlane
    {
        /// <summary>
        /// X/Y Plane (Z==0)
        /// </summary>
        XY,
        /// <summary>
        /// X/U Plane (Y==0)
        /// </summary>
        XZ,
        /// <summary>
        /// Y/Z Plane (X==)
        /// </summary>
        YZ
    }

    /// <summary>
    /// Position Mode 
    /// </summary>
    public enum CurvyPositionMode
    {
        Relative,
        WorldUnits
    }

    /// <summary>
    /// Bezier Handles editing modes
    /// </summary>
    public enum CurvyBezierModeEnum
    {
        /// <summary>
        /// Don't sync
        /// </summary>
        None = 0,
        /// <summary>
        /// Sync Direction
        /// </summary>
        Direction = 1,
        /// <summary>
        /// Sync Length
        /// </summary>
        Length = 2,
        /// <summary>
        /// Sync connected Control Points
        /// </summary>
        Connections = 4,
        /// <summary>
        /// Combine both Handles of a segment
        /// </summary>
        Combine= 8
    }

    /// <summary>
    /// Bezier Handles editing modes for AdvSplines
    /// </summary>
    public enum CurvyAdvBezierModeEnum
    {
        /// <summary>
        /// Don't sync
        /// </summary>
        None = 0,
        /// <summary>
        /// Sync Direction
        /// </summary>
        Direction = 1,
        /// <summary>
        /// Sync Length
        /// </summary>
        Length = 2,
        /// <summary>
        /// Combine both Handles of a segment
        /// </summary>
        Combine = 8
    }

    /// <summary>
    /// Determines the interpolation method
    /// </summary>
    public enum CurvyInterpolation
    {
        /// <summary>
        ///  Linear interpolation
        /// </summary>
        Linear = 0,
        /// <summary>
        /// Catmul-Rom splines
        /// </summary>
        CatmullRom = 1,
        /// <summary>
        /// Kochanek-Bartels (TCB)-Splines
        /// </summary>
        TCB = 2,
        /// <summary>
        /// Cubic Bezier-Splines
        /// </summary>
        Bezier = 3
    }

    /// <summary>
    /// Determines the clamping method used by Move-methods
    /// </summary>
    public enum CurvyClamping
    {
        /// <summary>
        /// Stop at splines ends
        /// </summary>
        Clamp = 0,
        /// <summary>
        /// Start over
        /// </summary>
        Loop = 1,
        /// <summary>
        /// Switch direction
        /// </summary>
        PingPong = 2
    }

    /// <summary>
    /// Determines Orientation mode
    /// </summary>
    public enum CurvyOrientation
    {
        /// <summary>
        /// Ignore rotation
        /// </summary>
        None = 0,
        /// <summary>
        /// Use the splines' tangent and up vectors to create a look rotation 
        /// </summary>
        Dynamic = 1,
        /// <summary>
        /// Interpolate between the Control Point's rotation
        /// </summary>
        Static = 2,
    }

    /// <summary>
    /// Swirl mode
    /// </summary>
    public enum CurvyOrientationSwirl
    {
        /// <summary>
        /// No Swirl
        /// </summary>
        None = 0,
        /// <summary>
        /// Swirl over each segment of anchor group
        /// </summary>
        Segment = 1,
        /// <summary>
        /// Swirl equal over current anchor group's segments
        /// </summary>
        AnchorGroup = 2,
        /// <summary>
        /// Swirl equal over anchor group's length
        /// </summary>
        AnchorGroupAbs = 3
    }

    

    /// <summary>
    /// Sceneview viewing modes
    /// </summary>
    [System.Flags]
    public enum CurvySplineGizmos : int
    {
        None = 0,
        Curve = 2,
        Approximation = 4,
        Tangents = 8,
        Orientation = 16,
        Labels = 32,
        Metadata=64,
        Bounds = 128,
        All = 65535
    }

   
    
}
