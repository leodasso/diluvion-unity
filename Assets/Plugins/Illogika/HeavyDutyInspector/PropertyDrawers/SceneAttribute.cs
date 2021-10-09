//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------
using UnityEngine;

namespace HeavyDutyInspector
{

	public class SceneAttribute : PropertyAttribute {

		public string BasePath
		{
			get;
			private set;
		}

		public SceneAttribute(string basePath)
		{
			BasePath = basePath;
		}
	}
	
}
	