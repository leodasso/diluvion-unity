using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;

[InitializeOnLoad]
public static class VPaintHotkeys
{
	public static List<Hotkey> Hotkeys = new List<Hotkey>();	
	static Dictionary<string, Hotkey> HotkeysByPref = new Dictionary<string, Hotkey>();
	
	static VPaintHotkeys ()
	{
		Init();
	}
	static void Init ()
	{
		Action<Hotkey> Add = (hk)=>
		{
			Hotkeys.Add(hk);
			HotkeysByPref.Add(hk.editorPref, hk);
		};
		
		Add(new Hotkey(
			"VP_HK_CurrentLayerMoveDown",
			"Move Current Layer Down",
			"#UpArrow",
			VPaint.CurrentLayerMoveUp
		));
		
		Add(new Hotkey(
			"VP_HK_CurrentLayerMoveUp",
			"Move Current Layer Up",
			"#DownArrow", 
			VPaint.CurrentLayerMoveDown
		));
		
		Add(new Hotkey(
			"VP_HK_CurrentLayerDuplicate", 
			"Duplicate Current Layer",
			"#D", 
			VPaint.CurrentLayerDuplicate
		));
		
		Add(new Hotkey(
			"VP_HK_CurrentLayerRemove", 
			"Remove Current Layer",
			"&R", 
			VPaint.CurrentLayerRemove
		));
		
		Add(new Hotkey(
			"VP_HK_CurrentLayerMergeDown", 
			"Merge Current Layer Down",
			"#E", 
			VPaint.CurrentLayerMergeDown
		));
		
		Add(new Hotkey(
			"VP_HK_MergeAllLayers", 
			"Collapse All Layers",
			"#%E", 
			VPaint.AllLayersMerge
		));
		
		Add(new Hotkey(
			"VP_HK_NewLayer", 
			"Create New Layer",
			"#N", 
			VPaint.CreateNewLayer
		));
		
		Add(new Hotkey(
			"VP_HK_CreateMergedLayer", 
			"Create Merged Layer",
			"#M", 
			VPaint.CreateAllMergedLayer
		));
		
		Add(new Hotkey(
			"VP_HK_IncreaseRadius", 
			"Increase Brush Radius",
			"RightBracket", 
			VPaint.IncreaseRadius
		));
		
		Add(new Hotkey(
			"VP_HK_DecreaseRadius", 
			"Decrease Brush Radius",
			"LeftBracket", 
			VPaint.DecreaseRadius
		));
		
		Add(new Hotkey(
			"VP_HK_IncreasePower", 
			"Increase Brush Power",
			"Equals", 
			VPaint.IncreasePower
		));
		
		Add(new Hotkey(
			"VP_HK_DecreasePower", 
			"Decrease Brush Power",
			"Plus", 
			VPaint.DecreasePower
		));
		
		Add(new Hotkey(
			"VP_HK_ToggleColorPreview", 
			"Toggle Vertex Color Preview",
			"Tab", 
			VPaint.ToggleVertexColorPreviewMode
		));
	}
	
	public static void ResetAll ()
	{
		foreach(var hk in Hotkeys)
		{
			EditorPrefs.DeleteKey(hk.editorPref);
		}
		Hotkeys.Clear();
		Init();
	}
	
	public static void Evaluate (Event e)
	{
		foreach(var hotkey in Hotkeys)
		{
			if(hotkey.TestEvent(e))
			{
				e.Use();
				hotkey.action();
			}
		}
	}
	
	public static string GetLabel (string hotkey)
	{
		if(HotkeysByPref.ContainsKey(hotkey))
		{
			return HotkeysByPref[hotkey].GetLabel();
		}
		return "";
	}
	
	public class Hotkey
	{
		public enum Modifier
		{
			Control,
			Shift,
			Alt
		}
		public List<Modifier> modifiers = new List<Modifier>();
		public KeyCode key;
		
		public bool isValid = true;
		public Action action;
	
		public string defaultValue;
		public string editorPref;
		public string label;
		public string value;
		
		public Hotkey (string editorPref, string label, string defaultValue, Action action)
		{
			this.action = action;
			this.editorPref = editorPref;
			this.label = label;
			this.defaultValue = defaultValue;
			Parse(EditorPrefs.GetString(editorPref, defaultValue));
		}
		
		public void Parse (string hotkey)
		{
			this.value = hotkey;
			isValid = false;
			modifiers.Clear();
			for(int i = 0; i < hotkey.Length; i++)
			{
				char c = hotkey[i];
				if(c == '%')
				{
					modifiers.Add(Modifier.Control);
				}
				else if(c == '#')
				{
					modifiers.Add(Modifier.Shift);
				}
				else if(c == '&')
				{
					modifiers.Add(Modifier.Alt);
				}
				else
				{
					try
					{
						key = (KeyCode)Enum.Parse(typeof(KeyCode), hotkey.Substring(i, hotkey.Length-i));
						isValid = true;
						break;
					}
					catch
					{
						isValid = false;
						break;
					}
				}
			}
		}
	
		public string GetLabel ()
		{
			if(!isValid) return "";
			string s = "";
			foreach(var m in modifiers)
			{
				s += GetName(m) + "+";
			}
			s += Enum.GetName(typeof(KeyCode), key);
			return s;
		}
		
		public string GetName (Modifier m)
		{
			switch(m)
			{
				case Modifier.Control:
					return "CTRL";
				case Modifier.Alt:
					return "ALT";
				case Modifier.Shift:
					return "SHIFT";
			}
			return "ERROR";
		}
		
		public bool TestEvent (Event e)
		{
			if(!isValid)
			{
				return false;
			}
			
			if(e.type != EventType.KeyDown)
			{
				return false;
			}
			
			foreach(var m in modifiers)
			{
				switch(m)
				{
					case Modifier.Alt:
						if(!e.alt) return false;
						break;
					case Modifier.Control:
						if(!e.control && !e.command) return false;
						break;
					case Modifier.Shift:
						if(!e.shift) return false;
						break;
				}
			}
			
			if(key != e.keyCode)
			{
				return false;
			}
			
			return true;
		}
	}
}