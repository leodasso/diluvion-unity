using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System;
using Type = System.Type;
using Valkyrie.VPaint;

public abstract class VPaintImportTextureWindowAbstract : VPaintWindowBase
{
	public static class Settings
	{
		static int _sampleRadius = 10;
		public static int sampleRadius {
			get{ return _sampleRadius; }
			set{
				if(value != _sampleRadius)
				{
					_sampleRadius = value;
					EditorPrefs.SetInt("VP_IT_SampleRadius", _sampleRadius);
				}
			}
		}
		
		static float _importOpacity = 1.0f;
		public static float importOpacity {
			get{ return _importOpacity; }
			set{
				if(value != _importOpacity)
				{
					_importOpacity = value;
					EditorPrefs.SetFloat("VP_IT_Opacity", _importOpacity);
				}
			}
		}
		
		static float _blurRadius = 1.0f;
		public static float blurRadius {
			get{ return _blurRadius; }
			set{
				if(value != _blurRadius)
				{
					_blurRadius = value;
					EditorPrefs.SetFloat("VP_IT_BlurRadius", _blurRadius);
				}
			}
		}
		
		static float _blurThreshhold = 1.0f;
		public static float blurThreshhold {
			get{ return _blurThreshhold; }
			set{
				if(value != _blurThreshhold)
				{
					_blurThreshhold = value;
					EditorPrefs.SetFloat("VP_IT_BlurThreshhold", _blurThreshhold);
				}
			}
		}
		
		static int _swizzleR = 0;
		public static int swizzleR {
			get{ return _swizzleR; }
			set{
				if(value != _swizzleR)
				{
					_swizzleR = value;
					EditorPrefs.SetInt("VP_IT_SwizzleR", _swizzleR);
				}
			}
		}
		
		static int _swizzleG = 1;
		public static int swizzleG {
			get{ return _swizzleG; }
			set{
				if(value != _swizzleG)
				{
					_swizzleG = value;
					EditorPrefs.SetInt("VP_IT_SwizzleG", _swizzleG);
				}
			}
		}
		
		static int _swizzleB = 2;
		public static int swizzleB {
			get{ return _swizzleB; }
			set{
				if(value != _swizzleB)
				{
					_swizzleB = value;
					EditorPrefs.SetInt("VP_IT_SwizzleB", _swizzleB);
				}
			}
		}
		
		static int _swizzleA = 3;
		public static int swizzleA {
			get{ return _swizzleA; }
			set{
				if(value != _swizzleA)
				{
					_swizzleA = value;
					EditorPrefs.SetInt("VP_IT_SwizzleA", _swizzleA);
				}
			}
		}
		
		static int _targetLayer = -1;
		public static int targetLayer {
			get{ return _targetLayer; }
			set{
				if(value != _targetLayer)
				{
					_targetLayer = value;
					EditorPrefs.SetInt("VP_IT_TargetLayer", _targetLayer);
				}
			}
		}

		public static void Load ()
		{
			_sampleRadius = EditorPrefs.GetInt("VP_IT_SampleRadius", 10);
			_importOpacity = EditorPrefs.GetFloat("VP_IT_Opacity", 1);
			_blurRadius = EditorPrefs.GetFloat("VP_IT_BlurRadius", 5f);
			_blurThreshhold = EditorPrefs.GetFloat("VP_IT_BlurThreshhold", 0.5f);
			_swizzleR = EditorPrefs.GetInt("VP_IT_SwizzleR", 0);
			_swizzleG = EditorPrefs.GetInt("VP_IT_SwizzleG", 1);
			_swizzleB = EditorPrefs.GetInt("VP_IT_SwizzleB", 2);
			_swizzleA = EditorPrefs.GetInt("VP_IT_SwizzleA", 3);
			_targetLayer = EditorPrefs.GetInt("VP_IT_TargetLayer", 0);
		}
	}
	
	public bool[] objectTargets;
	
	public override void OnValidatedEnable ()
	{
		Settings.Load();
	}
	
	public override void OnValidatedDisable ()
	{
		if(VPaint.Instance) VPaint.Instance.ReloadLayers();
	}
	
	Vector2 windowScroll;
	public override void OnValidatedGUI ()	
	{				
		windowScroll = EditorGUILayout.BeginScrollView(windowScroll);
		
		VPaintGUIUtility.BeginColumnView(position.width - 48);
		
		OnSettingsGUI ();
		
		GUILayout.Space(10);
		
		OnSharedSettingsGUI();
		
		GUILayout.FlexibleSpace();
		
		EditorGUILayout.EndScrollView();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		ImportButtons();
		EditorGUILayout.EndHorizontal();
	}
	
	void OnSharedSettingsGUI ()
	{
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.wordWrap = true;
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			GUILayout.Label("The transparency value to apply to the imported colors.", style);
		});
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			Settings.importOpacity = EditorGUILayout.Slider("Import Transparency", Settings.importOpacity, 0f, 1f);
		});
		
		GUILayout.Space(10);
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			GUILayout.Label("Pixel radius to sample from for each vertex.", style);
		});
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			Settings.sampleRadius = EditorGUILayout.IntField("Pixel Sample Radius:", Settings.sampleRadius);
		});
		
		GUILayout.Space(10);
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			GUILayout.Label("Post-process blur on the imported colors.", style);
		});
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			Settings.blurRadius = EditorGUILayout.FloatField("Blur Radius", Settings.blurRadius);
		});
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			Settings.blurThreshhold = EditorGUILayout.Slider("Blur Threshhold", Settings.blurThreshhold, 0f, 1f);
		});
		
		GUILayout.Space(10);
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			GUILayout.Label("Channel Assignments", style);
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Reset"))
			{
				Settings.swizzleR = 0;
				Settings.swizzleG = 1;
				Settings.swizzleB = 2;
				Settings.swizzleA = 3;
			}
			
			GUILayout.Space(5);
		});
		
		Func<GUIContent, int, int> getSwizzle = (s, c)=>{
			return EditorGUILayout.Popup(s, c+1, 
				new GUIContent[]{
					new GUIContent("Disabled"),
					new GUIContent("Red"),
					new GUIContent("Green"),
					new GUIContent("Blue"),
					new GUIContent("Alpha")
				}
			)-1;
		};
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{				
			Settings.swizzleR = getSwizzle(new GUIContent("Red"), Settings.swizzleR);
		});
		VPaintGUIUtility.DrawColumnRow(24, ()=>{				
			Settings.swizzleG = getSwizzle(new GUIContent("Green"), Settings.swizzleG);
		});
		VPaintGUIUtility.DrawColumnRow(24, ()=>{				
			Settings.swizzleB = getSwizzle(new GUIContent("Blue"), Settings.swizzleB);
		});
		VPaintGUIUtility.DrawColumnRow(24, ()=>{				
			Settings.swizzleA = getSwizzle(new GUIContent("Alpha"), Settings.swizzleA);
		});
		
		GUILayout.Space(10);
		
		VPaintGUIUtility.DrawColumnRow(24, ()=>{
			Settings.targetLayer = 
				EditorGUILayout.Popup("Target Layer", Settings.targetLayer, 
					new string[]{"New Layer","Selected Layer"});
		});
	}
	
	public void ImportButtons ()
	{
		GUI.enabled = AllowImport();
		if(GUILayout.Button("Preview"))
		{
			ImportTexture(true);
		}
		if(GUILayout.Button("Import"))
		{
			ImportTexture(false);
		}
	}
	
		
	public virtual void OnSettingsGUI () {}
	public virtual bool IsValid (VPaintObject vc)
	{
		return true;
	}
	public virtual Texture2D GetTexture (VPaintObject vc)
	{
		return null;
	}
	public virtual Vector2[] GetUVs (Mesh mesh) 
	{
		return mesh.uv;
	}
	public virtual bool AllowImport ()
	{
		return true;
	}
	public virtual Color PostProcessColor (Color c)
	{
		return c;
	}
	public virtual Func<Vector2, Vector2> GetUVTransformation (VPaintObject vc)
	{
		return (v)=>{return v;};
	}
	
	public void ImportTexture (bool preview)
	{
		try
		{
			var objectSizes = new Dictionary<VPaintObject, float>();
			float objectMax = 0f;
	
			foreach(VPaintObject vc in VPaint.Instance.maskedVPaintObjects())
			{
				if(!vc) continue;
				Bounds b = vc.GetMeshInstance().bounds;
				float size = (b.max - b.min).magnitude;
				objectMax = Mathf.Max(objectMax, size);
				objectSizes.Add(vc, size);
			}
			
			bool overwriteExistingLayer = Settings.targetLayer == 1;
			
			VPaintLayer layer = overwriteExistingLayer ? currentLayer.Clone() : new VPaintLayer();
			List<VPaintVertexData> paintData = new List<VPaintVertexData>();
			foreach(var vc in VPaint.Instance.maskedVPaintObjects())
			{
				if(!vc) continue;
				if(!IsValid(vc)) continue;
				
				Texture2D tx = GetTexture(vc);
				
				string texPath = AssetDatabase.GetAssetPath(tx);
				TextureImporter importer = TextureImporter.GetAtPath(texPath) as TextureImporter;
				if(!importer.isReadable)
				{
					importer.isReadable = true;
					AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
				}
				
				VPaintVertexData pd = new VPaintVertexData();
				Mesh m = vc.GetMeshInstance();
				Vector2[] uvs = GetUVs(m);
				pd.colors = new Color[uvs.Length];
				pd.transparency = new float[uvs.Length];
				Vector2 txSize = new Vector2(tx.width, tx.height);
				Func<Vector2, Vector2> uvTransformation = GetUVTransformation(vc);
				
				for(int i = 0; i < pd.colors.Length; i++)
				{
					if(EditorUtility.DisplayCancelableProgressBar("Sampling...", "Sampling texture onto '" + vc.name + "'", (float)i/pd.colors.Length))
					{
						EditorUtility.ClearProgressBar();
						return;
					}
					
					Vector2 uv = uvs[i];
					Vector2 normalizedPos = uvTransformation(uv);
					Vector2 pixelPos = Vector2.Scale(normalizedPos, txSize);
					var samplePositions = new List<Vector2>();
					int sampleRadius = Settings.sampleRadius;
					sampleRadius = Mathf.RoundToInt(sampleRadius * Mathf.Clamp01(objectSizes[vc]/objectMax));
					if(sampleRadius == 0)
					{
						samplePositions.Add(pixelPos);
					}
					else
					{
						for(int r = -sampleRadius; r <= sampleRadius; r++)
						{
							float f = Mathf.Cos((((r / sampleRadius) + 1)/2) * Mathf.PI);
							int div = Mathf.RoundToInt(f * sampleRadius);
							for(int y = -div; y <= div; y++)
							{
								samplePositions.Add(pixelPos + new Vector2(r, y));
							}
						}
					}
					float cr = 0;
					float cg = 0;
					float cb = 0;
					float ca = 0;
					foreach(Vector2 v in samplePositions)
					{
						Color c = tx.GetPixelBilinear((int)v.x/txSize.x, (int)v.y/txSize.y);
						cr += c.r;
						cg += c.g;
						cb += c.b;
						ca += c.a;
					}
					int count = samplePositions.Count;
					cr /= count;
					cg /= count;
					cb /= count;
					ca /= count;
					
					Color col = new Color();
					if(Settings.swizzleR != -1) col[Settings.swizzleR] = cr;
					if(Settings.swizzleG != -1) col[Settings.swizzleG] = cg;
					if(Settings.swizzleB != -1) col[Settings.swizzleB] = cb;
					if(Settings.swizzleA != -1) col[Settings.swizzleA] = ca;
					
					col = PostProcessColor(col);
					
					pd.colors[i] = col;
					pd.transparency[i] = Settings.importOpacity;
				}
				
				pd.vpaintObject = vc;
				
				paintData.Add(pd);
			}
			if(paintData.Count != 0)
			{
				layer.paintData = paintData;
			}
			
			if(Settings.blurRadius != 0)
			{
				var colorsToApply = new Dictionary<VPaintVertexData, Color[]>();
				foreach(var vc in VPaint.Instance.maskedVPaintObjects())
				{
					var vcrenderer = vc.GetComponent<Renderer>();
					if(!vcrenderer) continue;
					var data = layer.Get(vc);
					if(data == null) continue;
					
					var colors = new Color[data.colors.Length];
					var verts = vc.GetMeshInstance().vertices;
					
					var checkBounds = vcrenderer.bounds;
					checkBounds.Expand(Settings.blurRadius);
					
					var checkColorers = new List<VPaintVertexData>();
					foreach(var vc2 in VPaint.Instance.maskedVPaintObjects())
					{
						var vc2renderer = vc2.GetComponent<Renderer>();
						if(!vc2renderer) continue;
						
						var b = vc2renderer.bounds;
						b.Expand(Settings.blurRadius);
						if(!checkBounds.Intersects(b)) continue;
						
						var data2 = layer.Get(vc2);
						if(data2 == null) continue;
						
						checkColorers.Add(data2);
					}
					
					for(int i = 0; i < colors.Length; i++)
					{
						EditorUtility.DisplayProgressBar("Blurring...", "Blurring imported colors on '" + vc.name + "'", (float)i/colors.Length);
						
						var color = data.colors[i];
						var colorV4 = new Vector4(color.r, color.g, color.b, color.a);
						
						Vector4 composite = colorV4;
						float factor = 1f;
						
						var v = verts[i];
		
						foreach(var data2 in checkColorers)
						{
//							var vc2 = data2.colorer;
//							var verts2 = vc2.GetMeshInstance().vertices;
							var vp = data2.vpaintObject as VPaintObject;
							if(!vp) continue;
							var verts2 = vp.GetVertices();
							var colors2 = data2.colors;
							
							for(int d = 0; d < colors2.Length; d++)
							{
								var v2 = verts2[d];
								var dist = Vector3.Distance(v, v2);
								
								if(Settings.blurRadius < dist) continue;
								
								float fac = 1-dist/Settings.blurRadius;
								
								var col = colors2[d];
								var colV4 = new Vector4(col.r, col.g, col.b, col.a);
								
								float dot = Vector4.Dot(colorV4, colV4);
								
								fac *= 1-Mathf.Clamp01(Settings.blurThreshhold/dot);
								
								composite += colV4 * fac;
								factor += fac;
							}
						}
						colors[i] = (Color)(composite / factor);
					}
					
					colorsToApply.Add(data, colors);
				}
				
				foreach(var kvp in colorsToApply)
					kvp.Key.colors = kvp.Value;
			}
			
			VPaint.Instance.PushUndo("Import Texture");
			layer.blendMode = VPaintBlendMode.Opaque;
			if(preview) VPaint.Instance.LoadLayer(layer);
			else{
				if(overwriteExistingLayer)
				{
					VPaint.Instance.layerStack.layers[VPaint.Instance._currentPaintLayer] = layer;
				}
				else
				{
					VPaint.Instance.layerStack.layers.Add(layer);
				}
				VPaint.Instance.ReloadLayers();
			}
		}
		finally
		{
			EditorUtility.ClearProgressBar();
		}
	}
}