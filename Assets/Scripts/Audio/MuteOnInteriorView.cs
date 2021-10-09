using UnityEngine;
using System.Collections;
using Diluvion;

/// <summary>
/// Finds an audiosource component on the instance, and turns its volume down to 0 when the camera
/// is not in normal mode.
/// </summary>
public class MuteOnInteriorView : MonoBehaviour {

	public AudioSource audioSource;

	float volume = 1;

	// Use this for initialization
	void Start () {
		audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {

		if (audioSource == null) {
			Debug.Log("No audiosource linked.", gameObject);
			this.enabled = false;
		}

		OrbitCam cam = OrbitCam.Get();
		if (!cam) return;

		if (cam.cameraMode == CameraMode.Normal) volume = 1;
		else volume = 0;

		audioSource.volume = Mathf.Lerp(audioSource.volume, volume, Time.deltaTime * 8);	
	}
}
