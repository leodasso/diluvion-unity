//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2013 - 2015  Illogika
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections;
using HeavyDutyInspector;

namespace HeavyDutyInspector
{

	[CustomPropertyDrawer(typeof(ImageAttribute))]
	public class ImageDrawer : DecoratorDrawer
	{
		private Texture image;
		private GUIStyle imageStyle;
		
		
		ImageAttribute imageAttribute { get { return ((ImageAttribute)attribute); } }

		private float imageHeight
		{
			get{
				if(image == null)
				{
					image = (Texture)AssetDatabase.LoadAssetAtPath("Assets/" + imageAttribute.imagePath, typeof(Texture));
					imageStyle = new GUIStyle();
					imageStyle.normal.background = (Texture2D)image;
				}
				
				return image != null ? image.height : 0.0f;
			}
		}

		public override float GetHeight ()
		{
			return image != null ? imageHeight + 10.0f : 0.0f;
		}
		
		public override void OnGUI (Rect position)
		{
			int baseIndentLevel = EditorGUI.indentLevel;

			EditorGUI.indentLevel = 0;
			position = EditorGUI.IndentedRect(position);

			position.y += 5;
			position.height = imageHeight;

			if(image == null)
				return;

			switch(imageAttribute.alignment)
			{
			case Alignment.Left:
				// Left. Do nothing.
				break;
			case Alignment.Center:
				position.x = position.x + (position.width - image.width) / 2;
				break;
			case Alignment.Right:
				position.x = position.x + position.width - image.width;
				break;
			default:
				break;
			}

			position.width = image.width;
	    	EditorGUI.LabelField(position, GUIContent.none, imageStyle);

			EditorGUI.indentLevel = baseIndentLevel;
		}
	}

}
