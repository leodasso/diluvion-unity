using UnityEngine;
using UnityEditor;

namespace Valkyrie.VPaint
{	
	public class VertexEditorActionEditor 
	{		
		public static void OnGUI (VPaintLayerAction action, VPaintActionType type)
		{
			switch(type)
			{
				case VPaintActionType.Brightness:
					BrightnessGUI(action);
					break;
				case VPaintActionType.HueShift:
					HueShiftGUI(action);
					break;
				case VPaintActionType.Saturation:
					SaturationGUI(action);
					break;
				case VPaintActionType.OpacityAdjustment:
					OpacityGUI(action);
					break;
				case VPaintActionType.Contrast:
					ContrastGUI(action);
					break;
				case VPaintActionType.TintColor:
					TintColorGUI(action);
					break;
			}
		}
		
		public static void BrightnessGUI (VPaintLayerAction action)
		{
			VPaintGUIUtility.DrawColumnRow(24, ()=>{
				action.brightnessAdjustment = EditorGUILayout.Slider("Brightness", action.brightnessAdjustment, 0, 2);
			});
		}
		
		public static void HueShiftGUI (VPaintLayerAction action)
		{
			VPaintGUIUtility.DrawColumnRow(24, ()=>{
				action.hueAdjustment = EditorGUILayout.Slider("Hue", action.hueAdjustment, 0, 360);
			});
		}
		
		public static void SaturationGUI (VPaintLayerAction action)
		{
			VPaintGUIUtility.DrawColumnRow(24, ()=>{
				action.saturationAdjustment = EditorGUILayout.Slider("Saturation", action.saturationAdjustment, 0, 2);
			});
		}
		
		public static void OpacityGUI (VPaintLayerAction action)
		{
			VPaintGUIUtility.DrawColumnRow(24, ()=>{
				action.opacityAdjustment = EditorGUILayout.Slider("Opacity", action.opacityAdjustment, 0, 2);
			});
		}
		
		public static void ContrastGUI (VPaintLayerAction action)
		{
			VPaintGUIUtility.DrawColumnRow(24, ()=>{
				action.contrastAdjustment = EditorGUILayout.Slider("Contrast", action.contrastAdjustment, 0, 2f);
			});
			VPaintGUIUtility.DrawColumnRow(24, ()=>{
				action.contrastThreshhold = EditorGUILayout.Slider("Treshhold", action.contrastThreshhold, 0, 1f);
			});
		}
		
		public static void TintColorGUI (VPaintLayerAction action)
		{
			VPaintGUIUtility.DrawColumnRow(24, ()=>{
				action.tintColor = EditorGUILayout.ColorField("Tint Color", action.tintColor);
			});
			VPaintGUIUtility.DrawColumnRow(24, ()=>{
				action.tintColorOpacity = EditorGUILayout.Slider("Opacity", action.tintColorOpacity, 0f, 1f);
			});
			
			EditorGUILayout.BeginHorizontal();
			VPaintGUIUtility.DrawColumnRow(24, ()=>{
				action.tintUseValue = EditorGUILayout.Toggle("Mask By Value", action.tintUseValue);
			},
			()=>{
				action.tintInvertUseValue = EditorGUILayout.Popup(action.tintInvertUseValue ? 0 : 1,
														new string[] { "Screen", "Multiply" }) == 0;
			});
			EditorGUILayout.EndHorizontal();
		}
		
	}
}