using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Diluvion.Ships;
using Diluvion.Sonar;

namespace Diluvion
{
    [RequireComponent(typeof(SphereCollider))]
    public class AIWeapons : MonoBehaviour
    {

        [OnValueChanged("ResizeTarget")]
        public float range = 100;

        public LayerMask sonarMask;
        
        [InfoBox("Prioritizes the signature near the top of the list"),SerializeField]
        private List<Signature> enemySignatures = new List<Signature>();

        [SerializeField]
        private List<SonarStats> targets = new List<SonarStats>();

        [SerializeField] private SonarStats testSonarStats;

        private SphereCollider sc;
        private SphereCollider MySphereCollider
        {
            get
            {
                if (sc != null) return sc;
                sc = GetComponent<SphereCollider>();
                sc.isTrigger = true;
                sc.radius = range;
                return sc;
            }
        }
        
        private WeaponSystem ws;
        private WeaponSystem MyWeaponSystem {
            get
            {
                if (ws != null) return ws;
                ws = GetComponentInParent<WeaponSystem>();
                return ws;
            }
        }

        [Button]
        void TestFireTarget()
        {
            FireAtTarget(testSonarStats.transform);
        }
        
        [Button]
        void FireOn()
        {
            MyWeaponSystem.FireOn();
        }

        [Button]
        void FireOff()
        {
            MyWeaponSystem.FireOff();
        }

        void ResizeTarget()
        {
            MySphereCollider.radius = range;
        }

        private Transform highestTarget;
        Transform HighestTarget()
        {
            foreach (Signature s in enemySignatures)
            {
                if(s==null)continue;
                foreach (SonarStats t in targets)
                {
                    if(t==null||!t.gameObject.activeInHierarchy)continue;
                    if (t.signatures.Contains(s))
                        return t.transform;
                }
            }
            return highestTarget;
        }
        
        void Update()
        {
            if (targets.Count < 1)
            {
                FireOff();
                return;
            }

            Transform t = HighestTarget();

            if (t == null) return;
            if (MyWeaponSystem.Range() < Vector3.Distance(t.position, transform.position)) return;
            FireOn();
            FireAtTarget(t);
        }

        void FireAtTarget(Transform t)
        {
            MyWeaponSystem.SetAutoAimTarget(t);
        }

        void AddTarget(SonarStats target)
        {
            if (target == null) return;
            if (targets.Contains(target)) return;
            
            targets.Add(target);
        }

        void RemoveTarget(SonarStats target)
        {
            if (target == null) return;
            if (!targets.Contains(target)) return;
            targets.Remove(target);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other == null ) return;
            if (other.gameObject.layer != LayerMask.GetMask(sonarMask.ToString())) return;
            AddTarget(other.GetComponent<SonarStats>());
        }
        
        void OnTriggerExit(Collider other)
        {
            if (other == null ) return;
            if (other.gameObject.layer != LayerMask.GetMask(sonarMask.ToString())) return;
            RemoveTarget(other.GetComponent<SonarStats>());
        }
    }
}