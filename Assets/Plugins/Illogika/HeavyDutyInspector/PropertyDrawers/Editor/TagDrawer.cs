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

	[CustomPropertyDrawer(typeof(TagAttribute))]
	public class TagDrawer : IllogikaDrawer {
			
		TagAttribute tagAttribute { get { return ((TagAttribute)attribute); } }
		
		public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
		{
	       return base.GetPropertyHeight(prop, label);
	    }
		
		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, prop);

			if(prop.propertyType != SerializedPropertyType.String)
			{
				Debug.LogError("The TagAttribute is designed to be applied to strings only");
				WrongVariableTypeWarning("Tag", "strings");
				return;
			}

			int originalIndentLevel = EditorGUI.indentLevel;

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID (FocusType.Passive), label);
			EditorGUI.indentLevel = 0;

			if(prop.stringValue == "")
				prop.stringValue = "Untagged";

			if(prop.hasMultipleDifferentValues)
			{
				EditorGUI.BeginChangeCheck();

				string temp = EditorGUI.TagField(position, "-");

				if(EditorGUI.EndChangeCheck())
					prop.stringValue = temp;
			}
			else
			{
				prop.stringValue = EditorGUI.TagField(position, prop.stringValue);
			}

			EditorGUI.indentLevel = originalIndentLevel;
			EditorGUI.EndProperty();
		}
	}

}
