//----------------------------------------------
//            Heavy-Duty Inspector
//         Copyright © 2015  Illogika
//----------------------------------------------
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HeavyDutyInspector
{
	public class DynamicRangeAttribute : PropertyAttribute {

		public float minValue
		{
			get;
			private set;
		}

		public float maxValue
		{
			get;
			private set;
		}

		public string minDelegate
		{
			get;
			private set;
		}

		public string maxDelegate
		{
			get;
			private set;
		}

		/// <summary>
		/// Attribute used to restrict the value of a variable between a range of dynamic values.
		/// </summary>
		/// <param name="min">Min value for this variable.</param>
		/// <param name="maxDelegate">Float type variable or Parameterless Function with a float return type defining the maximum value for this variable.</param>
		public DynamicRangeAttribute(float min, string maxDelegate)
		{
			minValue = min;
			maxValue = float.NaN;
			this.maxDelegate = maxDelegate;
			minDelegate = String.Empty;
		}

		/// <summary>
		/// Attribute used to restrict the value of a variable between a range of dynamic values.
		/// </summary>
		/// <param name="minDelegate">Float type variable or Parameterless Function with a float return type defining the minimum value for this variable.</param>
		/// <param name="max">Max value for this variable.</param>
		public DynamicRangeAttribute(string minDelegate, float max)
		{
			minValue = float.NaN;
			maxValue = max;
			this.minDelegate = minDelegate;
			maxDelegate = String.Empty;
		}

		/// <summary>
		/// Attribute used to restrict the value of a variable between a range of dynamic values.
		/// </summary>
		/// <param name="minDelegate">Float type variable or Parameterless Function with a float return type defining the minimum value for this variable.</param>
		/// <param name="maxDelegate">Float type variable or Parameterless Function with a float return type defining the maximum value for this variable.</param>
		public DynamicRangeAttribute(string minDelegate, string maxDelegate)
		{
			minValue = float.NaN;
			maxValue = float.NaN;
			this.minDelegate = minDelegate;
			this.maxDelegate = maxDelegate;
		}
	}
	
}
	