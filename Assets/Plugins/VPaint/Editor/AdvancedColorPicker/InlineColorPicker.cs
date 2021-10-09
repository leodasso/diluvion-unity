using UnityEngine;
using UnityEditor;
using System;
using Valkyrie.VPaint;

public class InlineColorPicker : ScriptableObject
{	
	public Texture2D mainTexture;
	Color32[] mainColorBuffer;
	
	public Texture2D sidebarTexture;
	Color32[] sideColorBuffer;
	
	public Texture2D alphaTexture;
	Color32[] alphaColorBuffer;
	
	public enum Mode
	{
		Hue,
		Saturation,
		Brightness
	}
	Mode mode = Mode.Hue;
	
	int width;
	int height;
	
	HSBColor _hsb;
	Color _cachedColor;
	Mode _cachedMode = (Mode)(-1);
	
	public void Setup (int height)
	{				
		this.width = height;
		this.height = height;
		
		mainTexture = new Texture2D(width - 30, height - 100, TextureFormat.RGBA32, false, true);
		mainTexture.wrapMode = TextureWrapMode.Clamp;
		mainTexture.anisoLevel = 0;
		mainTexture.filterMode = FilterMode.Point;
		mainColorBuffer = new Color32[mainTexture.width*mainTexture.height];
		
		sidebarTexture = new Texture2D(1, height - 100, TextureFormat.RGBA32, false, true);
		sidebarTexture.wrapMode = TextureWrapMode.Clamp;
		sidebarTexture.anisoLevel = 0;
		sidebarTexture.filterMode = FilterMode.Point;
		sideColorBuffer = new Color32[sidebarTexture.height];
		
		alphaTexture = new Texture2D(width - 47, 1, TextureFormat.RGBA32, false, true);
		alphaTexture.wrapMode = TextureWrapMode.Clamp;
		alphaTexture.anisoLevel = 0;
		alphaTexture.filterMode = FilterMode.Point;
		alphaColorBuffer = new Color32[alphaTexture.width];
		DrawAlphaBar();
		alphaTexture.Apply();
		
		UpdateColor (new Color());
	}
	
	void DrawSidebar ()
	{
		var col = new HSBColor(_cachedColor);
		for(int y = 0; y < sidebarTexture.height; y++)
		{
			float interpY = (float)y/sidebarTexture.height;
			
			HSBColor color;
			switch(mode)
			{
				default:
				case Mode.Hue:
					color = new HSBColor(interpY, 1, 1, 1);
					break;
				case Mode.Brightness:
					color = new HSBColor(col.h, col.s, interpY, 1);
					break;
				case Mode.Saturation:
					color = new HSBColor(col.h, interpY, col.b, 1);
					break;
			}
			
			sideColorBuffer[y] = (Color32)color.ToColor();
		}
		sidebarTexture.SetPixels32(sideColorBuffer);
	}
	
	void DrawAlphaBar ()
	{
		for(int x = 0; x < alphaTexture.width; x++)
		{
			float interpX = (float)x/alphaTexture.width;
			
			alphaColorBuffer[x] = (Color32)(new Color(interpX, interpX, interpX, 1));
		}
		alphaTexture.SetPixels32(alphaColorBuffer);
	}
	
	void DrawMain ()
	{
		HSBColor col = new HSBColor(_cachedColor);
		for(int x = 0; x < mainTexture.width; x++)
		{
			float interpX = (float)x/mainTexture.width;
			for(int y = 0; y < mainTexture.height; y++)
			{
				int index = y*mainTexture.width+x;
				
				float interpY = (float)y/mainTexture.height;
				
				HSBColor color;
				
				switch(mode)
				{
					default:
					case Mode.Hue:
						color = new HSBColor(col.h, interpX, interpY, 1);
						break;
					case Mode.Brightness:
						color = new HSBColor(interpX, interpY, col.b, 1);
						break;
					case Mode.Saturation:
						color = new HSBColor(interpX, col.s, interpY, 1);
						break;
				}
				
				mainColorBuffer[index] = (Color32)color.ToColor();
			}
		}
		
		mainTexture.SetPixels32(mainColorBuffer);
	}
	
	bool mainDown = false;
	bool sideDown = false;
	bool alphaDown = false;
	public void DrawGUI (ref Color color)
	{		
		var rect = GUILayoutUtility.GetRect(width, height);
		
		var mainRect = rect;
		mainRect.width = mainTexture.width;
		mainRect.height = mainTexture.height;
		
		var sideRect = rect;
		sideRect.width = 20;
		sideRect.height = sidebarTexture.height;
		sideRect.x += mainRect.width + 4;
		
		var alphaRect = rect;
		alphaRect.width = alphaTexture.width;
		alphaRect.height = 16;
		alphaRect.x += 40;
		alphaRect.y = mainRect.y + mainRect.height + 4;
		
		GUI.DrawTexture(mainRect, mainTexture);
		GUI.DrawTexture(sideRect, sidebarTexture);
		GUI.DrawTexture(alphaRect, alphaTexture);
		
		GUI.Label(new Rect(rect.x, alphaRect.y, 40, 16), "Alpha:");
		
		var bottomRect = rect;
		bottomRect.y = mainRect.y + mainRect.height + 24;
		bottomRect.height = 80-4;
		
		
		//Channels
		var channelRect = bottomRect;
		channelRect.width /= 3;
		channelRect.height /= 4;
		EditorGUIUtility.LookLikeControls(20, channelRect.width - 20);
		
		color.r = Mathf.Clamp01(EditorGUI.FloatField(channelRect, "R:", (color.r * 255f))/255f);
		channelRect.y += channelRect.height;
		color.g = Mathf.Clamp01(EditorGUI.FloatField(channelRect, "G:", (color.g * 255f))/255f);
		channelRect.y += channelRect.height;
		color.b = Mathf.Clamp01(EditorGUI.FloatField(channelRect, "B:", (color.b * 255f))/255f);
		channelRect.y += channelRect.height;
		color.a = Mathf.Clamp01(EditorGUI.FloatField(channelRect, "A:", (color.a * 255f))/255f);
		
		
		//Hex
		var hexRect = bottomRect;
		hexRect.width /= 3;
		hexRect.height /= 4;
		hexRect.x += hexRect.width*2;
		hexRect.y += hexRect.height * 3;
		hexRect.width -= 6;
		EditorGUIUtility.LookLikeControls(15, hexRect.width - 15);
		
		float cr = Mathf.Clamp(color.r * 255f, 0, 255);
		float cg = Mathf.Clamp(color.g * 255f, 0, 255);
		float cb = Mathf.Clamp(color.b * 255f, 0, 255);
		
		string alpha = "0123456789ABCDEF";
		string hex = ""+
			alpha[Mathf.Clamp(Mathf.FloorToInt(cr / 16), 0, 15)] + alpha[Mathf.Clamp(Mathf.RoundToInt(cr % 16), 0, 15)]
		+	alpha[Mathf.Clamp(Mathf.FloorToInt(cg / 16), 0, 15)] + alpha[Mathf.Clamp(Mathf.RoundToInt(cg % 16), 0, 15)]
		+	alpha[Mathf.Clamp(Mathf.FloorToInt(cb / 16), 0, 15)] + alpha[Mathf.Clamp(Mathf.RoundToInt(cb % 16), 0, 15)];
		
		string newHex = EditorGUI.TextField(hexRect, "#", hex);
		if(newHex != hex && newHex.Length == 6)
		{
			color.r = (alpha.IndexOf(newHex[0]) * 16 + alpha.IndexOf(newHex[1])) / 255f;
			color.g = (alpha.IndexOf(newHex[2]) * 16 + alpha.IndexOf(newHex[3])) / 255f;
			color.b = (alpha.IndexOf(newHex[4]) * 16 + alpha.IndexOf(newHex[5])) / 255f;
		}
		
		//Swatch
		var swatchRect = bottomRect;
		swatchRect.x = hexRect.x;
		swatchRect.width = hexRect.width;
		swatchRect.height -= hexRect.height;
		EditorGUIUtility.DrawColorSwatch(swatchRect, color);
		
		//HSB
		EditorGUIUtility.LookLikeControls(20, channelRect.width - 40);
		
		var hsbRect = bottomRect;
		hsbRect.width /= 3;
		hsbRect.height /= 4;
		hsbRect.x += hsbRect.width;
		hsbRect.width -= 30;
		
		var hsbSelectRect = hsbRect;
		hsbSelectRect.width = 30;
		hsbSelectRect.x += 6;
		
		hsbRect.x += 24;
		
		EditorGUI.BeginChangeCheck();
		_hsb.h = Mathf.Repeat(EditorGUI.FloatField(hsbRect, "H:", _hsb.h*360f), 360f)/360f;
		if(EditorGUI.Toggle(hsbSelectRect, mode == Mode.Hue)) mode = Mode.Hue;
		
		hsbRect.y += hsbRect.height;
		hsbSelectRect.y += hsbRect.height;
		_hsb.s = Mathf.Clamp01(EditorGUI.FloatField(hsbRect, "S:", _hsb.s*255f)/255f);
		if(EditorGUI.Toggle(hsbSelectRect, mode == Mode.Saturation)) mode = Mode.Saturation;
		
		hsbRect.y += hsbRect.height;
		hsbSelectRect.y += hsbRect.height;
		_hsb.b = Mathf.Clamp01(EditorGUI.FloatField(hsbRect, "B:", _hsb.b*255f)/255f);
		if(EditorGUI.Toggle(hsbSelectRect, mode == Mode.Brightness)) mode = Mode.Brightness;
		
		if(EditorGUI.EndChangeCheck())
		{
			color = _hsb.ToColor();
		}		
		
		EditorGUIUtility.LookLikeControls();
		
		if(
			(Event.current.type == EventType.MouseDown || (Event.current.type == EventType.MouseDrag))
			&& Event.current.button == 0
		)
		{
			var mp = Event.current.mousePosition;
			
			if(!sideDown && !alphaDown && (mainRect.Contains(Event.current.mousePosition) || mainDown))
			{
				var mainInterp = new Vector2(
					Mathf.Clamp01((mp.x - mainRect.x)/mainRect.width), 
					Mathf.Clamp01((mp.y - mainRect.y)/mainRect.height)
				);
				
				switch(mode)
				{
					default:
					case Mode.Hue:
						_hsb.s = mainInterp.x;
						_hsb.b = 1-mainInterp.y;
						break;
					case Mode.Saturation:
						_hsb.h = mainInterp.x;
						_hsb.b = 1-mainInterp.y;
						break;
					case Mode.Brightness:
						_hsb.h = mainInterp.x;
						_hsb.s = 1-mainInterp.y;
						break;
				}
				color = _hsb.ToColor();
				
				Event.current.Use();
				
				mainDown = true;
			}
			
			if(!mainDown && !alphaDown && (sideRect.Contains(Event.current.mousePosition) || sideDown))
			{
				var sideInterp = Mathf.Clamp01((mp.y - sideRect.y)/sideRect.height);
				
				switch(mode)
				{
					default:
					case Mode.Hue:
						_hsb.h = 1-sideInterp;
						break;
					case Mode.Saturation:
						_hsb.s = 1-sideInterp;
						break;
					case Mode.Brightness:
						_hsb.b = 1-sideInterp;
						break;
				}
				color = _hsb.ToColor();
				
				Event.current.Use();
				
				sideDown = true;
			}
			
			if(!mainDown && !sideDown && (alphaRect.Contains(Event.current.mousePosition) || alphaDown))
			{
				var alphaInterp = Mathf.Clamp01((mp.x - alphaRect.x)/alphaRect.width);
				color.a = alphaInterp;
				
				Event.current.Use();
				alphaDown = true;
			}
		}
		
		if(Event.current.type == EventType.MouseUp && Event.current.button == 0)
		{
			sideDown = false;
			mainDown = false;
			alphaDown = false;
		}
			
		var pickerTexture = InlineColorPickerResources.pickerTexture;
		
		var pickerRect = new Rect(mainRect);
		pickerRect.width = pickerTexture.width;
		pickerRect.height = pickerTexture.height;
		
		Vector2 pickerPosition;
		float sidePickerPosition;
		
		switch(mode)
		{
			default:
			case Mode.Hue:
				pickerPosition.x = _hsb.s;
				pickerPosition.y = 1-_hsb.b;
				sidePickerPosition = 1-_hsb.h;
				break;
			case Mode.Saturation:
				pickerPosition.x = _hsb.h;
				pickerPosition.y = 1-_hsb.b;
				sidePickerPosition = 1-_hsb.s;
				break;
			case Mode.Brightness:
				pickerPosition.x = _hsb.h;
				pickerPosition.y = 1-_hsb.s;
				sidePickerPosition = 1-_hsb.b;
				break;
		}
		
		pickerRect.x += pickerPosition.x * mainTexture.width;
		pickerRect.y += pickerPosition.y * mainTexture.height;
		
		pickerRect.x -= pickerRect.width/2;
		pickerRect.y -= pickerRect.height/2;
		GUI.DrawTexture(pickerRect, pickerTexture);
		
		// Draw setting picker
		var sidePickerTexture = InlineColorPickerResources.arrow_horizontal;
		
		var sidePickerRect = sideRect;
		sidePickerRect.width = sidePickerTexture.width;
		sidePickerRect.height = sidePickerTexture.height;
		
		sidePickerRect.y += sidePickerPosition * sidebarTexture.height;
		sidePickerRect.y -= sidePickerTexture.height/2;
		GUI.DrawTexture(sidePickerRect, sidePickerTexture);
		
		sidePickerRect.x += sideRect.width;
		sidePickerRect.width = -sidePickerRect.width;
		GUI.DrawTexture(sidePickerRect, sidePickerTexture);
		
		// Draw alpha picker
		var alphaPickerTexture = InlineColorPickerResources.arrow_vertical;
		
		var alphaPickerRect = alphaRect;
		alphaPickerRect.width = alphaPickerTexture.width;
		alphaPickerRect.height = alphaPickerTexture.height;
		
		alphaPickerRect.x += alphaRect.width;
		alphaPickerRect.x -= (1-_hsb.a) * alphaTexture.width;
		alphaPickerRect.x -= alphaPickerRect.width/2;
		GUI.DrawTexture(alphaPickerRect, alphaPickerTexture);
		
		alphaPickerRect.y += alphaRect.height;
		alphaPickerRect.height = -alphaPickerRect.height;
		GUI.DrawTexture(alphaPickerRect, alphaPickerTexture);
		
//		GUI.Box(channelRect, GUIContent.none);
		
		
//		float buttonWidth = (bottomRect.width-8) / 3;
//		bottomRect.width = buttonWidth;
//		bottomRect.x -= bottomRect.width;
//		bottomRect.x -= 4;
//		Func<string, bool, bool> bottomButton = (name, state)=>{
//			bottomRect.x += buttonWidth + 4;
//			
//			GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
//			boxStyle.normal.textColor = Color.white;
//			
//			GUI.Box(bottomRect, name, boxStyle);
//			if(state) GUI.Box(bottomRect, name, boxStyle);
//			if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && bottomRect.Contains(Event.current.mousePosition))
//			{
//				state = true;
//			}
//			else
//			{
//				EditorGUIUtility.AddCursorRect(bottomRect, MouseCursor.Link);
//			}
//			return state;
//		};
//		
//		if(bottomButton("Hue", mode == Mode.Hue))
//		{
//			mode = Mode.Hue;
//		}
//		if(bottomButton("Saturation", mode == Mode.Saturation))
//		{
//			mode = Mode.Saturation;
//		}
//		if(bottomButton("Brightness", mode == Mode.Brightness))
//		{
//			mode = Mode.Brightness;
//		}
//		
//		_hsb.h = Mathf.Clamp01(_hsb.h);
//		_hsb.s = Mathf.Clamp01(_hsb.s);
//		_hsb.b = Mathf.Clamp01(_hsb.b);
		
		UpdateColor (color);
	}
	
	void UpdateColor (Color color)
	{
		if(_cachedMode != mode
		|| _cachedColor != color)
		{
			_hsb = new HSBColor(color);
			_cachedMode = mode;
			_cachedColor = color;
			
			DrawMain();
			DrawSidebar();
			
			mainTexture.Apply();
			sidebarTexture.Apply();
		}
	}
	
	public void OnDestroy ()
	{
		if(mainTexture) GameObject.DestroyImmediate(mainTexture);
		if(sidebarTexture) GameObject.DestroyImmediate(sidebarTexture);
		if(alphaTexture) GameObject.DestroyImmediate(alphaTexture);
	}
}
