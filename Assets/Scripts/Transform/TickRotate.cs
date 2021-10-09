using UnityEngine;
using System.Collections;
using HeavyDutyInspector;
using TMPro;

public class TickRotate : MonoBehaviour {

	public float tickWait = 1;
	public float rotateSpeed = 5;
	public float tickAmount;
	[Comment("What's the rotation where it will spell it's part of DILUVION")]
	public float keyRotation = 0;

    TextMeshPro letter;
	float timer = 0;
	Quaternion finalRot;
	bool finalized = false;

	// Use this for initialization
	void Start () {

        letter = GetComponentInChildren<TextMeshPro>();
        if ( letter ) letter.color = Color.clear;
	
		timer = Random.Range(0, tickWait);
		finalRot = transform.localRotation;
		
		transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, Random.Range(-180, 180), transform.localEulerAngles.z);
	}
	
	// Update is called once per frame
	void Update () {

		transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRot, Time.deltaTime * rotateSpeed);
	
		if (finalized) return;

		// Rotate by X degrees every wait period
		timer += Time.deltaTime;
		if (timer >= tickWait) {
			timer = 0;

			Vector3 newRot = new Vector3(0, tickAmount, 0);
			Quaternion newQuaternion = Quaternion.Euler(newRot);
			finalRot = transform.localRotation * newQuaternion;
		}
	}

	public void GotoKeyRotation() {
        if ( finalized ) return;
		finalRot = Quaternion.Euler(new Vector3(0, keyRotation, 0));
		finalized = true;

        if ( letter ) StartCoroutine(FadeInLetter());
	}

    IEnumerator FadeInLetter()
    {
        //yield return new WaitForSeconds(1);
        float progress = 0;
        float speed = 2;
        while (progress < 1)
        {
            progress += Time.unscaledDeltaTime * speed;
            letter.color = Color.Lerp(Color.clear, Color.white, progress);
            yield return null;
        }
    }
}
