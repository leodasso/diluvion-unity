//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	public class ChangeCheckCallbackAttribute : PropertyAttribute {

		public string callback
		{
			get;
			private set;
		}

		/// <summary>
		/// Calls a function in your script when the value of the variable changes.
		/// </summary>
		/// <param name="callbackName">The name of the function to call.</param>
		public ChangeCheckCallbackAttribute(string callbackName)
		{
			this.callback = callbackName;
		}
	}

}
