using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Diluvion;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
using System.Diagnostics;
#endif

namespace DUI
{

    /// <summary>
    /// Base class for all DUI panels. These should mostly be created / destroyed through the functions in UI Manager.
    /// </summary>
    public class DUIPanel : MonoBehaviour
    {
        [TabGroup("basics", order: 0)]
        public bool oneInstanceOnly;

        [TabGroup("basics"), Tooltip("Checks compatibility with other open panels to see if this should be hidden. Less efficient, " +
                                     "so should only be used on panels that need it.")]
        public bool checkCompatibility = true;

        [TabGroup("basics")]
        public CanvasType canvasType = CanvasType.panel;

        [TabGroup("basics"), Tooltip("If true, the camera won't be able to move while this panel is up.")]
        public bool locksCamera;

        [TabGroup("basics")]
        public float fadeSpeed = 10;

        [TabGroup("context")]
        [Space]
        [Tooltip("will be hidden but remain in the scene for these modes.")]
        public List<CameraMode> hiddenModes = new List<CameraMode>();

        [TabGroup("context")]
        [Tooltip("If the camera is in any of these modes, the element will be removed.")]
        public List<CameraMode> invalidModes = new List<CameraMode>();

        [TabGroup("context")]
        [Tooltip("Hides any instance of the given prefabs while this window is up.")]
        public List<DUIPanel> incompatiblePrefabs = new List<DUIPanel>();

        [TabGroup("Tutorial", order: -1), ToggleLeft]
        public bool showTutorial;

        [TabGroup("Tutorial"), ShowIf("showTutorial")]
        public TutorialObject tutorial;

        [TabGroup("Tutorial"), ShowIf("showTutorial")]
        public Transform tutorialPoint;

        [TabGroup("Tutorial"), ShowIf("showTutorial")]
        public bool showOnStart = false;

        [HideInInspector]
        public DUIPanel prefab;
        protected CanvasGroup group;
        protected float alpha = 1;
        float clampAlpha = 1;
        protected Animator animator;

        /// <summary>
        /// has the element been shown yet? keeping track of this lets us show the 'intro' animation once on instantiation.
        /// </summary>
        protected bool introduced;

        protected virtual void Awake()
        {
            animator = GetComponent<Animator>();
            if (animator)
            {
                animator.logWarnings = false;
                if (animator.GetBool("enabled")) 
                    animator.SetBool("enabled", false);
            }

            group = SpiderWeb.GO.MakeComponent<CanvasGroup>(gameObject);
            group.alpha = 0;
            clampAlpha = 0;
        }


        #if UNITY_EDITOR
        void InstantiatePrefab()
        {
            
        }
        #endif

        protected virtual void Start()
        { }

        protected virtual void Update()
        {            
            //Clamp the alpha if this element should be hidden
            if (hiddenModes.Contains(OrbitCam.CamMode()) || UIManager.ShouldHide(prefab))
                clampAlpha = 0;
            else clampAlpha = 1;

            float a = Mathf.Clamp(alpha, 0, clampAlpha);
            group.alpha = Mathf.Lerp(group.alpha, a, Time.unscaledDeltaTime * fadeSpeed);

            // End the instance if it's no longer valid for the current camera mode
            if (invalidModes.Contains(OrbitCam.CamMode())) End();

            // If visible, introduce the element
            if (!introduced && clampAlpha == 1)
            {
                if (showOnStart) ShowTutorial();

                introduced = true;
                if (animator) animator.SetBool("enabled", true);
            }
        }
        
        
        

        /// <summary>
        /// Makes the panel visible or invisible; also affects the 'blocks raycasts' and 'interactible' properties 
        /// of the canvas group
        /// </summary>
        public void SetInteractive(bool isInteractive)
        {
            if (isInteractive)
            {
                group.blocksRaycasts = true;
                group.interactable = true;
                alpha = 1;
            }
            else
            {
                group.blocksRaycasts = false;
                group.interactable = false;
                alpha = 0;
            }
        }
        

        public bool IsVisible()
        {
            return clampAlpha > 0;
        }

        /// <summary>
        /// Snaps the alpha of the group immediately to the given value. 
        /// <para>Doesn't change the main alpha value, so after the snap it will fade back to the previous alpha.</para>
        /// </summary>
        public void SnapAlpha(float newAlpha)
        {
            group.alpha = newAlpha;
        }

        /// <summary>
        /// Sets the alpha, so this element will smoothly lerp to the set alpha.
        /// <para>Also see <see cref="SnapAlpha(float)"/></para>
        /// </summary>
        public void SetAlpha(float newAlpha)
        {
            alpha = newAlpha;
        }

        /// <summary>
        /// Returns true if this can close and exit the interior view.
        /// <para>Example: If a popup is up when the player tries to transition,
        /// would return false.</para>
        /// </summary>
        public virtual bool CanTransition()
        {
            return true;
        }

        /// <summary>
        /// Returns a transform for a UI element to be in where it will follow the given world space position
        /// <param name="screenEdgePadding"> Padding in pixels on edge of screen.</param>
        /// </summary>
        protected Vector3 FollowTransform(Vector3 pos, float screenEdgePadding, Camera cam)
        {
            if (cam == null) return Vector3.zero;

            Vector3 newPos = cam.WorldToScreenPoint(pos);

            //return newPos;
            
            int switcher = 1;

            //Lock to screen
            float clampedX = Mathf.Clamp(newPos.x, screenEdgePadding, (Screen.width - screenEdgePadding));
            float clampedY = Mathf.Clamp(newPos.y, screenEdgePadding, (Screen.height - screenEdgePadding));

            if (newPos.z < 0) //TODO add "Behind you" Logic so we can be warned about something behind you
                switcher = -1;

            Vector3 canvasPosition = new Vector3(clampedX * switcher, clampedY * switcher, 0);
            return canvasPosition;
        }

        /// <summary>
        /// Attempts to show the tutorial. If tutorial panel was made, returns it. Otherwise returns null.
        /// </summary>
        protected virtual TutorialPanel ShowTutorial()
        {
            if (!showTutorial || tutorial == null) return null;
            return TutorialPanel.ShowTutorial(tutorial, tutorialPoint);
        }

        public virtual void End()
        {
            End(0);
        }

        public virtual void End(float destroyDelay)
        {
            alpha = 0;
            UIManager.RemoveFromList(this);
            if (group == null) return;
            group.interactable = false;
            Destroy(gameObject, destroyDelay);
        }

        public virtual void DelayedEnd(float delay = 1)
        {
            StartCoroutine(WaitAndEnd(delay));
        }

        IEnumerator WaitAndEnd(float waitTime = 1)
        {
            while (waitTime > 0)
            {
                waitTime -= Time.unscaledDeltaTime;
                yield return null;
            }

            End();
        }
    }
}