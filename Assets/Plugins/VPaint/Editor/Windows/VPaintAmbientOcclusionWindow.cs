using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Valkyrie.VPaint;

public class VPaintAmbientOcclusionWindow : VPaintWindowBase 
{
	public static class Settings {
		static int _samples = 1000;
		public static int samples {
			get{ return _samples; }
			set{
				_samples = value;
				EditorPrefs.SetInt("VP_AO_Samples", value);
			}
		}
		
		static float _radius = 1f;
		public static float radius {
			get{ return _radius; }
			set{
				_radius = value;
				EditorPrefs.SetFloat("VP_AO_Radius", value);
			}
		}
		
		static float _intensity = 1f;
		public static float intensity {
			get{ return _intensity; }
			set{
				_intensity = value;
				EditorPrefs.SetFloat("VP_AO_Intensity", value);
			}
		}
		
		static Color _darkColor = new Color();
		public static Color darkColor {
			get{ return _darkColor; }
			set{
				_darkColor = value;
				EditorPrefs.SetFloat("VP_AO_DarkColorR", value.r);
				EditorPrefs.SetFloat("VP_AO_DarkColorG", value.g);
				EditorPrefs.SetFloat("VP_AO_DarkColorB", value.b);
				EditorPrefs.SetFloat("VP_AO_DarkColorA", value.a);
			}
		}
		
		static Color _lightColor = Color.white;
		public static Color lightColor {
			get{ return _lightColor; }
			set{
				_lightColor = value;
				EditorPrefs.SetFloat("VP_AO_LightColorR", value.r);
				EditorPrefs.SetFloat("VP_AO_LightColorG", value.g);
				EditorPrefs.SetFloat("VP_AO_LightColorB", value.b);
				EditorPrefs.SetFloat("VP_AO_LightColorA", value.a);
			}
		}
		
		public static void Load ()
		{
			_samples = EditorPrefs.GetInt("VP_AO_Samples", 1000);
			_radius = EditorPrefs.GetFloat("VP_AO_Radius", 1f);
			_intensity = EditorPrefs.GetFloat("VP_AO_Intensity", 1f);
			
			_darkColor = new Color(
				EditorPrefs.GetFloat("VP_AO_DarkColorR", 0f),
				EditorPrefs.GetFloat("VP_AO_DarkColorG", 0f),
				EditorPrefs.GetFloat("VP_AO_DarkColorB", 0f),
				EditorPrefs.GetFloat("VP_AO_DarkColorA", 0f)
			);
			
			_lightColor = new Color(
				EditorPrefs.GetFloat("VP_AO_LightColorR", 1f),
				EditorPrefs.GetFloat("VP_AO_LightColorG", 1f),
				EditorPrefs.GetFloat("VP_AO_LightColorB", 1f),
				EditorPrefs.GetFloat("VP_AO_LightColorA", 1f)
			);
		}
	}
	
	public override void OnValidatedEnable ()
	{
		base.OnValidatedEnable ();
		Settings.Load();
		title = "Import Ambient Occlusion";
	}
	
	List<VPaintObject> targetObjects = new List<VPaintObject>();
	Vector2 targetObjectsScroll;
	
	Vector2 mainScroll;
	
	public override void OnValidatedGUI ()
	{
		VPaintGUIUtility.BeginColumnView(position.width-24);
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			GUILayout.Label("Import Ambient Occlusion");
		});
		
		VPaintGUIUtility.BeginColumnView(position.width-48);
		
		mainScroll = EditorGUILayout.BeginScrollView(mainScroll);
		
		GUILayout.Space(20);
		
		VPaintGUIUtility.DrawColumnRow(24,()=>{
			Settings.samples = EditorGUILayout.IntField("Samples:", Settings.samples);
		});
		VPaintGUIUtility.DrawColumnRow(24,()=>{
			Settings.radius = EditorGUILayout.FloatField("Radius: ", Settings.radius);
		});
		VPaintGUIUtility.DrawColumnRow(24,()=>{
			Settings.intensity = EditorGUILayout.FloatField("Intensity:", Settings.intensity);
		});
		
		VPaintGUIUtility.DrawColumnRow(24,()=>{
			Settings.lightColor = EditorGUILayout.ColorField("Light Color:", Settings.lightColor);
		});
		
		VPaintGUIUtility.DrawColumnRow(24,()=>{
			Settings.darkColor = EditorGUILayout.ColorField("Dark Color:", Settings.darkColor);
		});
		
		GUILayout.FlexibleSpace();
		
		VPaintGUIUtility.SelectionGroup("Objects:", targetObjects, ref targetObjectsScroll);
		
		GUILayout.Space(20);
		
		EditorGUILayout.EndScrollView();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("Preview"))
		{
			var layer = CalculateAmbientOcclusion(targetObjects.ToArray(), Settings.radius, Settings.intensity, Settings.samples, Settings.darkColor, Settings.lightColor);
			if(layer != null)
			{
				VPaint.Instance.LoadLayer(layer);
			}
		}
		if(GUILayout.Button("Import"))
		{
			var layer = CalculateAmbientOcclusion(targetObjects.ToArray(), Settings.radius, Settings.intensity, Settings.samples, Settings.darkColor, Settings.lightColor);
			if(layer != null)
			{
				VPaint.Instance.layerStack.layers.Add(layer);
				VPaint.Instance.ReloadLayers();
				VPaint.Instance.MarkDirty();
			}
		}
		EditorGUILayout.EndHorizontal();
	}
	
	VPaintLayer CalculateAmbientOcclusion (VPaintObject[] objects, float radius, float intensity, int sampleCount, Color darkColor, Color lightColor, Bounds? bounds = null)
	{
		foreach(var obj in objects)
		{
			VPaint.Instance.GetCollider(obj);
		}
		var async = VPaintUtility.CalculateAmbientOcclusionAsync(objects, radius, intensity, sampleCount, darkColor, lightColor, bounds);
		while(async.MoveNext())
		{
			var c = async.Current;
			if(EditorUtility.DisplayCancelableProgressBar("Sampling AO", c.message, c.progress))
			{
				EditorUtility.ClearProgressBar();
				return null;
			}
		}
		EditorUtility.ClearProgressBar();
		return async.Current.result;
	}
}