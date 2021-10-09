using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(CrewStatValue))]
public class StatValueDrawer : PropertyDrawer {

    public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
    {
        const float valueWidth = 50;

        SerializedProperty statObj = property.FindPropertyRelative("statBase");
        SerializedProperty value = property.FindPropertyRelative("value");

        Rect objRect = new Rect(r.position.x, r.position.y, r.width - valueWidth, r.height);
        Rect valueRect = new Rect(r.position.x + objRect.width, r.position.y, valueWidth, r.height);

        EditorGUI.PropertyField(objRect, statObj, GUIContent.none);
        EditorGUI.PropertyField(valueRect, value, GUIContent.none);
    }
}
