using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PathologicalGames;
using SpiderWeb;
using Sirenix.OdinInspector;

namespace Diluvion
{
    /// <summary>
    /// Explosive! Damages everything within a certain radius.
    /// </summary>
    //[RequireComponent(typeof(SphereCollider))]
    public class Explosive : Munition
    {
        [FoldoutGroup("Explosive")] 
        public bool requireTrigger = true;

        [PropertyTooltip(
            "The radius of the sphere trigger. Anything entering this range will trigger the explosive.")]
        [FoldoutGroup("Explosive"), OnValueChanged("SetTriggerCol")] 
        public float triggerRange = 3;

        public float explosionRadius = .5f;

        [FoldoutGroup("Explosive"), InlineEditor()]
        public Explosion explosionPrefab;

        //public event System.Action onExplode; 

        bool _exploded;
        SphereCollider _trigger;

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, triggerRange);
        }

        void SetTriggerCol()
        {
            if (!_trigger) _trigger = GO.MakeComponent<SphereCollider>(gameObject);
            _trigger.isTrigger = true;
            _trigger.radius = triggerRange;
        }

        protected override void Awake()
        {
            base.Awake();
            if (requireTrigger) SetTriggerCol();
        }

        void Start()
        {
            OnSpawned();
        }

        public override void OnSpawned()
        {
            base.OnSpawned();
            _exploded = false;
        }
        
        /// <summary>
        /// Check to see if any damageable elements entered the trigger, and if so, trigger the explosive.
        /// </summary>
        public virtual void OnTriggerEnter(Collider other)
        {
            if (!enabled || _exploded) return;

            // Check the other collider for damageable elements; Don't trigger from our own ship
            IDamageable damageable = GO.ComponentInParentOrSelf<IDamageable>(other.gameObject);
            if (sourceImpactables.Contains(damageable)) return;

            Impact(other.gameObject, transform.position);
        }

        /// <summary>
        /// Explodes on impact.
        /// </summary>
        public override void Impact(GameObject impactedObject, Vector3 pos)
        {
            Impactable i = GO.ComponentInParentOrSelf<Impactable>(impactedObject);

            if (!ValidImpact(i)) return;
            if (SourceMatch(impactedObject)) return;

            onImpact?.Invoke();

            string impacted = "nothing";
            if (impactedObject) impacted = impactedObject.name;
            Explode("Impacted with " + impacted);
        }

        /// <summary>
        /// Explodes without any of the safety checks
        /// </summary>
        public void SelfDestruct()
        {
            Debug.Log(name + " is self destructing.", gameObject);
            Impact(null, transform.position);
        }

        /// <summary>
        /// Spawns the explosion prefab
        /// </summary>
        protected void Explode(string why = "")
        {
            if (_exploded) return;
            _exploded = true;
            
            Explosion explosion = Instantiate(explosionPrefab, transform.position, transform.rotation);
            explosion.SetRadius(explosionRadius);
            Destroy(explosion.gameObject, 15);
        }
    }
}