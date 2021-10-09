using UnityEngine;
using System;

namespace Valkyrie.VPaint
{

	[Serializable]
	public class VPaintVertexData
	{
		public UnityEngine.Object colorer;
		
		public IVPaintIdentifier identifier {
			get{ return colorer as IVPaintIdentifier; }
			set{ colorer = value as UnityEngine.Object; }
		}
		public VPaintObject vpaintObject {
			get{ return colorer as VPaintObject; }
			set{ colorer = value as UnityEngine.Object; }
		}
		
		public Color[] colors;
		public float[] transparency;
		
		public VPaintVertexData Clone ()
		{
			VPaintVertexData data = new VPaintVertexData();
			data.colorer = colorer;
			data.colors = new Color[colors.Length];
			data.transparency = new float[colors.Length];
			
			for(int i = 0; i < colors.Length; i++)
			{
				data.colors[i] = colors[i];
				data.transparency[i] = transparency[i];
			}
			
			return data;
		}
	}
	
}