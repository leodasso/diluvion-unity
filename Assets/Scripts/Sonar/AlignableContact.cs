using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignableContact : MonoBehaviour, IAlignable
{

	public AlignmentToPlayer alignment;
	public float safeDistance = 15;

	public AlignmentToPlayer getAlignment()
	{
		return alignment;
	}

	public float SafeDistance()
	{
		return safeDistance;
	}
}
