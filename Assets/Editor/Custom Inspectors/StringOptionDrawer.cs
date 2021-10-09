using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer (typeof (StringOptionDrawer))]
public class StringOptionDrawer : PropertyDrawer 
{
	StringOption stringOption { get { return ((StringOption)attribute); } }


	public override float GetPropertyHeight (SerializedProperty prop, GUIContent label) 
	{
		return base.GetPropertyHeight (prop, label);
	}

	public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label) 
	{
		// Adjust height of the text field
		Rect textFieldPosition = position;
		DrawTextField (textFieldPosition, prop, label);
	
	}

	void DrawTextField (Rect position, SerializedProperty prop, GUIContent label) {
		// Draw the text field control GUI.
		int checkInt = stringOption.options.IndexOf(prop.stringValue);
		if(checkInt < 0)
			checkInt = 0;

		EditorGUI.BeginChangeCheck();
		checkInt = EditorGUI.Popup(position, checkInt, stringOption.options.ToArray());
		string optionValue = stringOption.options[checkInt];
			
		if (EditorGUI.EndChangeCheck ())
			prop.stringValue = optionValue;
	}



}
