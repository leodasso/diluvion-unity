using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using System.Collections;
using System.IO;

public class PixelPivot : EditorWindow {
	public static EditorWindow win;
	bool pivotIsSet;
	int pivotX;
	int pivotY;
	Sprite selectedSprite;
	float previewScale;
	float offsetX = 5f;
	float offsetY = 5f;
	float time;
	float colorWave;
	bool pivotInPixelPos;

	[MenuItem("Window/TileTool2D/Pixel Pivot")]
	public static void ShowWindow() {
		win = EditorWindow.GetWindow(typeof(PixelPivot));
		win.titleContent = new GUIContent("Pixel Pivot");
		win.minSize = new Vector2(200.0f, 200.0f);
	}

	void OnGUI(){
		if (SelectSprite()){
			GUIPreviewTexture();
			if(CheckMousePosition()){
				GUIMousePos();
				OnMouseClick();
			}
			GUINewPivot();
			GUICurrentPivot();
			SetPreviewScale();
			ColorWave();
		}else{
			GUI.enabled = true;
			GUINoSpriteSelected();
		}
		EditorGUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		GUIPivotWarning();
		GUISetPivotButton();
		EditorGUILayout.EndVertical();
	}

	void GUISetPivotButton(){
		EditorGUILayout.BeginHorizontal();
		if(selectedSprite == null) GUI.enabled = false;
		GUIStyle smallButton = new GUIStyle(GUI.skin.button);
		smallButton.fixedHeight = 16.0f;
		smallButton.fontSize = 9;
		smallButton.fixedWidth = 150f;
		if(GUI.enabled) GUI.color = Color.cyan;
		GUILayout.FlexibleSpace();
		if(GUILayout.Button("SET PIVOT POSITION", smallButton)) SetPivotPosition();
		EditorGUILayout.EndHorizontal();
	}


	void GUIPivotWarning(){
		if(selectedSprite !=null && !pivotInPixelPos){
			EditorGUILayout.BeginHorizontal("Box");
			GUIStyle lab = new GUIStyle();
			lab.alignment = TextAnchor.MiddleCenter;
			GUILayout.Label("<color=red><b>!!! PIVOT IS NOT IN PIXEL POSITION !!!</b></color>", lab);
			EditorGUILayout.EndHorizontal();
		}
	}

	void Update(){
		Repaint();
	}

	void DrawIndicator(float x, float y, Color col, Color col2, string shape = "Box"){
		pivotInPixelPos = false;
		if(x == Mathf.Floor(x) && y == Mathf.Floor(y)) pivotInPixelPos = true;
		x = offsetX + x * previewScale;
		y = (selectedSprite.rect.height - 1 - y)  * previewScale + offsetY;
		Color c = Color.Lerp(col, col2, colorWave);
		if(!pivotInPixelPos){ 
			c = Color.Lerp(Color.white, Color.red, colorWave);
			shape = "Cross";
		}
		DrawShape(new Rect(x, y, previewScale, previewScale), c, shape);
	}
	
	void DrawShape(Rect pos, Color col, string shape = "Box"){
		Texture2D t = new Texture2D(3, 3);
		if(shape == "Box"){
			t.SetPixel(0, 0, col);
			t.SetPixel(1, 0, col);
			t.SetPixel(2, 0, col);
			t.SetPixel(0, 1, col);
			t.SetPixel(1, 1, Color.clear);
			t.SetPixel(2, 1, col);
			t.SetPixel(0, 2, col);
			t.SetPixel(1, 2, col);
			t.SetPixel(2, 2, col);
		}
		else if(shape == "Cross"){
			t.SetPixel(0, 0, col);
			t.SetPixel(1, 0, Color.clear);
			t.SetPixel(2, 0, col);
			t.SetPixel(0, 1, Color.clear);
			t.SetPixel(1, 1, col);
			t.SetPixel(2, 1, Color.clear);
			t.SetPixel(0, 2, col);
			t.SetPixel(1, 2, Color.clear);
			t.SetPixel(2, 2, col);
		}
		else if(shape == "Plus"){
			t.SetPixel(0, 0, Color.clear);
			t.SetPixel(1, 0, col);
			t.SetPixel(2, 0, Color.clear);
			t.SetPixel(0, 1, col);
			t.SetPixel(1, 1, col);
			t.SetPixel(2, 1, col);
			t.SetPixel(0, 2, Color.clear);
			t.SetPixel(1, 2, col);
			t.SetPixel(2, 2, Color.clear);
		}
		t.filterMode = FilterMode.Point;
		t.Apply();
		GUI.DrawTexture(pos, t);
		DestroyImmediate(t);
	}



	void ColorWave(){
		colorWave = (Mathf.Sin(time * 2) + 1f) / 2.0f;
		time += 0.02f;
	}

	void SetPreviewScale(){
		float s = (position.height - offsetY*2 -40) / selectedSprite.rect.height;
		previewScale = s;
	}

	void GUINoSpriteSelected(){
		EditorGUILayout.BeginHorizontal("Box");
		GUIStyle lab = new GUIStyle();
		lab.wordWrap = true;
		lab.richText = true;
		GUILayout.Label("Please select a <b>Sprite</b> in the <b>Project Window</b>.\n\n<color=#808080ff><b>About</b>\nThis tool allows you to assign a pixel position to the sprite pivot point.</color>", lab);
		EditorGUILayout.EndHorizontal();
	}

	bool SelectSprite(){
		if (Selection.activeObject != null && Selection.activeObject is UnityEngine.Sprite){
			if (selectedSprite != (Sprite)Selection.activeObject){
				selectedSprite = (Sprite)Selection.activeObject;
				pivotIsSet = false;
			}
			return true;
		}
		selectedSprite = null;	
		return false;
	}

	void GUIPreviewTexture(){
		float w = selectedSprite.rect.width * previewScale;
		float h = selectedSprite.rect.height * previewScale;
		Rect texCoords = new Rect(	selectedSprite.textureRect.x / selectedSprite.texture.width, 
									selectedSprite.textureRect.y / selectedSprite.texture.height, 
									selectedSprite.textureRect.width / selectedSprite.texture.width, 
		                          	selectedSprite.textureRect.height / selectedSprite.texture.height);
		Rect texPos = new Rect(offsetX, offsetY, w, h);
		GUI.DrawTextureWithTexCoords(texPos, selectedSprite.texture, texCoords);
	}

	void GUIMousePos(){
		Vector2 pos = GetMousePixel();
		DrawIndicator(pos.x, selectedSprite.rect.height - 1 - pos.y,  Color.black, Color.white, "Plus");
	}

	Vector2 GetMousePixel(){
		return new Vector2((int)((Event.current.mousePosition.x - offsetX) / previewScale), 
		                   (int)((Event.current.mousePosition.y - offsetY) / previewScale));
	}

	void GUINewPivot(){
		if (!pivotIsSet) return;
		DrawIndicator(pivotX, pivotY, Color.cyan,  Color.white);
	}

	void GUICurrentPivot(){
		float x = selectedSprite.pivot.x;
		float y = selectedSprite.pivot.y;
		DrawIndicator(x, y, Color.green, Color.white, "Cross");
	}

	bool CheckMousePosition(){
		if(Event.current.mousePosition.x > offsetX && Event.current.mousePosition.y > offsetY && 
		   Event.current.mousePosition.x < offsetX + (selectedSprite.rect.width*previewScale) && 
		   Event.current.mousePosition.y < offsetY + (selectedSprite.rect.height*previewScale)){
			return true;
		}
		return false;
	}

	void OnMouseClick(){
		if (Event.current.isMouse){
			Vector2 pos = GetMousePixel();
			pivotIsSet = true;
			pivotX = (int)pos.x;
			pivotY = (int)selectedSprite.rect.height - 1 - (int)pos.y;
		}
	}

	void SetPivotPosition(){
		string spritePath = AssetDatabase.GetAssetPath(selectedSprite.texture);
		TextureImporter textureImporter = AssetImporter.GetAtPath(spritePath) as TextureImporter;
		SpriteMetaData[] sheet = textureImporter.spritesheet;
		bool cacheRead = textureImporter.isReadable;
		for (int i = 0; i < sheet.Length; i++){
			if (sheet[i].name == selectedSprite.name){
				textureImporter.isReadable = true;
				SpriteMetaData meta = sheet[i];
				meta.alignment = (int)SpriteAlignment.Custom;
				meta.pivot = new Vector2(pivotX / (float)selectedSprite.rect.width, pivotY / (float)selectedSprite.rect.height);
				sheet[i] = meta;
				textureImporter.spritesheet = sheet;
				textureImporter.isReadable = false;
				AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
				break;
			}
		}
		textureImporter.isReadable = cacheRead;
		pivotIsSet = false;
	}
}