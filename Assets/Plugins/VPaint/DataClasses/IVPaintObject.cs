using UnityEngine;
using System.Collections;

namespace Valkyrie.VPaint
{
	public interface IVPaintIdentifier
	{
//		void SetColors (Color[] colors);
		bool IsEqualTo (IVPaintIdentifier obj);
	}
}
