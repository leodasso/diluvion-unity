using UnityEngine;
using Sirenix.OdinInspector;

namespace Diluvion
{
    [RequireComponent(typeof(CameraStats))]
    public class SideViewerStats : MonoBehaviour
    {
        [Tooltip("The interior manager linked to this object.")]
        public InteriorManager intMan;
        [Space]

        [Tooltip("The size of the orthographic camera viewing the interior. 1 is normal, .5 is zoomed in, 2 is zoomed out.")]
        public float defaultOrthoSize = 1;

        [MinValue(0)]
        public float minOrthoSize = 0;

        [Tooltip("If true, the camera won't let the object freely rotate in the frame.")]
        public bool overrideAsStatic = false;

        [Tooltip("Distance from center of object that 3D cam will set the near clipping plane.")]

        public float nearClip = 5;

        [Tooltip("Distance from center of object that the 3D cam will set the far clipping plane.")]
        public float farClip = 5;
        public float sideViewSeconds = 1;
        public bool separateTromboneEffect = true;
        public bool useDOF = false;
        public float cushion = 0;
        public Vector3 sideViewRotation = new Vector3(0, 270, 0);

        bool interiorAdded = false;

        void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            
            //Get the vector 3 based on the side view rotation
            Quaternion sideViewRot = Quaternion.Euler(sideViewRotation);
            Vector3 camVector = sideViewRot * Vector3.forward;
            
            //camVector = transform.rotation * camVector;

            //Get the vectors for near, far clip
            Vector3 nearClipVector = camVector * -nearClip;
            Vector3 farClipVector = camVector * farClip;

            // DONT DELETE THIS MOTHERFUCKER
            Gizmos.color = Color.green;
            Gizmos.DrawRay(new Ray(Vector3.zero, nearClipVector));
            Gizmos.color = Color.red;
            Gizmos.DrawRay(new Ray(Vector3.zero, farClipVector));
        }

        [ButtonGroup]
        void CreateInterior()
        {
            GameObject newInterior = new GameObject();
            newInterior.transform.parent = transform;
            newInterior.transform.localPosition = newInterior.transform.localEulerAngles = Vector3.zero;

            newInterior.layer = LayerMask.NameToLayer("Interior");
            newInterior.name = name + " interior";

            GameObject orientation = new GameObject("orientation");
            orientation.transform.parent = newInterior.transform;
            orientation.transform.localPosition = orientation.transform.localEulerAngles = Vector3.zero;
            orientation.layer = LayerMask.NameToLayer("Interior");

            InteriorManager interior = newInterior.AddComponent<InteriorManager>();
            intMan = interior;
            AlignInterior();
        }
       
       
        [Button]
        [ButtonGroup]
        void AlignInterior()
        {
            if (intMan == null) return;
            intMan.transform.localEulerAngles = sideViewRotation;
        }

        void Awake ()
        {
            Interior();
        }

        public void DestroyInterior()
        {
            if (Interior() != null)
            {
                if (Application.isPlaying)
                {
                     Destroy(Interior().gameObject);
                }
                else
                    DestroyImmediate(Interior().gameObject);
            }
        }

        //Safe, Cached InteriorManagerGetter
        public InteriorManager Interior()
        {
            if (intMan != null) return intMan;
            intMan = GetComponentInChildren<InteriorManager>();
            return intMan;
        }

        /// <summary>
        /// Removes the current interior and adds the given interior.
        /// </summary>
        public void SwapInteriors(InteriorManager newInterior)
        {
            DestroyInterior();
         
            InteriorManager newInstance = Instantiate(newInterior, transform) as InteriorManager;
            newInstance.transform.position = transform.position;          
            intMan = newInstance;           
        }

        public float SideViewSeconds()
        {
            return sideViewSeconds;
        }


        void OnDisable()
        {
            if (!Application.isPlaying) return;
            if (intMan != null)
            {
                intMan.gameObject.SetActive(false);
            }
        }

        void OnEnable()
        {
            if (intMan != null) intMan.gameObject.SetActive(true);
        }


        void OnDestroy()
        {
            DestroyInterior();
        }
    }
}