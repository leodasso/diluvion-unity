using UnityEngine;
using UnityEditor;
using System;

namespace Valkyrie.VPaint
{
	[Serializable]
	public class VPaintInspectorGroup 
	{
		public bool foldout = true;
		public Action title;
		public Action<Rect> method;
		public float width = 0;
		public float leftMargin = 6;
		
		public void OnGUI ()
		{
			Rect rect = EditorGUILayout.BeginVertical();
			VPaintGUIUtility.BoxArea(4, 4, 1, ()=>{
				EditorGUILayout.BeginVertical(GUILayout.Width(width));
				
				GUILayout.Space(5);
				
				EditorGUILayout.BeginHorizontal();
				
				GUILayout.Space(3);
				
				if(GUILayout.Button(foldout ? "-" : "+", GUILayout.Width(23), GUILayout.Height(12)))
				{
					foldout = !foldout;
				}
				GUILayout.Space(4);
				title();
				
				EditorGUILayout.EndHorizontal();
				
				GUILayout.Space(7);
				
				if(foldout)
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(leftMargin);
					EditorGUILayout.BeginVertical();
					method(rect);
					EditorGUILayout.EndVertical();
					EditorGUILayout.EndHorizontal();
				}
				
				EditorGUILayout.EndVertical();
			});
			EditorGUILayout.EndVertical();
		}
	}
}