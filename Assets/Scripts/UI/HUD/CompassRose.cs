using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SpiderWeb;

namespace Diluvion
{
    /// <summary>
    /// Compass rose is a 3D GUI compass that appears around the player's ship. It contains pointers to all the 
    /// landmarks that the player currently has
    /// </summary>
    public class CompassRose : MonoBehaviour
    {
        [TabGroup("setup")]
        public float verticalOffset = 0;

        [TabGroup("setup")]
        public Transform shipNeedle;

        [TabGroup("setup")]
        public Transform shipCross;

        [TabGroup("setup")]
        public Transform camCross;

        [TabGroup("setup")]
        public LandmarkPointer landmarkIconPrefab;

        [TabGroup("runtime")]
        public Transform shipTransform;

        [ReadOnly, TabGroup("runtime")]
        public List<LandmarkPointer> landmarks = new List<LandmarkPointer>();

        [ReadOnly, ShowInInspector, TabGroup("runtime")]
        bool isActive = false;

        [ReadOnly, ShowInInspector, TabGroup("runtime")]
        List<LandMark> displayedLandmarks = new List<LandMark>();

        /// <summary>
        /// Creates a compass instance and places it inside the given transform. Loads from Resources "compass"
        /// </summary>
        public static void AddTo(Transform parent)
        {
            GameObject prefab = Resources.Load("compass") as GameObject;
            CompassRose instance = Instantiate(prefab, parent, false).GetComponent<CompassRose>();
            instance.shipTransform = parent;
        }

        // Use this for initialization
        void Start()
        {
            // load the player's known landmarks
            displayedLandmarks.Clear();
            foreach (LandMark l in LandMark.DiscoveredLandmarks())
                AddLandmark(l);
        }


        // Update is called once per frame
        void Update()
        {
            if (GameManager.Player().GetButtonDown("toggle nav"))
            {
                // AUDIO stuff
                if (!isActive) SpiderSound.MakeSound("Play_Compass_Toggle_ON", gameObject);
                else SpiderSound.MakeSound("Play_Compass_Toggle_OFF", gameObject);
                
                isActive = !isActive;
            }

            // Scale the compass to hide or show it.
            Vector3 scale = Vector3.zero;
            if (OrbitCam.Get().cameraMode == CameraMode.Normal && isActive) scale = Vector3.one;

            transform.localScale = Vector3.Lerp(transform.localScale, scale, Time.deltaTime * 8);

            // Move the ship needle
            if (shipTransform)
            {
                transform.rotation = Quaternion.Euler(Vector3.zero);
                transform.localPosition = Vector3.zero;
                transform.Translate(0, verticalOffset, 0);
                if (shipNeedle)
                {
                    shipNeedle.rotation = shipTransform.rotation;
                    shipNeedle.gameObject.SetActive(true);
                }

                if (shipCross) shipCross.rotation = shipTransform.rotation;
                if (camCross && OrbitCam.Get() != null) camCross.rotation = OrbitCam.Get().transform.rotation;
            }

            // If there's no ship transform, turn off the ship needle
            else if (shipNeedle)
                shipNeedle.gameObject.SetActive(false);
        }


        /// <summary>
        /// Add the given landmark to the list of landmarks currently being displayed by the compass
        /// </summary>
        public void AddLandmark(LandMark landmark)
        {
            if (displayedLandmarks.Contains(landmark)) return;
            displayedLandmarks.Add(landmark);

            LandmarkPointer newLandmark = Instantiate(landmarkIconPrefab) as LandmarkPointer;
            newLandmark.transform.SetParent(transform);
            newLandmark.transform.localPosition = Vector3.zero;
            newLandmark.Init(landmark);
            landmarks.Add(newLandmark);
        }
    }
}