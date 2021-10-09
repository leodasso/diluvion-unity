using UnityEngine;
using System.Collections;
using Cinemachine;
using Sirenix.OdinInspector;
using Diluvion;


[ExecuteInEditMode]
public class CinemaCam : MonoBehaviour
{

    public enum LookMode { none, player, orbitCamTarget, custom };

    public Vector2 offset;
    public LookMode lookMode = LookMode.none;

    [Range(10, 90)]
    public float FOV = 30;

    [MinMaxSlider(0.3f, 1000)]
    public Vector2 clippingPlanes = new Vector2(1, 300);

    [MinMaxSlider(0, 1000)]
    public Vector2 fogDistance = new Vector2(5, 100);

    [ShowIf("LookAtCustom")]
    public Transform subject;

    [ReadOnly, ToggleLeft]
    public bool controllingCam;

    [ToggleLeft]
    public bool toggleOtherObject;

    [ShowIf("toggleOtherObject"), Tooltip("Object to turn on / off for this camera")]
    public GameObject toggleObject;

    [ToggleLeft]
    public bool showing;

    [ToggleLeft]
    public bool freezeWhenViewing;

    [ToggleLeft]
    public bool allowPlayerInput;

    [ShowIf("allowPlayerInput")]
    public KeyCode toggleKey;

    Camera cam;
    Transform player;

    bool LookAtCustom ()
    {
        return (lookMode == LookMode.custom);
    }


    // Use this for initialization
    void Start ()
    {
        RemoveTestCam();
        if (toggleObject) toggleObject.SetActive(false);
    }

    // Update is called once per frame
    void Update ()
    {
        /*
        if (showing && !cam)
        {
            CreateCamera();
            return;
        }

        else if (!showing && cam)
        {
            RemoveCam();
            return;
        }
        */

        if (!cam) return;

        cam.transform.localPosition = offset;
        cam.fieldOfView = FOV;
        cam.orthographic = false;

        if (allowPlayerInput) PlayerInput();

        FOV = Mathf.Clamp(FOV, 5, 90);

        // Look at player
        if (lookMode == LookMode.player) LookAtPlayer();

        // look at custom target
        if (lookMode == LookMode.custom && subject)
            LookAt(subject);

        // look at orbit cam's target
        if (lookMode == LookMode.orbitCamTarget && OrbitCam.CurrentTarget())
            LookAt(OrbitCam.CurrentTarget());

        if (lookMode == LookMode.none) cam.transform.localEulerAngles = Vector3.zero;
    }

    void LookAtPlayer ()
    {
        if (PlayerManager.pBridge == null) return;
        player = PlayerManager.pBridge.transform;
        LookAt(player);
    }

    void LookAt (Transform t)
    {
        transform.rotation = Quaternion.LookRotation(t.position - transform.position, Vector3.up);
    }

    void Toggle ()
    {
        if (controllingCam) RemoveCam();
        else CreateCamera();
    }

    /// <summary>
    /// Activates this camera for the given number of seconds (in unscaled time)
    /// </summary>
    public void ShowFor(float seconds)
    {
        StartCoroutine(ShowForTime(seconds));
    }

    IEnumerator ShowForTime(float seconds)
    {
        //yield return new WaitForSeconds(.1f);

        FadeOverlay.FadeInThenOut(.5f, Color.black, CreateCamera);

        //yield return new WaitForSeconds(.5f);

        while (seconds > 0)
        {
            seconds -= Time.unscaledDeltaTime;
            yield return null;
        }

        FadeOverlay.FadeInThenOut(.5f, Color.black, RemoveCam);
    }

    /// <summary>
    /// Destroys local cam, and returns the normal camera to on
    /// </summary>
    [ButtonGroup("runtime")]
    public void RemoveCam ()
    {
        if (!Application.isPlaying)
        {
            RemoveTestCam();
            return;
        }

        controllingCam = false;
        if (cam) Destroy(cam.gameObject);

        GameManager.UnFreeze(this);

        if (toggleObject) toggleObject.SetActive(false);

        OrbitCam.ToggleCamera(true);

        showing = false;
    }

    /// <summary>
    /// Adds a local camera
    /// </summary>
    [ButtonGroup("runtime")]
    public void CreateCamera ()
    {
        showing = true;
        if (!Application.isPlaying)
        {
            Debug.Log("Creating test camera.");
            CreateTestCam();
            return;
        }

        if (freezeWhenViewing) GameManager.Freeze(this);

        if (toggleObject) toggleObject.SetActive(true);

        // Instantiate a copy of the camera
        cam = Instantiate(OrbitCam.Get().theCam);
        cam.transform.parent = transform;

        cam.nearClipPlane = clippingPlanes.x;
        cam.farClipPlane = clippingPlanes.y;

        Fog f = cam.GetComponent<Fog>();
        if (f)
        {
            f.startDistance = fogDistance.x;
            f.endDistance = fogDistance.y;
        }

        // turn off the orbit cam
        OrbitCam.ToggleCamera(false);

        controllingCam = true;
    }

    void CreateTestCam ()
    {
        GameObject newChild = new GameObject();
        newChild.transform.parent = transform;
        cam = newChild.AddComponent<Camera>();
    }

    void RemoveTestCam ()
    {
        controllingCam = false;
        if (cam)
        {
#if UNITY_EDITOR
            DestroyImmediate(cam.gameObject);
#else
            Destroy(cam.gameObject);
#endif
        }
    }

    #region testing and editor stuff

    #endregion

    void PlayerInput ()
    {
        // Adjust the FOV
        if (Input.GetKey(KeyCode.Keypad1)) FOV -= Time.unscaledDeltaTime * 3;
        if (Input.GetKey(KeyCode.Keypad2)) FOV += Time.unscaledDeltaTime * 3;
        if (Input.GetKey(KeyCode.Keypad4)) FOV -= Time.unscaledDeltaTime * 10;
        if (Input.GetKey(KeyCode.Keypad5)) FOV += Time.unscaledDeltaTime * 10;
        if (Input.GetKey(KeyCode.Keypad7)) FOV -= Time.unscaledDeltaTime * 30;
        if (Input.GetKey(KeyCode.Keypad8)) FOV += Time.unscaledDeltaTime * 30;

        // Move the offset with the arrow keys
        float x = 0;
        float y = 0;
        if (Input.GetKey(KeyCode.LeftArrow)) x -= Time.unscaledDeltaTime * 2;
        if (Input.GetKey(KeyCode.RightArrow)) x += Time.unscaledDeltaTime * 2;
        if (Input.GetKey(KeyCode.DownArrow)) y -= Time.unscaledDeltaTime * 2;
        if (Input.GetKey(KeyCode.UpArrow)) y += Time.unscaledDeltaTime * 2;

        offset += new Vector2(x, y);

        // Adjust depth of field
        if (Input.GetKeyDown(KeyCode.Backspace)) OrbitCam.Get().ToggleDof();
        if (Input.GetKey(KeyCode.Period)) OrbitCam.Get().AdjustDofDist(Time.unscaledDeltaTime * 8);
        if (Input.GetKey(KeyCode.Comma)) OrbitCam.Get().AdjustDofDist(-Time.unscaledDeltaTime * 8);

        if (Input.GetKeyDown(toggleKey)) Toggle();
    }
}
