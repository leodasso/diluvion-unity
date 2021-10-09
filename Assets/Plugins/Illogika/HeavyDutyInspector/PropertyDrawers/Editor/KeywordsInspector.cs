//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	[CustomEditor(typeof(KeywordsConfig))]
	public class KeywordsInspector : Editor
	{

		Texture2D olPlus;
		Texture2D olMinus;

		bool hasChanged = false;

		List<bool> editingCategoryName = new List<bool>();

		List<DuplicateKeyword> duplicateKeywords = new List<DuplicateKeyword>();

		public override void OnInspectorGUI()
		{
			if (olPlus == null)
			{
				olPlus = (Texture2D)Resources.Load("OlPlusGreen");
			}

			if (olMinus == null)
			{
				olMinus = (Texture2D)Resources.Load("OLMinusRed");
			}

			KeywordsConfig config = (KeywordsConfig)target;

			if (config.keyWordCategories.Count == 0)
			{
				config.keyWordCategories.Add(new KeywordCategory());
			}

			if ((from c in config.keyWordCategories where c.name == "" select c).FirstOrDefault() == null)
			{
				config.keyWordCategories.Insert(0, new KeywordCategory());
			}

			while (editingCategoryName.Count < config.keyWordCategories.Count)
			{
				editingCategoryName.Add(false);
			}

			EditorGUILayout.BeginVertical();

			GUI.backgroundColor = Color.blue;
			GUILayout.Label("", GUILayout.Height(22));
			Rect temp = GUILayoutUtility.GetLastRect();
			Rect backgroundRect = temp;
			backgroundRect.x = 0;
			backgroundRect.width = Screen.width;
			EditorGUI.HelpBox(backgroundRect, "", MessageType.None);
			temp.y += 3;
			GUI.Label(temp, "Keyword Categories");
			GUI.backgroundColor = Color.white;

			for (int i = 0; i < config.keyWordCategories.Count; ++i)
			{
				EditorGUILayout.BeginHorizontal();

				config.keyWordCategories[i].expanded = EditorGUILayout.Foldout(config.keyWordCategories[i].expanded, string.IsNullOrEmpty(config.keyWordCategories[i].name) ? "None" : config.keyWordCategories[i].name);

				if (i > 0)
				{
					if (editingCategoryName[i])
					{
						config.keyWordCategories[i].name = EditorGUILayout.TextField(config.keyWordCategories[i].name);

						if (GUILayout.Button(olPlus, "Label", GUILayout.Width(16)))
						{
							editingCategoryName[i] = false;

							config.keyWordCategories.Sort((x, y) => Comparer.Default.Compare(x.name, y.name));
						}
					}
					else
					{
						editingCategoryName[i] = GUILayout.Button("Rename", "minibutton");

						if (GUILayout.Button(olMinus, "Label", GUILayout.Width(16)))
						{
							if (EditorUtility.DisplayDialog("Delete Category", "Are you sure you want to delete this category and every keyword in it", "Delete", "Cancel"))
							{
								config.keyWordCategories.RemoveAt(i);
								--i;
								hasChanged = true;
							}
						}
					}
				}

				EditorGUILayout.EndHorizontal();

				if (config.keyWordCategories[i].expanded)
				{
					EditorGUILayout.BeginVertical();

					for (int j = 0; j < config.keyWordCategories[i].keywords.Count; ++j)
					{
						EditorGUILayout.BeginHorizontal();

						GUILayout.Label("", GUILayout.Width(0));

						config.keyWordCategories[i].keywords[j] = EditorGUILayout.TextField(config.keyWordCategories[i].keywords[j]);

						if (GUILayout.Button(olMinus, "Label", GUILayout.Width(16)))
						{
							config.keyWordCategories[i].keywords.RemoveAt(j);
							--j;
							hasChanged = true;
						}

						EditorGUILayout.EndHorizontal();
					}

					EditorGUILayout.BeginHorizontal();

					GUILayout.FlexibleSpace();

					EditorGUILayout.LabelField("New Keyword", GUILayout.Width(82));

					if (GUILayout.Button(olPlus, "Label", GUILayout.Width(16)))
					{
						config.keyWordCategories[i].keywords.Add("");
						hasChanged = true;
					}

					EditorGUILayout.EndHorizontal();

					EditorGUILayout.EndVertical();
				}
			}

			EditorGUILayout.BeginHorizontal();

			GUILayout.FlexibleSpace();

			EditorGUILayout.LabelField("New Category", GUILayout.Width(85));

			if (GUILayout.Button(olPlus, "Label", GUILayout.Width(16)))
			{
				config.keyWordCategories.Add(new KeywordCategory("New Category"));
				hasChanged = true;
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			if (duplicateKeywords.Count > 0)
			{
				EditorGUILayout.HelpBox(string.Format("You have {0} keyword{1} that {2} duplicate across two or more categories! \nPlease select in which category you want to keep the duplicate keyword{1}", duplicateKeywords.Count, (duplicateKeywords.Count > 1 ? "s" : ""), (duplicateKeywords.Count > 1 ? "are" : "is")), MessageType.Error);

				Rect tempRect = GUILayoutUtility.GetLastRect();
				tempRect.y += 55;

				for (int i = 0; i < duplicateKeywords.Count; ++i)
				{
					tempRect.height = 22 * (duplicateKeywords[i].duplicateCategories.Count + 1) + 10;

					GUI.Box(tempRect, "", "GroupBox");

					tempRect.x += 5;
					tempRect.width -= 10; ;
					tempRect.height = 18;
					tempRect.y += 6;
					GUI.Label(tempRect, "Keyword : " + duplicateKeywords[i].keyword);
					for (int j = 0; j < duplicateKeywords[i].duplicateCategories.Count; ++j)
					{
						tempRect.y += 22;
						if (GUI.Button(tempRect, "Keep in \"" + (string.IsNullOrEmpty(duplicateKeywords[i].duplicateCategories[j].name) ? "None" : duplicateKeywords[i].duplicateCategories[j].name) + "\""))
						{
							duplicateKeywords[i].duplicateCategories.RemoveAt(j);

							foreach (KeywordCategory category in duplicateKeywords[i].duplicateCategories)
							{
								category.keywords.Remove(duplicateKeywords[i].keyword);
							}
							duplicateKeywords.RemoveAt(i);
							--i;

							hasChanged = true;
							break;
						}
					}

					tempRect.y += 31;
					tempRect.x -= 5;
					tempRect.width += 10;
				}
			}
			else
			{
				if (GUILayout.Button("Sort"))
				{
					foreach (KeywordCategory category in config.keyWordCategories)
					{
						category.keywords.Sort();
						hasChanged = true;
					}
				}

				if (GUILayout.Button("Enforce Keyword Unicity in Categories"))
				{
					foreach (KeywordCategory category in config.keyWordCategories)
					{
						category.keywords.Sort();
						for (int i = 0; (i + 1) < category.keywords.Count; ++i)
						{
							if (category.keywords[i] == category.keywords[i + 1])
							{
								category.keywords.RemoveAt(i + 1);
								--i;
								hasChanged = true;
							}
						}
					}
				}

				if (GUILayout.Button("Enforce Keyword Unicity Across All Categories"))
				{
					foreach (KeywordCategory category in config.keyWordCategories)
					{
						category.keywords.Sort();
						for (int i = 0; (i + 1) < category.keywords.Count; ++i)
						{
							if (category.keywords[i] == category.keywords[i + 1])
							{
								category.keywords.RemoveAt(i + 1);
								--i;
								hasChanged = true;
							}
						}

						foreach (string keyword in category.keywords)
						{
							foreach (KeywordCategory otherCategory in config.keyWordCategories)
							{
								if (category != otherCategory)
								{
									if (otherCategory.keywords.Contains(keyword))
									{
										DuplicateKeyword dk = (from k in duplicateKeywords where k.keyword == keyword select k).FirstOrDefault();
										if (dk == null)
										{
											dk = new DuplicateKeyword();
											dk.keyword = keyword;
											duplicateKeywords.Add(dk);
										}

										if (!dk.duplicateCategories.Contains(category))
										{
											dk.duplicateCategories.Add(category);
										}

										if (!dk.duplicateCategories.Contains(otherCategory))
										{
											dk.duplicateCategories.Add(otherCategory);
										}
									}
								}
							}
						}
					}
				}

				EditorGUILayout.HelpBox("Drag a tabulated text file into the field below to automatically fill the Keywords editor with the content of a spreadheet. See the readme for information about the supported format.", MessageType.Info);

				TextAsset spreadsheetText = (EditorGUILayout.ObjectField("Keywords Spreadsheet", null, typeof(TextAsset), false) as TextAsset);

				if (spreadsheetText != null)
				{
					Spreadsheet spreadsheet = new Spreadsheet();
					spreadsheet.Load(spreadsheetText.text);

					config.keyWordCategories.Clear();

					for (int i = 0; i < spreadsheet.GetNbColumns(); ++i)
					{
						KeywordCategory category = (from c in config.keyWordCategories where c.name == spreadsheet.GetVariableName(i) select c).FirstOrDefault();

						if (category == null)
						{
							category = new KeywordCategory(spreadsheet.GetVariableName(i));
							config.keyWordCategories.Add(category);
						}
						else
						{
							Debug.LogWarning("Category named " + spreadsheet.GetVariableName(i) + " is duplicated in your spreadsheet");
						}

						for (int j = 0; j < spreadsheet.GetNbRows(); ++j)
						{
							if (category.keywords.Contains(spreadsheet.GetValue(j, i)))
							{
								Debug.LogWarning("Keyword " + spreadsheet.GetValue(j, i) + " is duplicate in category " + spreadsheet.GetVariableName(i) + ". Skipping.");
							}
							else if (!string.IsNullOrEmpty(spreadsheet.GetValue(j, i)))
							{
								category.keywords.Add(spreadsheet.GetValue(j, i));
							}
						}
					}

					if ((from c in config.keyWordCategories where string.IsNullOrEmpty(c.name) select c).FirstOrDefault() == null)
					{
						Debug.LogWarning("Spreadsheet did not contain a default empty category. Adding it.");

						config.keyWordCategories.Add(new KeywordCategory(""));
					}

					config.keyWordCategories.Sort((x, y) => Comparer.Default.Compare(x.name, y.name));
					hasChanged = true;
				}
			}

			if (hasChanged)
			{
				hasChanged = false;
				EditorUtility.SetDirty(target);
			}
		}
	}

	public class DuplicateKeyword
	{

		public string keyword;

		public List<KeywordCategory> duplicateCategories = new List<KeywordCategory>();
	}

}
