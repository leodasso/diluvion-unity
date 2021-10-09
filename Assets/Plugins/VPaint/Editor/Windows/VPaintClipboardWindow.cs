using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Valkyrie.VPaint;
using System;

public class VPaintClipboardWindow : VPaintWindowBase 
{
	public class ClipboardItem
	{
		public string name;
		public Color[] colors;
		public float[] transparency;
	}
	
	public static List<ClipboardItem> clipboard = new List<ClipboardItem>();
	public static void AddToClipboard (Color[] colors, float[] transparency, string name)
	{
		clipboard.Add(new ClipboardItem(){colors = colors, transparency = transparency, name = name});
	}
	
	public override bool LockSelection ()
	{
		return true;
	}
	
	public override void OnValidatedGUI ()
	{
		float width = position.width-24;
		
		VPaintGUIUtility.BeginColumnView(width);
		
		VPaintGUIUtility.DrawColumnRow(24,()=>{
			GUILayout.FlexibleSpace();
			GUILayout.Label("VPaint Clipboard");
			GUILayout.FlexibleSpace();
		});
		
		SettingsPanel();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(8);
		
		EditorGUILayout.BeginVertical(GUILayout.Width(width * 0.4f));
		
		Rect upperLeft = EditorGUILayout.BeginVertical();
		GUI.Box(upperLeft, GUIContent.none);
		
		UpperLeftPanel();
		
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndVertical();
		
		
		Rect lowerLeft = EditorGUILayout.BeginVertical();
		GUI.Box(lowerLeft, GUIContent.none);
		
		EditorGUILayout.BeginHorizontal(GUILayout.Height(72));
		EditorGUILayout.BeginVertical();
		LowerLeftPanel();
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.EndVertical();
		
		EditorGUILayout.EndVertical();
		
		
		
		EditorGUILayout.BeginVertical(GUILayout.Width(width * 0.2f));
		GUILayout.FlexibleSpace();
		
		CenterPanel();
		
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndVertical();
		
		
		
		EditorGUILayout.BeginVertical(GUILayout.Width(width * 0.4f));
		
		Rect upperRight = EditorGUILayout.BeginVertical();
		GUI.Box(upperRight, GUIContent.none);
		
		UpperRightPanel();
		
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndVertical();
		
		
		Rect lowerRight = EditorGUILayout.BeginVertical();
		GUI.Box(lowerRight, GUIContent.none);
		
		EditorGUILayout.BeginHorizontal(GUILayout.Height(72));
		EditorGUILayout.BeginVertical();
		LowerRightPanel();
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.EndVertical();
		
		EditorGUILayout.EndVertical();
		
		
		
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space(5);
	}
	
	[NonSerialized] int copyLayer = 0;
	[NonSerialized] int pasteLayer = 0;
	void SettingsPanel ()
	{
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			List<string> names = new List<string>();
			names.Add("Current Layer");		//0
			names.Add("Merged Layer");		//1
			
			copyLayer = EditorGUILayout.Popup("Copy Layer: ", copyLayer, names.ToArray());
		});
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			List<string> names = new List<string>();
			names.Add("Current Layer");		//0	
			names.Add("New Layer");			//1
			
			pasteLayer = EditorGUILayout.Popup("Paste To Layer: ", pasteLayer, names.ToArray());
		});
	}
	
	[NonSerialized] Vector2 clipboardScroll;
	[NonSerialized] int selectedClipboardItem = -1;
	void UpperLeftPanel ()
	{
		Rect top = EditorGUILayout.BeginVertical();
		GUILayout.Space(3);
		GUILayout.Label("Clipboard");
		GUILayout.Space(3);
		EditorGUILayout.EndVertical();
		top.y += top.height;
		top.height = 2;
		GUI.Box(top, GUIContent.none);
		
		clipboardScroll = EditorGUILayout.BeginScrollView(clipboardScroll);
		
		Rect all = EditorGUILayout.BeginVertical();
		
		for(int i = 0; i < clipboard.Count; i++)
		{
			Rect r = EditorGUILayout.BeginHorizontal();
			if(selectedClipboardItem == i) GUI.Box(r, GUIContent.none);
			if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && r.Contains(Event.current.mousePosition))
			{
				selectedClipboardItem = i;
				Event.current.Use();
			}
			GUILayout.Label(clipboard[i].name);
			EditorGUILayout.EndHorizontal();
		}
		
		EditorGUILayout.EndVertical();
		
		EditorGUILayout.EndScrollView();
		
		if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && all.Contains(Event.current.mousePosition))
		{
			selectedClipboardItem = -1;
			Event.current.Use();
		}
	}
	
	void LowerLeftPanel ()
	{
		if(selectedClipboardItem == -1)
		{
			return;
		}
		
		var clipboardItem = clipboard[selectedClipboardItem];
		
		clipboardItem.name = EditorGUILayout.TextField(clipboardItem.name);
		GUILayout.Label("Buffer: " + clipboardItem.colors.Length);
		
		if(GUILayout.Button("Remove"))
		{
			clipboard.RemoveAt(selectedClipboardItem);
			selectedClipboardItem = -1;
		}
	}
	
	void CenterPanel ()
	{
		GUI.enabled = selectedObjects.Count == 1;
		if(GUILayout.Button("< Copy"))
		{
			VPaintLayer layer = null;
			if(copyLayer == 0)
			{
				layer = currentLayer;
			}
			if(copyLayer == 1)
			{
				layer = currentLayerStack.GetMergedLayer();
			}
			int selectedObj = -1;
			foreach(var so in selectedObjects) selectedObj = so;
			var obj = VPaint.Instance.objectInfo[selectedObj].vpaintObject;
			var pd = layer.Get(obj);
			if(pd == null) Debug.LogError("Could not copy VPaint object, it has no paint data!");
			else {
				AddToClipboard(pd.colors, pd.transparency, obj.name);
			}
		}
		GUI.enabled = true;
		
		GUILayout.Space(20);
		
		GUI.enabled = selectedObjects.Count != 0 && selectedClipboardItem != -1;
		
		if(GUI.enabled)
		{
			var clipData = clipboard[selectedClipboardItem];
			foreach(var so in selectedObjects)
			{
				var obj = VPaint.Instance.objectInfo[so].vpaintObject;
				GUI.enabled &= obj.GetMeshInstance().vertices.Length == clipData.colors.Length;
			}
		}
		
		if(GUILayout.Button("Paste >"))
		{
			VPaint.Instance.PushUndo("Paste Colors");
			var clipData = clipboard[selectedClipboardItem];
			
			if(pasteLayer == 0)
			{
				foreach(var so in selectedObjects)
				{
					var obj = VPaint.Instance.objectInfo[so].vpaintObject;
					
					var pd = currentLayer.GetOrCreate(obj);
					pd.colors = clipData.colors;
					pd.transparency = clipData.transparency;
				}
			}
			if(pasteLayer == 1)
			{
				var layer = new VPaintLayer();
				foreach(var so in selectedObjects)
				{
					var obj = VPaint.Instance.objectInfo[so].vpaintObject;
					layer.paintData.Add(
						new VPaintVertexData(){
							colorer = obj,
							colors = clipData.colors,
							transparency = clipData.transparency
						}
					);
				}
				currentLayerStack.layers.Add(layer);
			}
			VPaint.Instance.ReloadLayers();
		}
		GUI.enabled = true;
	}
	
	Vector2 objectsScroll;
	HashSet<int> selectedObjects = new HashSet<int>();
	void UpperRightPanel ()
	{			
		Rect top = EditorGUILayout.BeginVertical();
		GUILayout.Space(3);
		GUILayout.Label("Objects");
		GUILayout.Space(3);
		EditorGUILayout.EndVertical();
		top.y += top.height;
		top.height = 2;
		GUI.Box(top, GUIContent.none);
		
		objectsScroll = EditorGUILayout.BeginScrollView(objectsScroll);
		Rect all = EditorGUILayout.BeginVertical();
		
		for(int i = 0; i < VPaint.Instance.objectInfo.Length; i++)
		{
			var info = VPaint.Instance.objectInfo[i];
			
			Rect r = EditorGUILayout.BeginHorizontal();
			if(selectedObjects.Contains(i))
			{
				GUI.Box(r, GUIContent.none);
			}
			if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && r.Contains(Event.current.mousePosition))
			{
				EditorGUIUtility.PingObject(info.vpaintObject);
				if(Event.current.shift)
				{
					selectedObjects.Add(i);
				}
				else if(Event.current.control)
				{
					selectedObjects.Remove(i);
				}
				else
				{
					selectedObjects.Clear();
					selectedObjects.Add(i);
				}
				Event.current.Use();
			}
			
			GUILayout.Label(info.vpaintObject.name);
			EditorGUILayout.EndHorizontal();
		}
		
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndScrollView();
		
		if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && all.Contains(Event.current.mousePosition))
		{
			selectedObjects.Clear();
			Event.current.Use();
		}
	}
	
	void LowerRightPanel ()
	{
		if(selectedObjects.Count == 0)
		{
			return;
		}
		
		int vertCount = -1;
		string names = null;
		foreach(var so in selectedObjects)
		{
			var info = VPaint.Instance.objectInfo[so];
			
			if(names == null) names = info.vpaintObject.name;
			else names += ", " + info.vpaintObject.name;
			
			var v = info.vpaintObject.GetMeshInstance().vertices.Length;
			if(vertCount == -2) continue;
			else if(vertCount == -1)
			{
				vertCount = v;
			}
			else
			{
				if(vertCount != v)
				{
					vertCount = -2;
					return;
				}
			}
		}
		
		if(40 < names.Length) names = names.Substring(0, 37)+"...";
		GUILayout.Label(names);
		if(vertCount == -2)
		{
			GUILayout.Label("Vertex Count: [Various]");
		}
		else
		{
			GUILayout.Label("Vertex Count: " + vertCount);
		}
	}
}
