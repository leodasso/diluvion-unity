using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FluffyUnderware.Curvy;
using Sirenix.OdinInspector;
using System.Linq;

public class ObjectsOnPath : MonoBehaviour
{
    [TabGroup("spawn options"), Tooltip("Switches movement to use distance instead of TF value (More accurate when non-uniform spline)")]
    public bool maintainOffset;

    [TabGroup("spawn options"), HideIf("maintainOffset"), Tooltip("moves the spline objects splineOffset value in addition to their spawned TF")]
    public float splineOffset; 	

    [TabGroup("spawn options"), ShowIf("maintainOffset")]
    public float splineOffsetDistance = 0;	// 			moves the spline objects splineOffsetDistance value in addition to their spawned distance

    [TabGroup("spawn options")]
    public bool clampOffsetDistance;
  
    [Tooltip("Allows the spline to update the movement of the objects in play mode")]
    public bool updateInPlayMode; 
    public CurvySplineBase spline;

    [TabGroup("object options")]
	[Tooltip("Make sure to set spline to Dynamic before spawning along!!")]
    public Transform objectParent;

    [TabGroup("object options")]
    public List<GameObject> objectsToSpawn = new List<GameObject>();

    [TabGroup("object options")]
    [Range(.1f, 500)]
    public float bufferSize; 				//			extra distance between objects

    [TabGroup("object options")]
    public bool adjustRotation; //		point direction offset of the spawned object 

    [TabGroup("object options")]
    public float upangle = 90; 				//			Rotates objects around the Tangent of the spline

    [TabGroup("object options")]
    public Vector3 orientation = Vector3.right; //		point direction offset of the spawned object 

    [TabGroup("object options")]
    public bool useCurveForScale;

    [TabGroup("object options"), ShowIf("useCurveForScale")]
    public AnimationCurve sizeOnSpline;

    int maxIterations = 500;

    protected float totalLength;

    Transform targetParent;

    List<SplineObject> childObjects = new List<SplineObject>();

    [Button]
    void RefreshAll()
    {
        LateUpdate();
    }

    
    [ButtonGroup("spawn")]
    void SpawnObjects()
    {
        SpawnObjectsAlongSpline();
    }

    [ButtonGroup("spawn")]
    void SpawnPrefabs()
    {
        SpawnObjectsAlongSpline(true);
    }

    /// <summary>
    /// Spawns and Sets the objects in objectsToSpawn to TF positions along thes pline, and saves their distance as well
    /// </summary>
    public virtual void SpawnObjectsAlongSpline(bool asPrefabs = false)
    {
        if (Application.isPlaying) return;
        DeletePrevObjects();
        CheckForSpline();
        if (spline == null) return;

        SetTargetParent();
    
        totalLength = spline.Length;
        float lengthProgression = 0;
        int partNumber = 0;

        //Debug.Log("Spawning objects on spline... total length: " + totalLength + " lengthProgression: " + lengthProgression);

        while (lengthProgression < totalLength)
        {
            //Debug.Log("Obj on path " + Time.time);  
            foreach (GameObject o in objectsToSpawn)
            {
                float tf = spline.DistanceToTF(lengthProgression + splineOffsetDistance);

                float tempLength = 0;
				GameObject returnObj = SpawnLength(o, orientation,  upangle, tf, out tempLength, asPrefabs);

                childObjects.Add(returnObj.GetComponent<SplineObject>());

                returnObj.name = name + "_Part_" + partNumber;
                returnObj.transform.SetParent(targetParent);

                lengthProgression += tempLength;
            }
            partNumber++;
            if (partNumber > maxIterations) break;
        }

        CleanList();
    }
    
    

    void SetTargetParent()
    {
        if (objectParent == null)
            targetParent = transform;
        else
            targetParent = objectParent;
    }

    void Start ()
    {
        SetTargetParent();
        childObjects = new List<SplineObject>(targetParent.GetComponentsInChildren<SplineObject>());
        CleanList();
    }

    protected virtual void CleanList()
    {
        childObjects = childObjects.Where(x => x != null).ToList();
    }


    /// <summary>
    /// Safe spline getter
    /// </summary>
    public CurvySplineBase CheckForSpline()
    {
        if (spline != null) return spline;
        spline = GetComponentInChildren<CurvySplineBase>();
        return spline;  
    }


    /// <summary>
    /// Moves all splineObject children to their position along the spline
    /// </summary>
    public virtual void UpdateObjectsAlongSpline()
    {
        SetTargetParent();

        CheckForSpline();
        if (spline == null) return;

        totalLength = spline.Length;     

        for (int i = 0; i < childObjects.Count; i ++)
        {
            SplineObject so = childObjects[i];
            if (so == null) continue;

            if (!maintainOffset)
                so.OffsetTF(splineOffset, orientation, upangle, adjustRotation);
            else
                so.OffsetDistance(splineOffsetDistance, orientation, upangle, adjustRotation);
        }
    }


    public void AddSegments(float remainingDistance){    }

    /// <summary>
    /// Deletes all the previous Objects
    /// </summary>
    public void DeletePrevObjects()
    {
        SetTargetParent();

        List<Transform> transformsToDelete = new List<Transform>();
        List<SplineObject> childrenToDelete = new List<SplineObject>(targetParent.GetComponentsInChildren<SplineObject>());
        
        foreach (SplineObject so in childrenToDelete)
        {
            transformsToDelete.Add(so.transform);    
        }

        foreach (Transform t in transformsToDelete) DestroyImmediate(t.gameObject);
    }

	/// <summary>
	/// Spawns the object along the spline.  TF is progress along spline, used for scaling with curve.
	/// </summary>
	public virtual GameObject SpawnLength(GameObject reference, Vector3 orient, float angle, float tf, out float length, bool asPrefab = false)
	{
	    GameObject returnObj = null;

	    if (asPrefab)
	    {
	        #if UNITY_EDITOR
	        returnObj = UnityEditor.PrefabUtility.InstantiatePrefab(reference) as GameObject;
#endif
	    }
	    
	    else  
	        returnObj = Instantiate(reference, transform.position, transform.rotation);
	    
        SplineObject splineObj = returnObj.AddComponent<SplineObject>();
        float size = 1;
        //float tempBuffer = bufferSize;
        if (useCurveForScale)
        {
            size = SizeOnSpline(tf);
            returnObj.transform.localScale *= size;
        }

		length = size * bufferSize;

		splineObj.SpawnOnSpline(spline, orient, tf, angle);

        return returnObj;
	}

    /// <summary>
    /// Returns the float value from the sizeOnSpline curve at time tf
    /// </summary>
	protected virtual float SizeOnSpline(float tf) {

		if (!useCurveForScale) return 1;

		float size = sizeOnSpline.Evaluate(tf);
		size = Mathf.Clamp(size, .1f, 20);

		return size;
	}


    public void LateUpdate()
    {
		if (!updateInPlayMode && Application.isPlaying) return;

		if (clampOffsetDistance && spline != null)
            splineOffsetDistance = SplineObject.clampedTFDist(splineOffsetDistance, spline.Length);

        UpdateObjectsAlongSpline();
    }


    //Gets the distance to the closer of the two siblings on this curve
    //Mainly stops a handle from exceeding its neighbouring segments
    public float ClosestDistanceToSegment(CurvySplineSegment seg)
    {
        Vector3 nextPointVector = Vector3.zero;
        if (seg.NextSegment)
            nextPointVector = (seg.NextSegment.position - seg.position);

        Vector3 prevPointSegment = Vector3.zero;
        if (seg.PreviousSegment)
            prevPointSegment = (seg.PreviousSegment.position - seg.position);

        if (seg.IsFirstSegment)
            return nextPointVector.magnitude;
        if (seg.IsLastSegment)
            return prevPointSegment.magnitude;

        if (nextPointVector.sqrMagnitude > prevPointSegment.sqrMagnitude)
            return prevPointSegment.magnitude;
        else
            return nextPointVector.magnitude;
    }

    // returns a turn over time(speed) towards aimDirection, based on the input segment handleIn, the vector magnitude will not exceed MaxMagnitude
    public Vector3 PointHandlesAtTarget(CurvySplineSegment segment, float speed, Vector3 aimDir, float maxMagnitude)
    {
        Vector3 pointDirection = Vector3.MoveTowards(segment.HandleIn.normalized, aimDir.normalized, speed * Time.deltaTime);
        float closestSegDistance = Mathf.Min(ClosestDistanceToSegment(segment), maxMagnitude);
        float clampedMagnitude = Mathf.Clamp(aimDir.magnitude / 2, 0.5f, closestSegDistance);
        return pointDirection * clampedMagnitude;
    }


    /// <summary>
    /// Attempts to aim at aimDirection, but clamps it to clampAngle from controlDirection TODO move to TOOLS
    /// </summary>
    public Vector3 ClampedAim(Vector3 controlDirection, Vector3 aimDirection, float clampAngle)
    {      
        Quaternion aimRotation = Quaternion.LookRotation(aimDirection);
        Quaternion forwardRotation = Quaternion.LookRotation(controlDirection);

       // Debug.DrawRay(transform.position, aimRotation * orientation, Color.yellow, 0.01f);
       // Debug.DrawRay(transform.position, forwardRotation * orientation, Color.red, 0.01f);

        float currentAngle = Quaternion.Angle(aimRotation, forwardRotation);
      

        Vector3 clampedAim;

        if (currentAngle < clampAngle)
            clampedAim = aimDirection;
        else
            clampedAim = Quaternion.Slerp(forwardRotation, aimRotation, clampAngle / (currentAngle+1.0f))  *orientation;

        Debug.DrawRay(transform.position, clampedAim.normalized, Color.blue, 0.01f);

        return clampedAim.normalized*aimDirection.magnitude;//TODO maybe return the aimDirection at a better magnitude
    }


    //points the handles of the input segment along localVector
    public void PointHandles(CurvySplineSegment segment, Vector3 localVector)
    {
        segment.HandleIn = localVector;
        segment.HandleOut = -localVector;
    } 

}
