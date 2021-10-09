using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;

namespace Diluvion
{

    /// <summary>
    /// Behavior for the thorn enemy which appears in the endless corridor and final boss.
    /// Fires homing bullets at player.
    /// </summary>
    public class Thorn : MonoBehaviour
    {

        [Tooltip("Time between shots in a volley. A volley is the same size as the number of weapon points.")]
        public float fireCooldown = 5;

        [Tooltip("Time between volleys.")]
        public float reloadCooldown = 10;

        public float startDelay = 5;

        public float maxRange = 100;

        [Tooltip("The amount of force the ammo is ejected with.")]
        public float ammoForce = 100;

        public GameObject ammo;
        public List<Transform> weaponPoints;
        public Color gizmoColor;

        bool _firing;        // Is the player within range?
        bool _canFire = true;
        bool _visible;
        float _fireTimer;
        int _weaponPointIndex = 0;
        ArkResentment _parentArk;
        Renderer _mainRender;

        void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, maxRange);
        }


        // Use this for initialization
        IEnumerator Start()
        {

            SendMessageUpwards("NewThorn", SendMessageOptions.RequireReceiver);
            _mainRender = GetComponent<Renderer>();
            _parentArk = GetComponentInParent<ArkResentment>();

            yield return new WaitForSeconds(startDelay);

            StartCoroutine(FireLoop());
        }


        // Update is called once per frame
        void Update()
        {
            _visible = _mainRender.isVisible;

            if (!PlayerManager.PlayerShip()) return;

            // Check distance to player
            float distToPlayer = Vector3.Distance(transform.position, PlayerManager.PlayerTransform().position);

            // If player is within range, begin firing
            if (distToPlayer < maxRange) _firing = true;
            else _firing = false;
        }



        /// <summary>
        /// When run in update, handles the cooldowns of firing from multiple weapon points.
        /// </summary>
        IEnumerator FireLoop()
        {
            while (true)
            {
                // Check if visible for the first shot
                while (!_visible)
                    yield return new WaitForSeconds(.1f);

                // delay so there's time to see it before it fires
                yield return new WaitForSeconds(.3f);

                while (!_firing) yield return new WaitForSeconds(.1f);

                // Wait until cooldown is ready
                while (!_canFire) yield return null;

                // find which point to shoot from, and fire
                Transform t = weaponPoints[_weaponPointIndex];
                Fire(t);
            }
        }


        void Fire(Transform exit)
        {
            if (!PlayerManager.PlayerTransform()) return;
            GameObject newAmmo = Instantiate(ammo, exit.position, exit.transform.rotation);

            //add force
            float force = Random.Range(ammoForce, ammoForce * 1.5f);
            newAmmo.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, 0, force));

            // tell to follow player
            newAmmo.GetComponent<MagneticMovement>().target = PlayerManager.PlayerTransform();

            //shake camera
            OrbitCam.ShakeCam(.2f, transform.position);

            StartCoroutine(Cooldown());
        }

        IEnumerator Cooldown()
        {
            _canFire = false;

            _weaponPointIndex++;
            if (_weaponPointIndex >= weaponPoints.Count)
            {

                _weaponPointIndex = 0;
                _fireTimer = reloadCooldown;
            }
            else _fireTimer = fireCooldown;

            yield return new WaitForSeconds(_fireTimer);
            _canFire = true;
        }

        void OnDisable()
        {
            if (!Application.isPlaying) return;
            Debug.Log("Destroyed " + name);

            if (_parentArk == null) Debug.Log("No parent ark exists!", gameObject);

            else _parentArk.StartDamagedRoutine();
        }
    }
}