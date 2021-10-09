using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CompassPointer : MonoBehaviour {

	public bool showing;
	public Vector3 pointPos;
    Vector3 targetPosition;
    Vector3 landmarkPosition;
	float alpha = 1;
	List<SpriteRenderer> sprites;

	void Start() {
		//sprites.AddRange(transform.GetComponentsInChildren<SpriteRenderer>());
	}

	
    public List<SpriteRenderer> Sprites()
    {
        if (sprites != null) return sprites;
        sprites = new List<SpriteRenderer>();
        sprites.AddRange(transform.GetComponentsInChildren<SpriteRenderer>());

        return sprites;
    }

	// Update is called once per frame
	void Update () {
	
        if(showing)
            transform.position = Vector3.Lerp(transform.position, transform.parent.position, Time.deltaTime * 1);

        Vector3 lookDirection = pointPos - transform.position;
        transform.rotation = Quaternion.LookRotation(lookDirection);    
	}

	public void SetPoint(Vector3 point)
    {
		showing = true;
		pointPos = point;
        GetComponent<Animator>().SetBool("inRange",true);
    }
		

	public void End() {
        showing = false;
        GetComponent<Animator>().SetBool("inRange", false);
    }


	public void Destroy() {
		Destroy(gameObject);
	}
}
