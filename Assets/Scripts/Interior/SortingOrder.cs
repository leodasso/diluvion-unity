using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEdode]
public class SortingOrder : MonoBehaviour {

    public int sortingLayer = 1;
    public int orderInLayer;

    Renderer r;

	// Use this for initialization
	void Start () {

        r = GetComponent<Renderer>();
		
	}
	
	// Update is called once per frame
	void Update () {

        sortingLayer = Mathf.Clamp(sortingLayer, 0, SortingLayer.layers.Length - 1);

        r.sortingLayerID = SortingLayer.layers[sortingLayer].id;
        r.sortingOrder = orderInLayer;
		
	}
}
