//----------------------------------------------
//            Heavy-Duty Inspector
//         Copyright © 2015  Illogika
//----------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

namespace HeavyDutyInspector
{
	[CustomPropertyDrawer(typeof(ComponentFieldRestrictionAttribute))]
	[CustomPropertyDrawer(typeof(ComponentField))]

	public class ComponentFieldDrawer : IllogikaDrawer {

		ComponentFieldRestrictionAttribute restrictionAttribute { get { return ((ComponentFieldRestrictionAttribute)attribute); } }

		public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
		{
			return base.GetPropertyHeight(prop, label) * 2 + base.GetPropertyHeight(prop, label) * prop.FindPropertyRelative("propertyPath").arraySize;
	    }

		private System.Type lastType;

		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, prop);

			position.height = EditorGUIUtility.singleLineHeight * 2;

			int originalIndentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			System.Type componentType = typeof(Component);
			if(restrictionAttribute != null && restrictionAttribute.componentType != null)
				componentType = restrictionAttribute.componentType;

			OnComponentGUI(position, prop.FindPropertyRelative("component"), label, "", new string[] { }, "", false, 0, componentType);
			position.y += EditorGUIUtility.singleLineHeight;
			
			position = EditorGUI.PrefixLabel(position, new GUIContent(" "));

			Component component = prop.FindPropertyRelative("component").objectReferenceValue as Component;

			position.height = EditorGUIUtility.singleLineHeight;

			SerializedProperty propertyPath = prop.FindPropertyRelative("propertyPath");

			if(component != null && (propertyPath.arraySize == 0 || !string.IsNullOrEmpty(propertyPath.GetArrayElementAtIndex(propertyPath.arraySize - 1).stringValue)) && (restrictionAttribute == null || restrictionAttribute.endMemberType == null || restrictionAttribute.endMemberType != lastType))
			{
				propertyPath.InsertArrayElementAtIndex(propertyPath.arraySize);
				propertyPath.GetArrayElementAtIndex(propertyPath.arraySize - 1).stringValue = "";
			}

			if(component == null)
			{
				prop.FindPropertyRelative("propertyPath").ClearArray();
				return;
			}

			List<string> fieldNames = new List<string>();
			bool lastWasProperty = false;				
			
			System.Type type = component.GetType();
			for(int i = 0; i < propertyPath.arraySize; ++i)
			{
				position.y += position.height;

				foreach(FieldInfo info in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
				{
					fieldNames.Add(info.Name);
				}
				foreach(PropertyInfo info in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
				{
					fieldNames.Add(info.Name);
				}
				
				fieldNames.Insert(0, "");

				int index = fieldNames.IndexOf(propertyPath.GetArrayElementAtIndex(i).stringValue);

				if(index < 0)
					index = 0;

				fieldNames[0] = "None";

				if(restrictionAttribute != null && restrictionAttribute.endMemberType != null && i == propertyPath.arraySize - 1 && !string.IsNullOrEmpty(propertyPath.GetArrayElementAtIndex(i).stringValue))
					GUI.color = Color.green;

				if(restrictionAttribute != null && restrictionAttribute.endMemberType != null && i == propertyPath.arraySize - 2 && string.IsNullOrEmpty(propertyPath.GetArrayElementAtIndex(i + 1).stringValue))
					GUI.color = Color.red;

				index = EditorGUI.Popup(position, index, fieldNames.ToArray());

				GUI.color = Color.white;

				fieldNames[0] = "";

				propertyPath.GetArrayElementAtIndex(i).stringValue = fieldNames[index];

				if(index != 0)
				{
					try
					{
						type = type.GetField(fieldNames[index], BindingFlags.Instance | BindingFlags.Public).FieldType;
						lastWasProperty = false;
					}
					catch
					{
						// Not a field. Assume a property.
						type = type.GetProperty(fieldNames[index], BindingFlags.Instance | BindingFlags.Public).PropertyType;
						lastWasProperty = true;
					}

					if(restrictionAttribute != null && restrictionAttribute.endMemberType != null && type == restrictionAttribute.endMemberType)
					{
						for(int j = i + 1; j < propertyPath.arraySize; ++j)
						{
							propertyPath.DeleteArrayElementAtIndex(j);
						}
					}
				}
				else
				{
					for(int j = i + 1; j < propertyPath.arraySize; ++j)
					{
						propertyPath.DeleteArrayElementAtIndex(j);
					}
				}
				fieldNames.Clear();
				lastType = type;
			}

			prop.FindPropertyRelative("isProperty").boolValue = lastWasProperty;

			EditorGUI.indentLevel = originalIndentLevel;

			EditorGUI.EndProperty();
		}
	}

}
