//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2013 - 2015  Illogika
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	[CustomPropertyDrawer(typeof(ComponentSelectionAttribute))]
	public class ComponentSelectionDrawer : IllogikaDrawer {

		ComponentSelectionAttribute componentSelectionAttribute { get { return ((ComponentSelectionAttribute)attribute); } }
		
		public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
		{
	       return base.GetPropertyHeight(prop, label) * 2;
	    }

		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, prop);

			OnComponentGUI(position, prop, label, componentSelectionAttribute.fieldName, componentSelectionAttribute.requiredValues, componentSelectionAttribute.defaultObject, componentSelectionAttribute.isPrefab, 0);

			EditorGUI.EndProperty();
		}
	}

}
