//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	[CustomPropertyDrawer(typeof(HideConditionalAttribute))]
	public class HideConditionalDrawer : IllogikaDrawer {
			
		HideConditionalAttribute hideConditionalAttribute { get { return ((HideConditionalAttribute)attribute); } }

		public bool isVisible(SerializedProperty prop)
		{
			bool visible = false;
			switch(hideConditionalAttribute.conditionType)
			{
			case HideConditionalAttribute.ConditionType.IsNotNull:
				object targetObject = null;
				FieldInfo fi = GetReflectedFieldInfoRecursively(prop, out targetObject, hideConditionalAttribute.variableName);
				if(fi != null)
				{
					visible = GetReflectedFieldRecursively<System.Object>(prop, hideConditionalAttribute.variableName) != null;
				}
				else
				{
					MonoBehaviour go = prop.serializedObject.targetObject as MonoBehaviour;
					if(go != null)
					{
						visible = (bool)CallMethod(prop, go, hideConditionalAttribute.variableName);
					}
					else
					{
						ScriptableObject so = prop.serializedObject.targetObject as ScriptableObject;
						if(so != null)
						{
							visible = (bool)CallMethod(prop, so, hideConditionalAttribute.variableName);
						}
					}
				}
				break;
			case HideConditionalAttribute.ConditionType.IntOrEnum:
				visible = hideConditionalAttribute.enumValues.Contains(GetReflectedFieldRecursively<int>(prop, hideConditionalAttribute.variableName));
				break;
			case HideConditionalAttribute.ConditionType.FloatRange:
				if(hideConditionalAttribute.minValue > hideConditionalAttribute.maxValue)
				{
					Debug.LogError("Min value has to be lower than max value");
					return false;
				}
				else
				{
					visible = GetReflectedFieldRecursively<float>(prop, hideConditionalAttribute.variableName) >= hideConditionalAttribute.minValue && GetReflectedFieldRecursively<float>(prop, hideConditionalAttribute.variableName) <= hideConditionalAttribute.maxValue;
				}
				break;
			case HideConditionalAttribute.ConditionType.IntRange:
				if(hideConditionalAttribute.minValue > hideConditionalAttribute.maxValue)
				{
					Debug.LogError("Min value has to be lower than max value");
					return false;
				}
				else
				{
					visible = GetReflectedFieldRecursively<int>(prop, hideConditionalAttribute.variableName) >= (int)hideConditionalAttribute.minValue && GetReflectedFieldRecursively<int>(prop, hideConditionalAttribute.variableName) <= (int)hideConditionalAttribute.maxValue;
				}
				break;
			case HideConditionalAttribute.ConditionType.Bool:
				visible = GetReflectedFieldRecursively<bool>(prop, hideConditionalAttribute.variableName) == hideConditionalAttribute.boolValue;
				break;
			default:
				break;
			}

			if(!hideConditionalAttribute.hide)
				visible = !visible;

			return visible;
		}

		public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
		{
			if(isVisible(prop))
			{
				if(string.IsNullOrEmpty(hideConditionalAttribute.comment))
					return EditorGUI.GetPropertyHeight(prop, label, true);
				else
					return GetCommentHeight(hideConditionalAttribute.comment, hideConditionalAttribute.messageType) + EditorGUI.GetPropertyHeight(prop, label, true);
			}

			return -2.0f;
	    }
		
		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, prop);

			if(isVisible(prop))
			{
				if(!string.IsNullOrEmpty(hideConditionalAttribute.comment))
				{
					position.height = GetCommentHeight(hideConditionalAttribute.comment, hideConditionalAttribute.messageType);

					EditorGUI.HelpBox(position, hideConditionalAttribute.comment, hideConditionalAttribute.messageType);

					position.y += position.height + 2f;
				}

				position.height = base.GetPropertyHeight(prop, label);

				PropertyFieldIncludingSpecialAndFoldouts(prop, position, position, label);
			}

			EditorGUI.EndProperty();
		}
	}
}
