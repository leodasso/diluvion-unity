using UnityEngine;
using System.Collections;
using HeavyDutyInspector;

public class FallAway : MonoBehaviour {

    public float minFallTime = 5;
    public float maxFallTime = 20;

    [Space]
    public bool deParent = true;
    [HideConditional(true, "deParent", true)]
    public Transform newParent;

    [Space]
    public bool rotate = false;
    [HideConditional(true, "rotate", true)]
    public float rotationDrag = 5;

    [HideConditional(true, "rotate", true)]
    public Vector3 initAngularVelocity;

    public float timeToDestroy = 25;

    [Space]
    public Vector3 fallDirection;
    public Space fallSpace = Space.World;
    public float acceleration = 5;
    public float maxSpeed = 20;

    float speed = 0;
    bool falling = false;
    Vector3 rotationAmt;

	// Use this for initialization
	IEnumerator Start () {

        float fallTime = Random.Range(0, maxFallTime - minFallTime);

        yield return new WaitForSeconds(minFallTime);
        yield return new WaitForSeconds(fallTime);
        Fall();
        yield break;
	
	}

    void Fall()
    {
        if ( deParent ) transform.parent = newParent;

        // Set timer to destroy
        StartCoroutine(WaitAndDestroy(timeToDestroy));

        // Set initial rotation speed
        if ( rotate ) rotationAmt = new Vector3(Random.Range(-initAngularVelocity.x, initAngularVelocity.x),
            Random.Range(-initAngularVelocity.y, initAngularVelocity.y),
            Random.Range(-initAngularVelocity.z, initAngularVelocity.z));

        falling = true;
    }

    IEnumerator WaitAndDestroy(float howLong)
    {
        yield return new WaitForSeconds(howLong);
       Debug.Log("DESPAWNING FROM:" + this.name , this); Destroy(gameObject);
    }
	
	// Update is called once per frame
	void Update () {

        if ( !falling ) return;

        // Rotation
        if (rotate)
        {
            rotationAmt = Vector3.Lerp(rotationAmt, Vector3.zero, Time.deltaTime * rotationDrag);
            transform.Rotate(rotationAmt * Time.deltaTime);
        }

        speed += Time.deltaTime * acceleration;
        speed = Mathf.Clamp(speed, 0, maxSpeed);

        transform.Translate(fallDirection * speed * Time.deltaTime, fallSpace);
	}
}
