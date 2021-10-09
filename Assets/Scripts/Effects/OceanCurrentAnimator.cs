using UnityEngine;
using System.Collections;

public class OceanCurrentAnimator : MonoBehaviour {

	public Material mat1;

	public float UOffsetSpeed = 0;
    public float VOfffsetSpeed = 0;

	float uoffset = 0;
    float voffset = 0;
  
  
	// Update is called once per frame
	void Update () {

        uoffset += Time.deltaTime * UOffsetSpeed;
        voffset += Time.deltaTime * VOfffsetSpeed;

        mat1.SetTextureOffset("_MainTex", new Vector2(uoffset, voffset));	
	}
}
