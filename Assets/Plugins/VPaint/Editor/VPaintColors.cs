using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.IO;

namespace Valkyrie.VPaint
{
	public static class VertexEditorColors
	{
		const int maxColors = 10;
		public static List<Color> colors = new List<Color>();
		public static void Deserialize ()
		{
			colors = new List<Color>();
			for(int i = 0; i < maxColors; i++)
			{
				float r = EditorPrefs.GetFloat("VP_Colors_"+i+"R", 1f);
				float g = EditorPrefs.GetFloat("VP_Colors_"+i+"G", 1f);
				float b = EditorPrefs.GetFloat("VP_Colors_"+i+"B", 1f);
				float a = EditorPrefs.GetFloat("VP_Colors_"+i+"A", 1f);
				colors.Add(new Color(r,g,b,a));
			}		
		}
		public static void Serialize ()
		{
			for(int i = 0; i < colors.Count; i++)
			{
				var col = colors[i];
				EditorPrefs.SetFloat("VP_Colors_"+i+"R", col.r);
				EditorPrefs.SetFloat("VP_Colors_"+i+"G", col.g);
				EditorPrefs.SetFloat("VP_Colors_"+i+"B", col.b);
				EditorPrefs.SetFloat("VP_Colors_"+i+"A", col.a);
			}
		}
	}
}