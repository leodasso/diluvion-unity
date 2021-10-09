using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;
using Diluvion.Ships;
using JetBrains.Annotations;
using Sirenix.OdinInspector;

namespace Diluvion
{

    public class OceanCurrent : MonoBehaviour
    {
        
        public OceanCurrentParams parameters;
        
        [Space]
        public Renderer debugRenderer;
        public bool deactivateRenderOnStart = true;
        
        [Tooltip("Set this object to tools layer on start")]
        public bool setToToolsLayer;
        
        List<Rigidbody> _bodiesInStream = new List<Rigidbody>();
        Bridge _currentBridge;
        Renderer _renderer;
        //bool _playerInStream;
        
        [Range(0, 1)]
        public float selectionChance = .2f;
        const float ShakeLevel = .03f;
        Vector3 _noisePos;
        float _noiseTime;

        void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position + _noisePos, 1);
        }


        [Button]
        public void RandomizeVisibleStreams()
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<ParticleSystem>()) continue;
                if (child.parent != transform) continue;
                
                child.gameObject.SetActive(!(Random.Range(0.0f, 1.0f) > selectionChance));
            }
        }

        // Use this for initialization
        void Start()
        {
            if (setToToolsLayer) gameObject.layer = LayerMask.NameToLayer("Tools");
            if (debugRenderer != null) debugRenderer.enabled = false;
        }


        Vector3 _finalForce;
        void FixedUpdate()
        {
            if (_bodiesInStream.Count < 1) return;
            
            // Generate a 'noisy' vector3 using perlin noise. The different inputs in the 3 axis are arbitrary,
            // its just so the axis aren't all doing the same noise pattern.
            _noisePos = new Vector3(
                Mathf.PerlinNoise(_noiseTime, -_noiseTime),
                Mathf.PerlinNoise(0, _noiseTime),
                Mathf.PerlinNoise(_noiseTime, 0));
            
            // subtract .5 from each, so that the values generated are between -.5 and .5, rather than 0 and 1
            _noisePos -= Vector3.one * .5f;
            
            foreach (Rigidbody rb in _bodiesInStream)
            {
                if (rb == null) continue;
                
                ApplyForceOnBody(rb);
                
                //shake camera
                if (rb.gameObject == PlayerManager.PlayerShip())
                    OrbitCam.ShakeCam(ShakeLevel, rb.transform.position);
            }
        }

        void ApplyForceOnBody(Rigidbody rb)
        {
            if (parameters == null) return;
            if (rb == null) return;
            _finalForce = transform.forward * parameters.force + parameters.noiseIntensity * _noisePos;
            rb.AddForceAtPosition(_finalForce, _noisePos * parameters.noiseOnPositionAmt + rb.transform.position);
        }

        void Update()
        {
            if (parameters == null) return;
            
            // Smoothly increase noiseTime using delta time. This is used to generate a perlin noise to apply 'random' 
            // force to any rigidbodies in the stream.
            _noiseTime += Time.deltaTime * parameters.noiseFrequency;
        }

        public Vector3 ForwardForce()
        {
            return transform.forward * parameters.force;
        }

        void OnTriggerEnter(Collider other)
        {
            // Ignore other currents
            if (other.GetComponent<IgnoreCurrent>()) return;

            // Find rigidbody
            Rigidbody otherRB = other.GetComponent<Rigidbody>();
            if (otherRB == null) return;

            // Tell the bridge it entered the current
            _currentBridge = other.GetComponent<Bridge>();
            if (_currentBridge != null)
            {
                _currentBridge.EnteredCurrent(transform.TransformDirection(transform.forward));

                // Add the ocean current particle
                if (_currentBridge == PlayerManager.pBridge)
                {
                    
                    PlayerManager.EnterOceanCurrent(this);
                    
                    /*
                    _playerInStream = true;
                    
                    // set the 'speed lines' particle
                    OceanCurrentParticle.Get().EnterStream(this);
                    SpiderSound.MakeSound("Play_Current_Loop", _currentBridge.gameObject);
                    */
                }
            }

            //and that rigidbody hasnt been added to list
            if (!_bodiesInStream.Contains(otherRB)) _bodiesInStream.Add(otherRB);
        }


        void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<IgnoreCurrent>()) return;
            if (!other.GetComponent<Rigidbody>()) return;
            
            _currentBridge = other.GetComponent<Bridge>();

            if (_currentBridge != null)
            {
                _currentBridge.ExitedCurrent();

                // remove the ocean current particle
                if (_currentBridge == PlayerManager.pBridge)
                {
                    
                    PlayerManager.ExitOceanCurrent(this);
                    //_playerInStream = false;
                    //OceanCurrentParticle.Get().ExitStream(this);
                    
                    //TODO Set the In and Out sound on the loop exit and enter calls
                    //SpiderSound.MakeSound("Stop_Current_Loop", _currentBridge.gameObject);
                }
            }

            //and that rigidbody is in our list
            if (_bodiesInStream.Contains(other.GetComponent<Rigidbody>()))
                _bodiesInStream.Remove(other.GetComponent<Rigidbody>());

        }

        void OnDestroy()
        {
            if (OceanCurrentParticle.Exists())
                OceanCurrentParticle.Get().ExitStream(this);
        }
    }
}