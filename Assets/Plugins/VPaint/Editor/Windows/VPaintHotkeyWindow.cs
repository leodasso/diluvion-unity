using UnityEngine;
using UnityEditor;
using Valkyrie.VPaint;

public class VPaintHotkeyWindow : VPaintWindowBase 
{
	
	public override void OnValidatedEnable ()
	{
		base.OnValidatedEnable ();
		title = "VPaint Hotkey Editor";
	}
	
	Vector2 scrollPosition;
	
	public override void OnValidatedGUI ()
	{
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		
		VPaintGUIUtility.BeginColumnView(position.width-24);
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>
		{
			GUILayout.Label("VPaint Hotkey Editor");
		});
		
		var wordWrapStyle = new GUIStyle(GUI.skin.label);
		wordWrapStyle.wordWrap = true;
		VPaintGUIUtility.DrawColumnRow(24, ()=>
		{
			GUILayout.Label("Use this window to modify hotkeys in VPaint. Although it cannot be guarenteed that every hotkey combination will work (the Unity Editor overrides many of them for its own purposes) this should help avoid conflicts with other tools.", wordWrapStyle);
		});
		VPaintGUIUtility.DrawColumnRow(24, ()=>
		{
			EditorGUILayout.BeginVertical();
			GUILayout.Label("To disable a hotkey, simply clear the text. Hotkeys are parsed with Unity KeyCode - see the Unity documentation for all avialable options. Use the below notation to indicate modifier keys for your hotkey.", wordWrapStyle);
			EditorGUILayout.LabelField("\tShift:", "#");
			EditorGUILayout.LabelField("\tAlt:", "&");
			EditorGUILayout.LabelField("\tCommand:", "%");
			EditorGUILayout.EndVertical();
		});
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>
		{
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Reset All"))
			{
				foreach(var hotkey in VPaintHotkeys.Hotkeys)
				{
					hotkey.Parse(hotkey.defaultValue);
					EditorPrefs.DeleteKey(hotkey.editorPref);
				}
			}
		});
		
		foreach(var hotkey in VPaintHotkeys.Hotkeys)
		{
			VPaintGUIUtility.DrawColumnRow(24, ()=>{
				EditorGUILayout.BeginVertical();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label(hotkey.label);
				GUILayout.FlexibleSpace();
				if(hotkey.isValid)
				{
					GUILayout.Label(hotkey.GetLabel());
				}
				else
				{
					GUILayout.Label("[INVALID]");
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				string newHotkey = EditorGUILayout.TextField("", hotkey.value);
				if(newHotkey != hotkey.value)
				{
					hotkey.Parse(newHotkey);
					EditorPrefs.SetString(hotkey.editorPref, newHotkey);
				}
				GUI.enabled = newHotkey != hotkey.defaultValue;
				if(GUILayout.Button("Reset To Default", GUILayout.Width(140)))
				{
					hotkey.Parse(hotkey.defaultValue);
					EditorPrefs.DeleteKey(hotkey.editorPref);
				}
				GUI.enabled = true;
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
			});
		}
		
		EditorGUILayout.EndScrollView();
	}
	
}
