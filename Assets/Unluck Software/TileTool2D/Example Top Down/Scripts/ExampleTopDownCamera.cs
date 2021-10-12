using UnityEngine;
using System.Collections;

public class ExampleTopDownCamera : MonoBehaviour {

	public Transform follow;
	
	void Update () {
		Vector3 nPos = new Vector3(follow.position.x, follow.position.y, transform.position.z);
		transform.position = Vector3.Lerp(transform.position, nPos, .05f);
	}
}
