//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

public class BaseKeywordDrawer : IllogikaDrawer {

	private static GUIContent plusContent
	{
		get;
		set;
	}

	private static GUIContent minusContent
	{
		get;
		set;
	}

	private static GUIContent checkContent
	{
		get;
		set;
	}

	private static GUIContent xContent
	{
		get;
		set;
	}

	protected KeywordsConfig scriptableConfig;
	protected List<KeywordCategory> config;

	private List<string> categories = new List<string> ();
	private List<string> keywords = new List<string>();

	Dictionary<string,string> newValues = new Dictionary<string,string>();

	Dictionary<string,int> currentCategories = new Dictionary<string,int>();

	protected void Init()
	{
		if(plusContent == null)
			plusContent = new GUIContent((Texture2D)Resources.Load("OlPlusGreen"), "New keyword");

		if(minusContent == null)
			minusContent = new GUIContent((Texture2D)Resources.Load("OLMinusRed"), "Delete Keyword");

		if(checkContent == null)
			checkContent = new GUIContent((Texture2D)Resources.Load("OlCheckGreen"), "Confirm");

		if(xContent == null)
			xContent = new GUIContent((Texture2D)Resources.Load("OLXRed"), "Cancel");

		PopulateLists();
	}

	protected void PopulateLists()
	{
		categories.Clear();
		keywords.Clear();

		foreach (KeywordCategory category in config)
		{
			if (!string.IsNullOrEmpty(category.name))
				categories.Add(category.name);

			foreach (string keyword in category.keywords)
			{
				if (!string.IsNullOrEmpty(keyword))
					keywords.Add(category.name + (string.IsNullOrEmpty(category.name) ? "" : "/") + keyword);
			}
		}

		categories.Sort();
		categories.Insert(0, "Uncategorized");

		keywords.Sort();
		keywords.Insert(0, "Empty String");
	}

	public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
	{
		return base.GetPropertyHeight(prop, label) * (isAddingString.Contains(prop.propertyPath) ? 2 : 1);
	}

	string isAddingString = string.Empty;

	public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
	{
		string keyword = prop.FindPropertyRelative("_key").stringValue;

		EditorGUI.BeginProperty(position, label, prop);
		
		position = EditorGUI.PrefixLabel(position, EditorGUIUtility.GetControlID(FocusType.Passive), label);
		
		int originalIndentLevel = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
		
		position.width -= 32;
		
		string temp;

		if(isAddingString.Contains(prop.propertyPath))
		{
			position.height = base.GetPropertyHeight(prop, label);

			currentCategories[prop.propertyPath] = EditorGUI.Popup(position, currentCategories[prop.propertyPath], categories.ToArray());

			position.y += position.height;

			EditorGUI.BeginChangeCheck();
			
			temp = EditorGUI.TextField(position, newValues[prop.propertyPath]);
			
			if(EditorGUI.EndChangeCheck())
			{
				newValues[prop.propertyPath] = temp;
			}
		}
		else
		{
			temp =  keyword;

			if(temp == "")
				temp = "Empty String";

			Color originalColor = GUI.color;
			int index = keywords.IndexOf(temp);
			
			if(index < 0)
			{
				index = keywords.Count;
				keywords.Add(temp + " (Missing)");
				GUI.color = Color.red;
			}
			
			
			EditorGUI.BeginChangeCheck();
			

			index = EditorGUI.Popup(position, index, keywords.ToArray());

			temp = keywords[index];

			if(temp == "Empty String")
				temp = "";

			GUI.color = originalColor;
			
			if(EditorGUI.EndChangeCheck())
			{
				prop.FindPropertyRelative("_key").stringValue = temp;
			}
		}

        position.y += 1;
		position.x += position.width;
		position.width = 16;

		if(GUI.Button(position, isAddingString.Contains(prop.propertyPath) ? checkContent : plusContent, "Label"))
		{
			if (temp.Contains(" (Missing)"))
			{
				KeywordCategory tempCategory = (from c in config where c.name == (keyword.LastIndexOf('/') < 0 ? "" : keyword.Substring(0, keyword.LastIndexOf('/'))) select c).FirstOrDefault();

				if(tempCategory == null)
				{
					config.Add(new KeywordCategory((keyword.LastIndexOf('/') < 0 ? "" : keyword.Substring(0, keyword.LastIndexOf('/')))));
					config.Last().keywords.Add(keyword);
				}
				else
				{
					tempCategory.keywords.Add(keyword);
				}
				EditorUtility.SetDirty(scriptableConfig);
				PopulateLists();
			}
			else
			{
				if(isAddingString.Contains(prop.propertyPath))
				{
					config[currentCategories[prop.propertyPath]].keywords.Add(newValues[prop.propertyPath]);
					EditorUtility.SetDirty(scriptableConfig);

					keywords.Add(config[currentCategories[prop.propertyPath]].name + (currentCategories[prop.propertyPath] == 0 ? "" : "/") + newValues[prop.propertyPath]);

					config[currentCategories[prop.propertyPath]].keywords.Sort();
					keywords.RemoveAt(0);
					keywords.Sort();
					keywords.Insert(0, "Empty String");

					SetReflectedFieldRecursively(prop, (Keyword)(config[currentCategories[prop.propertyPath]].name + (currentCategories[prop.propertyPath] == 0 ? "" : "/") + newValues[prop.propertyPath]));

					EditorUtility.SetDirty(prop.serializedObject.targetObject);
				}

				if(isAddingString.Contains(prop.propertyPath))
					isAddingString = isAddingString.Replace(prop.propertyPath, string.Empty);
				else
				{
					isAddingString += prop.propertyPath;
					if(!newValues.ContainsKey(prop.propertyPath))
						newValues.Add(prop.propertyPath, string.Empty);

					if(!currentCategories.ContainsKey(prop.propertyPath))
						currentCategories.Add(prop.propertyPath, 0);
				}
			}
		}
		
		position.x += 16;

		if(GUI.Button(position, isAddingString.Contains(prop.propertyPath) ? xContent : minusContent, "Label"))
		{
			if(isAddingString.Contains(prop.propertyPath))
			{
				newValues[prop.propertyPath] = "";
				isAddingString = isAddingString.Replace(prop.propertyPath, string.Empty);
			}
			else
			{
				if(EditorUtility.DisplayDialog("Remove string?", string.Format("Are you sure you want to remove \"{0}\" from the string list?", temp), "Yes", "No"))
				{
					keywords.Remove(temp);

					if(temp.Contains('/'))
						(from c in config where c.name == temp.Substring(0, temp.LastIndexOf('/')) select c.keywords).ToList().FirstOrDefault().Remove(keyword);
					else
						config[0].keywords.Remove(keyword);

					EditorUtility.SetDirty(Keywords.Config);
				}
			}
		}

		EditorGUI.indentLevel = originalIndentLevel;
		
		EditorGUI.EndProperty();
	}
}

[CustomPropertyDrawer(typeof(Keyword))]
public class KeywordDrawer : BaseKeywordDrawer
{

	public KeywordDrawer()
	{
		scriptableConfig = Keywords.Config;
		config = scriptableConfig.keyWordCategories;

		base.Init();
	}
}

}
