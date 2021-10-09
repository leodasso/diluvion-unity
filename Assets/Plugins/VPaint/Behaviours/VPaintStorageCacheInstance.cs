using UnityEngine;
using System.Collections;

public class VPaintStorageCacheInstance : MonoBehaviour 
{
	public bool isDirty = false;
	public VPaintStorageCache vpaintStorageCache;
	public VPaintGroup vpaintGroup;
	
#if UNITY_EDITOR
	[ContextMenu("Apply")]
	public void Apply ()
	{
		vpaintStorageCache.CachePaintGroup(vpaintGroup);
		isDirty = false;
	}
#endif
}
