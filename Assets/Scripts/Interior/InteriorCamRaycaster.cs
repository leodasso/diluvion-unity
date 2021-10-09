using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using Diluvion;


/// <summary>
/// Attach this to a camera. Will cast from the camera it's attached to outward towards pointer, and 
/// call onClick, onRelease etc events on any clickable objects the cast hits. Used for interacting with interior
/// stuff!
/// </summary>
[RequireComponent(typeof(Camera))]
public class InteriorCamRaycaster : MonoBehaviour
{

    public LayerMask layermask;
    public float dragThreshhold = 5;

    public bool overUiElement;

    float _dragAmount;
    Vector2 _deltaPos;
    Vector2 _prevPos;
    IClickable _hovered;
    IDraggable _draggable;
    GameObject _selectedObj;
    List<GameObject> _casted = new List<GameObject>();
    Camera _cam;
    bool _dragging;
    Player _player;

    int _uiCheckBufferFrames = 3;
    int _uiCheckTimer;

    static InteriorCamRaycaster _instance;

    public static bool Dragging()
    {
        if (!_instance) return false;
        return _instance._dragging;
    }

    // Use this for initialization
    void Start()
    {
        _instance = this;
        _cam = GetComponent<Camera>();
        _player = GameManager.Player();
        _prevPos = Input.mousePosition;
    }

    // Update is called once per frame
    void Update()
    {
        GetDeltaPositions();

        // Point ray towards cursor
        Ray r = _cam.ScreenPointToRay(Input.mousePosition);

        Debug.DrawRay(r.origin, r.direction, Color.green, .1f);

        // Search all colliders for the top one
        _casted.Clear();

        if (!OverUIElement())
        {
            foreach (RaycastHit2D h in Physics2D.RaycastAll(r.origin, r.direction, 500, layermask))
            {
                // Cull out any non-clickable objects
                IClickable c = h.collider.gameObject.GetComponent<IClickable>();
                if (c == null) continue;

                _casted.Add(h.collider.gameObject);
            }
        }

        // Get the first casted object
        _selectedObj = null;
        if (_casted.Count > 0)
        {
            _casted.Sort(CompareDepth);
            _selectedObj = _casted.Last();
        }

        // pointer exits the object
        if (_selectedObj == null && _hovered != null)
        {
            _hovered.OnPointerExit();
            _hovered = null;
        }

        // pointer enters object
        if (_selectedObj != null && _hovered == null)
        {
            _hovered = _selectedObj.GetComponent<IClickable>();
            _hovered.OnPointerEnter();
        }

        // Pointer moves from one object to another without exiting
        if (_selectedObj != null && _hovered != null)
        {
            IClickable newClickable = _selectedObj.GetComponent<IClickable>();
            if (_hovered != newClickable)
            {
                _hovered.OnPointerExit();
                _hovered = newClickable;
                _hovered.OnPointerEnter();
            }
        }

        if (_player.GetButtonDown("select")) MouseDownActions();
        if (_player.GetButtonUp("select")) MouseUpActions();

        // If there's a draggable object selected...
        if (_draggable != null) DragUpdate();
    }

    /// <summary>
    /// Returns true if the pointer is currently over a UI element.
    /// </summary>
    /// <returns></returns>
    bool OverUIElement()
    {
        var eventSystem = UIManager.GetEventSystem();
        if (!eventSystem) return false;
        
        if (!eventSystem.IsPointerOverGameObject())
        {
            _uiCheckTimer++;
            if (_uiCheckTimer >= _uiCheckBufferFrames)
                overUiElement = false;
        }
        else
        {
            _uiCheckTimer = 0;
            overUiElement = true;
        }
        
        return overUiElement;
    }


    /// <summary>
    /// Given two game objects, sorts them for which one appears in front using their sprite renderers.
    /// Objects without sprite renderers are assumed to be behind ones with them.
    /// </summary>
    static int CompareDepth(GameObject a, GameObject b)
    {
        SpriteRenderer spriteA = a.GetComponent<SpriteRenderer>();
        SpriteRenderer spriteB = b.GetComponent<SpriteRenderer>();

        if (!spriteA && !spriteB) return 0;
        if (spriteA && !spriteB) return 1;
        if (!spriteA && spriteB) return -1;

        float layerValueA = SortingLayer.GetLayerValueFromID(spriteA.sortingLayerID);
        float layerValueB = SortingLayer.GetLayerValueFromID(spriteB.sortingLayerID);
        if (layerValueA != layerValueB)
            return layerValueA.CompareTo(layerValueB);

        return spriteA.sortingOrder.CompareTo(spriteB.sortingOrder);
    }

    void GetDeltaPositions()
    {
        //Get the world space delta of the cursor
        //Vector3 prevWorldPos = _cam.ScreenToWorldPoint((Vector3)_prevPos);
        //Vector3 currentWorldPos = _cam.ScreenToWorldPoint((Vector3)Input.mousePosition);
        //_worldSpaceDelta = currentWorldPos - prevWorldPos;

        // Get the delta position of the cursor
        _deltaPos = (Vector2)Input.mousePosition - _prevPos;
        _prevPos = Input.mousePosition;
    }

    void DragUpdate()
    {
        // If there's a draggable object selected, check how far the cursor has moved.
        // If it's moved beyond the threshhold, start dragging the selected draggable.
        if (!_dragging)
        {
            _dragAmount += _deltaPos.magnitude;
            if (_dragAmount >= dragThreshhold)
            {
                _dragging = true;
                _draggable.OnDragStart();
            }
            return;
        }

        Vector3 currentWorldPos = _cam.ScreenToWorldPoint(Input.mousePosition);

        // If already dragging, call onDrag with the distance it's moved (in world space)
        _draggable.OnDrag(currentWorldPos);
    }


    void MouseDownActions()
    {
        _draggable = _hovered as IDraggable;
        if (_hovered != null) _hovered.OnClick();
    }

    void MouseUpActions()
    {
        // End the dragging
        if (_dragging && _draggable != null) _draggable.OnDragEnd();

        // if wasn't dragging, call on release (mouse up)
        else if (_hovered != null) _hovered.OnRelease();


        else if (!overUiElement)
        {
//            Debug.Log("Mouse cursor is not over a UI element, overUIelement: " + overUiElement);
            OrbitCam.ClearFocus();
        }

        // Clear the drag
        _draggable = null;
        _dragging = false;
        _dragAmount = 0;
    }
}