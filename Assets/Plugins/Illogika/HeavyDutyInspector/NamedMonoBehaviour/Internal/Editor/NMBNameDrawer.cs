//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2013 - 2015  Illogika
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	[CustomPropertyDrawer(typeof(NMBNameAttribute))]
	public class NMBNameDrawer : IllogikaDrawer {
			
		NMBNameAttribute nmbNameAttribute { get { return ((NMBNameAttribute)attribute); } }
		
		public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
		{
	       return base.GetPropertyHeight(prop, label);
	    }
		
		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, prop);

			position.width -= 55;
			EditorGUI.PropertyField(position, prop);

			EditorGUI.EndProperty();
		}
	}

}
