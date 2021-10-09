using UnityEngine;
using TMPro;
using System.Collections.Generic;
using HeavyDutyInspector;

[RequireComponent(typeof(CompassPointer))]
public class LandmarkPointer : MonoBehaviour {

	public float minDist = 10;
	public float maxDist = 100;
	public SpriteRenderer landmarkSprite;
	public Transform scaler;
	public TextMeshPro textMesh;

	public float minScale = .2f;
	public float maxScale = 1;

	[DynamicRange(0, "maxAlpha")]
	public float minAlpha = .4f;
	public float showNameThreshhold = .98f;

	[Range(0, 1)]
	public float maxAlpha = 1;

	LandMark displayedLandmark;
	List<SpriteRenderer> allSprites = new List<SpriteRenderer>();

    Billboard billboard;
	float dot;


    void Start()
    {
        billboard = GetComponentInChildren<Billboard>();
    }

	// Use this for initialization
	public void Init (LandMark newLandmark)
    {
		// Memorize my landmark
		displayedLandmark = newLandmark;
	
		// Add all my sprites to a list
		allSprites.AddRange( GetComponentsInChildren<SpriteRenderer>());

		GetComponent<CompassPointer>().SetPoint(displayedLandmark.transform.position);

		if (displayedLandmark.icon) landmarkSprite.sprite = displayedLandmark.icon;

		textMesh.text = displayedLandmark.LocalizedName();

        Color lmColor = displayedLandmark.color;
        //Color newColor = new Color(lmColor.r, lmColor.g, lmColor.b, alpha);
        foreach ( SpriteRenderer sr in allSprites ) sr.color = lmColor;

        textMesh.color = lmColor;
    }

	void Update() {

		// Take no action if my landmark is null
		if (!displayedLandmark) return;

		// Get a normalized distance so we can use it to set the alpha of the sprite.
		// at maxDist it'll have minAlpha, and at minDist it'll have maxAlpha
		float dist = Vector3.Distance(transform.position, displayedLandmark.transform.position);
		dist -= minDist;
		float compareDist = maxDist - minDist;
		float normalizedDist = dist / compareDist;
		normalizedDist = Mathf.Clamp01(normalizedDist);

		float alpha = Mathf.Lerp(maxAlpha, minAlpha, normalizedDist);
        if ( billboard ) billboard.maxAlpha = alpha;

		// Set scale
		float totalScale = Mathf.Lerp(maxScale, minScale, normalizedDist);
		Vector3 scale = Vector3.one * totalScale;
		scaler.transform.localScale = scale;

        /*
		// Show / hide name when pointer is near
		dot = Vector3.Dot(transform.forward, OrbitCam.Get().transform.forward);
		if (dot >= showNameThreshhold) 
			textMesh.color = lmColor;
		else textMesh.color = Color.clear;
        */
	}

}
