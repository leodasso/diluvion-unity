using UnityEngine;
using System.Collections;

// Attach to a TileTool2D layer
// Adds TileTool2DDepthSort to all objects in layer

public class TileTool2DLayerDepthSort : MonoBehaviour {

	public float sortResolution = 100f;     // How accurate the sorting order should be based on transform Y position. ( bigger number is more accurate but smaller game area )
	public float updateFrequency = .1f;     // How ofter sort should run every second.

	public bool addOnStart = true;

	void Start () {
		if(addOnStart) AddSortToChildren();
	}

	public void AddSortToChildren() {
		for (int i = 0; i < transform.childCount; i++) {
			Transform c = transform.GetChild(i);
			AddSort(c.gameObject);
		}
	}

	void AddSort(GameObject target) {
		TileTool2DDepthSort ds = target.GetComponent<TileTool2DDepthSort>();
		if (ds) return;
		ds = target.AddComponent<TileTool2DDepthSort>();
		ds.sortResolution = sortResolution;
		ds.updateFrequency = updateFrequency;
	}
}

