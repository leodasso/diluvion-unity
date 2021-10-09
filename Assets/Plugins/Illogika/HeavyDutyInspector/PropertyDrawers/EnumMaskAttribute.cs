//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------
using UnityEngine;

namespace HeavyDutyInspector
{

	public class EnumMaskAttribute : PropertyAttribute {
		
		/// <summary>
		/// Displays an enum field as an Enum Mask.
		/// *** Caution! When setting an Enum mask to Everything, in Unity and then removing one of the values from the mask, the Enum mask goes into the negative and its value becomes corrupted until you reset it to Nothing. ***
		/// </summary>
		public EnumMaskAttribute()
		{
			
		}
	}
	
}
	