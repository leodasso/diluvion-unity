using UnityEngine;
using System.Collections;

public class CameraRender : MonoBehaviour {

	// Use this for initialization
	void Awake () 
	{
		GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;

	}
	

}
