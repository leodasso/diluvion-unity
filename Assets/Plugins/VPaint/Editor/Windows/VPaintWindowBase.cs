using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;
using Valkyrie.VPaint;

[InitializeOnLoad]
public class VPaintWindowBase : EditorWindow 
{
	static VPaintWindowBase ()
	{
		EditorApplication.delayCall += ()=>{
			if(VPaint.Instance)
			{
				foreach(var instance in Instances)
				{
					if(instance.LockSelection()) VPaint.Instance.lockSelection = true;
					if(instance.OverrideTool()) VPaint.Instance.overrideTool = true;
				}
			}
		};
	}
	
	public VPaintLayerStack currentLayerStack
		{ get{ return VPaint.Instance.layerStack; } }
	
	public VPaintLayer currentLayer
		{ get{ return currentLayerStack.layers[currentLayerStack.currentLayer]; } }
	
	public static T GetVPaintWindow <T> ()
		where T : VPaintWindowBase 
	{
		foreach(var instance in Instances)
		{
			if(instance is T) return instance as T;
		}
		return null;
	}
	
	public static void RepaintAll ()
	{
		foreach(var instance in Instances) instance.Repaint();
	}
	
	public static List<VPaintWindowBase> Instances = new List<VPaintWindowBase>();
	public void OnEnable ()
	{
		Instances.Add(this);
		if(Validate())
		{
			if(LockSelection()) VPaint.Instance.lockSelection = true;
			if(OverrideTool()) VPaint.Instance.overrideTool = true;
			SceneView.onSceneGUIDelegate += OnSceneGUI;
			OnValidatedEnable();
		}
	}
	public virtual void OnValidatedEnable () {}
	
	public void OnDisable ()
	{
		Instances.Remove(this);
		SceneView.onSceneGUIDelegate -= OnSceneGUI;
		OnValidatedDisable();
		
		if(Validate())
		{
			if(VPaint.Instance.lockSelection)
			{
				bool shouldUnlock = true;
				foreach(var instance in Instances)
				{
					if(instance.LockSelection())
					{
						shouldUnlock = false;
						break;
					}
				}
				if(shouldUnlock)
				{
					Selection.activeObject = VPaint.Instance.layerCache as UnityEngine.Object;
					VPaint.Instance.lockSelection = false;
				}				
			}
			
			if(VPaint.Instance.overrideTool)
			{
				bool shouldDeOverrideTool = true;
				foreach(var instance in Instances)
				{
					if(instance.OverrideTool())
					{
						shouldDeOverrideTool = false;
						break;
					}
				}
				if(shouldDeOverrideTool)
				{
					VPaint.Instance.overrideTool = false;
				}
			}
		}
	}
	public virtual void OnValidatedDisable () {}
	
	public void OnGUI ()
	{
		if(!Validate())
		{
			VPaintGUIUtility.BoxArea(8f, 8f, 1, ()=>{
				GUILayout.Label("Vertex Painter is disabled.");
			});
			return;
		}
		OnValidatedGUI ();
	}
	public virtual void OnValidatedGUI () {}
	
	public void Update ()
	{
		if(!Validate())
		{
			if(CloseOnInvalid()) Close();
			return;
		}
		OnValidatedUpdate ();
	}
	public virtual void OnValidatedUpdate () {}
	
	public void OnSceneGUI (SceneView view) 
	{
		if(!Validate()) return;
		OnValidatedSceneGUI();
	}
	public virtual void OnValidatedSceneGUI () {}
	
	public void OnSelectionChange () {
		Repaint();
		if(Validate()) OnValidatedSelectionChange();
	}
	public virtual void OnValidatedSelectionChange () {
	}
	
	public virtual bool Validate ()
	{
		return VPaint.Instance && VPaint.Instance.layerStack != null;
	}
	
	public virtual bool LockSelection () { return false; }
	public virtual bool CloseOnInvalid () { return true; }
	public virtual bool OverrideTool () { return false; }
	
	protected GUIStyle GetWordWrappedLabel ()
	{
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.wordWrap = true;
		return style;
	}
}