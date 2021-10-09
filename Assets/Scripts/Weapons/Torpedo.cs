using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SpiderWeb;


namespace Diluvion
{
    using Sonar;
    using Ships;

    /// <summary>
    /// Torpedo component goes on any object with a munition component and controls driving, homing, and lock on.
    /// <see cref="TorpedoModule"/><see cref="TorpedoTube"/>
    /// </summary>
    [RequireComponent(typeof(SonarStats))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Munition))]
    public class Torpedo : MonoBehaviour
    {
        public float driveSpeed = 10;
        public float drivePower = 10;
        public float armingTime = 1;
        [ReadOnly]
        public float currentSpeed;
        [Tooltip("The forward speed at which steering is at full potential")]
        public float optimalSteeringSpeed = 5;
        [ReadOnly]
        public float calibratedSteering;

        [ReadOnly, Tooltip("The total steering power of this torpedo; factor of calibrated steering and speed")] 
        public float totalSteering;
        
        [Tooltip("The max steering of this torpedo")]
        public float steering = 1;
        public float forwardness = 100;
        public Transform target;
        public Vector3 torpedoTargetPosition;
        
        [Range(0, 1)]
        public float calibration = 1;

        public float startAimTime = 0;

        bool _targetedTorp;
        float _minSteering;
        float _timer;
        Munition _munition;
        Animator _anim;
        Rigidbody _rb;
        Vector3 _localVelocity;
        bool _begunDrive;
        List<ParticleSystem> particles = new List<ParticleSystem>();

        // Use this for initialization
        void Start()
        {
            _anim = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody>();
            _munition = GetComponent<Munition>();
            _munition.enabled = false;
            _munition.onDudify += SetDud;
            
            particles.AddRange(GetComponentsInChildren<ParticleSystem>());
            
        }

        void OnSpawned()
        {
            _timer = 0;
            if (_munition) _munition.enabled = false;
            _begunDrive = false;
        }

        public void StartParticles()
        {
            foreach (var ps in particles) ps.Play(true);
        }

        void SetDud()
        {
            enabled = false;
            foreach (var ps in particles) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        // Update is called once per frame
        void Update()
        {
            // timer
            if (_timer <= armingTime)
            {
                _timer += Time.deltaTime;
                return;
            }

            // Find valid target
            if (ValidTarget()) torpedoTargetPosition = target.position;
            
            _direction = torpedoTargetPosition - transform.position;
            RotateTowards(_direction);
            
            //Switch at the end of timer
            if(!_begunDrive)
            {
                _begunDrive = true;
                BeginDrive();
            }
        }
        
        Vector3 _direction;
        public void SetTorpedoTarget(Transform t, Vector3 startTargetPos)
        {
            target = t;
            
            if (target != null)
                _targetedTorp = true;
            else
                _targetedTorp = false;
            
            torpedoTargetPosition = startTargetPos;
            _direction = torpedoTargetPosition - transform.position;
        }

        bool ValidTarget()
        {
            return target && target.gameObject.activeInHierarchy;
        }

        bool FacingTarget()
        {
            return Vector3.Dot(_direction, transform.forward) > 0.99f;
        }
        
        void FixedUpdate()
        {
            if (!_munition.enabled) return;
            
            // Rotate towards target 
            if (ValidTarget())
            {
                torpedoTargetPosition = target.transform.position;
                _direction = torpedoTargetPosition - transform.position;
            }
            else
                if (!_targetedTorp)
                    if(!FacingTarget())
                        _direction = torpedoTargetPosition - transform.position;

            _localVelocity = transform.InverseTransformDirection(_rb.velocity);
            
            Vector3 flatLocalVelocity = new Vector3(_localVelocity.x, _localVelocity.y, 0) * forwardness;

            Vector3 totalForce = new Vector3(-flatLocalVelocity.x, -flatLocalVelocity.y, drivePower * 10);

            //apply speed
            if (ForwardVelocity() < driveSpeed)
                _rb.AddRelativeForce(totalForce * Time.fixedDeltaTime);
        }

        void RotateTowards(Vector3 dir)
        {
            totalSteering = Steering() * TurningPower();
            Quaternion newRot = Quaternion.Slerp(_rb.rotation, Quaternion.LookRotation(dir), totalSteering * Time.deltaTime);
            _rb.MoveRotation(newRot);
        }

        float Steering()
        {
            calibratedSteering = Mathf.Lerp(_minSteering, steering, calibration);
            return calibratedSteering;
        }

        public float TurningPower()
        {
            currentSpeed = ForwardVelocity();
            return Mathf.Clamp01(ForwardVelocity() / optimalSteeringSpeed);
        }

        public float ForwardVelocity()
        {
            if (!_rb) return 0;
            _localVelocity = transform.InverseTransformDirection(_rb.velocity);
            return _localVelocity.z;
        }

        /// <summary>
        /// Turn on the torpedo's motor, arms the munition
        /// </summary>
        void BeginDrive()
        {
            if (_anim) _anim.SetTrigger("launch");
            _munition.enabled = true;
        }

        void MakeFailNoise()
        {
            if (GetComponent<AKTriggerNegative>())
                GetComponent<AKTriggerNegative>().TriggerNeg();
        }

    }
}