using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System;
using Type = System.Type;
using Valkyrie.VPaint;

public class VPaintImportTextureWindow : VPaintImportTextureWindowAbstract
{
	enum UVType
	{
		UV1,
		UV2
	}
	UVType _uvType = UVType.UV1;
	UVType uvType {
		get{ return _uvType; }
		set{
			if(_uvType != value)
			{
				_uvType = value;
				EditorPrefs.SetInt("VP_IT_UV", (int)value);
			}
		}
	}
	
	Texture2D _texture;
	Texture2D texture {
		get{ return _texture; }
		set{
			if(_texture != value)
			{
				_texture = value;
				string s = "null";
				if(_texture) s = AssetDatabase.GetAssetPath(_texture);
				EditorPrefs.SetString("VP_IT_TX", s);
			}
		}
	}
	
	Vector2 _tile = Vector2.one;
	Vector2 tile {
		get{ return _tile; }
		set{
			if(_tile != value)
			{
				_tile = value;
				EditorPrefs.SetFloat("VP_IT_TileX", _tile.x);
				EditorPrefs.SetFloat("VP_IT_TileY", _tile.y);
			}
		}
	}
	
	Vector2 _offset = Vector2.zero;
	Vector2 offset {
		get{ return _offset; }
		set{
			if(_offset != value)
			{
				_offset = value;
				EditorPrefs.SetFloat("VP_IT_OffsetX", _offset.x);
				EditorPrefs.SetFloat("VP_IT_OffsetY", _offset.y);
			}
		}
	}
	
	void LoadSettings ()
	{
		_uvType = (UVType)EditorPrefs.GetInt("VP_IT_UV", (int)UVType.UV1);
		_tile = new Vector2(EditorPrefs.GetFloat("VP_IT_TileX", 1f), EditorPrefs.GetFloat("VP_IT_TileY", 1f));
		_offset = new Vector2(EditorPrefs.GetFloat("VP_IT_OffsetX", 1f), EditorPrefs.GetFloat("VP_IT_OffsetY", 1f));
		
		string s = EditorPrefs.GetString("VP_IT_TX", "null");
		if(s == "null") _texture = null;
		else texture = AssetDatabase.LoadAssetAtPath(s, typeof(Texture2D)) as Texture2D;
	}
	
	public override void OnValidatedEnable ()
	{
		base.OnValidatedEnable();
		LoadSettings();
	}
	
	public override void OnSettingsGUI ()
	{
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.wordWrap = true;
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			GUILayout.Label("Texture Settings", style);
		});
			
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			texture = EditorGUILayout.ObjectField("Texture", texture, typeof(Texture2D), false) as Texture2D;
			
			EditorGUILayout.BeginVertical();
			tile = EditorGUILayout.Vector2Field("Tile", tile);
			offset = EditorGUILayout.Vector2Field("Offset", offset);
			GUILayout.Space(10);
			EditorGUILayout.EndVertical();
		});
			
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			uvType = (UVType)EditorGUILayout.EnumPopup("UV Channel", uvType);
		});
		
		GUILayout.Space(10);
	}
	
	public override Vector2[] GetUVs (Mesh mesh)
	{
		switch(_uvType)
		{
			default:
			case UVType.UV1: return mesh.uv;
			case UVType.UV2: return mesh.uv2;
		}
	}
	public override Texture2D GetTexture (VPaintObject vc)
	{
		return _texture;
	}
	public override bool AllowImport ()
	{
		return _texture;
	}
	public override Func<Vector2, Vector2> GetUVTransformation (VPaintObject vc)
	{
		return (uv)=>{ return Vector2.Scale(uv, tile) + offset;	};
	}
}
