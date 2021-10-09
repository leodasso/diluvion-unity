using UnityEngine;
using System.Collections;

public class Translate : MonoBehaviour {

    public Space translationSpace;
    public Vector3 speed;
    public bool multiplyByScale;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        Vector3 totalSpeed = speed;
        if ( multiplyByScale ) totalSpeed = Vector3.Scale(transform.localScale, totalSpeed);

        transform.Translate(totalSpeed * Time.deltaTime, translationSpace);	
	}
}
