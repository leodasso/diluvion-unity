using UnityEngine;
using System.Collections;
using FluffyUnderware.Curvy;
using HeavyDutyInspector;

public class SplineObject : MonoBehaviour
{
    public CurvySplineBase spline;
    public float myTF;

	[Space]
    public float myTFDistance;
	[Button("Increase TF Dist by 1", "IncreaseTFDist1", true)] 		public bool hidden1;
	[Button("Increase TF Dist by 10", "IncreaseTFDist10", true)] 	public bool hidden2;
	[Button("Increase TF Dist by 100", "IncreaseTFDist100", true)] 	public bool hidden3;
	[Space]
	[Button("Decrease TF Dist by 1", "DecreaseTFDist1", true)] 		public bool hidden4;
	[Button("Decrease TF Dist by 10", "DecreaseTFDist10", true)] 	public bool hidden5;
	[Button("Decrease TF Dist by 100", "DecreaseTFDist100", true)] 	public bool hidden6;


    public float orientWidth;
    public MeshFilter myMesh;
    public float upAngle = 90;
    public Vector3 orientation;

	public Vector3 oscillation;
	public float oscIntensity;
	public float wavesOnBody = 5;
	public float oscSpeed = 1;
	public float sine;
    float tempOffset = 0;
    float tempDistance = 0;

	float totalSin = 0;

	Vector3 offset;

	void Update() {

		totalSin += Time.deltaTime * oscSpeed;

		if (oscIntensity < .01f) return;

		sine = Mathf.Sin((totalSin) + (myTF * wavesOnBody));

		offset = transform.TransformDirection(oscillation) * sine * oscIntensity;
	}

	public static float clampedTFDist(float distance, float splineLength) {
		float realDist = distance;

		if (distance < 0) {
			realDist = splineLength + distance;
		}

		if (distance > splineLength)
			realDist = distance - splineLength;

		return realDist;
	}
  
    /// <summary>
    /// Returns the total space this object occupies on the spline
    /// </summary>
    /// <param name="spl">the spline to attach to</param>
    /// <param name="offset">the offset orientation direction</param>
    /// <param name="tfToSpawn"> the TF location along the spline</param>
    /// <param name="angle">the angle around the tangent that we rotate the object</param>
    /// <returns></returns>
    public float SpawnOnSpline(CurvySplineBase spl, Vector3 offset, float tfToSpawn, float angle)
    {
      //  Debug.Log("TFONSPLINE: " + tfToSpawn);
        SetToSpline(spl, offset, tfToSpawn, angle);
        Vector3 tangent = spline.GetTangentFast(tfToSpawn);      
        return GetWidthAlong(tangent);
    }

   /// <summary>
   /// Returns the distance this object occupies on the spline for spacing to the next object
   /// </summary>
   /// <param name="tangent"></param>
   /// <returns></returns>
    public float GetWidthAlong(Vector3 tangent)
    {    
        Quaternion prevRot = transform.rotation;
        transform.rotation = Quaternion.identity;
	    
        if (myMesh == null) myMesh = GetComponentInChildren<MeshFilter>();

	    Vector3 sizes;
	    
	    Renderer r = GetComponentInChildren<Renderer>();
	    if (r)
	    {
		    Bounds b = r.bounds;
		    if (myMesh) b = myMesh.sharedMesh.bounds;

		    Vector3 bounds = Vector3.Scale(b.size, myMesh.transform.localScale);
		    
		    sizes = Vector3.Scale(prevRot*tangent.normalized, prevRot * bounds);
	    }

	    sizes = Vector3.Scale(prevRot * tangent.normalized, prevRot * Vector3.one);

        transform.rotation = prevRot;
        orientWidth = sizes.magnitude;

        return orientWidth;
    }

    //Gets the current TF position with offset on the spline
    /// <summary>
    ///Gets the current TF position with offset on the spline
    /// </summary>
    public float GetTotalTF()
    {
       return myTF + tempOffset;
    }

    /// <summary>
    /// Modifies the offset distance of this object, preserving its spawned distance and TF
    /// </summary>
    public void OffsetDistance(float distance, Vector3 offset, float angle, bool rotate = true)
    {
        tempDistance = distance;
        orientation = offset;
        upAngle = angle;
        MoveToDistance(myTFDistance + tempDistance, rotate);
    }


    /// <summary>
    /// Modifies the offset TF on this object, preserving its spawned distance and TF
    /// </summary>
    public void OffsetTF(float offsetTF, Vector3 offset, float angle, bool rotate = true)
    {
        tempOffset = offsetTF;
        orientation = offset;
        upAngle = angle;
        MoveToSplineTF(myTF+tempOffset);
        if (rotate) OrientToSplineTF(myTF+tempOffset);

    }


    /// <summary>
    /// Applies this object to follow the input spline, with input offset and position
    /// </summary>
    public void SetToSpline(CurvySplineBase spl, Vector3 offset, float positionTF, float angle)
    {
        spline = spl;
        orientation = offset;
        upAngle = angle;
        SetTF(positionTF);

    }

	// helper functions used in the inspector buttons
	void IncreaseTFDist1() {	SetDistanceDirect(myTFDistance + 1);	}
	void IncreaseTFDist10() {	SetDistanceDirect(myTFDistance + 10);	}
	void IncreaseTFDist100() {	SetDistanceDirect(myTFDistance + 100);	}

	void DecreaseTFDist1() {	SetDistanceDirect(myTFDistance - 1);	}
	void DecreaseTFDist10() {	SetDistanceDirect(myTFDistance - 10);	}
	void DecreaseTFDist100() {	SetDistanceDirect(myTFDistance - 100);	}

	void SetDistanceDirect(float newDist) {
		myTFDistance = clampedTFDist(newDist, spline.Length);
	}

    /// <summary>
    ///Sets the current TF And distance values on the spline
    /// </summary>
    public void SetTF(float positionTF)
    {
        //Debug.Log("SETTF: " + positionTF);
        myTF = positionTF;
        myTFDistance = spline.TFToDistance(myTF);
        MoveToSplineTF(myTF);
        OrientToSplineTF(myTF);
    }



    /// <summary>
    ///Moves the object to distance along the spline
    /// </summary>
    public void MoveToDistance(float distance, bool rotate = true)
    {

		myTF = spline.DistanceToTF(clampedTFDist(distance, spline.Length));
        MoveToSplineTF(myTF);
        if (rotate) OrientToSplineTF(myTF);
    }

    /// <summary>
    ///Moves the object to TF along the spline
    /// </summary>
    public void MoveToSplineTF(float tf)
    {
        if (spline == null) return;       
		transform.position = spline.Interpolate(tf) + spline.transform.position + offset;      
    }


    /// <summary>
    ///Rotates the object to TF along the spline
    /// </summary>
    public void OrientToSplineTF(float tf)
    {
        if (spline == null) return;
        if ( spline.enabled == false ) return;
        transform.rotation = RotationOnSpine(tf);

    }

    /// <summary>
    /// Figures out the orientation of the object on the spline around the tangent point at TF, using the normal as UP
    /// </summary>
    /// <param name="tf"></param>
    /// <returns></returns>
    Quaternion RotationOnSpine(float tf)
    {
        if (orientation == Vector3.zero)
            return Quaternion.identity;
        return Quaternion.LookRotation(spline.GetTangentFast(tf), spline.GetRotatedUpFast(tf, upAngle)) * Quaternion.LookRotation(orientation);
    }
}
