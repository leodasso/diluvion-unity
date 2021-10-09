using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AvoidanceTester : MonoBehaviour
{
	
	private Vector3 avoidVector;


	[SerializeField]
	private float magnitude;
	[SerializeField]
	private float crashDistance;
	
	// Update is called once per frame
	void Update ()
	{
		
		GetComponent<Avoider>()
			.NormalizedAvoidVector(transform.forward, magnitude, out avoidVector, out crashDistance);
	}
	
	
	
	
}
