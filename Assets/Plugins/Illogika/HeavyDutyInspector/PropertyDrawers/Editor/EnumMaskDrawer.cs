//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	[CustomPropertyDrawer(typeof(EnumMaskAttribute))]
	public class EnumMaskDrawer : IllogikaDrawer {
			
		EnumMaskAttribute enumMaskAttribute { get { return ((EnumMaskAttribute)attribute); } }
		
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

			System.Enum propEnum = GetReflectedFieldRecursively<System.Enum>(prop);

			if (propEnum == null)
				return;

			EditorGUI.BeginChangeCheck();

			propEnum = EditorGUI.EnumMaskField(position, propEnum);

			if(EditorGUI.EndChangeCheck())
			{
				Undo.RecordObjects(prop.serializedObject.targetObjects, "Inspector");

				SetReflectedFieldRecursively(prop, propEnum);

				EditorUtility.SetDirty(prop.serializedObject.targetObject);
			}

			EditorGUI.indentLevel = originalIndentLevel;

			EditorGUI.EndProperty();
		}
	}

}
