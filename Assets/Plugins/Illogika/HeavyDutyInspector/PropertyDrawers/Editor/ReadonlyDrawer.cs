//----------------------------------------------
//            Heavy-Duty Inspector
//         Copyright © 2015  Illogika
//----------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	[CustomPropertyDrawer(typeof(ReadonlyAttribute))]
	public class ReadonlyDrawer : IllogikaDrawer {
			
		ReadonlyAttribute readonlyAttribute { get { return ((ReadonlyAttribute)attribute); } }
		
		public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
		{
	       return base.GetPropertyHeight(prop, label);
	    }
		
		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, prop);

			Rect lockPosition = position;
			lockPosition.width = lockPosition.height;
			lockPosition.y += 1;
			lockPosition.x -= 13;
			GUI.Toggle(lockPosition, true, "", "IN LockButton");

			if(prop.propertyType != SerializedPropertyType.Quaternion && prop.propertyType != SerializedPropertyType.Vector2 && prop.propertyType != SerializedPropertyType.Vector3 && prop.propertyType != SerializedPropertyType.Vector4)
				position = EditorGUI.PrefixLabel(position, EditorGUIUtility.GetControlID(FocusType.Passive), label);

			int originalIndentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			switch(prop.propertyType)
			{
				case SerializedPropertyType.AnimationCurve:
					EditorGUI.CurveField(position, prop.animationCurveValue);
					break;
				case SerializedPropertyType.Boolean:
					EditorGUI.Toggle(position, prop.boolValue);
					break;
				case SerializedPropertyType.Bounds:
					EditorGUI.BoundsField(position, prop.boundsValue);
					break;
				case SerializedPropertyType.Color:
					EditorGUI.ColorField(position, prop.colorValue);
					break;
				case SerializedPropertyType.Enum:
					EditorGUI.SelectableLabel(position, prop.enumDisplayNames[prop.enumValueIndex]);
					break;
				case SerializedPropertyType.Float:
					EditorGUI.SelectableLabel(position, prop.floatValue.ToString());
					break;
				case SerializedPropertyType.Integer:
					EditorGUI.SelectableLabel(position, prop.intValue.ToString());
					break;
				case SerializedPropertyType.LayerMask:
					EditorGUI.LayerField(position, prop.intValue);
					break;
				case SerializedPropertyType.ObjectReference:
					EditorGUI.ObjectField(position, prop.objectReferenceValue, fieldInfo.FieldType, true);
					break;
				case SerializedPropertyType.Quaternion:
					EditorGUI.Vector3Field(position, label, prop.quaternionValue.eulerAngles);
					break;
				case SerializedPropertyType.Rect:
					EditorGUI.RectField(position, prop.rectValue);
					break;
				case SerializedPropertyType.String:
					EditorGUI.SelectableLabel(position, prop.stringValue);
					break;
				case SerializedPropertyType.Vector2:
					EditorGUI.Vector2Field(position, label, prop.vector2Value);
					break;
				case SerializedPropertyType.Vector3:
					EditorGUI.Vector3Field(position, label, prop.vector3Value);
					break;
				case SerializedPropertyType.Vector4:
					EditorGUI.Vector4Field(position, label.text, prop.vector4Value);
					break;
			}

			EditorGUI.indentLevel = originalIndentLevel;

			EditorGUI.EndProperty();
		}
	}

}
