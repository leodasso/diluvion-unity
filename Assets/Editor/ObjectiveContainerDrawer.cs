using UnityEngine;
using UnityEditor;
using System.Collections;

//[CustomPropertyDrawer(typeof(Quests.ObjectiveContainer))]
public class ObjectiveContainerDrawer : PropertyDrawer {

    const float propHeight = 16;

    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        SerializedProperty objective = property.FindPropertyRelative("objective");
        SerializedProperty overrideName = property.FindPropertyRelative("overrideName");
        SerializedProperty newName = property.FindPropertyRelative("newName");

        Vector2 pos = new Vector2(rect.position.x, rect.position.y + 4);
        Vector2 size = new Vector2(rect.width, propHeight);
        Rect objRect = new Rect(pos, size);
        Rect boolRect = new Rect(new Vector2(pos.x, pos.y + propHeight), size);
        Rect nameRect = new Rect(new Vector2(pos.x, pos.y + propHeight*2), size);
        Rect keyRect = new Rect(new Vector2(pos.x, pos.y + propHeight*3), size);

        GUI.color = Color.cyan;
        string n = "";
        if (objective.objectReferenceValue != null) n = objective.objectReferenceValue.name;

        EditorGUI.PropertyField(objRect, objective, new GUIContent(n));
        GUI.color = Color.white;
        EditorGUI.PropertyField(boolRect, overrideName);

        if (overrideName.boolValue)
        {
            EditorGUI.PropertyField(nameRect, newName);
            GUI.color = Color.white;
        }

        //base.OnGUI(position, property, label);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty overrideName = property.FindPropertyRelative("overrideName");
        if (overrideName.boolValue) return propHeight * 4 + 10;
        return propHeight * 2 + 10;
    }
}
