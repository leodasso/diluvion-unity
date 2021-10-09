using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LandmarkMapIcon : Selectable {

	public Text lmName;
	public Image lmIcon;
	bool highlighted;
	float highlightedSize = 1.5f;			// when highlighted, size delta will be defaultSize * highlightedSize
	Vector2 size;
	Vector2 defaultSize;

	RectTransform rectTransform;

	protected override void Start() {

        base.Start();
		
		rectTransform = GetComponent<RectTransform>();
		defaultSize = rectTransform.sizeDelta;
		size = Vector2.zero;
	}

	// Use this for initialization
	public void Init(LandMark lm)
    {

		lmIcon.sprite = lm.icon;
		lmName.text = lm.LocalizedName();
	}
	
	// Update is called once per frame
	void Update () {

		// Lerp the size so it gets bigger when hovered
		Vector2 currentSize = defaultSize;
		if (highlighted) currentSize *= highlightedSize;

		size = Vector2.Lerp(size, currentSize, Time.unscaledDeltaTime * 12);
		rectTransform.sizeDelta = size;
	}

	public void Highlight(bool nowHighlighted) 
	{
		highlighted = nowHighlighted;

        // Sound / other effects goes here
        if (highlighted)
            SpiderWeb.SpiderSound.MakeSound("Play_Captain_Log_Chimes", gameObject);

    }
}
