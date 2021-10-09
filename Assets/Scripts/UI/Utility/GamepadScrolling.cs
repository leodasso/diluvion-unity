using UnityEngine;
using UnityEngine.UI;
using Rewired;
using HeavyDutyInspector;
using System.Collections;

[RequireComponent(typeof(ScrollRect))]
public class GamepadScrolling : MonoBehaviour {

    [Comment("Used in conjunction with Rewired input. Enter the name of the axis' used for scrolling below, and they can be used to control the scroll rect on this object.")]

    public Player player;

    public float sensetivity = 50;

    public string horizontalInput;
    public string verticalInput;

    ScrollRect scrollRect;

	// Use this for initialization
	void Start () {

        scrollRect = GetComponent<ScrollRect>();
        player = ReInput.players.GetPlayer(0);
	}
	
	// Update is called once per frame
	void Update () {

        Vector2 input = player.GetAxis2D(horizontalInput, verticalInput);
        scrollRect.normalizedPosition += input * sensetivity * Time.unscaledDeltaTime;
	
	}
}
