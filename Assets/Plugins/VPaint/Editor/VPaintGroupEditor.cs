using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Valkyrie.VPaint;

[CustomEditor(typeof(VPaintGroup))]
public class VPaintGroupEditor : Editor 
{
	public override void OnInspectorGUI ()
	{
		var vpg  = target as VPaintGroup;
		
#if UNITY_3_5
		EditorGUIUtility.LookLikeInspector();
#endif
		
		EditorGUILayout.BeginHorizontal();
		
		GUILayout.FlexibleSpace();
		
		if(GUILayout.Button("Open VPaint"))
		{
			VPaint.OpenEditor();
		}
		
		EditorGUILayout.EndHorizontal();
		
		EditorGUI.BeginChangeCheck();
		
		vpg.autoLoadInEditor = EditorGUILayout.Toggle(new GUIContent("Auto Load In Editor"), vpg.autoLoadInEditor);
		vpg.autoApplySchedule = (AutoApplySchedule)EditorGUILayout.EnumPopup(new GUIContent("Auto Apply Schedule"), vpg.autoApplySchedule);
		
		if(EditorGUI.EndChangeCheck())
		{
			EditorUtility.SetDirty(target);
		}
		
		GUILayout.Space(5);
		
		if(VPaint.Instance && VPaint.Instance.layerCache == target)
		{			
			if(0 < VPaint.Instance.errorCount)
			{
				GUILayout.Space(10);
				
				Rect r = EditorGUILayout.BeginVertical();
				
				GUI.Box(r, GUIContent.none);
				
				GUILayout.Space(4);
				
				var style = new GUIStyle(GUI.skin.label);
				
				if(!EditorGUIUtility.isProSkin) style.normal.textColor = Color.black;
				style.wordWrap = true;			
				style.fontSize = 12;
				
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(4);
				GUILayout.Label("This VPaint Group contains errors!", style);
				if(GUILayout.Button("Object Maintenance", GUILayout.Height(18)))
				{
					EditorWindow.GetWindow<VPaintGroupMaintenance>(true);
				}
				GUILayout.Space(4);
				EditorGUILayout.EndHorizontal();
				
				GUILayout.Space(8);
				
				EditorGUILayout.EndVertical();
			}
		}
	}
}
