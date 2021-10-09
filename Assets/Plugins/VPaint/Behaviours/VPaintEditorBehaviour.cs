using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class VPaintEditorBehaviour : MonoBehaviour 
{
	public void OnEnable ()
	{
		gameObject.hideFlags = HideFlags.HideInHierarchy;
	}
}
