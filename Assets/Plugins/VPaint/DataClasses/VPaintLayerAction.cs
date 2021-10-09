using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Valkyrie.VPaint
{
	
	public enum VPaintActionType
	{
		HueShift,
		Saturation,
		Brightness,
		OpacityAdjustment,
		Contrast,
		TintColor
	}
	
	[Serializable]
	public class VPaintLayerAction
	{	
		public float hueAdjustment = 0f;
		
		public float saturationAdjustment = 1f;
		
		public float brightnessAdjustment = 1f;
		
		public float opacityAdjustment = 1f;
		
		public float contrastAdjustment = 1f;
		public float contrastThreshhold = 0.5f;
		
		public Color tintColor = Color.yellow;
		public float tintColorOpacity = 1f;
		public bool tintUseValue = false;
		public bool tintInvertUseValue = false;
				
		public void Apply (ref Color c, ref float t, VPaintActionType type)
		{
			HSBColor hsb = new HSBColor(c);
			switch(type)
			{
				case VPaintActionType.Brightness:
					if(brightnessAdjustment < 1f)
					{
						hsb.b = Mathf.Lerp(0, hsb.b, brightnessAdjustment);
					}
					else if(1f < brightnessAdjustment)
					{
						hsb.b = Mathf.Lerp(hsb.b, 1f, brightnessAdjustment-1f);
					}
					c = hsb.ToColor();
					break;
				case VPaintActionType.Saturation:
					hsb.s *= saturationAdjustment;
					c = hsb.ToColor();
					break;
				case VPaintActionType.HueShift:
					hsb.h += hueAdjustment/360f;
					if(1f < hsb.h) hsb.h--;
					c = hsb.ToColor();
					break;
				case VPaintActionType.Contrast:
					if(1 < contrastAdjustment)
					{
						c.a = Mathf.Pow(c.a+contrastThreshhold, contrastAdjustment)-contrastThreshhold;
						c.r = Mathf.Pow(c.r+contrastThreshhold, contrastAdjustment)-contrastThreshhold;
						c.g = Mathf.Pow(c.g+contrastThreshhold, contrastAdjustment)-contrastThreshhold;
						c.b = Mathf.Pow(c.b+contrastThreshhold, contrastAdjustment)-contrastThreshhold;
					}
					else
					{
						c = Color.Lerp(Color.grey, c, contrastAdjustment);
					}
					break;
				case VPaintActionType.TintColor:
					float tintOpa = tintColorOpacity;
					if(tintUseValue)
					{
						if(tintInvertUseValue)
							tintOpa *= hsb.b;
						else
							tintOpa *= 1 - hsb.b;
					}
					c = Color.Lerp(c, tintColor, tintOpa);
					break;
				case VPaintActionType.OpacityAdjustment:
					t = Mathf.Clamp01(t * opacityAdjustment);
					break;
			}
		}
		
		public void ApplyTo(VPaintLayer layer, params VPaintActionType[] types)
		{
			foreach(VPaintVertexData data in layer.paintData)
			{
				Color[] colors = data.colors;
				float[] transparency = data.transparency;
				for(int i = 0; i < colors.Length; i++)
				{
					foreach(var t in types)
						Apply(ref colors[i], ref transparency[i], t);
				}
				data.colors = colors;
			}
		}
	}
	
}