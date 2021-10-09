using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using Valkyrie.VPaint;

public class VPaintLayerAdjustment : VPaintWindowBase
{
	bool _autoPreview = true;
	bool autoPreview {
		get{ return _autoPreview; }
		set{
			if(_autoPreview != value)
			{
				if(VPaint.Instance)
				{
					if(value) Preview();
					else VPaint.Instance.ReloadLayers();
				}
				_autoPreview = value;
				EditorPrefs.SetBool("VP_LA_AutoPreview", _autoPreview);
			}
		}
	}
	
	public VPaintActionType type = VPaintActionType.Brightness;
	public VPaintLayerAction action = new VPaintLayerAction();
	
	public override void OnValidatedEnable ()
	{
		_autoPreview = EditorPrefs.GetBool("VP_LA_AutoPreview", true);
		if(_autoPreview) Preview();
	}
	public override void OnValidatedDisable ()
	{
		if(VPaint.Instance) VPaint.Instance.ReloadLayers();
	}
	
	public override bool OverrideTool ()
	{
		return true;
	}
	
	Vector2 scrollPosition;
	public override void OnValidatedGUI ()
	{
		VPaintGUIUtility.BeginColumnView(position.width - 48);
		
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.wordWrap = true;
		
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		
		bool doPreview = false;
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			GUILayout.Label("Targeted Layer: " + currentLayer.name);
		});
		
		EditorGUI.BeginChangeCheck();
		VertexEditorActionEditor.OnGUI(action, type);
		doPreview |= EditorGUI.EndChangeCheck();
		
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndScrollView();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		autoPreview = GUILayout.Toggle(autoPreview, "Auto");
		bool enabledCache = GUI.enabled;
		GUI.enabled = !autoPreview;
		if(GUILayout.Button("Preview") || (autoPreview && doPreview))
		{
			Preview();
		}
		GUI.enabled = enabledCache;
		if(GUILayout.Button("Apply"))
		{
			Apply();
		}
		EditorGUILayout.EndHorizontal();
	}
	
	void Preview ()
	{
		int len = currentLayerStack.layers.Count;
		List<VPaintLayer> layers = new List<VPaintLayer>();
		
		for(int i = 0; i < len; i++)
		{
			var layer = currentLayerStack.layers[i];
			if(!layer.enabled) continue;
			var layerClone = currentLayerStack.layers[i].Clone(); 
			if(i == VPaint.Instance._currentPaintLayer)
				action.ApplyTo(layerClone, type);
			layers.Add(layerClone);
		}
		VPaint.Instance.LoadLayers(layers);
	}
	
	void Apply ()
	{
		VPaint.Instance.PushUndo(Enum.GetName(typeof(VPaintActionType), type));
		action.ApplyTo(currentLayer, type);
		if(autoPreview) Preview();
		else VPaint.Instance.ReloadLayers();
		EditorApplication.delayCall += Close;
	}
	
}
