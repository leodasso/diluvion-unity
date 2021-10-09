//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2013 - 2015  Illogika
//----------------------------------------------
using System;
using UnityEngine;
using System.Collections;

namespace HeavyDutyInspector
{

	public enum Alignment {
		Left,
		Center,
		Right
	}

	[AttributeUsage (AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
	public class ImageAttribute : PropertyAttribute {
		
		public string imagePath
		{
			get;
			private set;
		}

		public Alignment alignment
		{
			get;
			private set;
		}

		/// <summary>
		/// Adds the specified image in the inspector before the variable.
		/// </summary>
		/// <param name="imagePath">Path to the image. The path is relative to the project's Asset folder.</param>
		/// <param name="alignment">The image's alignment, either Left, Center or Right.</param>
		public ImageAttribute(string imagePath, Alignment alignment = Alignment.Center, int order = 0)
		{
			if(imagePath.ToLower().Substring(0, 7).Equals("assets/"))
				imagePath = imagePath.Substring(7);

			this.imagePath = imagePath;
			this.alignment = alignment;
			this.order = order;
		}
	}
}

	