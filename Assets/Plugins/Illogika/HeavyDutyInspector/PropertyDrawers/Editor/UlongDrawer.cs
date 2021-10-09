//----------------------------------------------
//            Heavy-Duty Inspector
//         Copyright © 2015  Illogika
//----------------------------------------------
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace HeavyDutyInspector
{

	[CustomPropertyDrawer(typeof(UInt64))]
	public class UlongDrawerDrawer : IllogikaDrawer {
		
		public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
		{
	       return base.GetPropertyHeight(prop, label);
	    }
		
		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, prop);

			UInt64 temp = BitConverter.ToUInt64(BitConverter.GetBytes(prop.longValue), 0);

			bool hasChanged = false;
			EditorGUI.BeginChangeCheck();

			string s = "";

			if(prop.hasMultipleDifferentValues)
			{
				s = "--";
			}
			else
			{
				s = ((UInt64)temp).ToString();
			}

			s = EditorGUI.TextField(position, label, s);

			GUI.color = Color.clear;
			int tempInt = EditorGUI.IntField(position, label, 0);
			GUI.color = Color.white;

			if(EditorGUI.EndChangeCheck())
			{
				try
				{
					temp = UInt64.Parse(s) + (UInt64)tempInt;
					hasChanged = true;
				}
				catch
				{
					if(string.IsNullOrEmpty(s))
					{
						temp = (UInt64)tempInt;
						hasChanged = true;
					}

					// field had an invalid value. Ignore change this frame
				}
			}

			if(hasChanged)
			{
				prop.longValue = BitConverter.ToInt64(BitConverter.GetBytes(temp), 0);
			}

			EditorGUI.EndProperty();
		}
	}

}
