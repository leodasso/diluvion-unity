//----------------------------------------------
//            Heavy-Duty Inspector
//         Copyright © 2015  Illogika
//----------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	[CustomPropertyDrawer(typeof(EditablePropertyAttribute))]
	public class EditablePropertyDrawer : IllogikaDrawer {
			
		EditablePropertyAttribute editablePropertyAttribute { get { return ((EditablePropertyAttribute)attribute); } }
		
		public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
		{
	       return base.GetPropertyHeight(prop, label);
	    }

		PropertyInfo cachedInfo;
		MethodInfo cachedGetter;
		MethodInfo cachedSetter;

		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, prop);

			if(cachedInfo == null)
			{
				cachedInfo = null;
				if(string.IsNullOrEmpty(editablePropertyAttribute.accessorName))
				{
					cachedInfo = prop.serializedObject.targetObject.GetType().GetProperty(prop.propertyPath.Split('_').Last(), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				}
				else
				{
					cachedInfo = prop.serializedObject.targetObject.GetType().GetProperty(editablePropertyAttribute.accessorName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				}

				if(cachedInfo != null)
				{
					cachedGetter = cachedInfo.GetGetMethod();
					cachedSetter = cachedInfo.GetSetMethod();
				}
			}

			int originalIndentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Make a copy of the serialized property referencing our variable
			SerializedProperty copy = prop.Copy();

			// Use the getter to set the right value on the copy of the serialized property.
			switch(prop.propertyType)
			{
				case SerializedPropertyType.AnimationCurve:
					copy.animationCurveValue = (AnimationCurve)cachedGetter.Invoke(prop.serializedObject.targetObject, null);
					break;
				case SerializedPropertyType.Boolean:
					copy.boolValue = (bool)cachedGetter.Invoke(prop.serializedObject.targetObject, null);
					break;
				case SerializedPropertyType.Bounds:
					copy.boundsValue = (Bounds)cachedGetter.Invoke(prop.serializedObject.targetObject, null);
					break;
				case SerializedPropertyType.Color:
					copy.colorValue = (Color)cachedGetter.Invoke(prop.serializedObject.targetObject, null);
					break;
				case SerializedPropertyType.Enum:
					copy.enumValueIndex = (int)cachedGetter.Invoke(prop.serializedObject.targetObject, null);
					break;
				case SerializedPropertyType.Float:
					copy.floatValue = (float)cachedGetter.Invoke(prop.serializedObject.targetObject, null);
					break;
				case SerializedPropertyType.Integer:
					copy.intValue = (int)cachedGetter.Invoke(prop.serializedObject.targetObject, null);
					break;
				case SerializedPropertyType.LayerMask:
					copy.intValue = (int)cachedGetter.Invoke(prop.serializedObject.targetObject, null);
					break;
				case SerializedPropertyType.ObjectReference:
					copy.objectReferenceValue = (Object)cachedGetter.Invoke(prop.serializedObject.targetObject, null);
					break;
				case SerializedPropertyType.Quaternion:
					copy.quaternionValue = (Quaternion)cachedGetter.Invoke(prop.serializedObject.targetObject, null);
					break;
				case SerializedPropertyType.Rect:
					copy.rectValue = (Rect)cachedGetter.Invoke(prop.serializedObject.targetObject, null);
					break;
				case SerializedPropertyType.String:
					copy.stringValue = (string)cachedGetter.Invoke(prop.serializedObject.targetObject, null);
					break;
				case SerializedPropertyType.Vector2:
					copy.vector2Value = (Vector2)cachedGetter.Invoke(prop.serializedObject.targetObject, null);
					break;
				case SerializedPropertyType.Vector3:
					copy.vector3Value = (Vector3)cachedGetter.Invoke(prop.serializedObject.targetObject, null);
					break;
				case SerializedPropertyType.Vector4:
					copy.vector4Value = (Vector4)cachedGetter.Invoke(prop.serializedObject.targetObject, null);
					break;
			}

			EditorGUI.BeginChangeCheck();
			
			EditorGUI.PropertyField(position, copy);
			if(EditorGUI.EndChangeCheck())
			{
				// Use the setter to set the value on the variable. This value will be overriden by unity because we didn't modify the serialized property though.
				foreach(Object obj in prop.serializedObject.targetObjects)
				{
					switch(prop.propertyType)
					{
						case SerializedPropertyType.AnimationCurve:
							cachedSetter.Invoke(obj, new object[] { copy.animationCurveValue });
							break;
						case SerializedPropertyType.Boolean:
							cachedSetter.Invoke(obj, new object[] { copy.boolValue });
							break;
						case SerializedPropertyType.Bounds:
							cachedSetter.Invoke(obj, new object[] { copy.boundsValue });
							break;
						case SerializedPropertyType.Color:
							cachedSetter.Invoke(obj, new object[] { copy.colorValue });
							break;
						case SerializedPropertyType.Enum:
							cachedSetter.Invoke(obj, new object[] { copy.enumValueIndex });
							break;
						case SerializedPropertyType.Float:
							cachedSetter.Invoke(obj, new object[] { copy.floatValue });
							break;
						case SerializedPropertyType.Integer:
							cachedSetter.Invoke(obj, new object[] { copy.intValue });
							break;
						case SerializedPropertyType.LayerMask:
							cachedSetter.Invoke(obj, new object[] { copy.intValue });
							break;
						case SerializedPropertyType.ObjectReference:
							cachedSetter.Invoke(obj, new object[] { copy.objectReferenceValue });
							break;
						case SerializedPropertyType.Quaternion:
							cachedSetter.Invoke(obj, new object[] { copy.quaternionValue });
							break;
						case SerializedPropertyType.Rect:
							cachedSetter.Invoke(obj, new object[] { copy.rectValue });
							break;
						case SerializedPropertyType.String:
							cachedSetter.Invoke(obj, new object[] { copy.stringValue });
							break;
						case SerializedPropertyType.Vector2:
							cachedSetter.Invoke(obj, new object[] { copy.vector2Value });
							break;
						case SerializedPropertyType.Vector3:
							cachedSetter.Invoke(obj, new object[] { copy.vector3Value });
							break;
						case SerializedPropertyType.Vector4:
							cachedSetter.Invoke(obj, new object[] { copy.vector4Value });
							break;
					}
				}

				// So we need to get the value of our variable from the PropertyInfo directly without going through the getter again and apply that to the serialized property.
				switch(prop.propertyType)
				{
					case SerializedPropertyType.AnimationCurve:
						prop.animationCurveValue = (AnimationCurve)cachedInfo.GetValue(prop.serializedObject.targetObject, null);
						break;
					case SerializedPropertyType.Boolean:
						prop.boolValue = (bool)cachedInfo.GetValue(prop.serializedObject.targetObject, null);
						break;
					case SerializedPropertyType.Bounds:
						prop.boundsValue = (Bounds)cachedInfo.GetValue(prop.serializedObject.targetObject, null);
						break;
					case SerializedPropertyType.Color:
						prop.colorValue = (Color)cachedInfo.GetValue(prop.serializedObject.targetObject, null);
						break;
					case SerializedPropertyType.Enum:
						prop.enumValueIndex = (int)cachedInfo.GetValue(prop.serializedObject.targetObject, null);
						break;
					case SerializedPropertyType.Float:
						prop.floatValue = (float)cachedInfo.GetValue(prop.serializedObject.targetObject, null);
						break;
					case SerializedPropertyType.Integer:
						prop.intValue = (int)cachedInfo.GetValue(prop.serializedObject.targetObject, null);
						break;
					case SerializedPropertyType.LayerMask:
						prop.intValue = (int)cachedInfo.GetValue(prop.serializedObject.targetObject, null);
						break;
					case SerializedPropertyType.ObjectReference:
						prop.objectReferenceValue = (Object)cachedInfo.GetValue(prop.serializedObject.targetObject, null);
						break;
					case SerializedPropertyType.Quaternion:
						prop.quaternionValue = (Quaternion)cachedInfo.GetValue(prop.serializedObject.targetObject, null);
						break;
					case SerializedPropertyType.Rect:
						prop.rectValue = (Rect)cachedInfo.GetValue(prop.serializedObject.targetObject, null);
						break;
					case SerializedPropertyType.String:
						prop.stringValue = (string)cachedInfo.GetValue(prop.serializedObject.targetObject, null);
						break;
					case SerializedPropertyType.Vector2:
						prop.vector2Value = (Vector2)cachedInfo.GetValue(prop.serializedObject.targetObject, null);
						break;
					case SerializedPropertyType.Vector3:
						prop.vector3Value = (Vector3)cachedInfo.GetValue(prop.serializedObject.targetObject, null);
						break;
					case SerializedPropertyType.Vector4:
						prop.vector4Value = (Vector4)cachedInfo.GetValue(prop.serializedObject.targetObject, null);
						break;
				}
			}

			EditorGUI.indentLevel = originalIndentLevel;

			EditorGUI.EndProperty();
		}
	}

}
