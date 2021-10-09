using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveTools  {

	/// <summary>
	/// Integrate area under AnimationCurve between start and end time
	/// </summary>
	/// <param name="curve">The animation curve to integrate</param>
	/// <param name="startTime">The start time (x-axis)</param>
	/// <param name="endTime">The end time (x-axis)</param>
	/// <param name="steps">Number of steps to use. Higher is more accurate</param>
	/// <returns>Approximation of the total area of the curve</returns>
	public static float IntegrateCurve(AnimationCurve curve, float startTime, float endTime, int steps, out long calcTime)
	{
		return Integrate(curve.Evaluate, startTime, endTime, steps, out calcTime);
	}
	
	/// <summary>
	/// Integrate area under AnimationCurve between start and end time
	/// </summary>
	/// <param name="curve">The animation curve to integrate</param>
	/// <param name="startTime">The start time (x-axis)</param>
	/// <param name="endTime">The end time (x-axis)</param>
	/// <param name="steps">Number of steps to use. Higher is more accurate</param>
	/// <returns>Approximation of the total area of the curve</returns>
	public static float IntegrateCurve(AnimationCurve curve, float startTime, float endTime, int steps)
	{
		long calcTime;
		return Integrate(curve.Evaluate, startTime, endTime, steps, out calcTime);
	}
 
	// Integrate function f(x) using the trapezoidal rule between x=x_low..x_high
	static float Integrate(Func<float, float> f, float xLow, float xHigh, int nSteps, out long calculationTime)
	{
		Stopwatch sw = new Stopwatch();
		
		sw.Start();
		
		// step size - x axis range / steps
		float stepSize = (xHigh - xLow) / nSteps;
		
		float res = (f(xLow) + f(xHigh)) / 2;
		for (int i = 1; i < nSteps; i++)
		{
			res += f(xLow + i * stepSize);
		}
		
		sw.Stop();

		calculationTime = sw.ElapsedTicks;
		
		return stepSize * res;
	}
}
