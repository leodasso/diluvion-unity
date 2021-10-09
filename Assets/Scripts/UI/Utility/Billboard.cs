using UnityEngine;
using HeavyDutyInspector;
using System.Collections.Generic;
using TMPro;

public class Billboard : MonoBehaviour {

    public bool useInitAlpha = true;

    [HideConditional(true, "useInitAlpha", false)]
    public float maxAlpha = 1;

    public bool onlyRotateMainSprite = false;
    public Transform centerPoint;
    public SpriteRenderer mainSprite;
	List<SpriteRenderer> sprites = new List<SpriteRenderer>();

	public bool adjustAlpha = true;


	[HideConditional("adjustAlpha", true)]
	public float minCamDist = 15;

	[HideConditional("adjustAlpha", true)]
	public float maxCamDist = 30;

	float initAlpha = 1;
	float alpha = 1;

    TextMeshPro textMesh;

    Color color;

	// Use this for initialization
	void Start () {

        // check for a center point transform (used to fade the mesh)
        if ( centerPoint == null ) centerPoint = transform;

        // add all child sprites to list
        sprites.AddRange(GetComponentsInChildren<SpriteRenderer>());
        // add main sprite to list
        if ( mainSprite && !sprites.Contains(mainSprite)) sprites.Add(mainSprite);

		//sprite = GetComponent<SpriteRenderer>();
        textMesh = GetComponentInChildren<TextMeshPro>();

        if (mainSprite != null) {
		    initAlpha = mainSprite.color.a;
            color = mainSprite.color;
        }

        if ( textMesh != null ) {
            initAlpha = textMesh.color.a;
            color = textMesh.color;
        }

        if ( useInitAlpha ) maxAlpha = initAlpha;
	}
	
	// Update is called once per frame
	void Update () {

		if (Camera.main == null) return;

        if (onlyRotateMainSprite)
        {
            mainSprite.transform.rotation = Camera.main.transform.rotation;
        }else
		    transform.rotation = Camera.main.transform.rotation;

		if (!adjustAlpha) return;
		// Fade sprites that are closer to camera
		float dist = Vector3.SqrMagnitude(centerPoint.position - Camera.main.transform.position);
		float adjustedDist = dist - (minCamDist * minCamDist);
		float adjustedMaxDist = (maxCamDist - minCamDist);
		adjustedMaxDist = adjustedMaxDist * adjustedMaxDist;
		float ratio = adjustedDist / adjustedMaxDist;

		alpha = Mathf.Lerp(0, maxAlpha, ratio * 2);

        color = new Color(color.r, color.g, color.b, alpha);



        //if (sprite != null) sprite.color = color;
        for (int i = 0; i < sprites.Count; i++)
        {
            sprites[i].color = color;
        }

        if ( textMesh != null ) textMesh.color = color;
	}

	public void SetSprite(Sprite newSprite) 
	{
        if ( !mainSprite ) return;

        mainSprite.sprite = newSprite;
	}
}
