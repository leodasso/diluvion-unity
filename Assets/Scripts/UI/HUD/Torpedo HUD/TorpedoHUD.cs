using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Diluvion.Ships;
using UnityEngine;
using UnityEngine.UI;
using Diluvion.Sonar;

namespace DUI
{

    /// <summary>
    /// Controls a UI panel that displays info on the torpedo weapon system. This panel controls the weapon system as a whole,
    /// compared to TorpedoTubeHud, which displays info for only one tube in the system.
    /// </summary>
    public class TorpedoHUD : WeaponHUD
    {
        //public Animator reticleAnimator;
        public Transform tubeHudsParent;
        [Space]
        [Tooltip("Ref to the currently active target hud. There's a target hud for each torpedo tube, although only one can be" +
                 " active at a time.")]
        public TorpedoTargetHud activeTargetHud;
        public List<TorpedoTargetHud> targetHudInstances = new List<TorpedoTargetHud>();

        [Space] 
        public TorpedoTubeHud torpedoTubeHudPrefab;

        CanvasGroup _lockReticuleGroup;
        Image _torpIconImage;
        Vector2 _torpIconInitPos;
        Vector2 _torpIconSize;
        Transform _currentLockTarget;
        SonarStats _currentTarget;
        bool _isCharging;
        bool _aimingAtVulnerable;
        bool _hasLock;
        bool _isReloading;

        //float _currentAngle;
        //float _lockSize;
        Vector2 _reticulePos;
        RectTransform _rectTransform;

        protected override void Awake()
        {
            base.Awake();
            _rectTransform = GetComponent<RectTransform>();
        }

        public override void Init(WeaponSystem forWeaponSystem)
        {
            base.Init(forWeaponSystem);
            BuildTorpedoTubeHuds(forWeaponSystem);
        }

        
        void BuildTorpedoTubeHuds(WeaponSystem ws)
        {
            foreach (var tube in ws.allTorpedoTubeInstances)
            {
                var newTubeHud = Instantiate(torpedoTubeHudPrefab, tubeHudsParent).GetComponent<TorpedoTubeHud>();
                newTubeHud.torpedoTube = tube;
            }
        }

        
        protected override void Update()
        {
            base.Update();

            if (!weaponSystem) return;
            
            
            
            /*

            // check if player is charging
            _isCharging = weaponSystem.firing;

            // check if player has a lock on
            _aimingAtVulnerable = weaponSystem.AimingAtVulnerable();
            _hasLock = _isCharging && _aimingAtVulnerable;

            //_isReloading = weaponSystem.ReloadProgress() <= .98f;
            
            // send info to animator
            //reticleAnimator.SetBool("locked", _hasLock);
            //reticleAnimator.SetBool("charging", _isCharging);
            //reticleAnimator.SetBool("reloading", _isReloading);
            */
        }
        

        //Ray _castRay;
        //RaycastHit[] _hits;
        //List<RaycastHit> _orderedHits = new List<RaycastHit>();
        //Rigidbody _rb;
        

        Vector3 circlePos;
        void LateUpdate()
        {
            if (!weaponSystem) return;

            if (!_hasLock)
            {
                _rectTransform.anchoredPosition = Vector2.zero;
                circlePos = FollowTransform(weaponSystem.CleanAimPosition(), 20, Camera.main);
            }
            else
            {
                _reticulePos = FollowTransform(weaponSystem.GetSystemTarget().position, 20, Camera.main);
                circlePos = _reticulePos;
                
                transform.position = Vector3.Lerp(transform.position, _reticulePos,
                    Time.unscaledDeltaTime * 10);
            }
        }
    }
}
