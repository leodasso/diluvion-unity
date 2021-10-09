using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(UIMeshLine))]
public class MeshLineHelper : MonoBehaviour {

    [Tooltip("The follow targets for the list of points in the line.")]
    public List<Transform> followTargets = new List<Transform>();
    UIMeshLine line;

	// Use this for initialization
	void Start () {
        line = GetComponent<UIMeshLine>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!line) return;

        for (int i = 0; i < line.GetPointCount(); i++)
        {
            if (i >= followTargets.Count) continue;
            if (followTargets[i] == null) continue;
            line.SetPointPosition(i, transform.InverseTransformPoint(followTargets[i].position));
        }
	}
}
