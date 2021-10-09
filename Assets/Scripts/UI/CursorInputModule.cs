using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using Rewired;

public class CursorInputModule : PointerInputModule {

	public static CursorInputModule cursorInputModule;

	public DiluvionPointer cursorObject;
	public PointerEventData pointer;
	public float scrollSensetivity = 10;

	Player player;

    // Use this for initialization
    protected override void Start()
    {
		base.Start();

		// Check for the cursor
		if (cursorObject == null) {
			Debug.LogError("No cursor object found!");
			Destroy(this);
		}

		player = ReInput.players.GetPlayer(0);
	}

	/// <summary>
	/// Returns the UI object being hovered by the pointer, if any.
	/// </summary>
	public static GameObject HoveredUIObject() {

		if (cursorInputModule == null) 
			cursorInputModule = GameObject.FindObjectOfType<CursorInputModule>();

		return cursorInputModule.pointer.pointerEnter;
	}
	
	public override void Process() {

		// Get the pointer
		GetPointerData(1, out pointer, true);

		// Set scrolling values on pointer
		Vector2 scrollInput = player.GetAxis2D("scroll X", "scroll Y") * scrollSensetivity;

		//invert horizontal scrolling
		scrollInput = Vector2.Scale(scrollInput, new Vector2(-1, 1));

		pointer.scrollDelta = scrollInput;

		// Raycast
		pointer.position = cursorObject.rawPosition;
		EventSystem.current.RaycastAll(pointer, this.m_RaycastResultCache);
		RaycastResult raycastResult = FindFirstRaycast( this.m_RaycastResultCache);
		pointer.pointerCurrentRaycast = raycastResult;

		// Process movement of the pointer
		this.ProcessMove(pointer);

		//this.ProcessDrag(pointer);


		// Process clicks
		if (player.GetButtonDown("accept")) {

			pointer.pressPosition = cursorObject.rawPosition;
			pointer.clickTime = Time.unscaledDeltaTime;
			pointer.pointerPressRaycast = raycastResult;

			pointer.dragging = true;

			pointer.clickCount = 1;
			pointer.eligibleForClick = true;

			if ( m_RaycastResultCache.Count > 0)
			{
				// Presses
				pointer.selectedObject = raycastResult.gameObject;
				pointer.pointerPress = ExecuteEvents.ExecuteHierarchy ( raycastResult.gameObject, pointer, ExecuteEvents.submitHandler );
				pointer.rawPointerPress = raycastResult.gameObject;

				//scrolling
				//ExecuteEvents.Execute(raycastResult.gameObject, pointer, ExecuteEvents.scrollHandler);
			}
			else 
				pointer.selectedObject = pointer.pointerPress = pointer.rawPointerPress = null;
		}

		// If the player isn't pressing 'accept'
		if (player.GetButtonUp("accept"))
		{
			pointer.dragging = false;

			pointer.clickCount = 0;
			pointer.eligibleForClick = false;
			pointer.pointerPress = null;
			pointer.rawPointerPress = null;
		}

		if ( m_RaycastResultCache.Count > 0) {

			// Dragging
			if (player.GetButton("accept")) {
				//ExecuteEvents.ExecuteHierarchy(raycastResult.gameObject, pointer, ExecuteEvents.dragHandler);
			}	

			ExecuteEvents.ExecuteHierarchy(raycastResult.gameObject, pointer, ExecuteEvents.scrollHandler);
		}
	}
}
