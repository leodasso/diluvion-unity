using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Diluvion.Sonar;
using DUI;
using Loot;
using Rewired;
using SpiderWeb;
using Sirenix.OdinInspector;

namespace Diluvion.Ships
{

    /// <summary>
    /// Weapon system allows use of a weapon module. Gets all mounts that relate to it, and equips weapons,
    /// controls turrets, and controls multi-cannon firing.
    /// </summary>

    public class WeaponSystem : MonoBehaviour
    {
        [ToggleLeft]
        public bool autoLead = true;

        [ToggleLeft] public bool waverEnabled = true;
        
        [Space]
        public WeaponModule module;

        [ReadOnly]
        public Mount currentMount;
        
        /// <summary>
        /// The currently equipped weapon. Don't set this directly; use EquipWeapon()
        /// </summary>
        public DItemWeapon equippedWeapon;

        [ReadOnly] public Quaternion noiseRotation;
        
        [ReadOnly]
        public List<Mount> mounts = new List<Mount>();
        
        [ReadOnly]
        public List<WeaponPart> weaponParts = new List<WeaponPart>();

        [ReadOnly] 
        public List<Cannon> allCannonInstances = new List<Cannon>();
        [ReadOnly]
        public List<TorpedoTube> allTorpedoTubeInstances = new List<TorpedoTube>();

        [Space]
        [Tooltip("The higher the accuracy, the quicker gunners will be able to reduce waver")]
        public float accuracy = 0.7f;
        
        [Tooltip("Actual hit % chance is calculated based on this & the defense of the hull being hit")]
        public float critChance = 1;
        public float reloadSpeed = 1;
        public float calibrationSpeed = 1;

        [ReadOnly]
        public float waverAmount = 5;
        public float waverErraticness = 20;
        public float waverCorrectionSpeed = 0.35f;
        public float waverTurnPenalty = 3.5f;

        [Space]
        public bool firing;

        [ReadOnly]
        public bool inFiringSequence;
        
        /// <summary>
        /// The transform target to aim at.
        /// </summary>
        [SerializeField]
        Transform weaponSystemTarget;

        public delegate void FireWeapon(WeaponSystem thatFired);

        public FireWeapon fired;
        
        /// <summary>
        /// The transform target to aim at.
        /// </summary>
        public Transform autoAimTarget;
        
        public float firingWaitTime;
        //public float chargeAmt;

        protected WeaponTarget weaponTarget;
        
        [ ReadOnly]
        public int fireIndex;
        
        [ReadOnly]
        public bool isPlayer;
        Cannon _equippedCannon;
        PseudoVelocity _velo;

        Rigidbody _rb;
        Bridge _bridge;
        float _noiseCoord;


        // Use this for initialization
        IEnumerator Start()
        {
            _velo = GO.MakeComponent<PseudoVelocity>(gameObject);
            _velo.velocitySamples = 10;

            while (module == null) yield return null;

            _rb = GetComponent<Rigidbody>();

            Hull h = GetComponent<Hull>();
            if (h) h.imHit += AddWaver;

            GetMounts();

            // Discover if play or not
            Bridge b = GetComponent<Bridge>();
            if (b)
            {
                isPlayer = b.IsPlayer();
                b.equippedWeaponSystems.Add(this);
            }

            // If the weapon to equip is known and the actual instances haven't been equipped yet, do that
            if (equippedWeapon && !_equippedCannon) EquipWeapon();

            SetAimPosition(transform.position + transform.forward);
        }


        void Update()
        {
            _noiseCoord += Time.deltaTime * (waverAmount / waverErraticness);
            
            if (!module) return;
            if (!_equippedCannon) return;
            
            GenerateWaver();
            
            // Waver increases from the ship moving around
            if (_rb) waverAmount += _rb.angularVelocity.magnitude * Time.deltaTime / accuracy* waverTurnPenalty;

            //waver will always be approaching zero at a speed determined by accuracy
            waverAmount = Calc.EaseOutLerp(waverAmount, 0, Time.deltaTime * waverCorrectionSpeed);
            

            if (autoAimTarget)
                SetAimPosition(autoAimTarget.position);

            module.UpdateSystem(this);
        }


        bool _canSwap;
        Player _player;
        
        /// <summary>
        /// Checks for player inputs to swap weapons
        /// </summary>
        public void CheckSwapInputs()
        {
            if (module == null)
            {
                Debug.LogError("Attempted to swap weapons, but no weapon module was linked!", gameObject);
                return;
            }

            if (_player == null) _player = GameManager.Player();

            // Check if axis has returned to zero (un-pressed) to see if we can swap again. prevents a swap every frame
            // when the player is holding the axis
            if (Mathf.Abs(_player.GetAxis(module.weaponSwapAxisName)) < .1f) _canSwap = true;

            if (_canSwap)
            {
                if (_player.GetAxis(module.weaponSwapAxisName) > .1f)
                {
                    Debug.Log("Attempting to swap up on " + module.name);
                    _canSwap = false;
                    SwapWeapon(1);
                }
                
                else if (_player.GetAxis(module.weaponSwapAxisName) < -.1f)
                {
                    Debug.Log("Attempting to swap down on " + module.name);
                    _canSwap = false;
                    SwapWeapon(-1);
                }
            }
        }

        void SwapWeapon(int direction)
        {
            WeaponSwapHUD.SwapWeapon(direction, this);
        }
                
        public bool InRange()
        {
            if (_equippedCannon == null) return false;
            return RangeToTarget() <= _equippedCannon.maxRange + .5f;
        }

        public float RangeToTarget()
        {
            return Vector3.Distance(transform.position, GetTarget().transform.position);
        }

        public void SetAutoAimTarget(Transform target)
        {
            weaponSystemTarget = target;
            autoAimTarget = target;
        }
        
        public void SetSystemTarget(Transform trans)
        {
            weaponSystemTarget = trans;
        }

        public Transform GetSystemTarget()
        {
            return weaponSystemTarget;
        }
        
        public bool AimingAtVulnerable()
        {
            return weaponSystemTarget != null;
        }

        List<MountGroup> _mountGroups = new List<MountGroup>();
        List<MountGroup> MyMountGroups
        {
            get
            {
                if (_mountGroups != null && _mountGroups.Count > 0) return _mountGroups;
                _mountGroups = new List<MountGroup>();
                
                foreach(MountGroup mg in GetComponentsInChildren<MountGroup>())
                    if(mg.weaponModule == module)  _mountGroups.Add(mg);
                
                return _mountGroups;
            }
        }

        /// <summary>
        /// Gets the closest mountgroup to the target position
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public MountGroup GetClosestMountGroup(Vector3 target)
        {
            if (MyMountGroups.Count == 1) return MyMountGroups.First();
            return MyMountGroups.OrderBy(mg => Vector3.Distance(mg.transform.position, target)).First();
        }

        Listener _listener;
        Listener GetListener()
        {
            if (_listener) return _listener;

            _listener = GetComponent<Listener>();
            return _listener;
        }
        
        #region aiming

        /// <summary>
        /// Returns a list of vectors that are the exposed leads. Used for AI and for ship controls.
        /// </summary>
        readonly List<Vector3> _exposedLeads = new List<Vector3>();
        public List<Vector3> ExposedLeads()
        {
            _exposedLeads.Clear();

            if (GetListener() == null) return _exposedLeads;

            foreach (var sig in GetListener().ValidForLeading(Range()))
            {
                _exposedLeads.Add(LeadPosition(sig));
            }

            return _exposedLeads;
        }

        public Vector3 LeadPosition(SonarStats sig)
        {
            if (!sig) return Vector3.zero;
            
            // get lead of each thing
            float bSpeed = BulletSpeed();
            Vector3 v1 = Velocity();

            Vector3 tPos = sig.transform.position;
            Vector3 v2 = sig.MyVelocity();

            return Calc.FirstOrderIntercept(transform.position, v1, bSpeed, tPos, v2);
        }

        /// <summary>
        /// Add waver to the aim; Usually would be added from impacts, damage, ocean currents
        /// </summary>
        public void AddWaver(float amount, GameObject source)
        {
            waverAmount += amount/accuracy;
        }

        /// <summary>
        /// The exact position of the weapon target
        /// </summary>
        public Vector3 CleanAimPosition()
        {
            return GetTarget().transform.position;
        }

        /// <summary>
        /// The weapon target position plus the innacuracy noise
        /// </summary>
        public Vector3 TotalAimPosition()
        {
            if (!waverEnabled) return CleanAimPosition();
            
            // Get the direction of aim
            Vector3 dir = CleanAimPosition() - transform.position;

            // Add the noise waver rotation to it (for inaccuracy)
            dir = noiseRotation * dir;
            
            // cast a ray along the wavered direction
            Ray r = new Ray(transform.position, dir);
            
            // get a point along that ray which is the same distance from me as the 'clean' aim point
            float dist = Vector3.Distance(transform.position, CleanAimPosition());
            Vector3 point = r.GetPoint(dist);
            return point;
        }
        
                
        /// <summary>
        /// Adds a rotational waver to aim by using perlin noise
        /// </summary>
        void GenerateWaver()
        {
            if (OrbitCam.CamMode() != CameraMode.Normal) return;
            
            // Get noises between -.5 and .5
            float noiseX = Mathf.PerlinNoise(_noiseCoord, 0) - .5f;
            float noiseY = Mathf.PerlinNoise(0, _noiseCoord) - .5f;
            
            Vector3 noiseEulers = new Vector3(noiseX, noiseY, 0) * waverAmount;

            //Debug.Log(noiseEulers + " out of = " + noiseX+ " + " + noiseY + " * " + waverAmount, gameObject);
            // create a rotation based on that noise
            noiseRotation = Quaternion.Euler(noiseEulers);
        }

    

        /// <summary>
        /// Sets the system to aim at the given position by placing the weapon target at that position. Also
        /// clamps the position to the current equipped weapon's max range
        /// </summary>
        /// <param name="pos">World space position, NOT a direction!</param>
        public void SetAimPosition(Vector3 pos)
        {
            GetTarget().transform.position = pos;
        }

        #endregion
        

      #region torpedoes 

        public TorpedoTube activeTube;

        /*-
        TorpedoSpline chargeSpline;
        public TorpedoSpline ChargeSpline
        {
            get { return chargeSpline; }
            private set { chargeSpline = value; }
        }

        /// <summary>
        /// Calibrates torpedo tubes, one at a time
        /// </summary>
        /// <param name="defaultChargeTime"></param>
        public void ChargeTorpedo(float defaultChargeTime)
        {
            UpdateSpline();
            
            // Find the next torpedo that can calibrate, and calibrate it
            foreach (var tube in allTorpedoTubeInstances)
            {
                if (tube.ReadyToCalibrate())
                {
                    tube.CalibrationUpdate((Time.deltaTime * calibrationSpeed) / defaultChargeTime);
                    break;
                }
            }
        }
        
        
        public void ClearTorpedoTargeting()
        {
            foreach (var tube in allTorpedoTubeInstances)
            {
                tube.calibrationProgress = 0;
            }
        }

       /// <summary>
       /// Instantiates a spline if none exists. Controls the start and end points of the spline
       /// </summary>
       void UpdateSpline()
        {
            if (MySpline == null) return;
            
            if (chargeSpline == null)
            {
                chargeSpline = Instantiate(MySpline, Vector3.zero, Quaternion.identity);
                chargeSpline.SetStart(CurrentMount.transform, isPlayer);
            }
            //else chargeSpline.SetCharge(chargeAmt, maxCharge);


            Transform systemTarget = GetSystemTarget();
            if (systemTarget != null)
            {
                chargeSpline.SetTarget(systemTarget);
            }
            else
            {
                chargeSpline.SetTarget(CleanAimPosition());
            }
        }
      
        */

        #endregion


        [SerializeField, ReadOnly]
        float dps;
        /// <summary>
        /// Gets the total damage per second for this weapon system
        /// </summary>
        public float DamagePerSecond()
        {
            if (mounts == null || mounts.Count < 1) return 0;
            if (_equippedCannon == null) return 0;

            return dps = mounts.Count * _equippedCannon.damage / CoolDown();
        }

        public float ShotRadius()
        {
            if (!_equippedCannon) return 0;
            return _equippedCannon.shotSpread;
        }

        /// <summary>
        /// Range of equipped cannon.
        /// </summary>
        public float Range()
        {
            if (!_equippedCannon) return 0;
            return _equippedCannon.maxRange;
        }

        /// <summary>
        /// Returns the weapon target. Creates one if none exist.
        /// </summary>
        public WeaponTarget GetTarget()
        {
            if (weaponTarget) return weaponTarget;

            weaponTarget = WeaponTarget.Create();
            weaponTarget.name = "weapon target( " + module.name + "; " + gameObject.name + " ) ";
            return weaponTarget;
        }

        #region mounts & equipping
        [Button]
        void EquipWeapon()
        {
            EquipWeapon(equippedWeapon);
        }

        /// <summary>
        /// Returns weapons in the attached inventory that can be used by this weapon system
        /// </summary>
        public List<DItemWeapon> WeaponsInInventory()
        {
            if (module == null)
            {
                Debug.LogError("No weapons can be determined because no module is attached to this weapon system!", gameObject);
                return null;
            }

            return module.WeaponsInInventory(GetBridge());
        }

        /// <summary>
        /// Gets all mounts and turret rotators related to this weapon system.
        /// </summary>
        void GetMounts()
        {
            foreach (MountGroup g in MyMountGroups)
                g.ApplyModule();

            weaponParts.Clear();
            foreach (WeaponPart m in GetComponentsInChildren<WeaponPart>())
                if (m.weaponModule == module) weaponParts.Add(m);

            mounts.Clear();
            foreach (WeaponPart p in weaponParts)
            {
                Mount m = p as Mount;
                if (m)
                {
                    mounts.Add(m);
                    m.ClearOnFire();
                    m.onFire += WeaponWasFired;
                }
            }
        }

        /// <summary>
        /// Equip the given weapon on all appropriate mounts.
        /// </summary>
        public void EquipWeapon(DItemWeapon weaponItem)
        {
            allCannonInstances.Clear();
            allTorpedoTubeInstances.Clear();
            
            GetMounts();
            equippedWeapon = weaponItem;
            _equippedCannon = equippedWeapon.weapon;
            foreach (Mount m in mounts)
            {
                if(m == null) continue;
                
                var newCannon = m.EquipCannon(_equippedCannon);
                newCannon.SetWeaponSource(gameObject);
                
                // Add the instances to the appropriate weapon instance lists
                allCannonInstances.Add(newCannon);
                var newTube = newCannon as TorpedoTube;
                if (newTube != null) allTorpedoTubeInstances.Add(newTube);
            }
            ChangeCritRate(critChance);
        }

        #endregion

        /// <summary>
        /// SFX and chatter to give feedback when a fire even failed because there's no ammo.
        /// </summary>
        public void FeedbackNoAmmo()
        {
            // TODO
            GetBridge().GetCrewManager().BroadcastHashtag("noAmmo");
        }

        public Vector3 Velocity()
        {
            _velo = GO.MakeComponent<PseudoVelocity>(gameObject);
            return _velo.velocity;
        }

        public float BulletSpeed()
        {
            if (!_equippedCannon) return 10;
            return _equippedCannon.BulletSpeed();
        }

        /// <summary>
        /// Changes the critical hit rate, and applies it to all cannon instances.
        /// </summary>
        public void ChangeCritRate(float newRate)
        {
            critChance = newRate;
            foreach (Mount m in mounts)
            {
                if (m.cannonInstance)
                    m.cannonInstance.critRate = _equippedCannon.critRate * critChance;
            }
        }

        #region firing
        /// <summary>
        /// Fires the next available mount in the given list. Iterates the index, and returns the fired ammo.
        /// </summary>
        /// <param name="firingMounts">list of mounts to choose from</param>
        /// <param name="cooldown">use cooldown from this firing?</param>
        /// <returns>The ammo instance that was fired</returns>
        public GameObject FireNextMount()
        {
            if (!enabled) return null;
            int tries = 0;

            // Find the next mount ready to fire by asking the module if the mount is valid.
            while (!module.ValidMount(CurrentMount) )
            {
                tries++;
                IterateFireIndex();

                if (tries > 50) return null;
            }

            // Fire the mount
            GameObject newAmmo = module.FireWeapon(CurrentMount, this);
          
            //Debug.Log("returning : " + newAmmo.name + " from " + CurrentMount.name + " in " +name );
            return newAmmo;
        }

        public Mount CurrentMount => mounts[fireIndex];

        /// <summary>
        /// Added to mount delegate and called every time one of my mounts fires.
        /// </summary>
        void WeaponWasFired()
        {
            AddWaver(_equippedCannon.recoil * 7, null);
            IterateFireIndex();
            fired?.Invoke(this);

            // apply a cooldown
            firingWaitTime = CoolDown();
            
            // tell the bridge that i fired
            if (GetBridge()) 
                GetBridge().WeaponSystemFired(this);
        }

        
        public bool NotFiring()
        {
            return !firing || firingWaitTime > 0;
        }
        
        
        /// <summary>
        /// How many mounts in the weapon system are valid?
        /// </summary>
        public int ValidMounts()
        {
            int i = 0;
            if (module == null) return 0;
            foreach (Mount m in mounts)
                if (module.ValidMount(m)) i++;

            return i;
        }

        /// <summary>
        /// Counts down the time until next shot
        /// </summary>
        /// <returns></returns>
        public bool Reloading()
        {
            if (firingWaitTime > 0)
            {
                firingWaitTime -=  Time.deltaTime;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// The progress of the reload where 0 is none and 1 is done
        /// </summary>
        public float ReloadProgress()
        {
            return 1 - (firingWaitTime / CoolDown());
        }
        
        /// <summary>
        /// Time between the shots in seconds
        /// </summary>
        /// <returns></returns>
        public float CoolDown()
        {
           return _equippedCannon.cooldown / reloadSpeed / ValidMounts();
        }
        
        /// <summary>
        /// Sets the accuracy on the weapon system
        /// </summary>
        /// <param name="acc"></param>
        public void SetAccuracy(float acc =0.7f)
        {
            //Debug.Log("Setting accuracy on weapons for " + gameObject.name, gameObject);
            accuracy = Mathf.Clamp(acc, 0.01f, 100);
        }

        public void SetReloadSpeed(float newSpeed)
        {
            reloadSpeed = Mathf.Clamp(newSpeed, .01f, 100);
        }

        public void SetCalibrationSpeed(float newSpeed)
        {
            calibrationSpeed = Mathf.Clamp(newSpeed, .01f, 100);
        }
        
        
        void IterateFireIndex()
        {
            fireIndex++;
            if (fireIndex >= mounts.Count) fireIndex = 0;
        }

        [SerializeField] 
        GameObject testFiredShot;
        [Button]
        void TestFire()
        {
            testFiredShot = FireNextMount();
        }

        /// <summary>
        /// Begins firing. 
        /// </summary>
        public void FireOn(float rlMulti = 1)
        {
            reloadSpeed = rlMulti;
            if (!enabled) return;
            firing = true;
        }

        /// <summary>
        /// End firing. 
        /// </summary>
        public void FireOff()
        {
            firing = false;
        }

        public Bridge GetBridge()
        {
            if (_bridge) return _bridge;

            _bridge = GetComponent<Bridge>();
            return _bridge;
        }

        #endregion

        void OnDisable()
        {
            if (weaponTarget)
            {
                if (Application.isPlaying)
                    Destroy(weaponTarget.gameObject);
                else
                    DestroyImmediate(weaponTarget.gameObject);
            }
            firing = false;
        }
    }
}