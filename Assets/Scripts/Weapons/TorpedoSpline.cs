using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Components;
using Sirenix.OdinInspector;
using SpiderWeb;
using UnityEngine;


/// <summary>
/// Handles proper transformation of the spline used for torpedoes, as well as visible/invisible states
/// </summary>
[ExecuteInEditMode]
public class TorpedoSpline : MonoBehaviour
{
	[ToggleLeft]
	public bool editPositions;
	[ShowIf("editPositions")]
	public Vector3[] middleCPPositions;
	
	[SerializeField]
	CPFollowTransform startCP; // start point that follows the mount position and rotation

	[SerializeField] 
	Transform middleCP; // start point that follows the mount position and rotation
	
	[SerializeField] 
	Transform endCP; // end point that chases the target 

	[SerializeField]
	Transform targetTrans;

	[SerializeField] Vector3 inaccuracyOffset;
	
	[SerializeField]
	Vector3 targetPos;

	[SerializeField, ReadOnly] float mountForwardDOT = 1;
	[SerializeField, ReadOnly] float rightDot;

	/// <summary>
	/// Point where the curve splits from calculated curve to a straight path
	/// </summary>
	Vector3 _splitPosition;

	/// <summary>
	/// Tangent at the split
	/// </summary>
	Vector3 _splitTangent;

	#region Constructed Variables
	CurvySpline _curvySpline;

	public CurvySpline MySpline
	{
		get
		{
			if (_curvySpline != null) return _curvySpline;
			return _curvySpline = GetComponent<CurvySpline>();
		}
	}
	
	public LineRenderer lineRenderer;
	LineRenderer MyLineRenderer
	{
		get
		{
			if (lineRenderer != null) return lineRenderer;
			return lineRenderer = GetComponent<LineRenderer>();
		}
	}
	
	/// <summary>
	/// Current lateral move speed
	/// </summary>
	//private float CurrentSpeed => Mathf.Lerp(minMoveSpeed, maxMoveSpeed, calibration);

	/// <summary>
	/// Direction to target
	/// </summary>
	Vector3 DirectDir => TargetPosition() - startCP.transform.position;

	/// <summary>
	/// Direction to end Control Point
	/// </summary>
	Vector3 SplineDir => endCP.transform.position - startCP.transform.position;
	
	/// <summary>
	/// Distance to end control point
	/// </summary>
	float CurrentEndDist => Vector3.Distance(endCP.transform.position, startCP.transform.position);

	/// <summary>
	/// Current target distance from start point due to calibration
	/// </summary>
	//private float CalibrationDistance => Mathf.Clamp(Vector3.Distance(targetPos, startCP.transform.position) * calibration,35, 350);
	
	#endregion

	Vector3 mountWorldRight;

	[Button]
	void SetLeftRight()
	{
		mountWorldRight = Vector3.Cross(Vector3.up, startCP.transform.forward);
		rightDot = Vector3.Dot(mountWorldRight.normalized, DirectDir.normalized);
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(_splitPosition, 1);
		
		Gizmos.DrawRay(_splitPosition, _splitTangent * 80);
	}
	
	/// <summary>
	/// Init values for spline and control points
	/// </summary>
	/// <param name="target"></param>
	public void SetStart(Transform mountTrans)
	{
		HideVisual();
		
		startCP.SetTarget(mountTrans);
		startCP.transform.position = mountTrans.position;
		startCP.transform.rotation = mountTrans.rotation;
		
		SetLeftRight();
		endCP.transform.position = mountTrans.position + mountTrans.forward * 35;
		MoveMidCP();
		ShowVisual();

		inaccuracyOffset = Vector3.zero;
	}
	
	public void TorpedoDied()
	{
		Destroy(gameObject);
	}
	
	/// <summary>
	/// Directly sets the vector3 target
	/// </summary>
	public void SetTarget(Vector3 target)
	{
		targetPos = target;
		targetTrans = null;
	}
	
	//override for transforms, uses the position for target
	public void SetTarget(Transform target)
	{
		targetTrans = target;
		targetPos = targetTrans.position;
	}

	Vector3 TargetPosition()
	{
		if (targetTrans) return targetTrans.position;
		return targetPos;
	}

	/// <summary>
	/// Destroys the line renderer and visual of the spline
	/// </summary>
	public void HideVisual()
	{
		MyLineRenderer.enabled = false;
	}

	public void ShowVisual()
	{
		MyLineRenderer.enabled = true;
	}
	


	/// <summary>
	/// What happens when the Torpedo is spawned
	/// </summary>
	public void ReleasedTorpedo(float targetSphereRadius)
	{
		//Debug.Log(name + " releasing. Shouldn't move now");
		transform.parent = null;
		HideVisual();
		startCP.ClearTarget();

		inaccuracyOffset = targetSphereRadius * Random.onUnitSphere;
	}

	/// <summary>
	/// Moves the endCP to the calibrated point
	/// </summary>
	void MoveEndCP()
	{
		endCP.transform.position = TargetPosition() + inaccuracyOffset; 
	}


	void MoveMidCP()
	{
		float endDistance = CurrentEndDist;
		
		mountForwardDOT = Vector3.Dot(startCP.transform.forward.normalized, SplineDir.normalized);
		
		Vector3 lerpedMidLocalPos = Calc.MultiLerp(middleCPPositions,  1-mountForwardDOT);
		Vector3 flippingLocalPos  = lerpedMidLocalPos;
		if (rightDot < 0) flippingLocalPos = new Vector3(-lerpedMidLocalPos.x, lerpedMidLocalPos.y, lerpedMidLocalPos.z);
		
		Debug.DrawRay(startCP.transform.position,  startCP.transform.rotation * (flippingLocalPos), Color.cyan, 0.01f);
		middleCP.transform.position = startCP.transform.position + startCP.transform.rotation * (flippingLocalPos);
		
		float distanceToMidCP = Vector3.Distance(middleCP.position, startCP.transform.position);
		SetHandleLength(distanceToMidCP/2); // Set handle to half size of the curent curve length
	}
	

	
	/// <summary>
	/// Sets the start direction handle length 
	/// </summary>
	void SetHandleLength(float length)
	{
		startCP.SetForwardHandleDistance(length);
	}

	void Update()
	{
		if (Application.isPlaying)
			MoveEndCP();
		else
		{
			targetPos = endCP.position;
			SetLeftRight();
		}
			
		MoveMidCP();
	}
}
