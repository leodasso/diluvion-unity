using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;
using System.Linq;
using DUI;
using Diluvion.Sonar;
using Sirenix.OdinInspector;

namespace Diluvion.Ships
{

    public class ShipControls : MonoBehaviour
    {

        public enum ControlType
        {
            arcade = 0,
            sim = 1
        }

        [Range(0, 1)]
        public float flexibleReticuleSpace = .3f;
        //public Vector2 flexibleReticulePos;

        [ReadOnly]
        GameObject _aimer;

        public ShipMover ship;
        public ControlType controlType = ControlType.arcade;

        //public ShipAnimator shipAnim;
        public bool canSideView = true;

        public Vector3 LastAimedPosition { get; private set; }
        public SonarStats currentTarget;
        //public bool mainWeaponsActive = true;
        //public float torpLerp;
        public bool surfaced;

        float _ballastPitch = 100;
        float _simRotSpeed = 75;
        float _torpCharge = 0;
        float _torpMaxCharge = 3; //seconds
        float _hitDistance;
        SonarStats _rayHitSs;
        float _strafeSpeed = .7f;
        Bridge _bridge;
        bool _throttleChangeOk = true;   //.......allows for throttle change.  False until input axis returns to 0
        Vector3 _moveDirection;
        float _buoyancyAxis;
        float _pingPower = 0;
        DUIController _dui;
        ControlOverride _controlOverride;
        bool _hasOverride;
        float _lockOnRadius = 1.5f;      // for torpedoes
        bool _canSwapWeap;
        Vector3 _manouverDirection;
        float _clampedVerticalMovement = .50f;    // clamps the subs angle so it wont follow the camera direction all the way up
        Rewired.Player _player;

        float _overrideTimer;
        
        Vector3 _aimerEulers;
        static float _maxAimAngle = 25;


        public static string prefsControlType = "_subControlType";

        void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position, _moveDirection * 50);
        }

        #region init
        void Awake()
        {
            _player = GameManager.Player();
            _bridge = gameObject.GetComponent<Bridge>();
            ship = gameObject.GetComponent<ShipMover>();
            //shipAnim = gameObject.GetComponent<ShipAnimator>();
            RefreshAimAngle();
            _manouverDirection = transform.forward;
        }

        public static void RefreshAimAngle()
        {
            _maxAimAngle = PlayerPrefs.GetFloat("_maxAimAngle", 25);
        }
        

        void Start()
        {
            // Get the player's control preference
            int newControlType = PlayerPrefs.GetInt(prefsControlType, 1);
            controlType = (ControlType)newControlType;
        }

        public GameObject Aimer()
        {
            // The aimer is required to be inside the main camera to work, so if there's no cam, return null
            if (!OrbitCam.Exists()) return null;
            
            if (_aimer) return _aimer;
            
            _aimer = new GameObject("aimer " + gameObject.name);
            _aimer.transform.parent = OrbitCam.Cam().transform;
            _aimer.transform.localPosition = _aimer.transform.localEulerAngles = Vector3.zero;
            return _aimer;
        }

        #endregion

        #region updates

        void Update()
        {
            if (_player.GetButtonDown("Guide")) return;
            //if (CheatManager.Get().showing) return;
            if (!Cutscene.playerControl) return;

            //Take no action while paused
            if (GameManager.State() == GameState.Paused) return;
            if (Math.Abs(Time.timeScale) < .01f) return;

            if (_player.GetButtonDown("pause")) GameManager.Pause();
            if (_player.GetButtonDown("captains tools")) CaptainsTools.Create();

            // if Player is in control
            if (!_hasOverride)
            {
                GameInputs();
                ShipInputs();
            }

            // Control override - game takes control of the ship away from the player
            else
            {
                _overrideTimer -= Time.deltaTime;
                if (_overrideTimer <= 0)
                {
                    CancelOverride();
                    return;
                }
                
                //turn towards the override point
                if (_controlOverride != null)
                {
                    _moveDirection = _controlOverride.point - transform.position;
                    ship.ShipManeuver(_moveDirection);
                }
            }
            
            ControlAimer();
            AimContext();
        }

        
        void ControlAimer()
        {
            if (!OrbitCam.Exists()) return;
            if (OrbitCam.CamMode() != CameraMode.Normal) return;
            
            Vector2 aim = GameManager.Player().GetAxis2D("camX", "camY") * Time.deltaTime * 5;

            // add the input to the aimer euler angle
            _aimerEulers += new Vector3(-aim.y, aim.x);

            Vector3 unclampedEulers = _aimerEulers;

            _aimerEulers = Vector3.ClampMagnitude(_aimerEulers, _maxAimAngle);

            Vector3 overshoot = unclampedEulers - _aimerEulers;

            Aimer().transform.localEulerAngles = _aimerEulers;
            
            OrbitCam.Get().Rotate3DCam(new Vector2(overshoot.y, -overshoot.x) * .1f);
        }
        
        void FixedUpdate()
        {
            //Take no action while paused
            if (Math.Abs(Time.timeScale) < .01f) return;
            if (_hasOverride) return;
            if (!Cutscene.playerControl) return;

            if (OrbitCam.CamMode() == CameraMode.Normal)
            {
                if (surfaced) return;

                //Get input for change in depth
                _buoyancyAxis = _player.GetAxis("depth change");
                ship.ChangeBallast(_buoyancyAxis);
            }
        }

        #endregion

      
        void ToggleCrushDepth()
        {
            Hull hull = GetComponent<Hull>();
            if (!hull) return;

            hull.useCrushDepth = !hull.useCrushDepth;
        }

        void ToggleInvincible()
        {
            Hull hull = GetComponent<Hull>();
            if (!hull) return;

            hull.invincible = !hull.invincible;
        }

        float _trueBuoyancy = 0;

        /*
        /// <summary>
        /// Called whenever the player requests to fire a weapon. 
        /// </summary>
        public void PlayerWeaponRequest()
        {
            ShipInvisibler.SetInvisible(5);
            //ShipInvisibler.SetInvisibleIfObstructed(5);
        }
        */

        List<RaycastHit> _hits = new List<RaycastHit>();
        List<Transform> damageables = new List<Transform>();
        
        /// <summary>
        /// Returns the world space position that the player is aiming at.
        /// </summary>
        public Vector3 AimPosition(float spherecastRadius, float maxRange)
        {
            _hits.Clear();
            damageables.Clear();
            foreach (var h in Physics.SphereCastAll(Camera.main.transform.position, spherecastRadius, Aimer().transform.forward, maxRange, Calc.GunsafeLayer()))
            {
                if (h.collider == null) continue;
                if (h.collider.GetComponentInParent<ShipControls>() == this) continue;

                _hits.Add(h);
                IDamageable damageable = h.collider.GetComponent<IDamageable>();
                if(damageable!=null)
                    damageables.Add(h.collider.transform);
            }

            _hits = _hits.OrderBy(h => h.distance).ToList();

            if (_hits.Count < 1) 
                return Aimer().transform.forward * maxRange + Camera.main.transform.position;

            return _hits[0].point;
        }

        Transform GetTopTargetInRange(float range)
        {
            if (damageables.Count < 1) return null;
            Transform firstTransform = damageables.First();
            if (Vector3.Distance(transform.position, firstTransform.position) > range) return null;
            return firstTransform;
        }

        List<WeaponSystem> _weapons = new List<WeaponSystem>();

        /// <summary>
        /// Raycast to center of screen to judge context.  Returns transform
        /// being aimed at, if it has relevant components.
        /// </summary>
        public void AimContext()
        {
            if (_hasOverride || OrbitCam.CamMode() != CameraMode.Normal)
                return;

            _weapons.Clear();
            _weapons.AddRange(GetComponents<WeaponSystem>());

            foreach (WeaponSystem w in _weapons) AimWeaponSystem(w);
        }

        void AimWeaponSystem(WeaponSystem w)
        {
            if (!w) return;
            if (!OrbitCam.Exists()) return;
            
            w.autoLead = false;

            // Don't calculate aim for weapon systems that have no weapons
            if (!w.equippedWeapon) return;

            Cannon c = w.equippedWeapon.weapon;

            LastAimedPosition = AimPosition(w.module.aimCastRadius, c.maxRange);
            // Snap the aim position to the leads of targets
            if (w.module.showLeadGUI)
                w.SetAimPosition(LeadPosition(LastAimedPosition, w.ExposedLeads()));

            // Set the aim at the casted point
            else
                w.SetAimPosition(LastAimedPosition);

            // Tell the weapon system what object it's aiming at
            w.SetSystemTarget(GetTopTargetInRange(w.Range()));
        }

        public Transform CurrentTorpedoTarget()
        {
            return currentTarget.transform;
        }


        /// <summary>
        /// Returns any lead position that is near enough to the screen co-ords that the player is aiming at. 
        /// </summary>
        /// <param name="currentAimPosition">The world space position currently being aimed at</param>
        /// <param name="leadLocations">List of world space lead positions</param>
        public Vector3 LeadPosition(Vector3 currentAimPosition, List<Vector3> leadLocations)
        {
            if (!Camera.main) return currentAimPosition;

            Ray aimRay = new Ray(Camera.main.transform.position, currentAimPosition - Camera.main.transform.position);

            // order the lead locations by nearest first
            leadLocations = leadLocations.OrderBy(x => Vector3.Distance(x, aimRay.origin)).ToList();

            // Check each location
            foreach (Vector3 lead in leadLocations)
            {
                // get the distance from camera to the lead
                float distanceToLead = Mathf.Clamp(Vector3.Distance(lead, aimRay.origin), 1, 200);

                // find a point that is distance along the aim ray, where distance is the dist to the lead position
                Vector3 distancePoint = aimRay.GetPoint(distanceToLead);

                if (Calc.WithinDistance(GameManager.Mode().leadHelpRadius, lead, distancePoint))
                    return distancePoint;
            }

            return currentAimPosition;
        }

        /*
        public float PingPower()
        {
            return _pingPower;
        }
        */

        /// <summary>
        /// Handles player inputs specifically for control of the ship. Omits any imputs that happen
        /// when the camera target isn't the ship, or when the camera isn't in normal 3D mode.
        /// </summary>
        void ShipInputs()
        {
            //dont navigate while docked
            if (OrbitCam.CamMode() != CameraMode.Normal) return;

            foreach (var weapSystem in _bridge.equippedWeaponSystems)
            {
                weapSystem.CheckSwapInputs();
            }

            bool holdingThrottle = Mathf.Abs(_player.GetAxisRaw("throttle")) > .05f;

            if (!holdingThrottle) _throttleCooldown = 0;

            //manual control of the ship
            if (controlType == ControlType.arcade)
                ArcadeControls(!holdingThrottle);

            if (controlType == ControlType.sim)
            {
                SimControls();
            }

            float throttleInput = _player.GetAxisRaw("throttle");
            
            if (_throttleCooldown <= 0)
            {
                if (throttleInput > .5f)
                {
                    ship.ThrottleDelta(1);
                    _throttleCooldown = .15f;
                }
                else if (throttleInput < -.5f)
                {
                    ship.ThrottleDelta(-1);
                    _throttleCooldown = .15f;
                }
            }

            else _throttleCooldown -= Time.unscaledDeltaTime;

            // have each ship module check inputs.
            foreach (ShipModule m in _bridge.shipModules)
                m.CheckPlayerInputs(_bridge);

            //_throttleHeld = _player.getaxisti//Mathf.Max(_player.GetButtonTimePressed("throttle up"), _player.GetButtonTimePressed("throttle down"));
        }

        float _throttleCooldown;

        /// <summary>
        /// Handles player inputs that can happen at any time during the game. (docking, side veiw transition)
        /// </summary>
        void GameInputs()
        {
            // Docking
            if (OrbitCam.CamMode() == CameraMode.Normal)
            {
                if (_player.GetButtonDown("dock"))
                    _bridge.GetComponent<DockControl>().DockToggle();
            }

            SideViewInputs();
        }


        /// <summary>
        /// The ship will manouver towards the reticule, while holding throttle will keep it steady
        /// </summary>
        void ArcadeControls(bool towardsCamCenter)
        {
            if (towardsCamCenter) _manouverDirection = OrbitCam.Get().transform.forward;

            // Get a rotation between -90 and 90 for strafe rotatoin
            Quaternion strafeRot = Quaternion.AngleAxis(_player.GetAxis("strafe") * 90, Vector3.up);

            // Get the final direction
            _moveDirection = strafeRot * _manouverDirection;
            ship.ShipManeuver(_moveDirection, _clampedVerticalMovement);
        }

        /// <summary>
        /// The ship moves independent of camera. 
        /// </summary>
        void SimControls()
        {
            // Rotate the current direction based on players input
            Quaternion newRot = Quaternion.Euler(new Vector3(0, _simRotSpeed * _player.GetAxis("strafe"), 0));

            // Pitch the ship up or down when ballasting
            float verticalAmt = _player.GetAxis("depth change");

            Vector3 flattenedForward = new Vector3(transform.forward.x, verticalAmt, transform.forward.z);
            ship.ShipManeuver(newRot * flattenedForward, _clampedVerticalMovement);
        }

        /// <summary>
        /// Handles input for moving to / from side view.
        /// </summary>
        void SideViewInputs()
        {
            if (_hasOverride) return;
            if (_player.GetButtonDown("cancel")) CancelButtonActions();
            if (!canSideView) return;

            // Move to viewing the interior of the ship on Button DOWN
            if (_player.GetButtonDown("side view"))
            {
                if (OrbitCam.CamMode() == CameraMode.Normal)
                    OrbitCam.RequestTransition(true);

                if (OrbitCam.CamMode() == CameraMode.Interior)
                    CancelButtonActions(true);
            }
        }


        ///If docked to something, waits for camera to return to normal mode
        /// and then undocks.
        IEnumerator ReturnToShip()
        {
            while (OrbitCam.CamMode() != CameraMode.Normal) yield return null;
            GetComponent<DockControl>().Undock();
        }


        void CancelButtonActions(bool force = false)
        {
            if (Time.timeScale == 0) return;

            if (!UIManager.CanTransition()) return;

            //If interior focus, clear the focus and return.
            if (OrbitCam.ClearFocus() && !force) return;

            // Return to ship
            if (OrbitCam.CamMode() != CameraMode.Interior) return;
            OrbitCam.RequestTransition(false);
            StartCoroutine(ReturnToShip());
        }

        public void NewOverride(ControlOverride cOverride)
        {
            _overrideTimer = cOverride.overrideTime;
            _controlOverride = cOverride;
            _hasOverride = true;
        }

        void CancelOverride()
        {
            _controlOverride = null;
            _hasOverride = false;
        }

    }
}