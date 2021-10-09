using UnityEngine;
using System.Collections;
using Diluvion.Ships;
using Sirenix.OdinInspector;

namespace Diluvion
{

    public class TorpedoTube : Cannon
    {
        public enum TubeState
        {
            Standby,
            Calibrating,
            DetonateReady,
            Reloading,
            Empty
        }

        [Tooltip("If the tube is launched without calibrating at all, the torpedo will be sent to a random point in the sphere of this radius.")]
        public float defaultTargetingRadius = 10;
        
        [Space, Tooltip("After launch, how long before the drive begins")]
        public float armingTime = 1;

        [Tooltip("If the target is within this range, will detonate.")]
        public float triggerRange = 1;
        
        [ReadOnly]
        public TubeState tubeState { get; private set; }

        [ReadOnly, Tooltip("The tube that's currently the focus of the weapon system. There's only one of these at a time")]
        public bool isActiveTube;
        
        [Space]
        public Transform target;

        [Range(0, 1), ReadOnly]
        public float reloadProgress;

        [Range(0, 1), ReadOnly]
        public float calibrationProgress;

        [Tooltip("The most recently fired torpedo"), ReadOnly]
        public Explosive torpedoInstance;

        TorpedoSpline _splineInstance;
        
        static TorpedoSpline _splinePrefab;
        static TorpedoSpline SplinePrefab
        {
            get
            {
                if (_splinePrefab != null) return _splinePrefab;
                return _splinePrefab = Resources.Load<TorpedoSpline>("TorpedoSpline");
            }
        }
        
        void Awake()
        {
            tubeState = TubeState.Standby;
        }

        void OnDisable()
        {
            if (_splineInstance != null) Destroy(_splineInstance.gameObject);
        }

        /// <summary>
        /// Updates the spline for this torpedo tube.
        /// </summary>
        /// <param name="lockTarget">If there's a lock on an object, it goes here. This can be null if there's no 
        /// lock.</param>
        /// <param name="targetPosition">The aim position. Spline points here if there's no target.</param>
        public void UpdateSpline(Transform lockTarget, Vector3 targetPosition)
        {
            if (!_splineInstance)
            {
                _splineInstance = Instantiate(SplinePrefab).GetComponent<TorpedoSpline>();
            }
            
            // Tell the spline to place the start point at this tube. This only happens in tubeState calibrating,
            // because that's when player's targeting and needs to see up to date splines. as soon as torpedo is launched,
            // the spline's start point stays static.
            if (tubeState == TubeState.Calibrating) _splineInstance.SetStart(transform);

            if (isActiveTube && calibrationProgress > 0 && tubeState == TubeState.Calibrating)
            {
                if (IsPlayer())
                    _splineInstance.ShowVisual();
                
                if (lockTarget != null)
                    _splineInstance.SetTarget(lockTarget);
                else 
                    _splineInstance.SetTarget(targetPosition);
                
                return;
            }
            
            
            _splineInstance.HideVisual();
        }

        public void ReloadUpdate(float speed)
        {            
            reloadProgress = Mathf.Clamp01( reloadProgress + Time.deltaTime * speed);

            if (tubeState == TubeState.Empty)
                tubeState = TubeState.Reloading;

            if (reloadProgress >= 1)
                tubeState = TubeState.Standby;
        }

        public void CalibrationUpdate(float amount)
        {
            if (tubeState == TubeState.DetonateReady) return;
            calibrationProgress = Mathf.Clamp01(calibrationProgress + amount);

            if (calibrationProgress > 0) tubeState = TubeState.Calibrating;
        }

        public void ReloadFull()
        {
            ReloadUpdate(10000);
        }

        public bool ReadyToCalibrate()
        {
            if (calibrationProgress >= 1) return false;
            return tubeState == TubeState.Standby;
        }

        public bool ReadyToFire()
        {
            return tubeState == TubeState.Calibrating;
        }

        public override GameObject Fire()
        {
            // Check if this torpedo can fire
            if (!ReadyToFire()) return null;
            
            
            GameObject newTorp = base.Fire();

            torpedoInstance = newTorp.GetComponent<Explosive>();
            if (torpedoInstance)
            {
                torpedoInstance.triggerRange = triggerRange;
                torpedoInstance.onImpact += PostDetonation;
                torpedoInstance.onDudify += PostDetonation;
            }
            
            reloadProgress = 0;
            tubeState = TubeState.DetonateReady;

            SplineTorpedo splineTorpedo = newTorp.GetComponent<SplineTorpedo>();
            if (splineTorpedo)
            {
                splineTorpedo.Reset();
                splineTorpedo.driveBeginTime = armingTime;
                if (_splineInstance == null)
                {
                    Debug.LogError("Launching a spline torpedo, but tube " + name + " has no spline.");
                }
                else
                {
                    splineTorpedo.SetSpline(_splineInstance);
                    float radius = Mathf.Lerp(defaultTargetingRadius, .1f, calibrationProgress);
                    _splineInstance.ReleasedTorpedo(radius);
                }
            }

            // Rotate the torpedo to be facing the bridge's forward
            if (_bridge) newTorp.transform.rotation = _bridge.transform.rotation;

            return newTorp;
        }

        public bool DetonateReady()
        {
            if (torpedoInstance == null) return false;
            return torpedoInstance.isActiveAndEnabled;
        }

        public void Detonate()
        {
            if (!DetonateReady()) return;
            Debug.Log("Detonating torpedo instance " + torpedoInstance.name, torpedoInstance.gameObject);
            torpedoInstance.SelfDestruct();
        }

        void PostDetonation()
        {
            torpedoInstance = null;
            tubeState = TubeState.Empty;
            calibrationProgress = 0;
        }


        SplineTorpedo _torpedoPrefab;
        public override float BulletSpeed()
        {
            if (!ammoPrefab) return 0;

            if (!_torpedoPrefab)
            {
                _torpedoPrefab = ammoPrefab.GetComponent<SplineTorpedo>();
            }

            return _torpedoPrefab.speedCurve.Evaluate(5);
            //return driveSpeed;
        }
    }
}