using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;
using Diluvion;

public class SimpleAmmo : MonoBehaviour {

    public float damage = 1;
    public float lifeTime = 2;
    public Hull friendHull;


	public bool castInUpdate;

    float scale = 1;
    Vector3 initScale;
    bool init = false;
	Vector3 previousPos;
	LayerMask hitMask;

	public float sphereCastRadius = .1f;

	// Use this for initialization
	void Start () {

		previousPos = transform.position;
        initScale = transform.localScale;
	}

    public void Init()
    {
        init = true;
     
    }
	
	// Update is called once per frame
	void Update () {

        lifeTime -= Time.deltaTime;
        if (lifeTime < 0)
        {
            //decrease scale
            scale -= Time.deltaTime;

            //apply scale to object
            transform.localScale = initScale * scale;

            //destroy object if scale is 0
            if (scale <= 0) Destroy(gameObject);
        }

		if (castInUpdate) {
			DetectHit();
			previousPos = transform.position;
		}
	}

	void FixedUpdate() {

		if(hitMask != Calc.GunsafeLayer()) hitMask = Calc.GunsafeLayer();

		if (!castInUpdate) {
			DetectHit();
			previousPos = transform.position;
		}
	}


	public void DetectHit()
	{
		//Dont detect hits until the hull of what fired me is set
		if (friendHull == null) return;

		Hull hitHull = null;
		Ray impactRay = new Ray(previousPos, transform.position - previousPos);
		float hitDistance =  (transform.position - previousPos).magnitude;

		//RaycastHit[] hits = Physics.RaycastAll(impactRay, hitDistance, hitMask.value);
		RaycastHit[] hits = Physics.SphereCastAll(impactRay, sphereCastRadius, hitDistance, hitMask.value);

		if (hits.Length < 1) return;

		List<RaycastHit> sortedHits = hits.OrderBy(h => h.distance).ToList();

		// Clean up the sorted hits list to not include myself (torpedoes do this)
		foreach (RaycastHit h in hits) 
			if (h.collider.transform == transform) sortedHits.Remove(h);

		if (sortedHits.Count < 1) return;

		RaycastHit hit = sortedHits[0];
		Transform hitTransform = hit.collider.transform;

		while (true)
		{
			hitHull = hitTransform.GetComponent<Hull>();
			if (hitHull != null) break;
			if (hitTransform.parent == null) break;
			hitTransform = hitTransform.parent;
		}

		// If what i hit didn't have a hull, just destroy me.
		if (hitHull == null) { 
			Destroy(gameObject); 
			return; 
		}

		// If it's detecting what fired me, ignore the hit
		if (hitHull == friendHull) return;

		DoDamage(hitHull);
	}

	void OnTriggerEnter(Collider other) {
		Hull otherHull = other.GetComponent<Hull>();
		if (otherHull == null) return;

		DoDamage(otherHull);
	}

    void DoDamage(Hull damageHull)
    {
		if (damageHull == friendHull) return;
        damageHull.Damage(damage, 1, gameObject);
        Destroy(gameObject);
    }
}
