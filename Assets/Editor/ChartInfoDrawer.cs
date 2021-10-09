using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ChartInfo))]
public class ChartInfoDrawer : PropertyDrawer {

    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        // Find properties
        SerializedProperty chartItem = property.FindPropertyRelative("chartItem");        
        Rect newRect = new Rect(rect.x, rect.y, rect.width + 80, rect.height + 5);

        GUIContent[] labels = new GUIContent[3] {

            new GUIContent("", "LM Chart item"),
            new GUIContent("$"),
            new GUIContent("", "Been found by the player?")
        };

        EditorGUI.MultiPropertyField(newRect, labels, chartItem, GUIContent.none);
        
    }
}
