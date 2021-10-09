using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Stay : MonoBehaviour {


    public bool keepLocalPos;
    public bool keepWorldPos;

    [Space]
    public bool keepLocalRot;
    public bool keepWorldRot;

    Quaternion initRot;
    Quaternion initLocalRot;

    Vector3 initPos;
    Vector3 initLocalPos;

	// Use this for initialization
	void Start () {

        initRot = transform.rotation;
        initLocalRot = transform.localRotation;

        initPos = transform.position;
        initLocalPos = transform.localPosition;
		
	}
	
	// Update is called once per frame
	void Update () {

        if (keepLocalPos) transform.localPosition = initLocalPos;
        if (keepWorldPos) transform.position = initPos;

        if (keepLocalRot) transform.localRotation = initLocalRot;
        if (keepWorldRot) transform.rotation = initRot;		
	}
}
