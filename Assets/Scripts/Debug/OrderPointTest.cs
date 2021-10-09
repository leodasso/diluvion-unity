using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

public enum Direction
{
	ClockWise,
	CounterClockwise,
	Forward
}
public class OrderPointTest : MonoBehaviour
{

	public Direction direction;
	public Transform observer;
	public List<Transform> checkPoints;

	public List<Transform> sortedPoints;

	public List<Transform> patrolPoints;
	private Direction realDirection;


	Direction FacingDirection(Vector3 centerPoint)
	{

		Vector3 centerDirection = centerPoint - transform.position;

		Vector3 relativeRight = Vector3.Cross(Vector3.up, centerDirection.normalized);

		float rightDot = Vector3.Dot(relativeRight, observer.forward);

		if (rightDot >= 0)
			return Direction.ClockWise;
		else
			return Direction.CounterClockwise;
	}
	
	
	[Button]
	void OrderPoints()
	{
		Vector3 center = Vector3.zero;
		
		patrolPoints = new List<Transform>();
		
		foreach (Transform t in checkPoints)
			center += t.position;

		center /= checkPoints.Count;
		//Enter left Or Right comparison here

	}
}
