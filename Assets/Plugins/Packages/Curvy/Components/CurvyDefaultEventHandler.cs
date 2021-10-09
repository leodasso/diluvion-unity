// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.Curvy.Controllers;
using System.Collections.Generic;

namespace FluffyUnderware.Curvy.Components
{
    
    /// <summary>
    /// Class for some default/example event handlers
    /// </summary>
    [AddComponentMenu("Curvy/Misc/Curvy Default Event Handler")]
    [HelpURL(CurvySpline.DOCLINK + "defaulteventhandler")]
    public class CurvyDefaultEventHandler : MonoBehaviour
    {

        #region ### Public Static Methods ##

        /// <summary>
        /// Print some event details to the console
        /// </summary>
        /// <param name="e"></param>
        public static void DebugLogStatic(CurvySplineEventArgs e)
        {
            Debug.Log(string.Format("Sender/Spline/Data: {0}/{1}/{2}", e.Sender, e.Spline, e.Data));
        }

        /// <summary>
        /// Print some event details to the console
        /// </summary>
        /// <param name="e"></param>
        public static void DebugLogStatic(CurvySplineMoveEventArgs e)
        {
            Debug.Log(string.Format("Segment/TF/Direction: {0}/{1}/{2}", e.ControlPoint, e.TF, e.Direction));
        }

        /// <summary>
        /// Use a follow up, if present
        /// </summary>
        public static void UseFollowUpStatic(CurvySplineMoveEventArgs e)
        {
            // we need a SplineController as well as a following spline to work with
            if (e.Sender is SplineController && e.ControlPoint.FollowUp)
            {
                var me = e.ControlPoint;
                // Follow the connected spline
                e.Follow(e.ControlPoint.FollowUp, e.ControlPoint.FollowUpHeading);
                // Set the controller to use the new spline
                SplineController controller = (SplineController)e.Sender;
                controller.Spline = e.Spline;
                controller.RelativePosition = e.TF;
                // Handle controller events for the new passed ControlPoint
                if (me.FollowUp && me.FollowUp.FollowUp!=me)
                    controller.OnControlPointReached.Invoke(e);
            }
        }

        #endregion

        #region ### Public Methods ###

        public void DebugLog(CurvySplineEventArgs e)
        {
            DebugLogStatic(e);
        }
        public static void DebugLog(CurvySplineMoveEventArgs e)
        {
            DebugLogStatic(e);
        }

        public void UseFollowUp(CurvySplineMoveEventArgs e)
        {
            UseFollowUpStatic(e);
        }

        public void UseRandomConnectionStatic(CurvySplineMoveEventArgs e)
        {
            // we need a SplineController as well as a connection to work with
            if (e.Sender is SplineController && e.ControlPoint.Connection)
            {
                CurvySplineSegment current = e.ControlPoint;
                
                // Find a new spline to follow:
                    // Get all connected ControlPoints and check angle
                    var others = e.ControlPoint.Connection.OtherControlPoints(current);
                    // If it's smaller or equal 90°, consider the connected spline as a valid path to follow, otherwise remove it from the list
                    for (int i = others.Count - 1; i >= 0; i--)
                    {
                        if (e.AngleTo(others[i]) > 90)
                            others.RemoveAt(i);
                    }

                int randomIndex = Random.Range(-1, others.Count);

                if (randomIndex < 0) // don't follow another, but use FollowUp if present
                {
                    if (current.FollowUp)
                        e.Follow(current.FollowUp, current.FollowUpHeading);    // Follow the connected spline
                }
                else
                {
                    e.Follow(others[randomIndex]); // Follow the new spline
                }

                // Set the controller to use the new spline
                SplineController controller = (SplineController)e.Sender;
                controller.Spline = e.Spline;
                controller.RelativePosition = e.TF;

            }
        }

        public void UseRandomConnection(CurvySplineMoveEventArgs e)
        {
            UseRandomConnectionStatic(e);
        }

        #endregion
    }
}
