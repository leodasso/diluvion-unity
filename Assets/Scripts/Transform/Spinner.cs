using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;

public class Spinner : MonoBehaviour {

	public static List<Spinner> allSpinners;

	public float spinSpeed;
	public bool tick;
	public float tickDelay= .5f;
	public bool gotoFinalAngle = false;
	public float finalAngle = 0;
	public float maxWanderTime = 3;
	public bool useLimits = false;
	public Vector2 minMaxAngle;

	public AudioClip tickSound;
	public float tickPitch = 1;
	public AudioClip finalizeSound;
	public float finalSoundPitch = 1;

	float spinSpeedActual;
	float yRot;
	float tickTimer = 0;
	float dir = 1;				//direction of spinning
	float totalTime = 0;
	bool playedFinalSound = false;
	bool spinForever = false;

	// Use this for initialization
	void Start () {

		if (allSpinners == null) {
			allSpinners = new List<Spinner>();
		}

		allSpinners.Add(this);
	
		spinSpeedActual = spinSpeed;
		yRot = transform.localEulerAngles.y;
	}
	
	// Update is called once per frame
	void Update () {

		if (spinForever) {
			totalTime = 0;
		}

		if (gotoFinalAngle) {
			totalTime += Time.deltaTime;
			
			if (totalTime >= maxWanderTime)  {
				ApproachFinalAngle();
				return;
			}
		}

		Spin();
	}

	void ApproachFinalAngle() {

		yRot = Mathf.Lerp(yRot, finalAngle, Time.deltaTime * 8);
		transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, yRot, transform.localEulerAngles.z);

		float diff = Mathf.Abs( yRot - finalAngle);

		if (diff < 4) {
			yRot = finalAngle;
			if (finalizeSound != null && !playedFinalSound) {
                ///TODO WWISE Add spinnerclick, PITCH
				//SpiderSound.MakeSound("Play_Tick",Camera.main.gameObject);
				playedFinalSound = true;
			}
		}
	}

	public void Spin() {

		//smooth rotation
		if (!tick) {
			yRot += Time.deltaTime * spinSpeed * dir;
		}
		
		//ticking rotation
		if (tick) {
			
			tickTimer += Time.deltaTime;
			
			if (tickTimer > tickDelay) {
				Tick();
			}
			
			spinSpeedActual = Mathf.Lerp(spinSpeedActual, 0, Time.deltaTime * 5);
			yRot += Time.deltaTime * spinSpeedActual * dir;
		}
		
		//limits
		if (useLimits) {
			
			if (yRot > minMaxAngle.y) {
				dir = -1;
			}
			if (yRot < minMaxAngle.x) {
				dir = 1;
			}
			
		}
		transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, yRot, transform.localEulerAngles.z);
	}

	void Tick() {
		tickTimer = 0;
		spinSpeedActual = spinSpeed;

		if (tickSound != null) {
			//float rndmTickPitch = tickPitch + Random.Range(-.1f, .1f);
            //TODO WWISE RANDOM PITCH
            //SpiderSound.MakeSound("Play_Tick", Camera.main.gameObject);
            ////SpiderSound.MakeSound(Camera.main.transform.position, tickSound, 10, 100, 1, rndmTickPitch);
        }
	}

	public void End() {
		if (gotoFinalAngle) totalTime = maxWanderTime;
	}

	public void SpinForeverLocal() {
		if (gotoFinalAngle) spinForever = true;
	}

	public static void EndSpinners() {
		foreach (Spinner spinner in allSpinners) {
			spinner.End();
		}
	}

	public static void SpinForever() {
		foreach (Spinner spinner in allSpinners) {
			spinner.SpinForeverLocal();
		}
	}
}
