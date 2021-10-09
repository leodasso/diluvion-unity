//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2013 - 2015  Illogika
//----------------------------------------------

using UnityEngine;
using System;

namespace HeavyDutyInspector
{

	public class NamedMonoBehaviourAttribute : PropertyAttribute {

		/// <summary>
		/// Displays a reference using the NamedMonoBehaviour Drawer. Use with variables of a type extending NamedMonoBehaviour.
		/// </summary>
		[System.Obsolete("Use ComponentSelection instead.")]
		public NamedMonoBehaviourAttribute()
		{
			// Only here for the summary
		}
	}

}
