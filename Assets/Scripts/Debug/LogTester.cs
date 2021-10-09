using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LogTester : MonoBehaviour {

    public float value = 1;
    public float halfPoint = 5;
    public float maxOutput = 1;

    public float output = 1;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        output = SpiderWeb.Calc.LogBase(value, halfPoint, maxOutput);
	}
}
