using UnityEngine;

namespace Diluvion
{

    public class InteriorElement : MonoBehaviour
    {

        protected Collider2D col;

        // Use this for initialization
        protected virtual void Start()
        {

            col = GetComponent<Collider2D>();
        }

        protected virtual void Update()
        {
            ControlCollider();
        }

        /// <summary>
        /// Will turn off the collider (if found) if the camera isn't in interior view. Also turns it back on when camera is in interior.
        /// </summary>
        protected void ControlCollider()
        {
            if (col == null) return;

            if (OrbitCam.CamMode() != CameraMode.Interior && col.enabled) col.enabled = false;
            if (OrbitCam.CamMode() == CameraMode.Interior && !col.enabled) col.enabled = true;
        }
    }
}