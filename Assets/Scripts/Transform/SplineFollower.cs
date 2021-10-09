using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using FluffyUnderware.Curvy;

public class SplineFollower : MonoBehaviour {

    public CurvySplineBase spline;

    public bool followOnUpdate = true;

    [Tooltip("Adjust this to adjust position along spline.")]
    public float myTF;
    public float maxTF = 1;

	[ToggleGroup("adjustOrientation")]
	public bool adjustOrientation;

	[ToggleGroup("adjustOrientation")]
	public Vector3 orientation;


	
	// Update is called once per frame
	void Update () {

        if ( spline == null ) return;

        if (!Application.isPlaying)
            maxTF = spline.DistanceToTF(spline.Length);

        if (followOnUpdate || !Application.isPlaying)
            transform.position = Vector3.Scale(spline.Interpolate(myTF), spline.transform.lossyScale) + spline.transform.position;

		if (adjustOrientation)
		{
			Quaternion dirtyRot = spline.GetOrientationFast(myTF);
			Quaternion finalRot = dirtyRot * Quaternion.Euler(orientation);
			transform.rotation = finalRot;
		}
	}
}
