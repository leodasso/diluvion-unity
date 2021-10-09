using UnityEngine;
using System.Collections.Generic;
using System;

namespace Valkyrie.VPaint
{

	[Serializable]
	public class VPaintLayerStack
	{
		public List<VPaintLayer> layers = new List<VPaintLayer>(){new VPaintLayer()};
		public int currentLayer = 0;
		
		public void Clear ()
		{
			layers.Clear();
		}
		
		public VPaintLayerStack Clone ()
		{
			VPaintLayerStack stack = new VPaintLayerStack();
			foreach(VPaintLayer layer in layers)
				stack.layers.Add(layer.Clone());
			return stack;
		}
		
		public VPaintLayer NewLayer ()
		{
			VPaintLayer layer = new VPaintLayer();
			layers.Add(layer);
			return layer;
		}
		
		public IEnumerable<VPaintLayer> GetActiveLayers ()
		{
			foreach(VPaintLayer l in layers)
			{
				if(l.enabled) yield return l;
			}
		}
		
		public VPaintLayer GetMergedLayer ()
		{
			return GetMergedLayer(null);
		}
		
		public VPaintLayer GetMergedLayer (VPaintLayer baseLayer)
		{
			VPaintLayer merged = null;
			if(baseLayer == null)
			{
				merged = new VPaintLayer();
			}
			else
			{
				merged = baseLayer.Clone();
			}
			
			for(int i = 0; i < layers.Count; i++)
			{
				VPaintLayer layer = layers[i];
				if(!layer.enabled) continue;
				merged.Merge(layer);
			}
			return merged;
		}
		
		public void Collapse ()
		{
			layers = new List<VPaintLayer>(){GetMergedLayer()};
		}
		
		public void Sanitize ()
		{
			foreach(var layer in layers) layer.Sanitize();
		}
		
		public void Sanitize (List<IVPaintIdentifier> colorers)
		{
			foreach(var layer in layers) layer.Sanitize(colorers);
		}
	}
}