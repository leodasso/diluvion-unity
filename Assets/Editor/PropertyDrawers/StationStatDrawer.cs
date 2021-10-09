using UnityEditor;
using UnityEngine;
using HeavyDutyInspector;

// IngredientDrawer
[CustomPropertyDrawer(typeof(StationStat))]
public class StationStatDrawer : PropertyDrawer
{

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Don't make child fields be indented
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        float width = position.width/10;
        // Calculate rects//TODO GET BETTER RECTS AND LAYOUT BULLSHIT
        //var amountRect = new Rect(position.x, position.y, width*3, position.height);
        var unitRect = new Rect(position.x, position.y, width * 4, position.height);
        var nameRect = new Rect(position.x + width * 5, position.y, width * 5, position.height);

        // Draw fields - passs GUIContent.none to each so they are drawn without labels
        SerializedProperty baseStatContainer = property.FindPropertyRelative("baseStat");
     
        if (baseStatContainer != null)
        {
            Object statObject = baseStatContainer.objectReferenceValue;
          
            if (statObject != null)
            {              
                StationStatObject sop = (StationStatObject)statObject;
                GUI.backgroundColor = sop.statColor;
                GUI.Box(position, "");
                GUI.backgroundColor = Color.white;
                //if (sop != null)EditorGUI.LabelField(amountRect, sop.statName);
            }
        }
        EditorGUI.PropertyField(unitRect, baseStatContainer, GUIContent.none);
        EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("totalBonus"), GUIContent.none);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();

    }


}