//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2013 - 2015  Illogika
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace HeavyDutyInspector
{

	[CustomPropertyDrawer(typeof(NamedMonoBehaviour))]
	public class NamedMonoBehaviourDrawer : IllogikaDrawer {

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			OnNamedMonoBehaviourGUI(position, property, label, fieldInfo.FieldType, 0);
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label) * 2;
		}


	}

}
