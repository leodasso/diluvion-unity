using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
using FluffyUnderware.Curvy;
using UnityEngine;


public class CPFollowTransform : FollowTransform
{

	[SerializeField]
	bool lockHandlesToForward = false;


	private CurvySplineSegment css;

	private CurvySplineSegment MyCurvySegment
	{
		get
		{
			if (css != null) return css;
			return css = GetComponent<CurvySplineSegment>();
		}
	}

	public void SetForwardHandleDistance(float distance)
	{
		forwardHandleDistance = Mathf.Clamp(distance, 10, 30);
		MyCurvySegment.HandleOut =  transform.forward * forwardHandleDistance;
		MyCurvySegment.HandleIn =  -transform.forward * forwardHandleDistance;
	}

	private float forwardHandleDistance = 30;
	private void Awake()
	{
		forwardHandleDistance = Vector3.Distance(transform.position, MyCurvySegment.HandleOutPosition);
	}

	protected override void PerformMove(Vector3 targetPos, Quaternion rotation)
	{
		base.PerformMove(targetPos, rotation);
		if (!lockHandlesToForward) return;
		transform.rotation = rotation;
		SetForwardHandleDistance(forwardHandleDistance);
	}
}
