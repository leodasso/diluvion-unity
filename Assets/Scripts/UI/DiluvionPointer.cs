using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Rewired;
using Diluvion;

public enum CursorMode {
	joystick,
	mouse
}

public class DiluvionPointer : MonoBehaviour {

	public static DiluvionPointer pointerInstance;

	public float lerpSpeed = 15;
    public float snapDist = 30;
    public float minInputToSelect = .4f;
    public float snapOffset;

	public bool snapToUI;
	public bool snapToInterior;

    public int pointerID = 0;

	/// <summary>
	/// The multiplier of speed for when hovering over an object.
	/// </summary>
	public float hoverSpeed = .5f;

	/// <summary>
	/// The move speed in pixels per second (used only for joystick input)
	/// </summary>
	public float moveSpeed = 100;

	/// <summary>
	/// The raw position of the cursor, before any snapping. 
	/// </summary>
	public Vector3 rawPosition;

	/// <summary>
	/// Cursor's position on screen from -1 to 1, with 0,0 at the center.
	/// </summary>
	public Vector3 normalizedPosition;

	public bool 		overUIElement;
	public GameObject 	hoveredObject;
	public GameObject	hoveredUI;
	public CursorMode 	cursorMode = CursorMode.mouse;

    GameObject lastHoveredObject; // memorize the last hovered interior object

    Vector3 snapDir;
    float snapPower;

	float realSpeed;
	Vector3 prevMousePos;
	Vector2 centerOrientPosition;
	Player player;
	Camera interiorCam;
	CanvasGroup canvasGroup;
    float cooldown = .3f;
    float timer = 0;


	// Use this for initialization
	void Start () {

        GameManager.PrepInput();

		player = ReInput.players.GetPlayer(0);

		if (InteriorView.Get())
			interiorCam = InteriorView.Get().localCam;

		prevMousePos = Input.mousePosition;
		rawPosition = prevMousePos;

		canvasGroup = gameObject.GetComponent<CanvasGroup>();
	}


	void Update() {

		if ( !CursorValid() ) {
			canvasGroup.alpha = 0;
            timer = cooldown;
			return;
		}

        // Have a cooldown so a click to exit a UI element doesnt also register as a cursor click
        if (timer > 0)
        {
            timer -= Time.unscaledDeltaTime;
            return;
        }

		canvasGroup.alpha = 1;

		// Place on top of all other UI elements
		transform.SetAsLastSibling();

		FindHoveredObjects();

        /*
		//Check player's click / accept button actions
		if (player.GetButtonDown("select")) {

			//else 
			if ( !OverUIElement()) {

                // If clicking where no selectable object is, un-focus the camera.
                //if (hoveredObject == null) OrbitCam.ClearFocus();

                // If clicking on an object, tell that object it was clicked.
                //else hoveredObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
			}
		}
        */
	}

    Vector2 joystickInput;
    // Update is called once per frame
    void LateUpdate () {
			
		// Get the player's joystick input
		joystickInput = player.GetAxis2D("select X", "select Y");

		// Check if joystick / mouse is being used
		if (joystickInput != Vector2.zero) 				cursorMode = CursorMode.joystick;
		else if (prevMousePos != Input.mousePosition) 	cursorMode = CursorMode.mouse;

		prevMousePos = Input.mousePosition;


        if ( !CursorValid() )
        {
            canvasGroup.alpha = 0;
            return;
        }

        realSpeed = moveSpeed;
		//if (hoveredObject != null) realSpeed *= hoverSpeed;

		// Move the cursor based on the player's current input
		if (cursorMode == CursorMode.joystick)
			rawPosition += (Vector3)joystickInput * Time.unscaledDeltaTime * realSpeed;
		else 
			rawPosition = Input.mousePosition;

		rawPosition = new Vector3( 
			Mathf.Clamp(rawPosition.x, 0, Screen.width),
			Mathf.Clamp(rawPosition.y, 0, Screen.height), 0);

		// Get normalized position
		Vector2 screenSize = new Vector2(Screen.width, Screen.height);
		centerOrientPosition = (Vector2)rawPosition - (screenSize * .5f);
		normalizedPosition = new Vector2(centerOrientPosition.x / screenSize.x, centerOrientPosition.y / screenSize.y) * 2;

        //ControlMovement();
        transform.position = rawPosition;

        if ( cursorMode == CursorMode.joystick && snapToInterior)
            MagneticLerp();
	}

    /// <summary>
    /// Magnetically moves towards last hovered object
    /// </summary>
    void MagneticLerp()
    {
        if ( lastHoveredObject == null ) {
            snapDir = Vector3.zero;
            return;
        }

        // Get the on-screen position of the last hovered object
        Vector3 hoveredObjScreenPos = interiorCam.WorldToScreenPoint(TopCenter(lastHoveredObject, snapOffset));

        // Get the distance between that and my position
        float dist = Vector3.Distance(transform.position, hoveredObjScreenPos);

        if ( dist > snapDist && lastHoveredObject != hoveredObject) {
            snapDir = Vector3.zero;
            return;
        }

        snapPower = 1 - (dist / snapDist);
        snapPower = Mathf.Clamp(snapPower, .1f, 1);

        snapDir = (hoveredObjScreenPos - transform.position).normalized;

        //transform.position = rawPosition = Vector3.Lerp(transform.position, hoveredObjScreenPos, Time.unscaledDeltaTime * lerpPower * lerpSpeed);
        transform.Translate(snapDir * snapPower * Time.unscaledDeltaTime * 100 * lerpSpeed);
        rawPosition = transform.position;
    }

    /// <summary>
    /// Returns the top center position of the given object
    /// </summary>
    Vector3 TopCenter(GameObject GO, float percentageFromTop)
    {
        Collider2D col = GO.GetComponent<Collider2D>();
        if ( col == null ) return GO.transform.position;

        float yOffset = col.bounds.size.y * percentageFromTop;

        return new Vector3(col.bounds.center.x, col.bounds.center.y + col.bounds.extents.y - yOffset, GO.transform.position.z);
    }


    bool OverUIElement()
    {
        // Check if the cursor is over a UI element
        if ( EventSystem.current.IsPointerOverGameObject() ) overUIElement = true;
        else overUIElement = false;
        return overUIElement;
    }

	/// <summary>
	/// Finds the UI objects and the 2D interior objects that the cursor is
	/// hovered over.
	/// </summary>
	void FindHoveredObjects() {

        if (!interiorCam || OverUIElement())
        {
            hoveredObject = null;
            return;
        }

		// Find what gameobject the cursor is hovering over.  Only applies if the cursor isn't over
		// a UI element. Omits anything that isn't interior; can't check for hover over a 3D object.
		Collider2D interiorCol = SpiderWeb.Cam.RayHit2D(interiorCam, rawPosition);

        if (interiorCol != null)
        {
            if (joystickInput.magnitude > minInputToSelect)
                lastHoveredObject = interiorCol.gameObject;

            hoveredObject = interiorCol.gameObject;
        }
        else hoveredObject = null;
	}


	/// <summary>
	/// Returns true if the cursor should be shown and active, false if otherwise.
	/// </summary>
	bool CursorValid()
    {
        //if (CheatManager.Get())
         //   if (CheatManager.Get().showing) return true;

        if ( !Cutscene.playerControl ) return false;

        // check if gamepad mode and there's a UI window open
        if ( cursorMode == CursorMode.joystick )
            if (UIManager.InteractiveWindowOpen()) return false;

        if ( GameManager.Get().currentState != GameState.Running ) return true;
        if (OrbitCam.CamMode() == CameraMode.Interior) 	return true;

		return false;
	}

	/// <summary>
	/// Returns the collider that the player is currently hovering
	/// </summary>
	public static GameObject HoveredCollider() {

		if (pointerInstance == null) 
			pointerInstance = FindObjectOfType<DiluvionPointer>();

        if ( pointerInstance == null ) return null;
		
		return pointerInstance.hoveredObject;
	}

	/// <summary>
	/// Returns the position of the pointer
	/// </summary>
	public static Vector3 CursorPosition() {

		if (pointerInstance == null) 
			pointerInstance = FindObjectOfType<DiluvionPointer>();

		return pointerInstance.rawPosition;
	}

	/// <summary>
	/// Returns the position of the cursor with 0,0 at center of screen, and -1 to 1 for edges.
	/// </summary>
	public static Vector3 CursorNormalizedPosition() {
		
		if (pointerInstance == null) 
			pointerInstance = FindObjectOfType<DiluvionPointer>();

		return pointerInstance.normalizedPosition;
	}
}
