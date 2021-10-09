//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------
#if !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2 && NEW_VECTOR_DRAWER

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	[CustomPropertyDrawer(typeof(Vector2))]
	public class Vector2Drawer : IllogikaDrawer {
		
		public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
		{
	       return base.GetPropertyHeight(prop, label);
	    }
		
		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, prop);

			position = EditorGUI.PrefixLabel(position, EditorGUIUtility.GetControlID(FocusType.Passive), label);

			int originalIndentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			Rect labelPos = position;
			
			labelPos.width = 12;
			
			position.width /= 2;
			position.width -= 12;
			position.x += 12;
			
			Vector2 temp = prop.vector2Value;
			EditorGUI.BeginChangeCheck();
			
			GUIContent xLabel = new GUIContent("X");
			GUIContent yLabel = new GUIContent("Y");
			
			EditorGUI.PrefixLabel(labelPos, xLabel);
			labelPos.x += position.width + 12;
			
			GUI.backgroundColor = Color.red;
			temp.x = EditorGUI.FloatField(position, temp.x);
			GUI.backgroundColor = Color.white;
			
			EditorGUI.PrefixLabel(labelPos, yLabel);
			labelPos.x += position.width + 12;
			
			position.x += position.width + 12;
			
			GUI.backgroundColor = Color.green;
			temp.y = EditorGUI.FloatField(position, temp.y);
			GUI.backgroundColor = Color.white;
			
			if(EditorGUI.EndChangeCheck())
			{
				prop.vector2Value = temp;
			}

			EditorGUI.indentLevel = originalIndentLevel;

			EditorGUI.EndProperty();
		}
	}

}

#endif
