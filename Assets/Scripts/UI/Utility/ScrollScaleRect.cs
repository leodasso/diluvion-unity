using System.Collections;
using System.Collections.Generic;
using Diluvion;
using UnityEngine;
using Sirenix.OdinInspector;


/// <summary>
/// Using an input axis, can scale the attached rectTransform. Useful for stuff like zooming in / out with mouse wheel.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ScrollScaleRect : MonoBehaviour
{

	[MinMaxSlider(-1000, 1000, true)]
	[Tooltip("How far can the rect be sized up and down? (in pixels)")]
	public Vector2 sizeBounds = new Vector2(-200, 200);
	public float sensetivity = 1;
	RectTransform _rectTransform;


	Vector2 _initSizeDelta;
	Vector2 _additionalSize;

	// Use this for initialization
	void Start ()
	{
		_rectTransform = GetComponent<RectTransform>();
		_initSizeDelta = _rectTransform.sizeDelta;

	}
	
	// Update is called once per frame
	void Update ()
	{
		float delta = GameManager.Player().GetAxis("map zoom") * Time.unscaledDeltaTime;

		_additionalSize += Vector2.one * delta;
		
		// clamp the additional size
		_additionalSize = new Vector2(
			Mathf.Clamp(_additionalSize.x, sizeBounds.x, sizeBounds.y),
			Mathf.Clamp(_additionalSize.y, sizeBounds.x, sizeBounds.y));

		_rectTransform.sizeDelta = _initSizeDelta + _additionalSize;
	}
}
