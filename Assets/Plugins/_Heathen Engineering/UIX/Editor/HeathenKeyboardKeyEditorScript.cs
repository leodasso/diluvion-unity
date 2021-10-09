using UnityEngine;
using System.Collections;
using UnityEditor;
#pragma warning disable 618

[CustomEditor(typeof(HeathenEngineering.UIX.KeyboardKey))]
public class HeathenKeyboardKeyEditorScript : Editor
{
    public override void OnInspectorGUI()
    {
        HeathenEngineering.UIX.KeyboardKey key = target as HeathenEngineering.UIX.KeyboardKey;
        key.keyType = (HeathenEngineering.UIX.KeyboardKeyType)EditorGUILayout.EnumPopup("Type", key.keyType);

        key.EditorParseKeyCode = EditorGUILayout.ToggleLeft("Parse return values from key code?", key.EditorParseKeyCode);
        KeyCode previousCode = (KeyCode)EditorGUILayout.EnumPopup("Code", key.keyGlyph.code);
        if(key.keyGlyph.code != previousCode && key.EditorParseKeyCode)
        {
            key.keyGlyph.code = previousCode;
            key.keyGlyph.DefaultFromCode(key.keyGlyph.code);

            if(key.keyGlyph.normalDisplay != null)
                EditorUtility.SetDirty(key.keyGlyph.normalDisplay);

            if (key.keyGlyph.shiftedDisplay != null)
                EditorUtility.SetDirty(key.keyGlyph.shiftedDisplay);

            if (key.keyGlyph.altGrDisplay != null)
                EditorUtility.SetDirty(key.keyGlyph.altGrDisplay);

            if (key.keyGlyph.shiftedAltGrDisplay != null)
                EditorUtility.SetDirty(key.keyGlyph.shiftedAltGrDisplay);
        }
        else
            key.keyGlyph.code = previousCode;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("State Glyphs", EditorStyles.boldLabel);
        key.keyGlyph.normal = EditorGUILayout.ObjectField("Normal", key.keyGlyph.normal, typeof(RectTransform)) as RectTransform;
        if (key.keyGlyph.normal != null)
        {
            key.keyGlyph.normalDisplay = EditorGUILayout.ObjectField("Text", key.keyGlyph.normalDisplay, typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text;
            EditorGUILayout.LabelField("Return Value:");
            key.keyGlyph.normalString = EditorGUILayout.TextArea(key.keyGlyph.normalString);
        }
        EditorGUILayout.Space(); 
        key.keyGlyph.shifted = EditorGUILayout.ObjectField("On Shift", key.keyGlyph.shifted, typeof(RectTransform)) as RectTransform;
        if (key.keyGlyph.shifted != null)
        {
            key.keyGlyph.shiftedDisplay = EditorGUILayout.ObjectField("Text", key.keyGlyph.shiftedDisplay, typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text;
            EditorGUILayout.LabelField("Return Value:");
            key.keyGlyph.shiftedString = EditorGUILayout.TextArea(key.keyGlyph.shiftedString);
        }
        EditorGUILayout.Space();
        key.keyGlyph.altGr = EditorGUILayout.ObjectField("On AltGr", key.keyGlyph.altGr, typeof(RectTransform)) as RectTransform;
        if (key.keyGlyph.altGr != null)
        {
            key.keyGlyph.altGrDisplay = EditorGUILayout.ObjectField("Text", key.keyGlyph.altGrDisplay, typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text;
            EditorGUILayout.LabelField("Return Value:");
            key.keyGlyph.altGrString = EditorGUILayout.TextArea(key.keyGlyph.altGrString);
        }
        EditorGUILayout.Space();
        key.keyGlyph.shiftedAltGr = EditorGUILayout.ObjectField("On Shift + AltGr", key.keyGlyph.shiftedAltGr, typeof(RectTransform)) as RectTransform;
        if (key.keyGlyph.shiftedAltGr != null)
        {
            key.keyGlyph.shiftedAltGrDisplay = EditorGUILayout.ObjectField("Text", key.keyGlyph.shiftedAltGrDisplay, typeof(UnityEngine.UI.Text)) as UnityEngine.UI.Text;
            EditorGUILayout.LabelField("Return Value:");
            key.keyGlyph.shiftedAltGrString = EditorGUILayout.TextArea(key.keyGlyph.shiftedAltGrString);
        }

        EditorUtility.SetDirty(key);
        
    }
}
