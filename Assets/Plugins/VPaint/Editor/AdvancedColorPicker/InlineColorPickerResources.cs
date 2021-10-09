using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class InlineColorPickerResources : ScriptableObject 
{
	static string GetPath ()
	{
		var instance = CreateInstance<InlineColorPickerResources>();
		var monoscript = MonoScript.FromScriptableObject(instance);
		GameObject.DestroyImmediate(instance);
		var path = AssetDatabase.GetAssetPath(monoscript);		
		return path.Substring(0, path.Length - "/InlineColorPickerResources.cs".Length);
	}
	
	static Texture2D _pickerTexture;
	public static Texture2D pickerTexture
	{
		get{
			if(!_pickerTexture)
			{
				_pickerTexture = AssetDatabase.LoadAssetAtPath(GetPath() + "/Textures/Picker.png", typeof(Texture2D)) as Texture2D;
			}
			return _pickerTexture;
		}
	}
	
	static Texture2D _arrow_horizontal;
	public static Texture2D arrow_horizontal
	{
		get{
			if(!_arrow_horizontal)
			{
				_arrow_horizontal = AssetDatabase.LoadAssetAtPath(GetPath() + "/Textures/Arrow_Horizontal.png", typeof(Texture2D)) as Texture2D;
			}
			return _arrow_horizontal;
		}
	}
	
	static Texture2D _arrow_vertical;
	public static Texture2D arrow_vertical
	{
		get{
			if(!_arrow_vertical)
			{
				_arrow_vertical = AssetDatabase.LoadAssetAtPath(GetPath() + "/Textures/Arrow_Vertical.png", typeof(Texture2D)) as Texture2D;
			}
			return _arrow_vertical;
		}
	}
}
