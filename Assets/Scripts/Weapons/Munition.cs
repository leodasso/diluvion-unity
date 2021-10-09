using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;
using System.Linq;
using PathologicalGames;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Diluvion
{
    /// <summary>
    /// Munition is the base class for all things shot, thrown, blasted, etc. Mines, lasers, torpedoes!
    /// Munitions are always triggers, not colliders, and always include a rigidbody.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Munition : MonoBehaviour
    {
        [FoldoutGroup("Munition"), Tooltip("The default way munitions detect hits is by spherecasting from their previous" +
                                           " fixed update position to their current. Typically you would only disable this on" +
                                           " an inheriting class.")]
        public bool useSpherecastDetection = true;
        
        [Tooltip("Radius of the bullet. Used in spherecasting"), Range(0.1f, 10)]
        [FoldoutGroup("Munition"), EnableIf("useSpherecastDetection")]
        public float radius = .5f;
        
        [FoldoutGroup("Munition")]
        public float damage = 1;
        
        [FoldoutGroup("Munition")]
        public float critRate = 1;

        [FoldoutGroup("Munition"), Tooltip("Amount of time it will remain a dud before despawning. Allows effects time to finish")]
        public float dudTime = 2;
        
        [FoldoutGroup("Munition"), MinValue(0), Tooltip("The maximum effective range of this munition. Set to 0 for unlimited range.")]
        public float maxRange = 20;

        [FoldoutGroup("Munition")] 
        [Tooltip("The trail is spawned in on start, and de-parented from this object when it despawns. This way the trail can linger after" +
                 "the ammo itself has despawned.")]
        public GameObject trailEffectPrefab;
        //[Tooltip("The main cannon graphic needs to be seperate from the base object in order to allow particles to not dissapear")]
        //public GameObject graphic;
        
        /// <summary>
        /// The cannon that fired this munition
        /// </summary>
        [FoldoutGroup("Munition"), Tooltip("The cannon that fired this munition."), ReadOnly]
        public GameObject source;

        [FoldoutGroup("Munition"), Tooltip("The particle that spawns if this times out as a dud.")]
        public GameObject dudParticlePrefab;

        public event System.Action onDudify;
        public System.Action onImpact;

        Collider _col;
        Ray _impactRay;
        Vector3 _previousPos;
        RaycastHit _hit;
        RaycastHit[] _hits;
        LayerMask _hitMask;
        float _hitDistance;
        List<RaycastHit> _sortedHits = new List<RaycastHit>();
        Transform _trailEffectInstance;
        
        protected Rigidbody rb;
        protected bool _isDud;
        float _range;

        /// <summary>
        /// All damageable elements from the source of this munition. used to make sure this munition doesn't
        /// damage the ship that fired it
        /// </summary>
        protected List<Impactable> sourceImpactables = new List<Impactable>();

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = new Color(1, 0, 0, .3f);
            Gizmos.DrawSphere(transform.position, radius);
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody>();
            _hitMask = Calc.GunsafeLayer();
        }

        protected Rigidbody RB
        {
            get
            {
                if (rb != null) return rb;
                rb = GetComponent<Rigidbody>();
                return rb;
            }
        }
        
        protected Collider MyCollider()
        {
            if (_col != null) return _col;
            _col = GetComponent<Collider>();
            return _col;
        }

        /// <summary>
        /// Sets the source to the given object, and memorizes all damageable elements from that source.
        /// </summary>
        public void SetSource(GameObject newSource)
        {
            source = newSource;
            sourceImpactables.Clear();
            sourceImpactables.AddRange(source.GetComponentsInChildren<Impactable>());
        }


        public virtual void OnSpawned()
        {
            // Instantiate a trail effect
            if (trailEffectPrefab)
            {
                _trailEffectInstance = GameManager.Pool().Spawn(trailEffectPrefab, transform.position, transform.rotation);
                _trailEffectInstance.parent = transform;
            }

            _previousPos = transform.position - transform.forward*0.2f;
           
            //graphic.transform.localScale = _initScale;
            if (MyCollider()) MyCollider().isTrigger = true;
            //_firePos = transform.position;
            _isDud = false;
            _range = 0;
        }




        /// <summary>
        /// Determines if this impact is valid
        /// </summary>
        public bool ValidImpact(Impactable impactedObject)
        {
            //if (impactedObject == null) return false;
            
            // Don't impact if it's with my source (the thing that fired me)
            if (_isDud) return false;
            if (impactedObject != null)
                if (sourceImpactables.Contains(impactedObject)) return false;
            
            return true;
        }
        

        protected void Impact(GameObject impactedObject, Vector3 pos, Vector3 dir, float newDamage)
        {
            if (impactedObject == gameObject) return;
            if (impactedObject == source) return;

            // Impactable objects
            Impactable i = GO.ComponentInParentOrSelf<Impactable>(impactedObject);

            if (!ValidImpact(i)) return;
            
            onImpact?.Invoke();

            rb.velocity = Vector3.zero;
            
            if (pos == Vector3.zero) // If the position is zero, 
            {
                #if UNITY_EDITOR
                    EditorApplication.isPaused = true;
                #endif
                pos = transform.position;
            }

            Rigidbody impactedRigidbody = GO.ComponentInParentOrSelf<Rigidbody>(impactedObject);
            if (impactedRigidbody)
            {
                impactedRigidbody.AddForceAtPosition(dir * GameManager.Mode().impactForceMultiplier, pos);
            }

            i.Impact(dir, pos);

            // Damageable objects
            IDamageable damageable = GO.ComponentInParentOrSelf<IDamageable>(impactedObject);

            damageable?.Damage(newDamage, critRate, source);

            // Play audio
            if (damageable == null)
                SpiderSound.MakeSound("Play_WEA_Bolts_Impact_Indestructible", impactedObject);

            GameManager.Despawn(gameObject);
        }

        /// <summary>
        /// Impact with the given object. Checks for & interacts with impactable and damageable interfaces.
        /// </summary>
        public virtual void Impact(GameObject impactedObject, Vector3 pos)
        {
            Impact(impactedObject, pos, rb.velocity * rb.mass, damage);
        }

        #region Collisions and Triggers

        /// <summary>
        /// Raycasts between the previous position and the current position, looking for any hits in between.
        /// <para>Checks if the impacted hull belongs to what fired this ammo.</para>
        /// </summary>
        void DetectHit()
        {
            Vector3 rayDir = transform.position - _previousPos;
            _impactRay = new Ray(_previousPos, rayDir);
           
            _hitDistance = (transform.position - _previousPos).magnitude*1.1f;

            // Raycast 
            _hits = Physics.SphereCastAll(_impactRay, radius, _hitDistance, _hitMask.value);
    
            if (_hits.Length < 1) return;
    
            _sortedHits = _hits.OrderBy(h => h.distance).ToList();
    
            // Clean up the sorted hits list to not include myself (torpedoes do this)
            foreach (RaycastHit h in _hits)
                if (h.collider.transform == transform) _sortedHits.Remove(h);
    
            if (_sortedHits.Count < 1) return;
    
            _hit = _sortedHits[0];
            Vector3 returnHit = _hit.point;
            if (returnHit == Vector3.zero) // If the position is zero, 
                returnHit = _previousPos;

            Impact(_hit.collider.gameObject, returnHit);
        }

        protected virtual void FixedUpdate()
        {
            if (!_isDud) DetectHit();
            _previousPos = transform.position;

            // Make this a dud after it reaches max range. if Max range is 0, then its not calculated.
            if (maxRange > 0)
            {
                _range += VelocityMagnitude() * Time.fixedDeltaTime;
                if (_range >= maxRange) Dudify();
            }
        }

        float VelocityMagnitude()
        {
            if (rb && !rb.isKinematic) return rb.velocity.magnitude;
            if (MyVelocity()) return MyVelocity().velocity.magnitude;
            return 0;
        }

        /// <summary>
        /// Makes this thing a dud, as in it will no longer do damage.
        /// </summary>
        protected virtual void Dudify()
        {
            if (_isDud) return;
            _isDud = true;
            RB.useGravity = true;
            
            onDudify?.Invoke();
            
            StartCoroutine(DelayedDespawn(dudTime));
        }

        IEnumerator DelayedDespawn(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            GameManager.Despawn(gameObject);
        }

        /// <summary>
        /// Did this and the other given object come from the same source?
        /// </summary>
        protected bool SourceMatch(GameObject other)
        {
            //Debug.Log("Matching source: " + other, other);
            if (!other) return false;
            Munition m = other.GetComponent<Munition>();
            if (!m) return false;

            return (m.source == source);
        }

        #endregion

        PseudoVelocity _vel;
        protected PseudoVelocity MyVelocity()
        {
            if (_vel) return _vel;
            _vel = GetComponent<PseudoVelocity>();
            return _vel;
        }

        /// <summary>
        /// Sets up this instance to be just as it was at start.
        /// </summary>
        protected virtual void OnDespawned(SpawnPool pool)
        {
            Debug.Log("Despawned mkay");
            if (dudParticlePrefab) Instantiate(dudParticlePrefab, transform.position, transform.rotation);

            if (_trailEffectInstance)
            {
                _trailEffectInstance.parent = null;
                GameManager.Pool().Despawn(_trailEffectInstance, 6);
                _trailEffectInstance = null;
            }
            
            //graphic.transform.localScale = _initScale;
            transform.eulerAngles = Vector3.zero;
            if (RB)
            {
                RB.velocity = RB.angularVelocity = Vector3.zero;
                RB.useGravity = false;
            }
        }
    }
}