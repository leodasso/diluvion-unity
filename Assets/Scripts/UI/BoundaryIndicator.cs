using System.Collections;
using System.Collections.Generic;
using Diluvion;
using UnityEngine;
using Sirenix.OdinInspector;

public class BoundaryIndicator : MonoBehaviour
{
	public ColorChildren colorChildren;
	public LineRenderer lineRenderer;
	public float alpha = 1;

	[Tooltip("If the player is within this distance to the boundary, it will begin to show the alpha")]
	public float maxDistance = 50;

	[ReadOnly, ShowInInspector]
	float _normalizedCloseness;

	[ReadOnly] 
	public Color currentColor;

	[ReadOnly, ShowInInspector]
	Vector3 _orientedPlayerPos;
	float _defaultLineWidth = 1;

	// Use this for initialization
	void Start ()
	{
		// Get the default line width so we can adjust the width depending on player's distance from boundary
		_defaultLineWidth = lineRenderer.widthMultiplier;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!PlayerManager.PlayerShip())
		{
			SetAlpha(0);
			return;
		}

		if (!transform.parent)
		{
			Debug.Log(name + " must be the child of a boundary to behave properly!", gameObject);
			return;
		}

		// Get the player's position relative to the boundary
		_orientedPlayerPos = transform.parent.InverseTransformPoint(PlayerManager.PlayerShip().transform.position);
		
		// Slide along the boundary to match the player's position
		transform.localPosition = new Vector3(_orientedPlayerPos.x, _orientedPlayerPos.y, 0);
		
		// Get the player's normalized distance from the boundary
		float normalizedDist = Mathf.Abs(_orientedPlayerPos.z / maxDistance);
		normalizedDist = Mathf.Clamp01(normalizedDist);
		_normalizedCloseness = 1 - normalizedDist;

		// Set the colors
		SetAlpha(_normalizedCloseness);
		
		// Set the line renderer points
		lineRenderer.SetPosition(0, transform.position);
		lineRenderer.SetPosition(1, PlayerManager.PlayerShip().transform.position);
	}

	/// <summary>
	/// Sets the alpha of all the sprites, and sets the line width. Sets width instead of alpha because it lets the line
	/// renderer's material be anything.
	/// </summary>
	void SetAlpha(float newAlpha)
	{
		alpha = Mathf.Clamp01(newAlpha);
		
		currentColor = new Color(1, 1, 1, alpha);
		colorChildren.SetColor(currentColor);

		lineRenderer.widthMultiplier = newAlpha * _defaultLineWidth;
	}
}
