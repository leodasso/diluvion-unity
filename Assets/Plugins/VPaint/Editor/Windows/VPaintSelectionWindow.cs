using UnityEngine;
using UnityEditor;
using Valkyrie.VPaint;
using System.Collections.Generic;
using System.Linq;

public class VPaintSelectionWindow : VPaintWindowBase
{	
	
	public bool[] currentEditingContentsMask = new bool[0];
	
	public override bool LockSelection ()
	{
		return true;
	}
	
	public override void OnValidatedEnable ()
	{
		base.OnValidatedEnable ();
		title = "VPaint Selection Mask";
		currentEditingContentsMask = new bool[VPaint.Instance.currentEditingContents.Length];
		SetAll(true);
	}
	
	public void SetAll (bool state)
	{
		for(int i = 0; i < currentEditingContentsMask.Length; i++)
		{
			currentEditingContentsMask[i] = state;
		}
	}
	
	Vector2 scrollPosition;
	public override void OnValidatedGUI ()
	{
		GUI.enabled = Selection.gameObjects.Length != 0;
		VPaintGUIUtility.BeginColumnView(position.width-24);
		VPaintGUIUtility.DrawColumnRow(24,
		()=>{
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Mask Selected"))
			{
				for(int i = 0; i < currentEditingContentsMask.Length; i++)
				{
					var vc = VPaint.Instance.currentEditingContents[i];
					currentEditingContentsMask[i] = Selection.gameObjects.Contains(vc.gameObject);
				}
			}
			if(GUILayout.Button("Mask Selected + Children"))
			{
				var vcs = new List<VPaintObject>();
				foreach(var go in Selection.gameObjects)
				{
					vcs.AddRange(go.GetComponentsInChildren<VPaintObject>());
				}
				for(int i = 0; i < currentEditingContentsMask.Length; i++)
				{
					currentEditingContentsMask[i] = vcs.Contains(VPaint.Instance.currentEditingContents[i]);
				}
			}
			GUILayout.FlexibleSpace();
		});
		VPaintGUIUtility.DrawColumnRow(24,
		()=>{
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Invert Selection"))
			{
				for(int i = 0; i < currentEditingContentsMask.Length; i++)
				{
					currentEditingContentsMask[i] = !currentEditingContentsMask[i];
				}
			}
			if(GUILayout.Button("Mask None"))
			{
				for(int i = 0; i < currentEditingContentsMask.Length; i++)
				{
					currentEditingContentsMask[i] = false;
				}
			}
			if(GUILayout.Button("Mask All"))
			{
				for(int i = 0; i < currentEditingContentsMask.Length; i++)
				{
					currentEditingContentsMask[i] = true;
				}
			}
			GUILayout.FlexibleSpace();
		});
		GUI.enabled = true;
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		
		EditorGUILayout.BeginVertical(GUILayout.Width(position.width-24));
		
		VPaintGUIUtility.BeginColumnView(position.width-32);
		
		bool allMasked = true;
		foreach(var b in currentEditingContentsMask) allMasked &= b;
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			bool masked = EditorGUILayout.Toggle(allMasked, GUILayout.Width(16));
			if(masked != allMasked)
			{
				SetAll(masked);
			}
			GUILayout.Label("Objects:");
		});
		
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		
		VPaintGUIUtility.columnViewBoxCount = 0;
		
		for(int i = 0; i < VPaint.Instance.currentEditingContents.Length; i++)
		{
			var obj = VPaint.Instance.currentEditingContents[i];
			
			Rect r = EditorGUILayout.BeginHorizontal();
				
			currentEditingContentsMask[i] = EditorGUILayout.Toggle(currentEditingContentsMask[i], GUILayout.Width(16));
			
			EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
			if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && r.Contains(Event.current.mousePosition))
			{
				EditorGUIUtility.PingObject(obj.transform);
				Event.current.Use();
			}
			
			GUILayout.Label(obj.name);
			EditorGUILayout.EndHorizontal();
		}
		
		VPaintGUIUtility.columnViewBoxCount = 1;		
		
		EditorGUILayout.EndScrollView();
		
		EditorGUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
	}
	
}
