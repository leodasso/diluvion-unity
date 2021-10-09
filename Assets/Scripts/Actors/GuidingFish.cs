using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Diluvion
{
    [RequireComponent(typeof(Navigation))]
    public class GuidingFish : MonoBehaviour
    {
        [BoxGroup("Nav")]
        public Transform destination;

        public float jumpDistance = 20;
        [ShowInInspector]
        float _jumpFuel;

        public float jumpSpeed = 5;

        [BoxGroup("Nav")]
        [Tooltip("Refresh the course after moving this many units.")]
        public float refreshDistance = 10;

        [ReadOnly, BoxGroup("Nav")]
        public Vector3 destPosition;

        [ReadOnly, BoxGroup("Nav")]
        public Vector3 gotoPos;

        public ParticleSystem fishParticles;

        [ReadOnly, BoxGroup("Nav")]
        public Navigation navigation;

        Vector3 _lastRefresh;

        Vector3 _jumpPos;

        void OnDrawGizmos ()
        {
            if (!destination) return;
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(gotoPos, .5f);
            Gizmos.DrawLine(transform.position, gotoPos);
        }


        // Use this for initialization
        void Start ()
        {
            navigation = GetComponent<Navigation>();
            _jumpPos = transform.position;
        }

        // Update is called once per frame
        void Update ()
        {
            if (!navigation || !fishParticles || !destination ) return;
            
            // Get a direction to the next navigation waypoint
            Vector3 direction = (gotoPos - transform.position).normalized;
            
            _jumpFuel = Mathf.Lerp(_jumpFuel, 0, Time.deltaTime);
            transform.Translate(_jumpFuel * direction * jumpSpeed * Time.deltaTime, Space.World);

            // Rotate the fish to point towards the destination
            var finalRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, Time.deltaTime * 3);

            // Check distance to the point where we last refreshed. If we've moved more than the threshhold,
            // check the course again. This allows the nav system to take shortcuts and make a cleaner route.
            float dist = Vector3.Distance(_lastRefresh, transform.position);
            if (dist > refreshDistance)
            {
                _lastRefresh = transform.position;
                navigation.SetCourse(destPosition);
            }

            // Check if the destination has changed. If so, have the navigation
            // re-check the path to the destniation.
            if (destPosition != destination.position)
            {
                destPosition = destination.position;
                navigation.SetCourse(destPosition);
            }

            gotoPos = navigation.Waypoint();
        }


        /// <summary>
        /// On a trigger enter, bump the fish forward towards destination if the player has a waypoint active.
        /// </summary>
        void OnTriggerEnter (Collider other)
        {
            if (other.gameObject == PlayerManager.PlayerShip())
            {
                // Check for the main waypoint.
                QuestActor wp = QuestManager.MainWaypoint();
                if (wp != null) destination = wp.transform;

                if (!destination) return;

                navigation.SetCourse(destination.position);

                JumpForward();
            }
        }

        /// <summary>
        /// Moves the fish along the path
        /// </summary>
        [Button]
        public void JumpForward ()
        {
            if (!destination || !navigation) return;

            _jumpFuel = jumpDistance;

            //_jumpPos = transform.forward * jumpDistance + transform.position;
        }

        [ButtonGroup]
        public void ShowFish ()
        {
            fishParticles.Play();
        }

        [ButtonGroup]
        public void HideFish ()
        {
            fishParticles.Stop();
        }
    }
}