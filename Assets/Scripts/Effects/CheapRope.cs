using UnityEngine;
using System.Collections;


///Simulates a straight rope by using line renderer.  This can go between any
/// two transforms, and simulates taught rope well.  Won't simulate loose rope.
/// Pairs well with Hinge Joint

[RequireComponent(typeof(LineRenderer))]
public class CheapRope : MonoBehaviour {

	public GameObject object1;
	public GameObject object2;
	public Material ropeMat;

	LineRenderer lineRenderer;

	// Use this for initialization
	void Start () 
	{
		lineRenderer = GetComponent<LineRenderer>();
	
		if (ropeMat != null) {
			lineRenderer.material = ropeMat;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
		if (lineRenderer != null) {
			lineRenderer.SetVertexCount(2);
			lineRenderer.SetPosition(0, object1.transform.position);
			lineRenderer.SetPosition(1, object2.transform.position);
		}
	}
}
