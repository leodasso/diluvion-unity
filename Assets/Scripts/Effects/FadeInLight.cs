using UnityEngine;
using System.Collections;

/// <summary>
/// Fades the intensity of the attached light from given start intensity to the light's set intensity.
/// </summary>
[RequireComponent(typeof(Light))]
public class FadeInLight : MonoBehaviour {

	public float fadeTime;
	public float startIntensity;

	float initIntensity;
	Light myLight;

	// Use this for initialization
	IEnumerator Start() {

        myLight = GetComponent<Light>();
		initIntensity = myLight.intensity;
        myLight.intensity = startIntensity;

		float progress = 0;
		while (progress < 1) {

            myLight.intensity = Mathf.Lerp(startIntensity, initIntensity, progress);

			progress += Time.deltaTime / fadeTime;
			yield return null;
		}
		yield break;
	}
}
