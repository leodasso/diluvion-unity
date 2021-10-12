using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
// TileTool2DDepthSort sets Sorting Order based on transform Y position.
// GameObjects will be ordered to render in front of higher positioned gameObjects.
//
// Deph Sorting overrides TileTool2D layer sorting, please use Unity Sorting Layers. 
// Attach to GameObject containig sprites and/or particle system.


public class TileTool2DDepthSort : MonoBehaviour {
	public float customSortPoint;			// Point the object is placed in the game.
	public float sortResolution = 100f;		// How accurate the sorting order should be based on transform Y position. ( bigger number is more accurate but smaller game area )
	public float updateFrequency = .1f;     // How ofter sort should run every second.
	//[HideInInspector]
	public float lastPos;                  // Used to avoid running sort when gameObject has not changed position.
	
	public string sortingLayer = "";

	public Vector3 resetPosition;

	// References to components, avoid using GetComponent more than once per component for performance.
	[HideInInspector]	public SpriteRenderer cacheSpriteRenderer;
	[HideInInspector]	public ParticleSystem cacheParticleSystem;
	[HideInInspector]	public Renderer cacheParticleSystemRenderer;
	[HideInInspector]	public Transform cacheTransform;

	void Start() {
		if (lastPos == 0) lastPos = 99999.9f;
		CacheComponents();
		SetSortingOrder();
		SetSortingLayer();
		
	}


	// Get references to components.
	public void CacheComponents() {
		if (!cacheTransform) cacheTransform = GetComponent<Transform>();
		if (!cacheSpriteRenderer) cacheSpriteRenderer = GetComponent<SpriteRenderer>();
		if (!cacheParticleSystem) GetComponent<ParticleSystem>();
		if (cacheParticleSystem && !cacheParticleSystemRenderer) cacheParticleSystemRenderer = cacheParticleSystem.GetComponent<Renderer>();
	}
	
	// Overrides Unity Sorting Layer on start, if the layer name exists. (Good for particles attached to tiles)
	void SetSortingLayer() {
		if (sortingLayer != "") cacheSpriteRenderer.sortingLayerName = sortingLayer;
	}

	public void SetSortingOrder() {
		if(cacheTransform.position.y == lastPos) {
			// Object has not vertically, no point in changing sorting order.
			Invoke("SetSortingOrder", updateFrequency);
			return;
		}
		int pos = Mathf.RoundToInt((cacheTransform.position.y + customSortPoint) * sortResolution) * -1;
		if (cacheSpriteRenderer) cacheSpriteRenderer.sortingOrder = pos;
		if (cacheParticleSystemRenderer) cacheParticleSystemRenderer.sortingOrder = pos;
		lastPos = cacheTransform.position.y;
		Invoke("SetSortingOrder", updateFrequency);
	}

	// Draw the custom sort point in editor. (Code is only included in editor)
#if UNITY_EDITOR
	public Color customSortPointColor = Color.cyan;
	public bool sortInEditor;
	void OnDrawGizmos () {
		CacheComponents();
		if (sortInEditor && !Application.isPlaying) {
			SetSortingOrder();
			EditorUtility.SetDirty(cacheSpriteRenderer);
		}
		Vector2 v = new Vector2(0, customSortPoint);
		Gizmos.color = customSortPointColor;
		Vector2 p = (Vector2)cacheTransform.position + v;
		Gizmos.DrawWireCube(p, new Vector2(.05f,.05f));
		Gizmos.DrawWireCube(p, new Vector2(.01f, .01f));
	}
#endif
}