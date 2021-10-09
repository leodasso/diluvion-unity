using UnityEngine;
using System;

namespace Valkyrie.VPaint
{
	[Serializable]
	public class VPaintVertexCache
	{
		public UnityEngine.Object obj;
		public Vector3[] vertices;
		
		public VPaintObject vpaintObject {
			get{ return obj as VPaintObject; }
			set{ obj = value; }
		}
	}
}