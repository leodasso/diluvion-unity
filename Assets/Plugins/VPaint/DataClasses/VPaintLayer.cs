using UnityEngine;
using System.Collections.Generic;
using System;

namespace Valkyrie.VPaint
{

	[Serializable]
	public class VPaintLayer
	{
		public bool foldout = false;
		
		public string name = "Layer";
		
		public int tag = 0;
		
		public List<VPaintVertexData> paintData = new List<VPaintVertexData>();
		
		public VPaintBlendMode blendMode = VPaintBlendMode.Opaque;
		
		public bool enabled = true;
		
		public float opacity = 1f;
		
		public bool maskR = true;
		public bool maskG = true;
		public bool maskB = true;
		public bool maskA = true;
		
		public int selectedColor = 0;
		
		public VPaintVertexData GetOrCreate (VPaintObject vc)
		{
			if(vc==null)
			{
				return null;
			}
			
			var data = Get(vc);
			
			if(data != null)
			{
				return data;
			}
			
			data = new VPaintVertexData();
			data.vpaintObject = vc;
			
			var cols = vc.GetDefaultColors();
			data.colors = new Color[cols.Length];
			data.transparency = new float[cols.Length];
			
			paintData.Add(data);
			return data;
		}
	
		public VPaintVertexData Get (IVPaintIdentifier vc)
		{
			if(vc == null)
			{
				return null;
			}
			for(int i = 0; i < paintData.Count; i++)
			{
				VPaintVertexData data = paintData[i];
				if(vc.IsEqualTo(data.identifier)) return data;
			}
			return null;
		}
		
		public void Remove (IVPaintIdentifier vc)
		{
			for(int i = 0; i < paintData.Count; i++)
			{
				VPaintVertexData data = paintData[i];
				if(data.identifier.IsEqualTo(vc))
				{
					paintData.RemoveAt(i);
					i--;
				}
			}
		}
	
		public void Flood (IEnumerable<VPaintObject> objects, Color color)
		{
			foreach(var obj in objects)
			{
				var pd = GetOrCreate(obj);
				for(int i = 0; i < pd.colors.Length; i++)
				{
					pd.transparency[i] = 1;
					pd.colors[i] = color;
				}
			}
		}
		
		public VPaintLayer Clone ()
		{
			VPaintLayer layer = new VPaintLayer();
			
			foreach(var vpd in paintData)
			{
				var v = vpd.Clone();
				layer.paintData.Add(v);
			}
			layer.blendMode = blendMode;
			layer.name = name;
			layer.tag = tag;
			layer.opacity = opacity;
			layer.enabled = enabled;
			layer.maskR = maskR;
			layer.maskG = maskG;
			layer.maskB = maskB;
			layer.maskA = maskA;
			return layer;
		}
		
		public void Merge (VPaintLayer layer, Color baseColor = new Color())
		{
			foreach(VPaintVertexData data in layer.paintData)
			{
				var rootData = Get(data.identifier);
				
				Color[] cols = null;
				float[] trans = null;
				
				if(rootData != null)
				{
					cols = rootData.colors;
					trans = rootData.transparency;
				}
				else
				{					
					cols = new Color[data.colors.Length];
					
					for(int i = 0; i < cols.Length; i++)
					{
						cols[i] = baseColor;
					}
					
					trans = new float[data.transparency.Length];
					
					rootData = new VPaintVertexData();
					rootData.colors = cols;
					rootData.transparency = trans;
					rootData.colorer = data.colorer;
					
					paintData.Add(rootData);
					
				}
				
				VPaintUtility.MergeColors(
					cols, trans,
					data.colors, data.transparency,
					layer.blendMode, layer.opacity,
					layer.maskR, layer.maskG, layer.maskB, layer.maskA
				);
			}
		}
		
		public void Apply ()
		{
			foreach(var data in paintData)
			{
				if(data.vpaintObject == null)
				{
					continue;
				}
				data.vpaintObject.SetColors(data.colors);
			}
		}
		
		public void Sanitize ()
		{
			var checkedObjects = new HashSet<UnityEngine.Object>();
			for(int i = 0; i < paintData.Count; i++)
			{
				var pd = paintData[i];
				if(pd.colorer == null
				|| checkedObjects.Contains(pd.colorer))
				{
					paintData.RemoveAt(i--);
				}
				else
				{
					checkedObjects.Add(pd.colorer);
				}
			}
		}
		
		public void Sanitize (List<IVPaintIdentifier> colorers)
		{
			for(int i = 0; i < paintData.Count; i++)
			{
				var pd = paintData[i];
				if(!pd.colorer)
				{
					paintData.RemoveAt(i--);
					continue;
				}
				bool found = false;
				var id = pd.identifier;
				for(int b = 0; b < colorers.Count; b++)
				{
					var c = colorers[b];
					if(id.IsEqualTo(c))
					{
						found = true;
						break;
					}
				}
				if(!found)
				{
					paintData.RemoveAt(i--);
					continue;
				}
			}
		}
		
		public void CleanZeroTransparency ()
		{
			for(int i = 0; i < paintData.Count; i++)
			{
				var pd = paintData[i];
				bool rem = true;
				foreach(float f in pd.transparency)
				{
					if(f != 0) rem = false;
				}
				if(rem)
				{
					paintData.RemoveAt(i--);
				}
			}
		}
		
	}
	
}