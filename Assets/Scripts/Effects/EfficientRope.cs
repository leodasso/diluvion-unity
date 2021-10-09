using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class EfficientRope : MonoBehaviour {

	public bool refreshOnUpdate = true;
	[Space]
	public GameObject ropeStart;
	public GameObject ropeEnd;

	public LineRenderer lineRenderer;

	// Use this for initialization
	void Start () {
	
		if ( !lineRenderer) lineRenderer = GetComponent<LineRenderer>();
		SetPositions();
	}
	
	// Update is called once per frame
	void Update () {
	
		if (lineRenderer == null) return;

		if (refreshOnUpdate) SetPositions();
	}

	void SetPositions() {

		if (!lineRenderer) return;
		if (!ropeStart || !ropeEnd) return;

		Vector3[] points = new Vector3[] {ropeStart.transform.position, ropeEnd.transform.position};

		lineRenderer.SetPositions(points);
	}
}
