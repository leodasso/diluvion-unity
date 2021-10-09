using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Diluvion.Ships
{

    [RequireComponent(typeof(ShipMover))]
    public class ShipAnimator : MonoBehaviour
    {

        public float rudderRotation = 30;
        public float elevatorRotation = 30;
        public float propellerSpeed = 10;
        public float targetPropSpeed;
        public bool localAxis;
        
        public List<Transform> rudders;
        public List<Transform> elevators;
        public List<Transform> propellers;

        float emissionMultiplier = .5f;
        float changeShake = .04f;
        float spinTime = 1f;

        List<ParticleSystem> _elevatorParticles = new List<ParticleSystem>();
        List<ParticleSystem> _rudderParticles = new List<ParticleSystem>();
        List<ParticleSystem> _overdriveParticles = new List<ParticleSystem>();

        float _realPropellerSpeed;
        Vector3 _targetVector;

        float _rudderRot;
        float _elevatorRot;

        float _rudderResistance;
        float _elevatorResistance;

        GameObject _throttleChangeParticle;
        GameObject _newPropParticle;

        Rigidbody _rb;
        
        
        static GameObject _overdriveFx;
        static GameObject OverdriveEffectsPrefab()
        {
            if (_overdriveFx) return _overdriveFx;
            _overdriveFx = Resources.Load<GameObject>("effects/overdrive fx");
            return _overdriveFx;
        }

        static GameObject _throttleFx;
        static GameObject ThrottleEffectsPrefab()
        {
            if (_throttleFx) return _throttleFx;
            _throttleFx = Resources.Load("effects/throttle particle") as GameObject;
            return _throttleFx;
        }

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            // get the throttle change particle from the resources folder
            _throttleChangeParticle = ThrottleEffectsPrefab();

            GetParticles();
            
            // Create overdrive effects in each particle
            foreach (Transform prop in propellers)
            {
                GameObject newOverdriveFx = Instantiate(OverdriveEffectsPrefab(), prop);
                newOverdriveFx.transform.localEulerAngles = newOverdriveFx.transform.localPosition = Vector3.zero;
                _overdriveParticles.AddRange(newOverdriveFx.GetComponentsInChildren<ParticleSystem>());
            }
            
            // turn off overdrive particles
            SetOverdriveParticles(false);
        }

        public void SetOverdriveParticles(bool active)
        {
            if (active)
                foreach (var fx in _overdriveParticles) fx.Play();
            else 
                foreach (var fx in _overdriveParticles) fx.Stop();
        }

        /// <summary>
        /// Finds all the particles that are a direct child of the elevators / rudders
        /// </summary>
        void GetParticles()
        {
            foreach (Transform elevator in elevators)
                _elevatorParticles.Add(DirectParticleChildren(elevator));

            foreach (Transform rudder in rudders)
                _rudderParticles.Add(DirectParticleChildren(rudder));

            _elevatorParticles = _elevatorParticles.Where(x => x != null).ToList();
            _rudderParticles = _rudderParticles.Where(x => x != null).ToList();
        }

        /// <summary>
        /// Returns the first particle system that is direct child of transform t in the hierarchy.
        /// </summary>
        ParticleSystem DirectParticleChildren(Transform t)
        {
            foreach (ParticleSystem p in t.GetComponentsInChildren<ParticleSystem>())
            {
                if (p.transform.parent == t) return p;
            }
            return null;
        }

        public void MoveRudders(Vector3 moveDirection)
        {
            //get vector to target
            Vector3 newDir = transform.InverseTransformDirection(moveDirection);
            _targetVector = Vector3.Lerp(_targetVector, newDir, Time.deltaTime * 3);
        }

        void Update()
        {
            // Rotate the propellers
            _realPropellerSpeed = Mathf.MoveTowards(_realPropellerSpeed, targetPropSpeed, 220 * Time.deltaTime);

            if (_realPropellerSpeed != 0)
                SpinPropellers(_realPropellerSpeed);

            SurfaceResistances();

            // Move the rudders / elevators
            ControlRudderObjects();
        }

        /// <summary>
        /// Get the rudder / elevator resistance based on speed, angles
        /// </summary>
        void SurfaceResistances()
        {
            if (_rb == null) return;

            // Get forward velocity
            float forwardVel = transform.InverseTransformDirection(_rb.velocity).z;

            _rudderResistance = Mathf.Abs(_rudderRot * forwardVel);
            _elevatorResistance = Mathf.Abs(_elevatorRot * forwardVel);

            // Adjust the rate of emission of each particle system
            foreach (ParticleSystem ps in _rudderParticles)
                SetParticleEmission(ps, _rudderResistance * emissionMultiplier);

            foreach (ParticleSystem ps in _elevatorParticles)
                SetParticleEmission(ps, _elevatorResistance * emissionMultiplier);
        }

        ParticleSystem.EmissionModule em;
        void SetParticleEmission(ParticleSystem ps, float emission)
        {
            em = ps.emission;
            ParticleSystem.MinMaxCurve rate = em.rate;

            rate.constantMin = emission;
            rate.constantMax = emission;

            em.rate = rate;
        }


        void ControlRudderObjects()
        {

            _rudderRot = rudderRotation * _targetVector.x;
            _elevatorRot = elevatorRotation * _targetVector.y;

            // Control each rudder
            for (int i = 0; i < rudders.Count; i++) ControlRudder(rudders[i]);

            // Control each elevator
            for (int i = 0; i < elevators.Count; i++) ControlElevator(elevators[i]);

        }

        void ControlRudder(Transform rudder)
        {
            if (rudder == null) return;
            if (!localAxis)
                rudder.transform.localEulerAngles = new Vector3(0, _rudderRot, 0);
            else
                rudder.transform.localEulerAngles = new Vector3(rudder.transform.localRotation.eulerAngles.x, _rudderRot, rudder.transform.localRotation.eulerAngles.z);
        }

        void ControlElevator(Transform elevator)
        {
            if (elevator == null) return;
            elevator.transform.localEulerAngles = new Vector3(_elevatorRot, 0, 0);
        }

        /// <summary>
        /// Entirely cosmetic; calling this will have the propellers shake and vibrate, as if changing gears
        /// </summary>
        public void ShakePropellers()
        {
            if (!enabled) return;
            OrbitCam.ShakeCam(changeShake, transform.position);

            foreach (Transform p in propellers)
            {
                if (_throttleChangeParticle != null)
                {
                    _newPropParticle = Instantiate(_throttleChangeParticle, p.transform.position, p.transform.rotation) as GameObject;
                    _newPropParticle.transform.parent = p;
                }

                StartCoroutine(SpinBurstProp(p, spinTime));
            }
        }

        IEnumerator SpinBurstProp(Transform propeller, float time)
        {
            float t = 0;
            float lerp = 0;

            while (t < time)
            {
                yield return new WaitForEndOfFrame();

                lerp = t / time;

                propeller.Rotate(0, 0, propSpeed * lerp);
                t += Time.deltaTime;
            }

            yield break;
        }

        float propSpeed;
        void SpinPropellers(float speed)
        {
            propSpeed = speed * 10 * propellerSpeed * Time.deltaTime;

            foreach (Transform prop in propellers)
            {

                if (prop == null) continue;
                prop.Rotate(0, 0, propSpeed);
            }
        }
    }
}