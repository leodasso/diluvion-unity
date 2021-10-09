using UnityEngine;
using UnityEditor;

namespace Valkyrie.VPaint
{
/*
	[InitializeOnLoad]
	public class VPaintInitialization
	{
		static VPaintInitialization ()
		{
			EditorApplication.update += CheckSceneChange;
		}
		
		static string currentScene;
		static void CheckSceneChange ()
		{
			if(EditorApplication.currentScene != currentScene)
			{
				currentScene = EditorApplication.currentScene;
				ReloadGroups();
			}
		}
		
		static void ReloadGroups ()
		{
			var groups = GameObject.FindObjectsOfType(typeof(VPaintGroup));
			foreach(var g in groups)
			{
				var pg = g as VPaintGroup;
				if(!pg.autoLoadInEditor) continue;
				pg.Apply();
			}
		}
	}
*/// Deprecated
}
