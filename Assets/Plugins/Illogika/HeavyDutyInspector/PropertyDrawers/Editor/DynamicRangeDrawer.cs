//----------------------------------------------
//            Heavy-Duty Inspector
//         Copyright © 2015  Illogika
//----------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace HeavyDutyInspector
{

	[CustomPropertyDrawer(typeof(DynamicRangeAttribute))]
	public class DynamicRangeDrawer : IllogikaDrawer {
			
		DynamicRangeAttribute dynamicRangeAttribute { get { return ((DynamicRangeAttribute)attribute); } }
		
		public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
		{
	       return base.GetPropertyHeight(prop, label);
	    }

		public float GetBoundValue(SerializedProperty prop, float floatValue, string delegateName)
		{
			float bound = floatValue;

			if(float.IsNaN(bound))
			{
				bound = 0;
				object targetObject = null;
				FieldInfo fieldInfo = GetReflectedFieldInfoRecursively(prop, out targetObject, delegateName);
				if(fieldInfo != null)
				{
					bound = (float)fieldInfo.GetValue(targetObject);
				}
				else
				{
					MonoBehaviour go = prop.serializedObject.targetObject as MonoBehaviour;
					if(go != null)
					{
						bound = (float)CallMethod(prop, go, delegateName);
					}
					else
					{
						ScriptableObject so = prop.serializedObject.targetObject as ScriptableObject;
						if(so != null)
						{
							bound = (float)CallMethod(prop, so, delegateName);
						}
					}
				}
			}
			return bound;
		}

		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, prop);

			position = EditorGUI.PrefixLabel(position, EditorGUIUtility.GetControlID(FocusType.Passive), label);

			int originalIndentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			float min = GetBoundValue(prop, dynamicRangeAttribute.minValue, dynamicRangeAttribute.minDelegate);
			float max = GetBoundValue(prop, dynamicRangeAttribute.maxValue, dynamicRangeAttribute.maxDelegate);

			if(prop.propertyType == SerializedPropertyType.Float)
			{
				EditorGUI.BeginChangeCheck();

				float temp = EditorGUI.Slider(position, prop.floatValue, min, max);

				if(EditorGUI.EndChangeCheck())
				{
					prop.floatValue = temp;
				}
			}
			else if(prop.propertyType == SerializedPropertyType.Integer)
			{
				EditorGUI.BeginChangeCheck();

				int temp = (int)EditorGUI.Slider(position, (float)prop.intValue, (float)(int)min, (float)(int)max);

				if(EditorGUI.EndChangeCheck())
				{
					prop.intValue = temp;
				}
			}

			EditorGUI.indentLevel = originalIndentLevel;

			EditorGUI.EndProperty();
		}
	}

}
