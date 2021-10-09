using UnityEngine;
using UnityStandardAssets.ImageEffects;
using System.Collections;
using Cinemachine;
using SpiderWeb;
using Rewired;
using Diluvion.Ships;
using DUI;
using Sirenix.OdinInspector;
using UnityEngine.PostProcessing;

namespace Diluvion
{

    public enum CameraMode
    {
        Normal,
        ChangingTarget,
        Translate,
        Dolly,
        Interior,
        MaskTransition,
        None
    }

    public enum TransitionType
    {
        ToInterior,
        To3D
    }

    public class OrbitCam : MonoBehaviour
    {
        #region declare
        public static OrbitCam orbitCam;

        /// <summary>
        /// Prevents any 3D/2D transitions
        /// </summary>
        public static bool transitionLock;

        [TabGroup("General")]
        public CameraMode cameraMode = CameraMode.Normal;

        [TabGroup("General")]
        public float camMoveInput = 0;

        [TabGroup("General")]
        public bool allowCinematicMovement;

        [TabGroup("General"), EnableIf("allowCinematicMovement")]
        public float cinematicMoveSpeed = 2;

        [TabGroup("General"), EnableIf("allowCinematicMovement")]
        public float cinAccel = 5;          // acceleration of cinematic distance

        [TabGroup("views", "3D" )]
        public Transform target;

        [TabGroup("views", "3D" )]
        public Transform biasTarget = null;

        [Tooltip("This vCam is directly manipulated by this component. The actual camera is then controlled by cinemachine.")]
        [TabGroup("views", "3D" )]
        public CinemachineVirtualCamera vCam;

        [Tooltip("If the player's camera input totals more than this, no longer looks at bias target.")]
        [TabGroup("views", "3D" )]
        public float biasTargetBreak = 40;

        [TabGroup("views", "3D" ), PropertyOrder(-199)]
        public float distance = 7;

        [TabGroup("views", "3D"), PropertyOrder(-198)]
        public float additionalDistance
        {
            get { return _addlDist; }
            private set { _addlDist = value; }
        }
        float _addlDist;

        [TabGroup("views", "3D"), MinMaxSlider(-90, 90)]
        public Vector2 verticalDegreesClamp;

        [TabGroup("views", "3D"), Range(0, .75f)] public float aimDeadZone = .4f;

        [TabGroup("views", "3D"), ReadOnly]
        public Camera theCam;

        [TabGroup("views", "3D")] 
        public PostProcessingProfile postProcessingProfile;
        [TabGroup("views", "3D")] 
        [MinMaxSlider(0, 1, true)]
        [Tooltip("Vignetting is used to indicate the camera is very close to terrain. This determines what the min " +
                 "(camera has no obstruction) and the max (camera is fully pushed in by terrain) vignetting values are")]
        public Vector2 minMaxVignetting;

        [TabGroup("views", "3D"), ReadOnly]
        public float defaultFarClip = 0;
        
        [Tooltip("The ratio between the change in FOV and how much the offset is affected. " +
            "This will make it so that when the ship speeds up and FOV increases, the offset also increases" +
            "to maintain the ships position away from center screen.")]
        [TabGroup("views", "3D")]
        public float FOVOffsetRatio = 1;

        [TabGroup("views", "3D")]
        [Tooltip("When the camera has to push in towards the target because of nearby terrain, the FOV will approach this value. " +
                  "Use this to give a clearer view of the sub.")]
        public float collisionFov = 65;
        
        [TabGroup("views", "3D")]
        [ReadOnly]
        public float _normalizedCollisionAmount;

        [TabGroup("Collision")]
        public bool collisionDetect = true;

        [TabGroup("Collision"), EnableIf("collisionDetect")]
        public bool intersecting = false;

        [TabGroup("Collision"), EnableIf("collisionDetect")]
        public float collisionSpherecastRadius;

        [TabGroup("Collision"), EnableIf("collisionDetect")]
        public LayerMask collisionMask;

        [TabGroup("General")]
        public DepthOfField dof;

        [TabGroup("General")]
        public float dofTiming = 3;

        [TabGroup("views", "2D")]
        public float zoomOutLimit = 30;

        [TabGroup("views", "2D")]
        [Range(0, 1)]
        public float interiorZoom = 0.8f;

        [TabGroup("views", "2D")]
        public float sensitivity = 2;

        [TabGroup("views", "2D")]
        public float zoomOutWait = 3;               // Seconds to wait before beginning zoomout

        [TabGroup("views", "2D"), ReadOnly]
        public GameObject interiorFocus;

        [TabGroup("views", "2D"), ReadOnly]
        public InteriorManager viewedInterior;

        [TabGroup("views", "2D")]
        public float cushion = .5f;

        [TabGroup("views", "2D")]
        public float transitionTime = 1;

        [TabGroup("views", "2D")]
        public float maxParallax = 50;

        [TabGroup("views", "2D")]
        [Range(0, 1)]
        public float deadZone = .5f;

        [TabGroup("views", "2D")]
        public Vector2 focusOffset;
        private Vector2 _focusOffset;

        [TabGroup("views", "2D"), ReadOnly ]
        public bool inDeadZone;

        [TabGroup("Camera Shake")]
        public bool camShake = true;

        [TabGroup("Camera Shake")]
        public float stability = 3.5f;

        [TabGroup("Camera Shake")]
        public float shakeSpeed = 30;

        [TabGroup("Camera Shake")]
        public float maxDistForShake = 100;

        [TabGroup("Camera Shake")]
        public float shakeIntensity = 5;

        [TabGroup("Camera Shake")]
        public float interiorShake = 1;

        [TabGroup("Camera Shake"), ShowInInspector, ReadOnly]
        float _shakeMagnitude;
        float _sin;
        Vector3 _shakeOffset = Vector3.one;
        Vector3 _shakePosition;                      // what gets added to camera position / rotation

        Vector2 _clampedNormalizedInteriorCam;

        int _playerId = 0;
        CameraStats _targetStats;
        SideViewerStats _targetSideView;
        CameraQuality _camQuality;

        // Defaults, get memorized at start
        float _vertDegreesUp;
        float _vertDegreesDown;

        RaycastHit[] _allCamHit;
        float _xOffsetClamp = 99;
        Vector2 _normalOffset = new Vector2(0, 0);   //...x and y offset in normal mode
        Vector3 _eulerInput;                         //...player input to control the camera rotation
        const float BaseFov = 32;           //...base Field of View of the camera in normal mode
        float _normalFov = 45;           //...adjusted Field of View of the camera in normal mode
        float _fov;
        float _foVmultiplier;            //...Used to adjust the FOV when target changes speed
        float _foVadjustment;
        float _memFrustrumHeight;
        const float startOrtho = .5f;
        float _zoomOutTimer;
        float _initNearClip ;
        float _initFogEnd;
        float _initFogStart;
        float _xOffsetLerp;
        float _cinematicDistance;
        float _cinDistCurrentSpeed;
        Bounds _interiorBounds;
        float _interiorMoveSpeed;            //how fast the camera moves in interior view, b/t 0 and 1
        float _dofAperture = 0;
        const float blurryAperture = 24.35f;
        bool _changingTargets;
        bool _inTransition;
        float _transitionSpeedMultiplier = 1;        // Allow for faster transitions if player holds down the transition button
        float _playerZoom;

        public static float distMultiplier = 1.5f;
        public static float offsetMultiplier = -.5f;
        public static float joyMultiplier = 1;

        public static string prefsDistKey = "_distMult";
        public static string prefsOffsetKey = "_offsetMult";
        public static string prefsJoyCamSens = "_joyCamSens";
        #endregion

        #region static functions
        public static CameraMode CamMode()
        {
            if (!Exists()) return CameraMode.None;
            return Get().cameraMode;
        }

        
        
        public static Transform CurrentTarget()
        {
            if (!Exists()) return null;
            return Get().target;
        }
        
        public static float VisibleDistance()
        {
            return RenderSettings.fogEndDistance;  //Get().CamFog.endDistance;
        }

        /// <summary>
        /// Shake the camera.
        /// </summary>
        /// <param name="newMagnitude">Magnitude of this shake.</param>
        /// <param name="shakeLoc">The position of the source of the shake.</param>
        public static void ShakeCam(float magnitude, Vector3 position)
        {
            if (!Exists()) return;
            Get().Shake(magnitude, position);
        }

        public static bool TargetIsShip()
        {
            if (!Exists()) return false;
            return Get().TargIsShip();
        }

        public static bool Exists()
        {
            return (orbitCam != null);
        }

        public static OrbitCam Get()
        {
            if (orbitCam) return orbitCam;

            //If still null, instantiate from resources
            Debug.Log("Creating a new orbit cam instance.");

            if (Camera.main != null)
            {
                Debug.LogError("A main camera was already in the scene when creating orbit cam. This will " +
                               "cause errors. The camera will be destroyed.");
                
                Destroy(Camera.main.gameObject);
            }

            GameObject orbitCamPrefab = Resources.Load("OrbitCam") as GameObject;
            orbitCam = Instantiate(orbitCamPrefab).GetComponent<OrbitCam>();

            return orbitCam;
        }

        /// <summary>
        /// Set the offset the interior camera will have from the subject when focusing on something.
        /// </summary>
        public static void SetFocusOffset(Vector2 newOffset)
        {
            if (!Exists()) return;
            Get().focusOffset = newOffset;
        }

        /// <summary>
        /// Clears the interior focus offset back to vector2.zero
        /// </summary>
        public static void ClearFocusOffset()
        {
            SetFocusOffset(Vector2.zero);
        }

        public static Vector2 NormalizedInteriorCamPos()
        {
            return Get()._clampedNormalizedInteriorCam;
        }

        /// <summary>
        /// Turns the camera component on or off. Doesn't affect orbit cam movement
        /// </summary>
        /// <param name="includeInteriorCam">Should the interior cam also be toggled?</param>
        public static void ToggleCamera(bool active, bool includeInteriorCam = true)
        {
            Get().theCam.enabled = active;

            if (includeInteriorCam) InteriorView.ActivateCam(active);
        }

        public static Camera Cam()
        {
            if (!Exists()) return null;
            return Get().theCam;
        }

        public static CinemachineVirtualCamera VirtualCam()
        {
            if (!Exists()) return null;
            return Get().vCam;
        }

        public static void SetFov(float fov)
        {
            if (!Exists()) return;
            Get()._foVadjustment = fov;
        }

        public static float GetFov()
        {
            if (!Exists()) return 45;
            return VirtualCam().m_Lens.FieldOfView;
        }

        #region interior focusing
        /// <summary>
        /// If the interior focus is the given object, removes focus.
        /// </summary>
        public static void ClearFocus(GameObject focus)
        {
            if (!Exists()) return;
            if (Get().interiorFocus == focus) ClearFocus();
        }

        /// <summary>
        /// Request a transition between 3D & 2D modes.
        /// </summary>
        public static void RequestTransition(bool toInterior)
        {
            if (!Exists()) return;
            Get().TransitionRequest(toInterior);
        }

        /// <summary>
        /// Clears the interior focus. Returns true if there was a focus, false if there wasnt.
        /// </summary>
        public static bool ClearFocus()
        {
            if (!Exists()) return false;
            GameObject interiorFocus = Get().interiorFocus;
            if (!interiorFocus) return false;

            IClickable focusClickable = interiorFocus.GetComponent<IClickable>();
            focusClickable?.OnDefocus();

            Get().interiorFocus = null;
            Get().interiorZoom = Get()._playerZoom;
            
            return true;
        }

        /// <summary>
        /// Focus on the given interior object. This won't work in 3D view.
        /// </summary>
        public static void FocusOn(GameObject focus, bool zoom = true)
        {
            if (!focus) return;
            if (Get().interiorFocus == focus) return;
            
            ClearFocus();

            IClickable focusClickable = focus.GetComponent<IClickable>();
            focusClickable?.OnFocus();

            Get().interiorFocus = focus;

            if (zoom) Get().interiorZoom = .1f;
        }

        /// <summary>
        /// Attempts to focus on an inhabitant of the given name (interior view only)
        /// </summary>
        public static void FocusOn(CharacterInfo character)
        {
            Character focusedCrew = Get()._targetSideView.intMan.GetInhabitant(character);
            if (focusedCrew == null)
            {
                Debug.LogError("No crew found by name " + character.name);
                return;
            }

            FocusOn(focusedCrew.gameObject);
        }
        #endregion


        /// <summary>
        /// Gets all the values that come from player prefs. Use this if the settings have been changed recently
        /// </summary>
        public static void RefreshPlayerPrefsValues()
        {
            distMultiplier = PlayerPrefs.GetFloat(prefsDistKey, distMultiplier);
            offsetMultiplier = PlayerPrefs.GetFloat(prefsOffsetKey, offsetMultiplier);
            joyMultiplier = PlayerPrefs.GetFloat(prefsJoyCamSens, 1);
            
            //Debug.Log("Orbit cam values refreshed. Offset multiplier is now " + offsetMultiplier);
        }

        #endregion

        void Awake()
        {
            GameManager.PrepInput();
            
            // instantiate the camera prefab
            GameObject camPrefab = Resources.Load("main camera") as GameObject;
            GameObject newCam = Instantiate(camPrefab);

            // reference cam instance
            theCam = newCam.GetComponent<Camera>();
            _camQuality = theCam.GetComponent<CameraQuality>();
            _vertDegreesDown = verticalDegreesClamp.x;
            _vertDegreesUp = verticalDegreesClamp.y;

            if (dof)
            {
                dof.enabled = false;
                dof.aperture = 0;
            }

            RefreshPlayerPrefsValues();

            //memorize the camera's inital far clipping  plane
            defaultFarClip = theCam.farClipPlane;
            _initNearClip = theCam.nearClipPlane;

            _normalizedCollisionAmount = 1;
        }


        #region updates
        // Update is called once per frame
        void Update()
        {
            if (GameManager.State() == GameState.Dying) NormalCamera();
            if (Time.timeScale == 0) return;

            // Camera transition speed
            _transitionSpeedMultiplier = 1;

            // detect intensity of player's input.
            if (Cutscene.playerControl)
                camMoveInput += GameManager.Player().GetAxis2D("camX", "camY").magnitude;

            if (allowCinematicMovement)
            {
                // cinematic movement (for trailers)
                if (Input.GetKey(KeyCode.Period))
                {
                    _cinDistCurrentSpeed += cinAccel * Time.unscaledDeltaTime;
                }
                else if (Input.GetKey(KeyCode.Comma))
                {
                    _cinDistCurrentSpeed -= cinAccel * Time.unscaledDeltaTime;
                }
                else
                    _cinDistCurrentSpeed = Mathf.Lerp(_cinDistCurrentSpeed, 0, Time.unscaledDeltaTime * 5);

                _cinDistCurrentSpeed = Mathf.Clamp(_cinDistCurrentSpeed, -cinematicMoveSpeed, cinematicMoveSpeed);

                _cinematicDistance += Time.unscaledDeltaTime * _cinDistCurrentSpeed;
                _cinematicDistance = Mathf.Clamp(_cinematicDistance, -100, 10000);
            }


            // lerp the cam move input back to 0
            camMoveInput = Mathf.Lerp(camMoveInput, 0, Time.unscaledDeltaTime * 10);

            // Zoom out timer
            if (_zoomOutTimer < zoomOutWait) _zoomOutTimer += Time.unscaledDeltaTime;

            if (target == null) return;

            //adjust interior camera move speed based on if there's a target
            if (interiorFocus == null) _interiorMoveSpeed = Mathf.Lerp(_interiorMoveSpeed, 1, Time.unscaledDeltaTime / 5);
            else _interiorMoveSpeed = Mathf.Lerp(_interiorMoveSpeed, 0, Time.unscaledDeltaTime * 20);

            ControlCursor();

            if (!_changingTargets) transform.position = target.position;

            ShakeUpdate();

            if (cameraMode == CameraMode.Interior)
                if (!_inTransition) InteriorCamera();


            //update for Normal (3D) camera mode
            if (cameraMode == CameraMode.Normal && !_inTransition) NormalCamera();


            if (cameraMode == CameraMode.Normal && !_inTransition)
                _foVmultiplier = Mathf.Lerp(_foVmultiplier, 3, Time.unscaledDeltaTime * 10);

            else
                //remove the FOV multiplier so it doesnt interfere with any transition effects
                _foVmultiplier = Mathf.Lerp(_foVmultiplier, 0, Time.unscaledDeltaTime * 10);
        }


        void ShakeUpdate()
        {
            // create a sin wave
            _sin = Mathf.Sin(Time.time * shakeSpeed);

            // slow sin wave
            float slowSin = Mathf.Sin(Time.time * 2);
            // slow sin wave offset by half a second
            float slowCos = Mathf.Cos(Time.time * 2);

            _shakeOffset = new Vector3(slowSin, slowCos, slowSin);
            _shakeMagnitude = Mathf.Lerp(_shakeMagnitude, 0, Time.deltaTime * stability);
            _shakePosition = _shakeOffset * _sin * _shakeMagnitude * shakeIntensity;
        }

        /// <summary>
        /// Shake the camera.
        /// </summary>
        void Shake(float newMagnitude, Vector3 shakeLoc)
        {
            if (Time.timeScale == 0) return;

            float dist = Vector3.Distance(transform.position, shakeLoc);
            if (dist > maxDistForShake) return;

            // Get a shake intensity between 0 and 1 based on distance from the shake source
            float normalizedDist = Mathf.Clamp01((maxDistForShake - dist) / maxDistForShake);

            // Get the total magnitude based on magnitude of shake and distance from the shake source
            float totalMagnitude = normalizedDist * newMagnitude;

            // if shake magnitude is lower than the new magnitude, set it to new magnitude
            _shakeMagnitude = Mathf.Clamp(_shakeMagnitude, totalMagnitude, 999);
        }

        bool _distRoutineBreak = false;

        /// <summary>
        /// Sets the additional distance to the given value.
        /// </summary>
        /// <param name="dist">Additional distance. Gets added on to the distance of the target.</param>
        /// <param name="time">Time in unscaled time it will take to lerp to the new distance</param>
        public void SetAddlDist(float dist, float time)
        {
            _distRoutineBreak = true;
            StartCoroutine(ChangeAddlDist(dist, time));
        }

        /// <summary>
        /// Removes any additional distance, returning the camera to default.
        /// </summary>
        /// <param name="time">Time in unscaled time it will take to lerp back to default distance.</param>
        public void RemoveAddlDist(float time)
        {
            _distRoutineBreak = true;
            StartCoroutine(ChangeAddlDist(0, time));
        }

        IEnumerator ChangeAddlDist(float newDist, float time)
        {
            float currDist = additionalDistance;
            float prog = 0;

            // Wait a frame to allow previous routine to break
            yield return null;
            _distRoutineBreak = false;

            while (prog < 1)
            {
                // Break the routine
                if (_distRoutineBreak)
                {
                    _distRoutineBreak = false;
                    Debug.Log("Distance routine was broken. " + Time.unscaledTime);
                    yield break;
                }

                prog += Time.unscaledDeltaTime / time;
                additionalDistance = Calc.EaseBothLerp(currDist, newDist, prog);
                yield return null;
            }

            additionalDistance = newDist;
            yield break;
        }

        /// <summary>
        /// Returns the multiplier value for player ship, and 1 for other targets. (such as towns, salvage, etc
        /// </summary>
        /// <returns></returns>
        float DistanceMultiplier()
        {
            if (target == null) return 1;
            if (OnPlayerShip()) return distMultiplier;
            return 1;
        }

        Vector2 _dynamicBaseOffset = new Vector2();
        
        /// <summary>
        /// Moves the virtual camera.
        /// </summary>
        void MoveCam()
        {
            if (!target) return;

            //if the target has camera stats...
            if (_targetStats)
            {
                //use the distance specified by the target
                distance = Mathf.Lerp(distance, _targetStats.cameraStartDistance * DistanceMultiplier(), Time.unscaledDeltaTime * 5);

                //determine the camera offset
                float FOVChange = _normalFov - BaseFov;

                _dynamicBaseOffset = _targetStats.baseOffset * ((FOVChange * FOVOffsetRatio) + 1);

                // If viewing the player ship, apply offset multiplier
                if (PlayerManager.PlayerShip() == target.gameObject)
                {
                    _dynamicBaseOffset = new Vector2(_dynamicBaseOffset.x * offsetMultiplier, _dynamicBaseOffset.y);
                }
                _normalOffset = Vector2.Lerp(_normalOffset, _dynamicBaseOffset, Time.unscaledDeltaTime * 5);
            }

            //if the target has rigidbody, adjust FOV based on velocity
            Rigidbody targetRB = target.GetComponent<Rigidbody>();
            if (targetRB)
            {
                float totalVel = targetRB.velocity.magnitude * _foVmultiplier;
                float newFOV = BaseFov + totalVel * 1.5f;
                _normalFov = Mathf.Clamp(newFOV, BaseFov, 75) + _foVadjustment;
            }
        }

        /// <summary>
        /// Update for the camera's normal 3D mode
        /// </summary>
        void NormalCamera()
        {

            if (theCam == null) return;
            if (_inTransition) return;
            
            SetVignetting(_normalizedCollisionAmount);

            // Reset the interior focus
            interiorFocus = null;

            //lerp the far clip to original position
            vCam.m_Lens.FarClipPlane = Mathf.Lerp(vCam.m_Lens.FarClipPlane, defaultFarClip, Time.unscaledDeltaTime * 5);
            vCam.m_Lens.NearClipPlane = Mathf.Lerp(vCam.m_Lens.NearClipPlane, _initNearClip, Time.unscaledDeltaTime * 5);

            MoveCam();

            //control FOV
            _fov = Mathf.Lerp(_fov, _normalFov, Time.unscaledDeltaTime * 6);

            if (!Cutscene.playerControl) return;
            
            // break the bias target if player tries to look away
            if (camMoveInput > biasTargetBreak) biasTarget = null;

            //bias rotation towards a target
            if (biasTarget != null)
            {
                Quaternion look = Quaternion.LookRotation((biasTarget.transform.position - target.transform.position).normalized, transform.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.unscaledDeltaTime * 1);
            }

            ApplyCamStats();
            
            // have the ship invisibler set the ship's opacity based on how close the collision is
            ShipInvisibler.SetOpacity(Mathf.Lerp(.5f, 1, _normalizedCollisionAmount));
        }

        /// <summary>
        /// Rotates the camera based on player input.
        /// </summary>
        public void Rotate3DCam(Vector2 input)
        {
            // Joystick axis is between -1 and 1, which is far less sensetive than mouse. Use a multiplier if the last 
            // controller was a joystick to fix this.
            float joy_m = 1;
            if (Controls.LastUsedControllerType() == ControllerType.Joystick)
                joy_m = joyMultiplier * 10;

            //Rotate the camera based on the players input
            Vector2 rotateInput = input * 10 * joy_m;

            //clamp the vertical rotation
            _eulerInput = new Vector3(transform.eulerAngles.x - rotateInput.y, transform.eulerAngles.y + rotateInput.x, 0);
            _eulerInput = new Vector3(Mathf.Clamp(Calc.CleanAngle180(_eulerInput.x), verticalDegreesClamp.x, verticalDegreesClamp.y), _eulerInput.y, 0);

            //add the rotate input to the Y euler, and clamp it before applying it to the transform.
            transform.eulerAngles = new Vector3(_eulerInput.x, _eulerInput.y, 0);
        }

        void ClampCamera(float upperClamp, float lowerClamp)
        {
            float xRot = Mathf.Clamp( Calc.CleanAngle180(transform.eulerAngles.x), lowerClamp, upperClamp);
            transform.eulerAngles = new Vector3(xRot, transform.eulerAngles.y, 0);
        }


        float _zoomDelta;
        /// <summary>
        /// Controls camera in 2D mode. Smoothly moves cam to focus on interior targets
        /// </summary>
        void InteriorCamera()
        {
            transform.rotation = SideViewRotation();

            Transform interiorRoot = _targetSideView.intMan.transform;

            float lerpSpeed = Time.unscaledDeltaTime * 10;
            
            // zoom in / out input
            _zoomDelta = GameManager.Player().GetAxis("2d cam zoom");
            _playerZoom += -_zoomDelta * Time.unscaledDeltaTime * 3;

            _playerZoom = Mathf.Clamp01(_playerZoom);
            
            // lerp the focus offset
            _focusOffset = Vector2.Lerp(_focusOffset, focusOffset, lerpSpeed);

            //if there's an object to focus on
            if (interiorFocus != null)
            {
                _zoomOutTimer = 0;

                //Find an appropriate size for the orthographic camera 
                Renderer focusedRenderer = interiorFocus.GetComponentInChildren<Renderer>();

                //get the position of the interior focus object.
                Vector3 interiorFocusPos = interiorFocus.transform.position;
                if (focusedRenderer != null) 
                    interiorFocusPos = focusedRenderer.bounds.center;

                //clamp interior focus position
                interiorFocusPos = InteriorClampedPosition(interiorFocusPos);

                //get relative position of object
                Vector3 relativePos = interiorFocusPos - interiorRoot.position + (Vector3)_focusOffset;

                //lerp the offset to match the relative position of the focused object
                Vector2 newOffset = new Vector2(relativePos.x, relativePos.y);
                _normalOffset = Vector2.Lerp(_normalOffset, newOffset, lerpSpeed / 2);
                
            }
            
            // Free, un-focused camera
            else if (!UIManager.Locked())
            {
                if (_zoomOutTimer >= zoomOutWait)
                {
                    // Return zoom to the player zoom
                    interiorZoom = _playerZoom;
                    
                    // check dead zone
                    Vector2 normalizedCursor = DiluvionPointer.CursorNormalizedPosition();

                    if (!CursorInDeadZone(normalizedCursor))
                        _normalOffset += normalizedCursor * sensitivity * theCam.orthographicSize * Time.unscaledDeltaTime;

                    // Clamp cam position to interior bounds
                    _normalOffset = InteriorClampedPosition(_normalOffset);
                }
            }
  
            theCam.orthographicSize = Mathf.Lerp(theCam.orthographicSize, InteriorOrthoSize(), lerpSpeed / 2);

            ApplyCamStats();
            
            ShipInvisibler.SetOpacity(1);
        }

        float InteriorOrthoSize()
        {
            float minSize = Mathf.Clamp(_targetSideView.minOrthoSize, .2f, 1000);
            return Mathf.Lerp(minSize, _targetSideView.defaultOrthoSize * 1.5f, interiorZoom);
        }
        
        
        

        bool CursorInDeadZone(Vector2 cursorPos)
        {

            Vector2 absCursorPos = new Vector2(Mathf.Abs(cursorPos.x), Mathf.Abs(cursorPos.y));

            if (absCursorPos.x > deadZone || absCursorPos.y > deadZone) inDeadZone = false;
            else inDeadZone = true;

            return inDeadZone;
        }


        /// <summary>
        /// Returns a camera position clamped to the bounds of the current interior.
        /// </summary>
        Vector3 InteriorClampedPosition(Vector3 inputPos)
        {
            float totalCushion = (cushion + _targetSideView.cushion) * theCam.orthographicSize;

            var b = _interiorBounds;

            //get max and min coords the camera can go to
            float minX = Mathf.Clamp(b.min.x + totalCushion, -100000, b.max.x);
            float maxX = Mathf.Clamp(b.max.x - totalCushion, b.min.x,  100000);
            
            float minY = Mathf.Clamp(b.min.y + totalCushion, -100000, b.max.y);
            float maxY = Mathf.Clamp(b.max.y - totalCushion, b.min.y,  100000);

            maxX = Mathf.Clamp(maxX, minX + .2f, 10000);
            maxY = Mathf.Clamp(maxY, minY + .2f, 10000);

            //Get final coord bounds
            Vector2 xBounds = new Vector2(minX, maxX);
            Vector2 yBounds = new Vector2(minY, maxY);

            // clamp the input position by bounds
            float xPos = Mathf.Clamp(inputPos.x, xBounds.x, xBounds.y);
            float yPos = Mathf.Clamp(inputPos.y, yBounds.x, yBounds.y);

            _clampedNormalizedInteriorCam = new Vector2(
                NormalizedPos(xBounds.x, xBounds.y, xPos),
                NormalizedPos(yBounds.x, yBounds.y, yPos));

            return new Vector3(xPos, yPos, inputPos.z);
        }


        float NormalizedPos(float min, float max, float input)
        {
            float length =  max - min;
            length = Mathf.Clamp(length, .1f, 9999);
            float output = input / length;

            output -= .5f;
            return output;
        }

        #endregion

        #region depth of field
        bool DofAllowed()
        {
            if (_camQuality == null) return true;
            return _camQuality.dofActive;
        }

        public void ToggleDof()
        {
            
            if (!dof) return;

            if (!dof.enabled && DofAllowed()) BeginDof();
            else EndDof();
            
        }

        public void BeginDof()
        {
            if (!dof) return;

            if (!DofAllowed())
            {
                dof.enabled = false;
                return;
            }

            dof.enabled = true;

            StartCoroutine(AdjustAperture(blurryAperture, true, dofTiming));
        }

        public void EndDof()
        {
            if (!dof) return;
            if (!dof.enabled) return;
            StartCoroutine(AdjustAperture(0, false, dofTiming));
        }

        public void AdjustDofDist(float adjustment)
        {
            if (!dof) return;
            if (!dof.enabled) return;

            dof.focalLength += adjustment;
        }

        IEnumerator AdjustAperture(float finalAperture, bool setEnabled, float duration, bool clearTarget = false)
        {
            if (!DofAllowed())
            {
                if (dof) dof.enabled = false;
                yield break;
            }

            float startAperture = dof.aperture;
            float progress = 0;

            while (progress < 1)
            {
                dof.aperture = Mathf.Lerp(startAperture, finalAperture, progress);

                progress += Time.unscaledDeltaTime / duration;
                yield return null;
            }

            if (clearTarget) ClearDofTarget();

            dof.aperture = finalAperture;
            dof.enabled = setEnabled;
            yield break;
        }

        void SetDofTarget(Transform target)
        {
            if (!dof) return;
            dof.focalTransform = target;
        }

        void ClearDofTarget()
        {
            if (dof) dof.focalTransform = null;
        }

        #endregion

        /// <summary>
        /// Clamp the angles on the camera's vertical movement
        /// </summary>
        /// <param name="newLowerClamp"></param>
        public void SetClampAngles(float newLowerClamp)
        {
            SetClampAngles(newLowerClamp, verticalDegreesClamp.y);
        }

        public void SetClampAngles(float newLowerClamp, float newUpperClamp)
        {
            Debug.Log("Setting clamp angles to " + newLowerClamp + " and " + newUpperClamp);
            verticalDegreesClamp.x = newLowerClamp;
            verticalDegreesClamp.y = newUpperClamp;
        }

        public void DefaultClampAngles()
        {
            verticalDegreesClamp.x = _vertDegreesDown;
            verticalDegreesClamp.y = _vertDegreesUp;
        }

        /// <summary>
        /// Returns the correct rotation for side view of the given target
        /// </summary>
        /// <param name="clippingMultiplier"> Multiplies the clipping planes</param>
        Quaternion SideViewRotation()
        {
            if (target == null) return Quaternion.identity;

            //get the rotation for side view
            //for ships, zero out X and Z so that it can free rotate in side view
            Vector3 targetEuler = new Vector3(0, target.transform.eulerAngles.y, target.transform.eulerAngles.z);

            //for towns, outposts, everything else, just get the exact rotation
            if (!TargetIsShip() || _targetSideView.overrideAsStatic) targetEuler = target.transform.eulerAngles;

            return Quaternion.Euler(targetEuler) * Quaternion.Euler(_targetSideView.sideViewRotation);
        }

        Vector3 _targetOffsetPosition;
        /// <summary>
        /// Applies the main position, child camera's offset position
        /// </summary>
        void ApplyCamStats()
        {
            if (target == null) return;

            //Lerp animate the xOffset
            _xOffsetLerp = Mathf.Lerp(_xOffsetLerp, _normalOffset.x, Time.unscaledDeltaTime * 10);

            if (cameraMode == CameraMode.Normal)
                _xOffsetLerp = Mathf.Clamp(_xOffsetLerp, 0, _xOffsetClamp);

            float totalDist = distance + _cinematicDistance;
            
            // Add additional distance for normal camera mode only
            if (cameraMode == CameraMode.Normal) totalDist += additionalDistance;

            _targetOffsetPosition = new Vector3(_xOffsetLerp, _normalOffset.y, -totalDist);
            
            //control the local position of the camera.  (offset x & y, distance)
            vCam.transform.localPosition = _targetOffsetPosition;

            if (camShake)
            {
                if (cameraMode == CameraMode.Interior) vCam.transform.localPosition += (_shakePosition * interiorShake);
                if (cameraMode == CameraMode.Normal) vCam.transform.localEulerAngles = new Vector3(_shakePosition.x, _shakePosition.y, 0);
            }

            // Control the FOV
            vCam.m_Lens.FieldOfView = Mathf.Lerp( collisionFov, _fov, _normalizedCollisionAmount);

            DetectCollision();
        }

        /*
        CinemachinePostFX _postEffects;
        CinemachinePostFX PostEffects()
        {
            if (_postEffects != null) return _postEffects;

            _postEffects = vCam.GetComponent<CinemachinePostFX>();

            if (!_postEffects)
            {
                Debug.LogError("No Cinemachine Post FX component could be found on the vCam. :(");
                return null;
            }

            return _postEffects;
        }
        */


        /// <summary>
        /// Applies correct clipping planes and fog for the given dolly progress.
        /// </summary>
        /// <param name="clippingMultiplier">Multiplies the width of the visible area around the target. Increase this if 
        /// you see some clipping during transitions.</param>
        void SetClippingPlanes(float dollyProgress, float clippingMultiplier)
        {
            //Get the ideal distance for the near clipping plane by subtracting the target's near clip from the 
            //current distance between camera and target
            float distToNearPlane = Mathf.Clamp(distance - _targetSideView.nearClip * clippingMultiplier, .1f, 1000);

            //Do the same process for the far clipping plane
            float distToFarPlane = Mathf.Clamp(distance + _targetSideView.farClip * clippingMultiplier, distToNearPlane + .1f, 5000);

            //Lerp between the 3D clip plane and the 2D clip plane
            vCam.m_Lens.NearClipPlane = distToNearPlane;
            vCam.m_Lens.FarClipPlane = Mathf.Lerp(defaultFarClip, distToFarPlane, dollyProgress);

            //Adjust the fog to fit with the change in distance, keeping the target visible. total fog distance is the 
            float fogDist = distToFarPlane + 15;
            
            RenderSettings.fogStartDistance = Mathf.Lerp(_initFogStart,fogDist, dollyProgress);
            RenderSettings.fogEndDistance = Mathf.Lerp(_initFogEnd, fogDist, dollyProgress);
        }



        /// <summary>
        /// Controls locking / visibility of cursor based on camera mode
        /// </summary>
        void ControlCursor()
        {
            if (cameraMode == CameraMode.Interior)
            {
                if (Application.isEditor) Cursor.lockState = CursorLockMode.None;
                else Cursor.lockState = CursorLockMode.Confined;
                //Cursor.visible = true;
            }
            else if (PlayerManager.IsPlayerAlive())
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        RaycastHit _collisionHit;
        RaycastHit _nearestHit;
        
        void DetectCollision()
        {
            _normalizedCollisionAmount = 1;
            
            if (_changingTargets) return;
            if (target == null || _targetStats == null) return;
            if (!OnPlayerShip()) return;
            if (cameraMode != CameraMode.Normal) return;
            if (!collisionDetect) return;

            // Get the maximum distance from target. For player ship, this includes the dist multiplier
            float maxDist = _targetStats.cameraStartDistance * DistanceMultiplier();

            Vector3 dir = vCam.transform.position - target.transform.position;
            dir.Normalize();

            Ray colRay = new Ray(target.transform.position, dir);

            //SPherecast to optimal position
            _allCamHit = Physics.SphereCastAll(colRay, collisionSpherecastRadius, maxDist + collisionSpherecastRadius, collisionMask);

            // Check all the hits to find the nearest valid hit
            float hitDist = 1000;
            for (int i = 0; i < _allCamHit.Length; i++)
            {
                _collisionHit = _allCamHit[i];
                //ignore ships
                if (_collisionHit.collider.gameObject.GetComponent<ShipMover>() != null) continue;
                //ignore triggers
                if (_collisionHit.collider.isTrigger) continue;

                //if this collision distance is shorter than any other in the loop so far, set it as the new max distance
                if (_collisionHit.distance < hitDist)
                {
                    hitDist = _collisionHit.distance;
                    _nearestHit = _collisionHit;
                }
            }

            // If there were no hits, then there's nothing to collide with! Do nothing.
            if (_allCamHit.Length < 1)
            {
                intersecting = false;
                return;
            }
            intersecting = true;
            
            //ShipInvisibler.SetInvisible(.2f);

            // Now we have the nearest hit. Get the distance from target to that point
            float distToContact = Vector3.Distance(target.transform.position, _nearestHit.point);

            // Now that we have the distance, just subtract the spherecast radius (to give a cushion between camera and wall)
            distToContact -= collisionSpherecastRadius;

            // Clamp the distance to the contact
            distToContact = Mathf.Clamp(distToContact, 1, maxDist);

            Vector3 newCamPoint = colRay.GetPoint(distToContact);
            vCam.transform.position = newCamPoint;
            
            // now get the normalized value of the distance vs what the distance should be
            _normalizedCollisionAmount = distToContact / maxDist;
            _normalizedCollisionAmount = Mathf.Clamp01(_normalizedCollisionAmount);
        }

        void SetVignetting(float normalizedValue)
        {
            if (!postProcessingProfile) return;
            postProcessingProfile.vignette.enabled = true;

            var settings = postProcessingProfile.vignette.settings;
            settings.intensity = Mathf.Lerp(minMaxVignetting.y, minMaxVignetting.x, normalizedValue);
            postProcessingProfile.vignette.settings = settings;
        }

        /// <summary>
        /// Is the camera currently looking at player ship?
        /// </summary>
        bool OnPlayerShip()
        {
            if (PlayerManager.PlayerShip() == null || target == null) return false;
            if (PlayerManager.PlayerShip() == target.gameObject) return true;
            return false;
        }


        #region transition camera mode

        /// <summary> Returns the unscaled delta time * the transition speed multiplier </summary>
        float TransitionDeltaTime()
        {
            return Time.unscaledDeltaTime * _transitionSpeedMultiplier;
        }

        /// <summary>
        /// Translates the camera to the target as part of the transition between 3D / 2D modes. 
        /// </summary>
        /// <param name="translateProgress">Translate progress - 0 is 3D, 1 is towards 2D, at the side view rotation with 0 offset.</param>
        IEnumerator TranslateAndDolly(int direction, float start, float duration)
        {

            cameraMode = CameraMode.Translate;

            float progress = start;
            Quaternion standardRotation = Quaternion.Euler(_eulerInput);
            float startingHeight = FrustrumHeightAtDistance(ActualDistance());

            //Vector2 offset = Vector2.zero;
            Vector2 offset = _interiorOffset;
            if (direction < 0) offset = _normalOffset;

            while (!LerpOutOfBounds(direction, progress))
            {

                // Wait for the update
                yield return null;

                float easeLerpValue = Calc.EaseBothLerp(0, 1, progress);

                // Lerp the camera offsets
                _normalOffset = Vector2.Lerp(_targetStats.baseOffset, offset, easeLerpValue);

                // Lerp the rotation
                transform.rotation = Quaternion.Lerp(standardRotation, SideViewRotation(), easeLerpValue);

                //adjust distance of camera from target
                distance = Calc.EaseBothLerp(_targetStats.cameraStartDistance * DistanceMultiplier(), 200, easeLerpValue);

                //center rotation of the child camera
                vCam.transform.localEulerAngles = Vector3.zero;

                //adjust the camera's FOV to keep the target size on screen the same
                _fov = FOVForHeightAndDistance(startingHeight, distance);

                ApplyCamStats();

                // adjust clipping planes
                SetClippingPlanes(easeLerpValue, 2);

                progress += TransitionDeltaTime() * direction / duration;
            }
        }


        /// <summary>
        /// Translates the camera to the target as part of the transition between 3D / 2D modes. 
        /// </summary>
        /// <param name="translateProgress">Translate progress - 0 is 3D, 1 is towards 2D, at the side view rotation with 0 offset.</param>
        IEnumerator TranslateCamera(int direction, float start, float duration)
        {

            cameraMode = CameraMode.Translate;

            float progress = start;
            float FOVprogress = 0;
            float initDist = 0;
            Quaternion standardRotation = Quaternion.Euler(_eulerInput);

            yield return new WaitForEndOfFrame();

            float initFOV = _fov;

            //Vector2 offset = Vector2.zero;
            Vector2 offset = _interiorOffset;
            if (direction < 0)
            {
                offset = _normalOffset;
                initDist = distance;
            }

            while (!LerpOutOfBounds(direction, progress))
            {

                // Wait for the update
                yield return null;

                float easeLerpValue = Calc.EaseInLerp(0, 1, progress);

                // Lerp the camera offsets
                _normalOffset = Vector2.Lerp(_targetStats.baseOffset, offset, easeLerpValue);

                // Lerp the rotation
                transform.rotation = Quaternion.Lerp(standardRotation, SideViewRotation(), easeLerpValue);

                //center rotation of the child camera
                vCam.transform.localEulerAngles = Vector3.zero;

                _fov = Mathf.Lerp(initFOV, BaseFov, FOVprogress);

                if (direction < 0)
                {
                    distance = Mathf.Lerp(_targetStats.cameraStartDistance * DistanceMultiplier(), initDist, easeLerpValue);
                }

                ApplyCamStats();

                vCam.m_Lens.NearClipPlane = Mathf.Lerp(vCam.m_Lens.NearClipPlane, _initNearClip, Time.unscaledDeltaTime * 5);

                float delta = TransitionDeltaTime() / duration;
                FOVprogress += delta;
                progress += delta * direction;
            }
        }


        /// <summary>
        /// Dollies the camera back, adjusting the field of view as well, creating an effect of moving from 3D to 2D.
        /// </summary>
        IEnumerator DollyCamera(int direction, float start, float duration)
        {
            cameraMode = CameraMode.Dolly;
            float progress = start;
            float startingHeight = FrustrumHeightAtDistance(ActualDistance());

            while (!LerpOutOfBounds(direction, progress))
            {

                yield return null;

                float easeLerpValue = Mathf.Lerp(0, 1, progress);

                //adjust distance of camera from target
                distance = Mathf.Lerp(_targetStats.cameraStartDistance * DistanceMultiplier(), 200, easeLerpValue);

                //center rotation of the child camera
                vCam.transform.localEulerAngles = Vector3.zero;

                //hold the cam to the side view rotation of the target
                transform.rotation = SideViewRotation();

                //adjust the camera's FOV to keep the target size on screen the same
                _fov = FOVForHeightAndDistance(startingHeight, distance);

                ApplyCamStats();

                // adjust clipping planes
                SetClippingPlanes(easeLerpValue, 2);

                progress += TransitionDeltaTime() * direction / duration;
            }
        }


        IEnumerator SetOrthoSize(int direction, float start, float duration)
        {
            float progress = start;
            _memFrustrumHeight = FrustrumHeightAtDistance(ActualDistance());

            while (!LerpOutOfBounds(direction, progress))
            {

                yield return null;

                float easedLerp = Calc.EaseOutLerp(0, 1, progress);
                theCam.orthographicSize = Mathf.Lerp(startOrtho * _memFrustrumHeight, InteriorOrthoSize(), easedLerp);
                progress += TransitionDeltaTime() * direction / duration;
            }
        }

        bool LerpOutOfBounds(int direction, float lerp)
        {
            if (direction > 0 && lerp > 1) return true;
            if (direction < 0 && lerp < 0) return true;
            return false;
        }

        // Diff between the sprite bounds center and interior object.
        Vector2 _interiorOffset = Vector2.zero;
        IEnumerator TransitionToInterior(float duration)
        {

            _inTransition = true;
            _initFogEnd = RenderSettings.fogEndDistance; //CamFog.endDistance;
            _initFogStart = RenderSettings.fogStartDistance; //CamFog.startDistance;

            // Get the bounds of the target's interior
            _interiorBounds = _targetSideView.Interior().GetSpriteBounds();
            _interiorOffset = _targetSideView.Interior().GetInteriorOffset();

            yield return StartCoroutine(TranslateAndDolly(1, 0, duration / 2));
            
            SetBrainActive(false);

            cameraMode = CameraMode.MaskTransition;
            theCam.orthographic = true;
            InteriorView.ActivateCam(true);

            interiorZoom = _playerZoom = .8f;

            if (dof) dof.aperture = 500;

            // Open mask to show interior
            float maskTime = (duration / 3) / _transitionSpeedMultiplier;
            if (_targetSideView) InteriorView.ShowInterior(_targetSideView.Interior(), _interiorBounds, maskTime);

            // coroutine for lerping orthographic size
            yield return StartCoroutine(SetOrthoSize(1, 0, duration / 3));

            cameraMode = CameraMode.Interior;
            _inTransition = false;
        }

        /// <summary>
        /// Turn on or off the cinemachine brain
        /// </summary>
        void SetBrainActive(bool enabled)
        {
            var brain = theCam.GetComponent<CinemachineBrain>();
            if (brain) brain.enabled = enabled;

            if (enabled) theCam.transform.parent = null;
            else
            {
                theCam.transform.parent = vCam.transform;
                theCam.transform.localEulerAngles = theCam.transform.localPosition = Vector3.zero;
            }
        }

        /// <summary>
        /// Returns the camera back to default normal 3D mode
        /// </summary>
        public void SetToNormal()
        {
            //orthorReset
            theCam.orthographicSize = startOrtho * FrustrumHeightAtDistance(_targetStats.cameraStartDistance);
            theCam.orthographic = false;

            InteriorView.ActivateCam(false);

            _normalOffset = _targetStats.baseOffset * offsetMultiplier;

            // Lerp the rotation
            transform.rotation = Quaternion.Euler(_eulerInput);

            //adjust distance of camera from target
            distance = _targetStats.cameraStartDistance;

            //center rotation of the child camera
            vCam.transform.localEulerAngles = Vector3.zero;

            //adjust the camera's FOV to keep the target size on screen the same
            _fov = FOVForHeightAndDistance(FrustrumHeightAtDistance(ActualDistance()), distance);

            ApplyCamStats();

            _inTransition = false;
           //RenderSettings.fog = true;
            cameraMode = CameraMode.Normal;
        }

        IEnumerator TransitionToNormal(float duration)
        {

            _inTransition = true;
            // Debug.Log("transtitioning to normal");

            cameraMode = CameraMode.MaskTransition;

            // Mask out the interior
            float maskTime = (duration / 3) / _transitionSpeedMultiplier;
            InteriorView.HideInterior(maskTime);

            // coroutine for lerping orthographic size
            yield return StartCoroutine(SetOrthoSize(-1, 1, duration / 3));

            cameraMode = CameraMode.Dolly;

            yield return new WaitForSeconds(.1f);

            theCam.orthographic = false;
            InteriorView.ActivateCam(false);

            yield return new WaitForEndOfFrame();

            /*
            if (targetSideView.useDOF && dof)
            {
                dof.aperture = sideViewAperture;
                StartCoroutine(AdjustAperture(0, false, duration / 2, true));
            }
            */


            // Return the player's throttle to what it was before entering side view
            if (_targetStats.GetComponent<Bridge>())
            {
                if (_targetStats.GetComponent<Bridge>() == PlayerManager.pBridge)
                {
                    PlayerManager.PlayerShip().GetComponent<ShipMover>().ResumeThrottle();
                }
            }

            /*if (targetSideView.separateTromboneEffect)
            {
                yield return StartCoroutine(DollyCamera(-1, 1, duration / 3));
                yield return StartCoroutine(TranslateCamera(-1, 1, duration / 3));
            }
            else */
            
            SetBrainActive(true);
            
            yield return StartCoroutine(TranslateAndDolly(-1, 1, duration / 2));

            //RenderSettings.fog = true;

            _inTransition = false;
            cameraMode = CameraMode.Normal;
        }


        /// <summary>
        /// Resets the camera to normal view
        /// </summary>
        public void ResetTransition()
        {
            if (cameraMode == CameraMode.Normal) return;
            StartCoroutine(TransitionToNormal(0));
        }

        /// <summary>
        /// Determines action to take when a transition is requested.  
        /// </summary> 
        void TransitionRequest(bool toInterior)
        {
            //don't transition to side view if the player is dead
            if (!PlayerManager.IsPlayerAlive()) return;

            if (transitionLock) return;

            //Check to see if the interior has a lock on it
            if (!toInterior && _targetSideView)
            {
                InteriorLock interiorLock = _targetSideView.Interior().GetComponent<InteriorLock>();
                if (interiorLock != null)
                {
                    //if the player is locked at the given interior...
                    if (!interiorLock.CanLeave())
                    {
                        //Call to show the popup
                        interiorLock.ShowPopup();
                        return;
                    }
                }
            }

            //attempting to transition while already in one
            if (_inTransition)
            {
                Debug.Log("Already in a transition."); return;
            }

            if (_changingTargets)
            {
                Debug.Log("Can't transition while changing targets.");
                return;
            }
            
            // end any adventure music that might be playing
            AKMusic.Get().EndAdventure();

            QuestManager.Tick();

            // Transitioning from 3D to interior view
            if (toInterior)
            {
                EndDof();

                //Null the player's throttle
                PlayerManager.PlayerShip().GetComponent<ShipMover>().NullThrottle();

                // Move interior to the stage
                if (_targetSideView)
                {
                    _targetSideView.Interior().transform.localPosition = Vector3.zero;

                    // Reset title info to show this interior's stats
                    //SonarStats ss = _targetSideView.GetComponent<SonarStats>();
                    UIManager.Create(UIManager.Get().interiorTitle as InteriorHeader).Init(_targetSideView.intMan);

                    if (cameraMode == CameraMode.Normal)
                    {
                        viewedInterior = _targetSideView.intMan;
                        GetComponent<AKTriggerPositive>().TriggerPos();

                        StartCoroutine(TransitionToInterior(transitionTime * _targetSideView.sideViewSeconds));
                        //AKMusic.Get().SetSideViewState(Combat_State.SideView);
                        return;
                    }
                }
            }

            // Transitioning from interior to 3D view
            else if (cameraMode == CameraMode.Interior)
            {
                PlayerManager.pBridge.GetInventory().OnChanged();
                viewedInterior = null;
                StartCoroutine(TransitionToNormal(transitionTime));
                GetComponent<AKTriggerNegative>().TriggerNeg();
                //AKMusic.Get().ReturnCombatState(false);
                return;
            }
        }
        #endregion


        #region set and change target

        /// <summary>
        /// Changes the camera's target to the new target
        /// </summary>
        /// <param name="queueSideView">Attempt to transition to side view once the target is changed?</param>
        /// <param name="transitionTime">Time to transition between current & new positions.</param>
        public static void SetTarget(Transform newTarget, bool queueSideView = false, float transitionTime = .5f)
        {
            if (newTarget == null)
            {
                Debug.LogError("Attempting to set orbit cam's target, but the newTarget ref is null!");
            }
            Debug.Log("Orbit cam is setting target to " + newTarget.name, newTarget);
            Get().ChangeTarget(newTarget, queueSideView, transitionTime);
        }

        /// <summary>
        /// Changes the camera's target to the given new target.
        /// </summary>
        void ChangeTarget(Transform newTarget, bool queueSideView, float changeTime)
        {
            
            Debug.Log("Orbit cam setting target :" + newTarget, gameObject);
            target = newTarget;

            //check the new target for relavent components
            _targetStats = target.GetComponent<CameraStats>();
            _targetSideView = target.GetComponent<SideViewerStats>();

            //vCam.LookAt = newTarget;

            StartCoroutine(TargetChange(queueSideView, changeTime));
        }


        IEnumerator TargetChange(bool queueSideView, float changeTime)
        {

            transitionLock = true;
            _changingTargets = true;
            float progress = 0;

            Vector3 initPos = transform.position;

            // lerp
            while (progress < 1)
            {
                transform.position = Vector3.Lerp(initPos, target.position, Calc.EaseBothLerp(0, 1, progress));
                progress += Time.unscaledDeltaTime / changeTime;
                yield return null;
            }

            _changingTargets = false;

            transitionLock = false;

            // Pending side view
            if (queueSideView) TransitionRequest(true);
        }

        bool TargIsShip()
        {
            if (!target) return false;
            if (target.GetComponent<Bridge>()) return true;
            return false;
        }
        #endregion



        /// <summary>
        /// Returns the true distance from camera to target.
        /// </summary>
        float ActualDistance()
        {
            if (!target) return 0;
            return Vector3.Distance(vCam.transform.position, target.transform.position);
        }


        float FrustrumHeightAtDistance(float distance)
        {
            return 2.0f * distance * Mathf.Tan(GetFov() * 0.5f * Mathf.Deg2Rad);
        }

        float FOVForHeightAndDistance(float height, float distance)
        {
            return 2f * Mathf.Atan(height * 0.5f / distance) * Mathf.Rad2Deg;
        }


    }
}