using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Diluvion.Ships;

namespace Diluvion
{

    /// <summary>
    /// Makes a list of all swarmers in this trigger. Then, if a ship enters the trigger, it sets that ship as
    /// the swarmers' target.
    /// </summary>
    public class SwarmerTrigger : Trigger
    {

        public Transform defaultTarget;
        List<Swarmer> swarmers = new List<Swarmer>();

        protected override void Start()
        {
            base.Start();
            StartCoroutine(LateStart());
        }

        IEnumerator LateStart()
        {
            yield return new WaitForSeconds(2);
            if (defaultTarget != null)
                SetSwarmersTarget(defaultTarget);
        }

        public override void Triggered(Collider other)
        {
            base.Triggered(other);

            // Add any swarmers inside my trigger to a list
            Swarmer otherSwarmer = other.GetComponent<Swarmer>();
            if (otherSwarmer)
            {
                if (!swarmers.Contains(otherSwarmer)) swarmers.Add(otherSwarmer);
                return;
            }
        }


        public override void TriggerAction(Bridge otherBridge)
        {
            base.TriggerAction(otherBridge);

            // If a hull enters the trigger, set it as the target
            Hull otherHull = otherBridge.GetComponent<Hull>();

            if (otherHull == null) return;

            SetSwarmersTarget(otherHull.transform);
        }


        void SetSwarmersTarget(Transform target)
        {
            foreach (Swarmer swarmer in swarmers) swarmer.SetTarget(target);
        }
    }
}