
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;
using HeavyDutyInspector;
using Rewired;
using Diluvion;
using Diluvion.SaveLoad;
using Diluvion.Ships;

public enum InputType
{
    button,
    axis
}

namespace DUI
{

    public class DUITutorialItem : MonoBehaviour
    {

        /*
        public CameraMode validCameraMode = CameraMode.Normal;
        public bool requireControlType = false;
        [HideConditional(true, "requireControlType", true)]
        public ShipControls.ControlType controlType = ShipControls.ControlType.arcade;
        public float delay = 1;
        [Comment("By default, tutorial items will save after their input is pressed. If Save After Shown is checked," +
            "the tutorial will save after it's been shown, regardless of input.")]
        public bool saveAfterShown;
        public bool slowsTime = true;
        public bool useTimeLimit = false;

        [HideConditional("useTimeLimit", true)]
        public float timeLimit = 0;

        [Space]
        public bool usePrerequisite;

        [HideConditional("usePrerequisite", true)]
        public DUITutorialItem prerequisite;

        [Space]
        public bool needsTag = false;
        new public string tag = "";
        public bool inhibitSideView = false;
        public bool inhibitChatter = false;
        public bool waitForShow = true;

        [Space]
        public bool checkHP;
        [Range(0, 1)]
        public float hpAmount = .5f;

        [Space]
        public string inputName = "input name";
        public bool twoInputs = false;
        [HideConditional(true, "twoInputs", true)]
        public string inputName2 = "input name";
        public InputType inputType;

        public bool complete = false;
        public float alpha = 0;

        Text tutorialText;
        CanvasGroup canvasGroup;
        //DUITutorial parentTutorial;
        RectTransform rectTransform;
        float gotoAlpha = 0;   //the alpha that the lerp will approach (most likely 0 or 1)
        OrbitCam orbitCam;
        Color gold = new Color(1, 1, 0);
        string colorString = "=#ffff00ff";
        bool showing;
        string replacement = "a button";
        bool reserved;
        float startPos = 400;
        float defaultPos;
        float yPos;

        bool endTutCoroutine = false;   // is the coroutine running now

        Player player;

        /*
        void Awake()
        {

            player = ReInput.players.GetPlayer(0);
            rectTransform = GetComponent<RectTransform>();
            defaultPos = rectTransform.anchoredPosition.y;
            yPos = startPos;
            rectTransform.anchoredPosition = new Vector2(0, startPos);
        }


        // Use this for initialization
        void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            parentTutorial = GetComponentInParent<DUITutorial>();
            orbitCam = OrbitCam.Get();
            tutorialText = GetComponent<Text>();

            // Load from save file
            if (DSave.current != null)
                if (DSave.current.TutorialComplete(tag)) complete = true;

            if (!needsTag) StartCoroutine(TryToShow());

            alpha = 0;
            ApplyAlpha();

            // Don't let the player use side view until this tutorial shows
            if (inhibitSideView && !complete)
                PlayerManager.PlayerShip().GetComponent<ShipControls>().canSideView = false;

            //if ( inhibitChatter && !complete && DUIController.Get() != null ) DUIController.Get().disallowChatter = true;
        }

        /// <summary>
        /// Finalizes the text by replacing any # with the button / axis name
        /// </summary>
        void FinalizeText()
        {

            string keyMapName = Controls.InputMappingName(inputName);

            // Optional second input
            if (twoInputs) keyMapName += ", " + Controls.InputMappingName(inputName2);

            tutorialText.text = tutorialText.text.Replace("#", keyMapName);

            //replace text highlight color with this color
            tutorialText.text = tutorialText.text.Replace("=red", colorString);
        }


        // Update is called once per frame
        void Update()
        {

            if (!showing) return;

            //if in side view, tutorial items should be invisible
            if (orbitCam.cameraMode != validCameraMode) alpha = 0;
            else alpha = Mathf.Lerp(alpha, gotoAlpha, Time.unscaledDeltaTime * 8);

            //adjust vertical position to animate in
            Vector2 pos = new Vector2(0, yPos);
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, pos, Time.unscaledDeltaTime * 8);

            ApplyAlpha();

            if (complete) return;

            //check if the player did what the current tutorial item is asking for
            if (InputWasUsed())
            {
                //turn the text gold
                tutorialText.color = new Color(gold.r, gold.g, gold.b, alpha);

                if (!endTutCoroutine)
                    StartCoroutine(EndTutorial());
            }
        }

        IEnumerator EndTutorial()
        {
            endTutCoroutine = true;

            if (waitForShow)
            {
                while (alpha < .98f)
                    yield return null;

                float timeElapsed = 0;

                while (timeElapsed < .5f)
                {
                    timeElapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            OrbitCam.Get().EndDOF();
            End(true);
            endTutCoroutine = false;
        }


        /// <summary>
        /// Return true if the player pressed the button i wanted them to
        /// </summary>
        bool InputWasUsed()
        {
            if (inputName.Length < 1) return false;
            if (inputType == InputType.button)
            {
                if (player.GetButtonDown(inputName)) return true;
            }
            else if (Mathf.Abs(player.GetAxis(inputName)) > .02f) return true;

            return false;
        }

        /// <summary>
        /// Returns true if the player's HP (percentage) is lower than the HP percentage I'm checking for.
        /// </summary>
        bool PlayerHPMatch()
        {

            if (!checkHP) return true;
            if (PlayerManager.pBridge == null) return false;

            if (PlayerManager.pBridge.hull.CurrentHPNormalized() < hpAmount) return true;
            return false;
        }


        IEnumerator TryToShow()
        {

            if (complete || showing || reserved) yield break;

            // Don't show tutorial if it's for the control type that is not currently selected
            if (requireControlType && (int)controlType != PlayerPrefs.GetInt(ShipControls.prefsControlType)) yield break;

            // Wait for conditions to be right to show
            while (
                !PlayerHPMatch() ||
                orbitCam.cameraMode != validCameraMode ||
                !PrereqMet() ||
                UIManager.InteractiveWindowOpen() ||
                parentTutorial.Busy()
            )
                yield return new WaitForSeconds(.5f);

            parentTutorial.Reserve(this);
            reserved = true;

            // Wait for the initial delay
            yield return new WaitForSeconds(delay);

            while (orbitCam.cameraMode != validCameraMode) yield return new WaitForSeconds(.5f);

            if (complete) yield break;

            Show();

            yield break;
        }



        void Show()
        {
            if (DSave.current != null) if (DSave.current.tutorialSkipped) return;

            // check for replacement string
            FinalizeText();

            // If this item inhibits side view, then allow side view when it shows
            if (inhibitSideView)
                PlayerManager.PlayerShip().GetComponent<ShipControls>().canSideView = true;

            //if ( inhibitChatter )
            //    DUIController.Get().disallowChatter = false;

            // Tell the tutorial manager that I'm showing
            parentTutorial.ShowItem(this);

            yPos = defaultPos;

            if (useTimeLimit) StartCoroutine(WaitAndEnd());

            showing = true;
            gotoAlpha = 1;
        }

        bool PrereqMet()
        {

            if (usePrerequisite)
            {
                if (prerequisite == null) return false;
                return prerequisite.complete;
            }
            return true;
        }

        public void CheckTag(string newTag)
        {
            if (newTag == tag) StartCoroutine(TryToShow());
        }

        IEnumerator WaitAndEnd()
        {

            yield return new WaitForSeconds(timeLimit);
            if (complete) yield break;
            End(false);
            yield break;
        }

        public void End(bool completed)
        {
            complete = true;
            gotoAlpha = 0;

            // saving
            if (completed || saveAfterShown)
            {
                if (DSave.current != null) DSave.current.TrySaveTutorial(tag);
            }

            // tell manager that I'm complete
            parentTutorial.HideItem(this);
        }

        /// <summary> Applies current alpha to the canvas group.</summary>
        void ApplyAlpha()
        {
            canvasGroup.alpha = alpha;
        }

        void OnDestroy()
        {

            // Don't let the player use side view until this tutorial shows
            if (PlayerManager.pBridge == null) return;

            if (inhibitSideView) PlayerManager.PlayerShip().GetComponent<ShipControls>().canSideView = true;
            //if ( inhibitChatter ) DUIController.Get().disallowChatter = false;
        }
        */
    }
}