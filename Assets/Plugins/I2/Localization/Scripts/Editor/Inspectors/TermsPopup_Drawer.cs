using UnityEngine;
using UnityEditor;

namespace I2.Loc
{
	[CustomPropertyDrawer (typeof (TermsPopup))]
	public class TermsPopup_Drawer : PropertyDrawer 
	{
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowGUI(position, property, label, null);
        }


        public static bool ShowGUI (Rect position, SerializedProperty property, GUIContent label, LanguageSource source) 
		{
			EditorGUI.BeginChangeCheck ();

			var Terms = (source==null ? LocalizationManager.GetTermsList() : source.GetTermsList());
			Terms.Sort(System.StringComparer.OrdinalIgnoreCase);
            Terms.Add("");
            Terms.Add("<inferred from text>");
            Terms.Add("<none>");
            var index = (property.stringValue == "-" || property.stringValue == "" ? Terms.Count - 1 : 
                        (property.stringValue == " " ? Terms.Count - 2 : 
                        Terms.IndexOf(property.stringValue)));
            var newIndex = EditorGUI.Popup(position, label.text, index, Terms.ToArray());

            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = (newIndex < 0 || newIndex == Terms.Count - 1) ? string.Empty : Terms[newIndex];
                if (newIndex == Terms.Count - 1)
                    property.stringValue = "-";
                else
                if (newIndex < 0 || newIndex == Terms.Count - 2)
                    property.stringValue = string.Empty;
                else
                    property.stringValue = Terms[newIndex];
                return true;
            }
            return false;
		}
	}

    [CustomPropertyDrawer(typeof(LocalizedString))]
    public class LocalizedStringDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var termRect = rect;    termRect.xMax -= 25;
            var termProp = property.FindPropertyRelative("mTerm");
            TermsPopup_Drawer.ShowGUI(termRect, termProp, label, null);

            var maskRect = rect;    maskRect.xMin = maskRect.xMax - 25;
            var termIgnoreRTL       = property.FindPropertyRelative("mRTL_IgnoreArabicFix");
            var termConvertNumbers  = property.FindPropertyRelative("mRTL_ConvertNumbers");
            int mask = (termIgnoreRTL.boolValue ? 0 : 1) + 
                       (termConvertNumbers.boolValue ? 0 : 2);

            int newMask = EditorGUI.MaskField(maskRect, mask, new string[] { "Arabic Fix", "Ignore Numbers in RTL" });
            if (newMask != mask)
            {
                termIgnoreRTL.boolValue      = (newMask & 1) == 0;
                termConvertNumbers.boolValue = (newMask & 2) == 0;
            }
        }
    }
}