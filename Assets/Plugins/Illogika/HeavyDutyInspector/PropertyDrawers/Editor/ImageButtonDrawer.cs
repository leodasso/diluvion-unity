//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	[CustomPropertyDrawer(typeof(ImageButtonAttribute))]
	public class ImageButtonDrawer : IllogikaDrawer {

		private Texture image;
		private GUIStyle imageStyle;

		ImageButtonAttribute imageButtonAttribute { get { return ((ImageButtonAttribute)attribute); } }

		bool ShowVariable(SerializedProperty prop)
		{
			bool showVariable = !imageButtonAttribute.hideVariable;
			
			return showVariable;
		}

		private float imageHeight
		{
			get{
				if(image == null)
				{
					image = (Texture)AssetDatabase.LoadAssetAtPath("Assets/" + imageButtonAttribute.imagePath, typeof(Texture));
					imageStyle = new GUIStyle();
					imageStyle.normal.background = (Texture2D)image;
				}
				
				return image != null ? image.height : 0.0f;
			}
		}

		public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
		{
			float baseHeight = base.GetPropertyHeight(prop, label);
			return ShowVariable(prop) ? baseHeight + imageHeight: imageHeight;
		}
		
		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, prop);
			
			bool showVariable = ShowVariable(prop);

			Rect indentPosition = EditorGUI.IndentedRect(position);
			indentPosition.height = image.height;

			if(GUI.Button(indentPosition, ""))
			{
				foreach(Object obj in prop.serializedObject.targetObjects)
				{
					MonoBehaviour go = obj as MonoBehaviour;
					if (go != null)
					{
						CallMethod(prop, go, imageButtonAttribute.buttonFunction);
					}
					else
					{
						ScriptableObject so = obj as ScriptableObject;
						if(so != null)
						{
							CallMethod(prop, so, imageButtonAttribute.buttonFunction);
						}
					}
				}
			}

			indentPosition.x = indentPosition.x + ( indentPosition.width / 2 - image.width / 2);
			indentPosition.width = image.width;

			indentPosition.y = indentPosition.y + ( indentPosition.height / 2 - image.height / 2);
			indentPosition.height = image.height;

			EditorGUI.LabelField(indentPosition, GUIContent.none, imageStyle);

			if (showVariable)
				position.y += indentPosition.height;
			
			if(showVariable)
			{
				EditorGUI.PropertyField(position, prop);	
			}
			
			EditorGUI.EndProperty();
		}
	}

}
