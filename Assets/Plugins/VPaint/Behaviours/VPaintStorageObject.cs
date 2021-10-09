using UnityEngine;
using Valkyrie.VPaint;

public class VPaintStorageObject : ScriptableObject, IVPaintIdentifier
{
	public virtual void OnEnable ()
	{
		hideFlags = HideFlags.HideInHierarchy;
	}
	public virtual bool IsEqualTo (IVPaintIdentifier obj)
	{
		return false;
	}
}
