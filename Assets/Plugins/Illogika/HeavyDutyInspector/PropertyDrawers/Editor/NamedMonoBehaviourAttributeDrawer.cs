//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2013 - 2015  Illogika
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	[CustomPropertyDrawer(typeof(NamedMonoBehaviourAttribute))]
	public class NamedMonoBehaviourAttributeDrawer : NamedMonoBehaviourDrawer {

		public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
		{
	       return base.GetPropertyHeight(prop, label);
	    }
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			if(property.propertyType != SerializedPropertyType.ObjectReference)
			{
				WrongVariableTypeWarning("NamedMonoBehaviour", "object references");
				return;
			}

			if(!fieldInfo.FieldType.IsArray && !fieldInfo.FieldType.IsSubclassOf(typeof(NamedMonoBehaviour)) && !(fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericArguments()[0].IsSubclassOf(typeof(NamedMonoBehaviour))))
			{
				WrongVariableTypeWarning("NamedMonoBehaviour", "classes extending NamedMonoBehaviour");
				return;
			}

			Type type; 
			if(fieldInfo.FieldType.IsArray)
				type = fieldInfo.FieldType.GetElementType();
			else if(fieldInfo.FieldType.IsGenericType)
				type = fieldInfo.FieldType.GetGenericArguments()[0];
			else
				type = fieldInfo.FieldType;

			OnNamedMonoBehaviourGUI(position, property, label, type, 0);
			EditorGUI.EndProperty();
		}
	}

}
