using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpringJoint))]
public class SpringJointHelper : MonoBehaviour
{
	List<SpringJoint> _springJoints = new List<SpringJoint>();

	// Use this for initialization
	void Start ()
	{
		_springJoints.AddRange(GetComponents<SpringJoint>());

		foreach (var s in _springJoints)
		{
			s.connectedAnchor = transform.position + s.anchor;
		}
	}
}
