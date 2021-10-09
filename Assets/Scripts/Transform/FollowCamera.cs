using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {

	Transform cam;

	// Use this for initialization
	void Start () {
        if (Camera.main == null) return;
		cam = Camera.main.transform;
	}
	
	// Update is called once per frame
	void Update () {

        if (Camera.main == null) return;
       
        if (cam == null)
            cam = Camera.main.transform;     
   
        transform.position = cam.position;
	}
}
