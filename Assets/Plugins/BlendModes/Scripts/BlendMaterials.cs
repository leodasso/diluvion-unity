using UnityEngine;
using System.Collections.Generic;

namespace BlendModes
{
	/// <summary>
	/// Holds blend material data.
	/// </summary>
	public struct BlendMaterial
	{
		public Material Material;

		public ObjectType ObjectType;
		public RenderMode RenderMode;
		public BlendMode BlendMode;

		public BlendMaterial (ObjectType objectType, RenderMode renderMode, BlendMode blendMode)
		{
			this.Material = null;
			this.ObjectType = objectType;
			this.RenderMode = renderMode;
			this.BlendMode = blendMode;
		}

		public bool IsEqual (BlendMaterial mat)
		{
			return mat.ObjectType == ObjectType && mat.RenderMode == RenderMode && mat.BlendMode == BlendMode;
		}
	}

	/// <summary> 
	/// Manages all the materials used for blending and provides caching.
	/// </summary> 
	public static class BlendMaterials
	{
		private static List<BlendMaterial> cachedMaterials = new List<BlendMaterial>();

		/// <summary>
		/// Looks for suitable cached material and creates a new one if needed.
		/// </summary>
		public static Material GetMaterial (ObjectType objectType, RenderMode renderMode = RenderMode.Grab, 
			BlendMode blendMode = BlendMode.Normal, Shader shader = null)
		{
			// Framebuffer won't work in the editor, so fallback to Grab mode.
			if (Application.isEditor && renderMode == RenderMode.Framebuffer) renderMode = RenderMode.Grab;

			var blendMataterial = new BlendMaterial(objectType, renderMode, blendMode);

			// Disable caching for mesh and particle materials, as they are sharing them.
			if (objectType != ObjectType.MeshDefault && objectType != ObjectType.ParticleDefault && cachedMaterials.Exists(m => m.IsEqual(blendMataterial)))
				return cachedMaterials.Find(m => m.IsEqual(blendMataterial)).Material;
			else
			{
				var mat = new Material(shader ? shader : Resources.Load<Shader>(string.Format("BlendModes/{0}/{1}", objectType, renderMode)));
				mat.hideFlags = HideFlags.HideAndDontSave;
				mat.EnableKeyword("Bm" + blendMode.ToString());

				blendMataterial.Material = mat;

				cachedMaterials.Add(blendMataterial);

				return mat;
			}
		}
	}
}
