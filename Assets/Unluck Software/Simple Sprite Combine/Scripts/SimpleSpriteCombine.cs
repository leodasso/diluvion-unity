/****************************************
	Simple Sprite Combine
	Copyright 2016 Unluck Software	
 	www.chemicalbliss.com
*****************************************/

using UnityEngine;

[AddComponentMenu("Simple Sprite Combine")]
public class SimpleSpriteCombine : MonoBehaviour {
#if UNITY_EDITOR

	//public GameObject combineHolder;

	public GameObject combined;                     //Stores the combined mesh gameObject
	public bool _canGenerateLightmapUV;
	public int vCount;
	public bool keepStructure = true;
	public bool destroyOldColliders = true;
	public GameObject copyTarget;
	public GameObject copyTargetMesh;
	public GameObject copyTargetColliders;
	public GameObject copyTargetExcluded;
	public GameObject[] excludeFromCombine;
	public GameObject selfColliders;
	public bool toggleCollidersStatus = true;

	public float sortingOrderToZPositionMultiplier = -0.0001f;
	public float sortingOrderToZPositionOffset = 0f;


	public bool editorOnlyTagged = false;
	public bool useCutoutShader = false;

	public float[] oldZPositions;

#endif
}