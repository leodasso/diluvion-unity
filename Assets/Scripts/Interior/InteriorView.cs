using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

namespace Diluvion
{

    /// <summary>
    /// Interior view is used to look at any interior.  Controls masks to have a smooth transition 'cutaway' effect
    /// of the interior appearing on top of the 3D model.
    /// </summary>
    public class InteriorView : MonoBehaviour
    {

        static InteriorView interiorView;
        static float zigzagScale = 1;
        public static float mainShipZ = 0;

        //[Range(0, 100)]
        //public float maxParallax;

        [TabGroup("Runtime"), ShowInInspector, ReadOnly]
        InteriorManager interiorObject;

        [TabGroup("Setup")]
        public Camera localCam;

        [TabGroup("Setup")]
        public GameObject mask1;

        [TabGroup("Setup")]
        public GameObject mask2;

        [TabGroup("Setup")]
        public Transform maskRoot;

        [TabGroup("Runtime"), ShowInInspector, ReadOnly]
        InteriorManager viewedInterior;

        [TabGroup("Runtime"), ShowInInspector, ReadOnly]
        float spriteStackHeight = 0;

        [TabGroup("Runtime"), ShowInInspector, ReadOnly]
        Vector3 mask1Closed;

        [TabGroup("Runtime"), ShowInInspector, ReadOnly]
        Vector3 mask2Closed;

        [TabGroup("Runtime"), ShowInInspector, ReadOnly]
        float maskWidth = 0;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            localCam = GetComponentInChildren<Camera>();
            CenterMasks();
            mask1Closed = mask1.transform.localPosition;
            mask2Closed = mask2.transform.localPosition;
        }

        Camera LocalCam()
        {
            if (localCam) return localCam;
            localCam = GetComponentInChildren<Camera>();
            return localCam;
        }

        void LateUpdate()
        {
            MatchMainObject();
        }

        void CenterMasks()
        {
            mask1.transform.localPosition = -EdgePosition(mask1.transform);
            mask2.transform.localPosition = EdgePosition(mask2.transform);
        }

        void PlaceMaskRoot(Vector3 offset)
        {
            maskRoot.localPosition = offset;
        }

        Vector3 EdgePosition(Transform t)
        {
            return new Vector3((t.transform.localScale.x / 2) - (ZigzagWidth() * -.24f), 0, 0);
        }

        public static InteriorManager ViewedInterior()
        {
            return Get().interiorObject;
        }

        public static Camera InteriorCam()
        {
            return Get().LocalCam();
        }

        public static InteriorView Get()
        {
            if (interiorView != null) return interiorView;

            GameObject prefab = Resources.Load("Interior Cam") as GameObject;
            interiorView = Instantiate(prefab, Vector3.zero, Quaternion.identity).GetComponent<InteriorView>();
            return interiorView;
        }

        /// <summary>
        /// Destroys the interior view instance if it exists.
        /// </summary>
        public static void Clear()
        {
            if (!interiorView) return;
            
            //Debug.Log("DESPAWNING FROM:" + interiorView.name , interiorView); 
            DestroyImmediate(interiorView.gameObject);
        }


        /// <summary>
        /// Show the given interior.
        /// </summary>
        /// <param name="transitionTime">Duration of the masking in transition</param>
        public static void ShowInterior(InteriorManager interior, Bounds interiorBounds, float transitionTime = 1)
        {
            Get().viewedInterior = interior;

            if (interior == null)
            {
                Debug.Log("Can't display interior because it's null.");
                return;
            }

            // Center the interior
            interior.transform.localPosition = Vector3.zero;

            // Get the width & center of the interior, and use those to position the masks
            Bounds b = interiorBounds;
            
            float width = b.extents.x * interior.transform.localScale.x;
            Vector3 localCenter = Get().transform.InverseTransformPoint(b.center);
            Get().OpenMasks(transitionTime, width, localCenter);

            // play the audio
            interior.InteriorAudio(true);

            interior.onShow?.Invoke();
        }

        /// <summary>
        /// Hides the given interior.
        /// </summary>
        /// <param name="transitionTime">Duration of the masking out transition</param>
        public static void HideInterior(float transitionTime)
        {
            if (Get().viewedInterior == null)
            {
                Debug.Log("Can't display interior because it's null.");
                return;
            }

            
            Get().viewedInterior.EnableInstances();
            
            Get().StartCoroutine(Get().CloseMasks(transitionTime));
           // if (!Get().viewedInterior) return;
            Get().viewedInterior.InteriorAudio(false);

            if (Get().viewedInterior.onHide != null) Get().viewedInterior.onHide();
        }

        /// <summary>
        /// Opens the masks to view the interior.
        /// </summary>
        void OpenMasks(float transitionTime, float width, Vector3 offset)
        {
            maskRoot.localPosition = offset;
            SetMasks(true);
            CenterMasks();
            maskWidth = width * 1.2f;
            StartCoroutine(Mask(0, 1, transitionTime, width));
        }

        /// <summary>
        /// Close the masks to hide the interior. 
        /// </summary>
        IEnumerator CloseMasks(float transitionTime)
        {
            SetMasks(true);
            yield return StartCoroutine(Mask(1, 0, transitionTime, maskWidth));

            if (!viewedInterior) yield break;
            viewedInterior.TranslateOutOfView();
        }

        /// <summary>
        /// Moves the mask objects from start (closed) to end (open). When the masks are open, the interior
        /// will be fully visible.
        /// </summary>
        /// <param name="start">The value to start at. 0 is closed, 1 is open.</param>
        /// <param name="end">The value to end at.</param>
        /// <param name="transTime">Duration in seconds of the transition.</param>
        /// <param name="moveDist">Distance to move the masks.</param>
        /// <returns></returns>
        IEnumerator Mask(float start, float end, float transTime, float moveDist)
        {
            Vector3 mask1Open = new Vector3(mask1Closed.x - moveDist, mask1Closed.y, mask1Closed.z);
            Vector3 mask2Open = new Vector3(mask2Closed.x + moveDist, mask2Closed.y, mask2Closed.z);

            float value = start;
            float dir = end - start;

            while (Between(value, start, end))
            {
                value += dir * Time.deltaTime / transTime;
                float easedValue = SpiderWeb.Calc.EaseBothLerp(0, 1, value);

                mask1.transform.localPosition = Vector3.Lerp(mask1Closed, mask1Open, easedValue);
                mask2.transform.localPosition = Vector3.Lerp(mask2Closed, mask2Open, easedValue);
                yield return null;
            }

            SetMasks(false);
        }

        /// <summary>
        /// Returns true if the given value is in between start and end values.
        /// </summary>
        bool Between(float value, float start, float end)
        {
            if (value <= end && value >= start) return true;
            else if (value <= start && value >= end) return true;
            return false;
        }

        /// <summary>
        /// Sets the masks visible or invisible, via activating the game object.
        /// </summary>
        void SetMasks(bool active)
        {
            mask1.SetActive(active);
            mask2.SetActive(active);
        }

        /// <summary>
        /// Add the given interior object to the interior viewer. Generally this would be done as soon as the
        /// interior object is created.
        /// </summary>
        public static void AddInterior(InteriorManager intMan)
        {
            Get().AddInteriorObj(intMan);
        }

        /// <summary>
        /// Adds the given interior object and places it correctly out of view.
        /// </summary>
        void AddInteriorObj(InteriorManager intMan)
        {
            Vector3 localScale = intMan.transform.localScale;

            //put my interior inside the interior viewer
            intMan.transform.SetParent(transform, false);
            intMan.transform.localEulerAngles = Vector3.zero;
            intMan.transform.localScale = localScale;
            intMan.TranslateOutOfView();
        }

        /// <summary>
        /// Toggles the interior view camera component
        /// </summary>
        public static void ActivateCam(bool nowActive)
        {
            Get().ActivateInteriorCam(nowActive);
        }

        void ActivateInteriorCam(bool nowActive)
        {
            if (localCam == null) localCam = GetComponentInChildren<Camera>();
            localCam.enabled = nowActive;
        }

        /// <summary>
        /// Returns the width of the little zigzag at the jagged edge of the mask.
        /// </summary>
        static float ZigzagWidth()
        {
            return (zigzagScale * Mathf.Sqrt(2));
        }

        /// <summary>
        /// Call this after spawning a mask to fix Unity's stupid mask bug
        /// </summary>
        public void Jiggle()
        {
            StartCoroutine(QuickJiggle());
        }

        IEnumerator QuickJiggle()
        {
            yield return new WaitForSeconds(0.3f);
            localCam.enabled = true;
        }

        /// <summary>
        /// will match the angle of the 2d to the angle of the 3d object, 
        /// also matches the camera stats
        /// </summary>
        void MatchMainObject()
        {
            if (!OrbitCam.CurrentTarget()) return;
            if (localCam == null) return;

            //Get the main camera's target
            GameObject mainObject = OrbitCam.CurrentTarget().gameObject;

            SideViewerStats targetSideView = mainObject.GetComponent<SideViewerStats>();
            if (targetSideView == null) return;

            //Get the interior of the main object
            interiorObject = targetSideView.Interior();

            if (interiorObject == null) return;

            //rotate interior to match ship / object
            if (OrbitCam.TargetIsShip() && !targetSideView.overrideAsStatic)
            {
                mainShipZ = mainObject.transform.localEulerAngles.x;
                interiorObject.transform.localEulerAngles = new Vector3(localCam.transform.localEulerAngles.x, localCam.transform.localEulerAngles.y, -mainShipZ);
            }
            else
            {
                interiorObject.transform.localEulerAngles = new Vector3(localCam.transform.localEulerAngles.x, localCam.transform.localEulerAngles.y, 0);
            }

            //match all stats of this cam to other cam
            localCam.orthographicSize = Camera.main.orthographicSize;

            //get the height of the interior object
            float yPos = interiorObject.transform.localPosition.y;

            Vector3 camPos = OrbitCam.VirtualCam().transform.localPosition;
            Vector3 relativePos = new Vector3(camPos.x, camPos.y + yPos, -20);

            localCam.transform.localPosition = relativePos;
        }
    }
}