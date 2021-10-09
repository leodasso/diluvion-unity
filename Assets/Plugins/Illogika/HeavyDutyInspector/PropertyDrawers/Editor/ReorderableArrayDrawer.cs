//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2013 - 2015  Illogika
//----------------------------------------------
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace HeavyDutyInspector
{

	[CustomPropertyDrawer(typeof(ReorderableArrayAttribute))]
	public class ReorderableArrayDrawer : IllogikaDrawer {

		ReorderableArrayAttribute reorderableListAttribute { get { return ((ReorderableArrayAttribute)attribute); } }

		public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
		{
			if(prop.serializedObject.targetObjects.Length > 1)
			{
				if(int.Parse(prop.propertyPath.Split('[')[1].Split(']')[0]) != 0)
					return -2.0f;
				else
					return base.GetPropertyHeight(prop, label) * 2;
			}

			if(prop.hasChildren && prop.isExpanded)
			{
				return EditorGUI.GetPropertyHeight(prop, label, true);
			}
			else
			{				
				if(fieldInfo.FieldType.GetElementType().IsSubclassOf(typeof(Component)) && !reorderableListAttribute.useDefaultComponentDrawer)
				{
					return (base.GetPropertyHeight(prop, label) + 1) * 2;
				}
				else
				{
					return base.GetPropertyHeight(prop, label);
				}
			}
	    }
		
		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, prop);

			int index = int.Parse(prop.propertyPath.Split(']')[prop.propertyPath.Split(']').Length-2].Split('[').Last());

			if(!fieldInfo.FieldType.IsArray)
			{
				Debug.LogWarning("The Reorderable Array Attribute can only be used with Arrays.");
			}

			if(prop.serializedObject.targetObjects.Length > 1)
			{
				if(index == 0)
				{
					position.height = base.GetPropertyHeight(prop, label) * 2;
					EditorGUI.indentLevel = 1;
					position = EditorGUI.IndentedRect(position);
					EditorGUI.HelpBox(position, "Multi object editing is not supported.", MessageType.Warning);
				}
				return;
			}

			//Debug.Log(prop.serializedObject.FindProperty(prop.propertyPath.Split(new string [] {".data"}, StringSplitOptions.RemoveEmptyEntries)[0]).propertyPath);

			SerializedProperty array = prop.serializedObject.FindProperty(prop.propertyPath.Split(new string[] { ".data" }, StringSplitOptions.RemoveEmptyEntries)[0]);

			Rect basePosition = position;

			position.width -= 84;
			position.height = 16;

			if(prop.hasChildren && prop.isExpanded)
			{
				PropertyFieldIncludingSpecialAndFoldouts(prop, position, basePosition, label);
			}
			else if(fieldInfo.FieldType.GetElementType().IsSubclassOf(typeof(Component)) && !reorderableListAttribute.useDefaultComponentDrawer)
			{
				OnComponentGUI(basePosition, prop, label, "", null, "", false, 82);
			}
			else
			{
				EditorGUI.PropertyField(position, prop);
			}

			basePosition.x += basePosition.width - 82;
			basePosition.width = 25;
			basePosition.height = 16;

			if(index != 0)
			{
				if(GUI.Button(basePosition, reorderableListAttribute.arrowUp, "ButtonLeft"))
				{
					Undo.RecordObjects(prop.serializedObject.targetObjects, "Move Item Up In List");

					array.MoveArrayElement(index, index - 1);

					foreach(Object obj in prop.serializedObject.targetObjects)
					{
						EditorUtility.SetDirty(obj);
					}
				}
			}
			else
			{
				Color temp = GUI.color;
				GUI.color = Color.gray;
				GUI.Box(basePosition, reorderableListAttribute.arrowUp, "ButtonLeft");
				GUI.color = temp;
			}

			basePosition.x += 25;

			if(index != array.arraySize - 1)
			{
				if(GUI.Button(basePosition, reorderableListAttribute.arrowDown, "ButtonRight") && index != array.arraySize - 1)
				{
					Undo.RecordObjects(prop.serializedObject.targetObjects, "Move Item Down In List");

					array.MoveArrayElement(index, index + 1);

					foreach(Object obj in prop.serializedObject.targetObjects)
					{
						EditorUtility.SetDirty(obj);
					}
				}
			}
			else
			{
				Color temp = GUI.color;
				GUI.color = Color.gray;
				GUI.Box(basePosition, reorderableListAttribute.arrowDown, "ButtonRight");
				GUI.color = temp;
			}

			basePosition.x += 26;
            basePosition.y += 2;
			basePosition.width = 16;

            if (GUI.Button(basePosition, reorderableListAttribute.olPlus, "Label"))
			{
				Undo.RecordObjects(prop.serializedObject.targetObjects, "Add Item In List");

				array.InsertArrayElementAtIndex(index + 1);

				foreach(Object obj in prop.serializedObject.targetObjects)
				{
					EditorUtility.SetDirty(obj);
				}
			}

			basePosition.x += 16;

            if (GUI.Button(basePosition, reorderableListAttribute.olMinus, "Label"))
			{
				Undo.RecordObjects(prop.serializedObject.targetObjects, "Remove Item In List");

				array.DeleteArrayElementAtIndex(index);

				foreach(Object obj in prop.serializedObject.targetObjects)
				{
					EditorUtility.SetDirty(obj);
				}
			}
			
			EditorGUI.EndProperty();
		}

		object CreateInnerFieldsRecursively(FieldInfo fieldInfo, object parentObject)
		{
			if(fieldInfo.FieldType.IsClass && !fieldInfo.FieldType.IsSubclassOf(typeof(MonoBehaviour)) && fieldInfo.FieldType.GetConstructor(new Type[0]) != null)
			{
				object obj = System.Activator.CreateInstance(fieldInfo.FieldType);
				foreach(FieldInfo field in fieldInfo.FieldType.GetFields())
				{
					field.SetValue(obj, CreateInnerFieldsRecursively(field, fieldInfo.GetValue(parentObject)));
				}
				return obj;
			}
			else
			{
				return fieldInfo.GetValue(parentObject);
			}
		}
	}

}
