using UnityEngine;
using System.Collections;
using Rewired.Utils.Libraries.TinyJson;


public class FollowTransform : MonoBehaviour {

	[SerializeField]
	Transform transformToFollow;

	Vector3 targetPosition;
	Quaternion targetRotation;
		
	[SerializeField]
	protected float followSpeed = 9999;

	bool canTransform => transformToFollow != null || transform.position != targetPosition;

	protected virtual void PerformMove(Vector3 targetPos, Quaternion rotation)
	{
		transform.position = Vector3.MoveTowards(transform.position,targetPos,followSpeed*Time.deltaTime);
	}

	public void SetTarget(Transform target)
	{
		transformToFollow = target;
	}

	public void SetPosition(Vector3 pos)
	{
		targetPosition = pos;
	}

	public void ClearTarget()
	{
		//Debug.Log(name + " is clearing target. should be static now. ", gameObject);
		transformToFollow = null;
	}
	
	// Update is called once per frame
	protected virtual void Update ()
	{
		if (!canTransform) return;

		if (transformToFollow)
		{
			SetPosition(transformToFollow.position);
			targetRotation = transformToFollow.rotation;
		}
		
		PerformMove(targetPosition, targetRotation);
	}
}
