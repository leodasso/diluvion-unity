using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Diluvion;

public class BasicMover : MonoBehaviour {

    public Transform targetTrans;
    public float speed = 100;
    float moveDistance = 5;
    Vector3 previousTargetPos = Vector3.zero;
    Navigation navigation;
    Navigation Navigation
    {
        get
        {
            if (navigation == null)
                Navigation = GetComponentInChildren<Navigation>();
            if (navigation == null)
                Navigation = gameObject.AddComponent<Navigation>();
            return navigation;
        }
        set
        {
            navigation = value;
        }
    }
    Avoider avoider;
    Avoider Avoider
    {
        get
        {
            if (avoider != null) return avoider;
            Avoider = GetComponent<Avoider>();
            if (avoider == null)
                Avoider = gameObject.AddComponent<Avoider>();
            return avoider;
        }
        set
        {
            avoider = value;
        }
    }
    private void OnDrawGizmos()
    {
      
     /*   if (!Navigation.Navigating()) return;
      
        transform.Translate((Navigation.Waypoint() - transform.position) * Time.deltaTime);*/
    }
    // Use this for initialization
    void Start () {
		
	}

    Vector3 AvoidVector(Vector3 direction)
    {
        return Vector3.zero;
       // return Avoider.AvoidVector(transform.position, direction);
    }

    [Button]
    void SetDestination(bool? complete)
    {
        previousTargetPos = targetTrans.position;
        Navigation.SetCourse(targetTrans.position, SetDestination);
    }

	// Update is called once per frame
	void Update ()
    {    

        if (!Navigation.Navigating()) return;
        Vector3 nextWPDir = (Navigation.Waypoint() - transform.position).normalized;
        Vector3 steerDir = nextWPDir + AvoidVector(nextWPDir).normalized;
        transform.Translate(steerDir * speed * Time.deltaTime, Space.World);
        transform.rotation = Quaternion.LookRotation(steerDir);
    }
}
