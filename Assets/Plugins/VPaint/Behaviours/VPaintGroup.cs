#define HideInternals

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using Valkyrie.VPaint;
using System.Linq;

public enum AutoApplySchedule
{
	OnStart,
	OnAwake,
	Never
}

[ExecuteInEditMode]
[AddComponentMenu("VPaint/VPaint Group")]
public class VPaintGroup : MonoBehaviour//, IVPaintable
{
#if HideInternals
	[HideInInspector]
#endif
	public List<VPaintObject> colorers = new List<VPaintObject>();
	
#if HideInternals
	[HideInInspector]
#endif
	public VPaintLayerStack layerStack = new VPaintLayerStack();
	
	public AutoApplySchedule autoApplySchedule = AutoApplySchedule.OnStart;
	public bool autoLoadInEditor = true;
	
	[NonSerialized] public VPaintLayer paintLayer;
	
#if HideInternals
	[HideInInspector] 
#endif
	public List<VPaintVertexCache> vertexCache = new List<VPaintVertexCache>();
	
	public VPaintLayerStack GetLayerStack ()
	{
		return layerStack;
	}
	public VPaintObject[] GetVPaintObjects ()
	{
		for(int i = 0; i < colorers.Count; i++)
		{
			if(!colorers[i]) colorers.RemoveAt(i--);
		}
		return colorers.ToArray();
	}
	
	public VPaintLayer GetBaseLayer ()
	{
		return new VPaintLayer();
	}
	
	public void AddColorer (VPaintObject vc)
	{
		if(!vc)
		{
			return;
		}
		if(colorers.Contains(vc))
		{
			return;
		}
		if(vc.GetMeshInstance() == null)
		{
			Debug.LogError("VPaint Object is missing a mesh.");
			return;
		}
		colorers.Add(vc);
		vertexCache.Add(new VPaintVertexCache(){
			vpaintObject = vc,
			vertices = vc.GetMeshInstance().vertices
		});
#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
#endif
	}
	public void RemoveColorer (VPaintObject vc)
	{
		colorers.Remove(vc);
		vertexCache.RemoveAll( obj => obj.vpaintObject==vc );
	}
	
	void Awake ()
	{
		if(!Application.isPlaying)
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.delayCall += ()=>{
				if(autoLoadInEditor)
				{
					Apply();
				}
			};
#endif
		}
		else
		{
			paintLayer = layerStack.GetMergedLayer();
			if(autoApplySchedule == AutoApplySchedule.OnAwake) Apply();
		}
	}
	
	void Start ()
	{
		if(!Application.isPlaying) return;
		if(autoApplySchedule == AutoApplySchedule.OnStart) Apply();
	}
	
	[ContextMenu("Apply")]
	public void Apply ()
	{				
		if(paintLayer == null) paintLayer = layerStack.GetMergedLayer();
		foreach(var pd in paintLayer.paintData)
		{
			var vpd = pd.vpaintObject;
			if(!vpd) continue;
			vpd.SetColors(pd.colors);
		}
	}
	
	public void ApplyProgressive ()
	{
		LoadLayers(layerStack.layers);
	}
	
	void LoadLayers (List<VPaintLayer> layers)
	{
		foreach(var vc in colorers)
		{
			vc.colorsBuilder = null;
			vc.transparencyBuilder = null;
		}
		List<VPaintObject> affectedColorers = new List<VPaintObject>();
		foreach(VPaintLayer layer in layers)
		{
			if(!layer.enabled) continue;
			LoadLayer(layer, affectedColorers);
		}
		foreach(VPaintObject vc in affectedColorers)
		{
			vc.ApplyColorsBuilder();
		}
	}
	
	void LoadLayer (VPaintLayer layer, List<VPaintObject> affectedColorers)
	{
		foreach(var vc in colorers)
		{	
			if(!vc) continue;
			
			var paintData = layer.Get(vc);
			if(paintData != null)
			{				
				PaintObject(vc, layer, paintData);
				if(!affectedColorers.Contains(vc))
					affectedColorers.Add(vc);
			}
		}
	}	
	
	void PaintObject (VPaintObject vc, VPaintLayer layer, VPaintVertexData data)
	{
		if(vc.colorsBuilder == null)
		{
			vc.colorsBuilder = new Color[data.colors.Length];
		}
		
		if(vc.transparencyBuilder == null)
		{
			vc.transparencyBuilder = new float[data.colors.Length];
		}
		
		VPaintUtility.MergeColors(
			vc.colorsBuilder, vc.transparencyBuilder,
			data.colors, data.transparency,
			layer.blendMode, layer.opacity,
			layer.maskR, layer.maskG, layer.maskB, layer.maskA
		);		
	}
	
	public void PaintObjects (List<VPaintObject> objs)
	{
		foreach(var vc in objs)
		{
			PaintObject(vc);
		}
	}
	
	void PaintObject (VPaintObject vc)
	{
		vc.colorsBuilder = null;
		vc.transparencyBuilder = null;
		
		foreach(var layer in layerStack.layers)
		{
			if(!layer.enabled) continue;
			var data = layer.Get(vc);
			if(data == null) continue;
			PaintObject(vc, layer, data);
		}
		
		vc.ApplyColorsBuilder();
	}
	
	public void ApplyToTangents ()
	{
		List<VPaintObject> vcs = new List<VPaintObject>();
		foreach(var t in colorers)
		{
			var cols = t.GetComponentsInChildren<VPaintObject>();
			foreach(var vc in cols)
			{
				if(!vcs.Contains(vc)) vcs.Add(vc);
			}
		}
		
		foreach(var vc in vcs)
		{
			var data = paintLayer.Get(vc);
			if(data == null) continue;
			vc.SetTangents(data.colors);
		}
	}
	
	public IEnumerator BlendTo (VPaintGroup target, float time)
	{
		var myBaseLayer = paintLayer;
		var targetBaseLayer = target.paintLayer;
		
		List<IEnumerator> routines = new List<IEnumerator>();
		foreach(var obj in GetVPaintObjects())
		{
			if(!target.colorers.Contains(obj)) continue;
			
			int colorLength = obj.GetMeshInstance().colors.Length;
			
			var baseData = myBaseLayer.Get(obj);
			Color[] baseColors = null;
			if(baseData == null){
				baseColors = new Color[colorLength];
			}
			else baseColors = baseData.colors;
			
			var targetData = targetBaseLayer.Get(obj);
			Color[] targetColors = null;
			if(targetData == null){
				targetColors = new Color[colorLength];
			}
			else targetColors = targetData.colors;
			
			routines.Add(VPaintUtility.LerpColors (obj, baseColors, targetColors, time));
		}
		
		while(true)
		{
			bool b = true;
			foreach(var r in routines)
			{
				if(r.MoveNext()) b = false;
			}
			if(b) break;
			yield return null;
		}
	}
	
	
#if UNITY_EDITOR
	public void CleanupData ()
	{
		var paintObjects = GetVPaintObjects();
		var list = new List<IVPaintIdentifier>(paintObjects);
		layerStack.Sanitize(list);
		
		vertexCache.RemoveAll(obj => !obj.vpaintObject);
	}
	
	public void CacheVertices ()
	{
		vertexCache = new List<VPaintVertexCache>();
		foreach(var col in colorers)
		{
			vertexCache.Add(
				new VPaintVertexCache()
				{
					vpaintObject = col,
					vertices = col.GetMeshInstance().vertices
				}
			);
		}
	}
	public void CacheVertices (VPaintObject obj)
	{
		var cache = vertexCache.Find(c=>c.vpaintObject == obj);
		if(cache == null)
		{
			vertexCache.Add(
				new VPaintVertexCache()
				{
					vpaintObject = obj,
					vertices = obj.GetMeshInstance().vertices
				}
			);
		}
		else
		{
			cache.vertices = obj.GetMeshInstance().vertices;
		}
	}
#endif
}
