using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sirenix.OdinInspector;
[ExecuteInEditMode]
public class EnableDepthBuffer : MonoBehaviour {

    [Button()]
    void EnableDepth()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }

    public Material mat;


    void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source,destination,mat);
      
    }
}
