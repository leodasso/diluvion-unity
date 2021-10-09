//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;


//[CustomPropertyDrawer(typeof(DTagListAttribute))]

public class DTagListDrawer : MonoBehaviour {
    /*
    DTagListAttribute tagListAttribute { get { return ((DTagListAttribute)attribute); } }
		
	public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
	{
		/*if(prop.serializedObject.targetObjects.Length > 1)
		{
			if(int.Parse(prop.propertyPath.Split('[')[1].Split(']')[0]) != 0)
				return -2.0f;
			else
				return base.GetPropertyHeight(prop, label) * 2;
		}

	    return base.GetPropertyHeight(prop, label);
	}

    List<Tag> dtags = new List<Tag>();
    List<string> dTagNames = new List<string>();

    bool gotLists = false;
    public void DrawTags()
    {
        if (gotLists) return;
        dTagNames = new List<string>();
        dtags = TagsGlobal.Get().allItems;

        foreach(Tag dt in dtags)            
            dTagNames.Add(dt.name);
        gotLists = true;
    }


    //int selectedPopup = 0;

    public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
	{
        DrawTags();

        EditorGUI.BeginProperty(position, label, prop);

		int index = int.Parse(prop.propertyPath.Split('[')[1].Split(']')[0]);

		IList list = null;
		try
		{
			list = (prop.serializedObject.targetObject as MonoBehaviour).GetType().GetField(prop.propertyPath.Split('.')[0]).GetValue(prop.serializedObject.targetObject) as IList;
		}
		catch
		{
			try{
				list = (prop.serializedObject.targetObject as ScriptableObject).GetType().GetField(prop.propertyPath.Split('.')[0]).GetValue(prop.serializedObject.targetObject) as IList;
			}
			catch{
				Debug.LogWarning(string.Format("The script has no property named {0} or {0} is not an IList",prop.propertyPath.Split('.')[0]));
			}
		}

		if(prop.serializedObject.targetObjects.Length > 1)
		{
			if(index == 0)
			{
				position.height = base.GetPropertyHeight(prop, label) * 2;
				EditorGUI.indentLevel = 1;
				position = EditorGUI.IndentedRect(position);
				EditorGUI.HelpBox(position, "Multi object editing is not supported.", MessageType.Warning);
			}
			return;
		}

		int originalIndentLevel = EditorGUI.indentLevel;

		position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID (FocusType.Passive), label);
		EditorGUI.indentLevel = 0;

		if(tagListAttribute.canDeleteFirstElement || index != 0)
			position.width -= 18;

        int popIndex = 0;

        try
        {
            popIndex = dtags.IndexOf(prop.objectReferenceValue as Tag);
        }
        catch
        {
            prop.objectReferenceValue = null;
        }

        try
        {
            prop.objectReferenceValue = dtags[EditorGUI.Popup(position, popIndex, dTagNames.ToArray())];                      
        }
        catch
        {
        }

          

		position.x += position.width;
		position.width = 16;

		if((tagListAttribute.canDeleteFirstElement || index != 0) && GUI.Button(position, "", "OL Minus"))
		{
			list.RemoveAt(index);
		}


		EditorGUI.indentLevel = originalIndentLevel;
		EditorGUI.EndProperty();
	
    */

}
