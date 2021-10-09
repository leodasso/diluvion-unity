using UnityEngine;
using System.Collections;

public class RandomRotation : MonoBehaviour {

    public Vector3 minRotation;
    public Vector3 maxRotation;
    public Space rotationSpace = Space.Self;
    public bool randomizeOnStart = true;
    public bool randomizeOnEnable = false;

	// Use this for initialization
	void Start ()
    {
        if ( randomizeOnStart ) Randomize();
	}

    void OnEnable()
    {
        if ( randomizeOnEnable ) Randomize();
    }
	
	void Randomize()
    {
        Vector3 totalRotation = new Vector3(Random.Range(minRotation.x, maxRotation.x),
            Random.Range(minRotation.y, maxRotation.y),
            Random.Range(minRotation.z, maxRotation.z));

        transform.Rotate(totalRotation, rotationSpace);
    }
}
