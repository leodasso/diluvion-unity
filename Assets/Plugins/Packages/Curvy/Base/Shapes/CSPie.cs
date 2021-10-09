// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy.Shapes
{
    /// <summary>
    /// Pie Shape (2D)
    /// </summary>
    [CurvyShapeInfo("2D/Pie")]
    [RequireComponent(typeof(CurvySpline))]
    [AddComponentMenu("Curvy/Shape/Pie")]
    public class CSPie : CSCircle
    {

        [Range(0, 1)]
        [SerializeField]
        float m_Roundness = 1f;
        public float Roundness
        {
            get { return m_Roundness; }
            set
            {
                float v=Mathf.Clamp01(value);
                if (m_Roundness != v)
                {
                    m_Roundness = v;
                    Dirty = true;
                }
                
            }
        }

        public enum EatModeEnum
        {
            Left,
            Right,
            Center
        }
        
        [SerializeField]
        [RangeEx(0,"maxEmpty","Empty","Number of empty slices")]
        int m_Empty = 1;
        public int Empty
        {
            get { return m_Empty; }
            set
            {
                int v = Mathf.Clamp(value, 0, maxEmpty);
                if (m_Empty != v)
                {
                    m_Empty = v;
                    Dirty = true;
                }
            }
        }

        int maxEmpty { get { return Count;}}

        [Label(Tooltip="Eat Mode")]
        [SerializeField]
        EatModeEnum m_Eat=EatModeEnum.Right;
        public EatModeEnum Eat
        {
            get { return m_Eat; }
            set
            {
                if (m_Eat != value)
                {
                    m_Eat = value;
                    Dirty = true;
                }
            }
        }



#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Empty = m_Empty;
            Eat = m_Eat;
            Roundness = m_Roundness;
        }
#endif

        protected override void Reset()
        {
 	         base.Reset();
             Roundness = .5f;
             Empty = 1;
             Eat = EatModeEnum.Right;
        }

        Vector3 cpPosition(int i, int empty, float d)
        {
            switch (Eat)
            {
                case EatModeEnum.Left:
                    return new Vector3(Mathf.Sin(d * i) * Radius, Mathf.Cos(d * i) * Radius, 0);
                case EatModeEnum.Right:
                    return new Vector3(Mathf.Sin(d * (i + empty)) * Radius, Mathf.Cos(d * (i + empty)) * Radius, 0);
                default:
                    return new Vector3(Mathf.Sin(d * (i + empty * 0.5f)) * Radius, Mathf.Cos(d * (i + empty * 0.5f)) * Radius, 0);
            }
        }

        protected override void ApplyShape()
        {
 	        PrepareSpline(CurvyInterpolation.Bezier,CurvyOrientation.Static);
            PrepareControlPoints(Count - Empty + 2);

                float d = 360f * Mathf.Deg2Rad / Count;
                float distPercent = Roundness * 0.39f;

                for (int i = 0; i < Spline.ControlPointCount-1; i++)
                {
                    Spline.ControlPoints[i].AutoHandles = true;
                    Spline.ControlPoints[i].AutoHandleDistance = distPercent;
                    SetPosition(i,cpPosition(i,Empty,d));
                    SetRotation(i,Quaternion.Euler(90, 0, 0));
                }
                

                // Center
                SetPosition(Spline.ControlPointCount - 1, Vector3.zero);
                SetRotation(Spline.ControlPointCount - 1, Quaternion.Euler(90, 0, 0));
                SetBezierHandles(Spline.ControlPointCount - 1, 0);
                
                // From Center
                Spline.ControlPoints[0].AutoHandles = false;
                Spline.ControlPoints[0].HandleIn = Vector3.zero;
                Spline.ControlPoints[0].SetBezierHandles(distPercent,
                                                         cpPosition(Count-1 , Empty, d)-Spline.ControlPoints[0].localPosition,
                                                         cpPosition(1, Empty, d) - Spline.ControlPoints[0].localPosition, false, true);

                // To Center
                Spline.ControlPoints[Spline.ControlPointCount - 2].AutoHandles = false;
                Spline.ControlPoints[Spline.ControlPointCount - 2].HandleOut = Vector3.zero;
                Spline.ControlPoints[Spline.ControlPointCount - 2].SetBezierHandles(distPercent,
                                                         cpPosition(Count-1-Empty, Empty, d) - Spline.ControlPoints[Spline.ControlPointCount - 2].localPosition,
                                                         cpPosition(Count+1-Empty, Empty, d) - Spline.ControlPoints[Spline.ControlPointCount - 2].localPosition, true, false);

        }
        
    }
}
