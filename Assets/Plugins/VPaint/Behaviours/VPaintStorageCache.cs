using UnityEngine;
using Valkyrie.VPaint;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class VPaintStorageCache : ScriptableObject
{
	public VPaintLayerStack layerStack = new VPaintLayerStack();
	[HideInInspector][SerializeField] public List<VPaintStorageObject> storageObjects = new List<VPaintStorageObject>();
	[HideInInspector][SerializeField] public List<VPaintVertexCache> vertexCache = new List<VPaintVertexCache>();
	
	public VPaintGroup CreatePaintGroup (string name, bool copyVertexCache = true)
	{
		var paintGroup = new GameObject(name).AddComponent<VPaintGroup>();
		var stack = layerStack.Clone();
		var objects = new List<VPaintObject>();
		objects.AddRange(GetAllVPaintObjects());
		foreach(var layer in stack.layers)
		{
			foreach(var data in layer.paintData)
			{
				var storageObject = data.colorer as VPaintStorageObject;
				var vpaintObject = GetVPaintObject(storageObject);
				if(!vpaintObject)
				{
					continue;
				}
				
				data.vpaintObject = vpaintObject;
				if(!objects.Contains(vpaintObject)) objects.Add(vpaintObject);
			}
		}
		
		if(copyVertexCache)
		{
			foreach(var cache in vertexCache)
			{
				var vpaintObject = GetVPaintObject(cache.obj as VPaintStorageObject);
				if(!vpaintObject) continue;
				paintGroup.vertexCache.Add(new VPaintVertexCache()
				{
					vpaintObject = vpaintObject,
					vertices = cache.vertices
				});
			}
		}
		
		var instance = paintGroup.gameObject.AddComponent<VPaintStorageCacheInstance>();
		instance.vpaintStorageCache = this;
		instance.vpaintGroup = paintGroup;
		
		foreach(var obj in objects)
		{
			paintGroup.AddColorer(obj);
		}
		paintGroup.layerStack = stack;
		
		return paintGroup;
	}	
	
	public virtual VPaintObject[] GetAllVPaintObjects ()
	{
		return null;
	}
	public virtual VPaintObject GetVPaintObject (VPaintStorageObject obj)
	{
		return null;
	}
	
#if UNITY_EDITOR	
	public virtual void CachePaintGroup (VPaintGroup grp)
	{
		foreach(var obj in storageObjects)
		{
			GameObject.DestroyImmediate(obj, true);
		}
		storageObjects.Clear();
		
		layerStack = grp.layerStack.Clone();
		
		try
		{
			var swaps = new Dictionary<VPaintObject, VPaintStorageObject>();
			for(int i = 0; i < grp.colorers.Count; i++)
			{
				EditorUtility.DisplayProgressBar("Caching VPaint Data", "Transferring color data", (float)i/grp.colorers.Count);
				var obj = grp.colorers[i];
				var t = CreateStorageObject(obj);
				AssetDatabase.AddObjectToAsset(t, this);
				swaps.Add(obj, t);
				storageObjects.Add(t);
			}
			
			for(int i = 0; i < layerStack.layers.Count; i++)
			{
				float offset = (float)i/layerStack.layers.Count;
				float factor = ((float)(i+1)/layerStack.layers.Count)-offset;
				var layer = layerStack.layers[i];
				for(int d = 0; d < layer.paintData.Count; d++)
				{
					EditorUtility.DisplayProgressBar("Caching VPaint Data", "Updating layer data", offset + ((float)d/layer.paintData.Count)*factor);
					var data = layer.paintData[d];
					var vp = data.vpaintObject as VPaintObject;
					if(!vp) continue;
					if(!swaps.ContainsKey(vp))
					{
						data.vpaintObject = null;
						continue;
					}
					data.colorer = swaps[vp];
				}
			}
			
			for(int i = 0; i < grp.vertexCache.Count; i++)
			{
				EditorUtility.DisplayProgressBar("Copying Vertex Cache", "Copying cache " + i, (float)i/grp.vertexCache.Count);
				var cache = grp.vertexCache[i];
				var t = CreateStorageObject(cache.vpaintObject);
				AssetDatabase.AddObjectToAsset(t, this);				
				storageObjects.Add(t);
				vertexCache.Add(new VPaintVertexCache()
				{
					obj = t,
					vertices = cache.vertices
				});
			}
			
		}finally{
			EditorUtility.ClearProgressBar();
		}
		
		EditorUtility.SetDirty(this);
	}
	public virtual VPaintStorageObject CreateStorageObject (VPaintObject source)
	{
		return null;
	}
#endif
}
