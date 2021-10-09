using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetachOnStart : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
		if (transform.parent)
			transform.parent = transform.parent.parent;
	}
}
