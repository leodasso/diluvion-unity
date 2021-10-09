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

	[CustomPropertyDrawer(typeof(BackgroundAttribute))]
	public class BackgroundDrawer : DecoratorDrawer {
			
		BackgroundAttribute backgroundAttribute { get { return ((BackgroundAttribute)attribute); } }
		
		public override float GetHeight ()
		{
			return 0;
	    }
		
		public override void OnGUI (Rect position)
		{
			int originalIndentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			position.x = 0;
			position.width = Screen.width;
			position.height = base.GetHeight();

			Color temp = GUI.color;

			GUI.color = backgroundAttribute.color;
			EditorGUI.HelpBox(position, "", MessageType.None);

			GUI.color = temp;

			EditorGUI.indentLevel = originalIndentLevel;
		}
	}

}
