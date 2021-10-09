using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System;
using Type = System.Type;
using Valkyrie.VPaint;

public class VPaintImportLightmapWindow : VPaintImportTextureWindowAbstract
{
	int _lightmapIntensity = 8;
	public int lightmapIntensity {
		get{ return _lightmapIntensity; }
		set{
			if(value != _lightmapIntensity)
			{
				_lightmapIntensity = value;
				EditorPrefs.SetInt("VP_IT_Intensity", _lightmapIntensity);
			}
		}
	}
	public override void OnValidatedEnable ()
	{
		base.OnValidatedEnable ();
		_lightmapIntensity = EditorPrefs.GetInt("VP_IT_Intensity", 8);
	}
	public override void OnSettingsGUI ()
	{
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.wordWrap = true;
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			GUILayout.Label("The light intensity to import the lightmap at. Default is 8.", style);
		});
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			lightmapIntensity = EditorGUILayout.IntField("Import Intensity:", lightmapIntensity);
		});
	}
	
	public override Vector2[] GetUVs (Mesh mesh)
	{
		return mesh.uv2;
	}
	public override bool IsValid (VPaintObject vc)
	{
		return GameObjectUtility.AreStaticEditorFlagsSet(vc.gameObject, StaticEditorFlags.LightmapStatic);
	}
	public override Texture2D GetTexture (VPaintObject vc)
	{
		Renderer renderer = vc.GetComponent<Renderer>();
		
		LightmapData lmData = LightmapSettings.lightmaps[renderer.lightmapIndex];
		return lmData.lightmapColor;
//		offset = renderer.lightmapTilingOffset;
//		texture = lmData.lightmapFar;
	}
	public override Func<Vector2, Vector2> GetUVTransformation (VPaintObject vc)
	{
		Renderer renderer = vc.GetComponent<Renderer>();
#if UNITY_5
		var offset = renderer.lightmapScaleOffset;
#else
		var offset = renderer.lightmapScaleOffset;
#endif
		return (uv)=>{
			return new Vector2((offset.x * uv.x) + offset.z, (offset.y * uv.y) + offset.w);
		};
	}
	public override Color PostProcessColor (Color c)
	{
		c = c * c.a * lightmapIntensity;
		c.a = 1;
		return c;
	}
}