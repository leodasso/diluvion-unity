using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Valkyrie.VPaint;

public class VPaintBlendWindow : VPaintWindowBase
{
	public enum Mode
	{
		Radial,
		Directional
	}
	
	public static class Settings
	{
		static Vector3 _position;
		public static Vector3 position {
			get{ return _position; }
			set{
				if(_position != value)
				{
					_position = value;
					EditorPrefs.SetFloat("VP_BW_PosX", value.x);
					EditorPrefs.SetFloat("VP_BW_PosY", value.y);
					EditorPrefs.SetFloat("VP_BW_PosZ", value.z);
				}
			}
		}
		
		static Vector3 _size;
		public static Vector3 size {
			get{ return _size; }
			set{
				if(_size != value)
				{
					_size = value;
					EditorPrefs.SetFloat("VP_BW_SizeX", value.x);
					EditorPrefs.SetFloat("VP_BW_SizeY", value.y);
					EditorPrefs.SetFloat("VP_BW_SizeZ", value.z);
				}
			}
		}
		
		static int _mode;
		public static int mode {
			get{ return _mode; }
			set{
				if(_mode != value)
				{
					_mode = value;
					EditorPrefs.SetInt("VP_BW_Mode", value);
				}
			}
		}
		
		static bool _useBounds;
		public static bool useBounds {
			get{ return _useBounds; }
			set{
				if(_useBounds != value)
				{
					_useBounds = value;
					EditorPrefs.SetBool("VP_BW_UseBounds", value);
				}
			}
		}
		
		static float _radius;
		public static float radius {
			get{ return _radius; }
			set{
				if(_radius != value)
				{
					_radius = value;
					EditorPrefs.SetFloat("VP_BW_Radius", value);
				}
			}
		}
		
		static Vector3 _direction;
		public static Vector3 direction {
			get{ return _direction; }
			set{
				if(_direction != value)
				{
					_direction = value;
					EditorPrefs.SetFloat("VP_BW_DirX", value.x);
					EditorPrefs.SetFloat("VP_BW_DirY", value.y);
					EditorPrefs.SetFloat("VP_BW_DirZ", value.z);
				}
			}
		}
		
		static Vector3 _offset;
		public static Vector3 offset {
			get{ return _offset; }
			set{
				if(_offset != value)
				{
					_offset = value;
					EditorPrefs.SetFloat("VP_BW_DirOffX", value.x);
					EditorPrefs.SetFloat("VP_BW_DirOffY", value.y);
					EditorPrefs.SetFloat("VP_BW_DirOffZ", value.z);
				}
			}
		}
		
		static float _directionalDistance;
		public static float directionalDistance {
			get{ return _directionalDistance; }
			set{
				if(_directionalDistance != value)
				{
					_directionalDistance = value;
					EditorPrefs.SetFloat("VP_BW_DirDist", value);
				}
			}
		}
		
		static float _directionalPower;
		public static float directionalPower {
			get{ return _directionalPower; }
			set{
				if(_directionalPower != value)
				{
					_directionalPower = value;
					EditorPrefs.SetFloat("VP_BW_DirPow", value);
				}
			}
		}
		
		static float _intensity;
		public static float intensity {
			get{ return _intensity; }
			set{
				if(_intensity != value)
				{
					_intensity = value;
					EditorPrefs.SetFloat("VP_BW_Intensity", value);
				}
			}
		}
		
		static int _layer;
		public static int layer {
			get{ return _layer; }
			set{
				if(_layer != value)
				{
					_layer = value;
					EditorPrefs.SetInt("VP_BW_Layer", value);
				}
			}
		}
		
		public static void Load ()
		{
			_position = new Vector3(
				EditorPrefs.GetFloat("VP_BW_PosX", 0f),
				EditorPrefs.GetFloat("VP_BW_PosY", 0f),
				EditorPrefs.GetFloat("VP_BW_PosZ", 0f));
			
			_size = new Vector3(
				EditorPrefs.GetFloat("VP_BW_SizeX", 0f),
				EditorPrefs.GetFloat("VP_BW_SizeY", 0f),
				EditorPrefs.GetFloat("VP_BW_SizeZ", 0f));
			
			_direction = new Vector3(
				EditorPrefs.GetFloat("VP_BW_DirX", 0f),
				EditorPrefs.GetFloat("VP_BW_DirY", -1f),
				EditorPrefs.GetFloat("VP_BW_DirZ", 0f));
			
			_offset = new Vector3(
				EditorPrefs.GetFloat("VP_BW_DirOffX", 0f),
				EditorPrefs.GetFloat("VP_BW_DirOffY", 0f),
				EditorPrefs.GetFloat("VP_BW_DirOffZ", 0f));
			
			_radius = EditorPrefs.GetFloat("VP_BW_Radius", 1f);
			_directionalDistance = EditorPrefs.GetFloat("VP_BW_DirDist", 1f);
			_intensity = EditorPrefs.GetFloat("VP_BW_Intensity", 1f);
			_directionalPower = EditorPrefs.GetFloat("VP_BW_DirPow", 1f);
			
			_useBounds = EditorPrefs.GetBool("VP_BW_UseBounds", true);
			_mode = EditorPrefs.GetInt("VP_BW_Mode", 1);
			_layer = EditorPrefs.GetInt("VP_BW_Layer", 0);
		}
	}
	
	public override void OnValidatedEnable ()
	{
		ValidateSelection();
	}
	
	public UnityEngine.Object currentSelection;
	void ValidateSelection ()
	{
		Settings.Load();
	}
	
	public override bool LockSelection ()
	{
		return true;
	}
	
	public override bool CloseOnInvalid ()
	{
		return false;
	}
	
	public List<VPaintObject> blendObjects = new List<VPaintObject>();
	public List<VPaintObject> blendTargets = new List<VPaintObject>();
	
	Vector2 blendObjectsScroll;
	Vector2 blendTargetsScroll;
	
	Vector2 blendScroll;
	Vector2 mainScroll;
	public override void OnValidatedGUI ()
	{
		mainScroll = EditorGUILayout.BeginScrollView(mainScroll);
		
		BlendSettingsGUI();
		
		GUILayout.Space(20);
		
		BlendAreaGUI();
		
		GUILayout.Space(20);
		
		BlendHierarchyGUI();
		
		EditorGUILayout.EndScrollView();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("Preview"))
		{
			switch((Mode)Settings.mode)
			{
				case Mode.Directional:
					BlendDirectional(true);
					break;
				case Mode.Radial:
					BlendRadial(true);
					break;
			}
		}
		if(GUILayout.Button("Apply"))
		{
			switch((Mode)Settings.mode)
			{
				case Mode.Directional:
					BlendDirectional(false);
					break;
				case Mode.Radial:
					BlendRadial(false);
					break;
			}
		}
		EditorGUILayout.EndHorizontal();
	}
	
	void BlendSettingsGUI ()
	{
		VPaintGUIUtility.BeginColumnView(position.width - 48);
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			Settings.mode = (int)(Mode)EditorGUILayout.EnumPopup("Mode", (Mode)Settings.mode);
		});
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			Settings.intensity = EditorGUILayout.Slider("Intensity", Settings.intensity, 0f, 1f);
		});
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			Settings.layer = EditorGUILayout.Popup("Layer", Settings.layer, new string[]{"Select Layer", "All Layers"});
		});
		
		switch((Mode)Settings.mode)
		{
			case Mode.Directional:
				DirectionalSettings();
				break;
			case Mode.Radial:
				RadialSettings();
				break;
		}
	}
	
	void RadialSettings ()
	{
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			Settings.radius = EditorGUILayout.FloatField("Radius", Settings.radius);
		});
	}
	
	void DirectionalSettings ()
	{
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			Settings.direction = EditorGUILayout.Vector3Field("Direction", Settings.direction);
		});
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			Settings.offset = EditorGUILayout.Vector3Field("Offset", Settings.offset);
		});
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			Settings.directionalPower = EditorGUILayout.FloatField("Falloff", Settings.directionalPower);
		});
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			Settings.directionalDistance = EditorGUILayout.FloatField("Distance", Settings.directionalDistance);
		});
	}
	
	void BlendAreaGUI ()
	{
		VPaintGUIUtility.BeginColumnView(position.width - 48);
		VPaintGUIUtility.DrawColumnRow(24, (r)=>{
			if(r.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				Settings.useBounds = !Settings.useBounds;
				Event.current.Use();
			}
			EditorGUILayout.Toggle(Settings.useBounds, GUILayout.Width(16));
			GUILayout.Label("Bounding Area:");
		});
		
		bool activeCache = GUI.enabled;
		GUI.enabled = Settings.useBounds;
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			Settings.position = EditorGUILayout.Vector3Field("Position", Settings.position);
		});
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			Settings.size = EditorGUILayout.Vector3Field("Size", Settings.size);				
		});
		GUI.enabled = activeCache;
	}
	
	void BlendHierarchyGUI ()
	{		
		VPaintGUIUtility.BeginColumnView(position.width - 48);
		
		VPaintGUIUtility.DualSelectionGroup(
			"Blend To", blendObjects, ref blendObjectsScroll,
			"Blend From", blendTargets, ref blendTargetsScroll);
	}
	
	public override bool OverrideTool ()
	{
		return true;
	}
	
	public override void OnValidatedSelectionChange ()
	{
		ValidateSelection();
	}
	
	public override void OnValidatedDisable ()
	{
		if(VPaint.Instance) VPaint.Instance.ReloadLayers();
	}
	
	public override void OnValidatedSceneGUI ()
	{
		if(Settings.useBounds)
		{
			Settings.position = Handles.PositionHandle(Settings.position, Quaternion.identity);
			
			var size = Settings.size;
			
			Action<Vector3, Vector3, float> scaleHandle = (offset, direction, sign)=>{
				var v1 = Settings.position + offset;
				var v2 = Handles.Slider(v1, direction, 0.05f, Handles.DotHandleCap, 0f);
				size += (v2-v1)/2*sign;
			};
			
			scaleHandle(new Vector3(size.x/2,0,0), Vector3.right, 1);
			scaleHandle(new Vector3(-size.x/2,0,0), -Vector3.right, -1);
			scaleHandle(new Vector3(0,size.y/2,0), Vector3.up, 1);
			scaleHandle(new Vector3(0,-size.y/2,0), -Vector3.up, -1);
			scaleHandle(new Vector3(0,0,size.z/2), Vector3.forward, 1);
			scaleHandle(new Vector3(0,0,-size.z/2), -Vector3.forward, -1);
			
			Settings.size = size;
			
			var s = Settings.size * 0.5f;
			var p = Settings.position;
			
			var p0 = p + Vector3.Scale(s, new Vector3(-1,-1,-1));
			var p1 = p + Vector3.Scale(s, new Vector3( 1,-1,-1));
			var p2 = p + Vector3.Scale(s, new Vector3( 1,-1, 1));
			var p3 = p + Vector3.Scale(s, new Vector3(-1,-1, 1));
			var p4 = p + Vector3.Scale(s, new Vector3(-1, 1,-1));
			var p5 = p + Vector3.Scale(s, new Vector3( 1, 1,-1));
			var p6 = p + Vector3.Scale(s, new Vector3( 1, 1, 1));
			var p7 = p + Vector3.Scale(s, new Vector3(-1, 1, 1));
			
			Handles.DrawLine(p0, p1);
			Handles.DrawLine(p1, p2);
			Handles.DrawLine(p2, p3);
			Handles.DrawLine(p3, p0);
			Handles.DrawLine(p4, p5);
			Handles.DrawLine(p5, p6);
			Handles.DrawLine(p6, p7);
			Handles.DrawLine(p7, p4);
			Handles.DrawLine(p0, p4);
			Handles.DrawLine(p1, p5);
			Handles.DrawLine(p2, p6);
			Handles.DrawLine(p3, p7);
			
			Repaint();
		}
	}
	
	VPaintLayer[] GetLayers (bool preview)
	{
		var list = new List<VPaintLayer>();	
		if(Settings.layer == 0)
		{
			list.Add(currentLayer);
		}
		else
		{
			list.AddRange(currentLayerStack.layers);
		}
		if(preview)
		{
			for(int i = 0; i < list.Count; i++) list[i] = list[i].Clone();
		}
		return list.ToArray();
	}
	
	VPaintObject[] GetBlendObjects ()
	{
		return blendObjects.ToArray();
	}
	
	public void BlendRadial (bool preview)
	{
		if(!preview) VPaint.Instance.PushUndo("Blend Objects");
		
		Bounds bounds = new Bounds(Settings.position, Settings.size);
		
		var validTargets = new List<VPaintObject>();
		foreach(var vc in blendTargets)
		{
			if(Settings.useBounds)
			{
				if(!bounds.Intersects(vc.GetBounds()))
					continue;
			}
			validTargets.Add(vc);
		}
		
		var layers = GetLayers(preview);
		
		var asyncOperation = VPaintUtility.BlendRadialAsync(
			layers, GetBlendObjects(), validTargets.ToArray(), 
			Settings.radius, Settings.intensity, Settings.useBounds ? bounds : (Bounds?)null
		);
			
		while(asyncOperation.MoveNext())
		{
			if(EditorUtility.DisplayCancelableProgressBar("Applying Radial Blending", asyncOperation.Current.message, asyncOperation.Current.progress))
			{
				return;
			}
		}
		EditorUtility.ClearProgressBar();
		
		if(preview)
		{
			VPaint.Instance.LoadLayers(new List<VPaintLayer>(layers));
		}
		else
		{
			VPaint.Instance.ReloadLayers();
		}
	}
	
	public void BlendDirectional (bool preview)
	{
		if(!preview) VPaint.Instance.PushUndo("Blend Objects");
		
		Bounds bounds = new Bounds(Settings.position, Settings.size);
			
		var validTargets = new List<VPaintObject>();
		foreach(var vc in blendTargets)
		{
			if(Settings.useBounds)
			{
				if(!bounds.Intersects(vc.GetBounds()))
					continue;
			}
			if(!vc.editorCollider) VPaint.Instance.GetCollider(vc);
			validTargets.Add(vc);
		}
		
		var layers = GetLayers(preview);
		
		var asyncOperation = VPaintUtility.BlendDirectionalAsync(
			layers, GetBlendObjects(), validTargets.ToArray(), 
			Settings.direction, Settings.directionalDistance, Settings.intensity, Settings.directionalPower,
			Settings.offset, Settings.useBounds ? bounds : (Bounds?)null
		);
		
		while(asyncOperation.MoveNext())
		{
			if(EditorUtility.DisplayCancelableProgressBar("Applying Directional Blending", asyncOperation.Current.message, asyncOperation.Current.progress))
			{
				EditorUtility.ClearProgressBar();
				return;
			}
		}
		EditorUtility.ClearProgressBar();
		
		if(preview)
		{
			VPaint.Instance.LoadLayers(new List<VPaintLayer>(layers));
		}
		else
		{
			VPaint.Instance.ReloadLayers();
		}
	}
}
