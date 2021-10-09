using UnityEngine;
using System.Collections;
using Diluvion;
using UnityEngine.EventSystems;
using Rewired;
using Sirenix.OdinInspector;

namespace DUI
{

    [RequireComponent(typeof(CanvasGroup))]
    public class DUIView : DUIPanel
    {

        [TabGroup("Interactive Panel")]
        public bool usesNavigation = true;

        [TabGroup("Interactive Panel")]
        public bool fadeInOnStart = false;

        [TabGroup("Interactive Panel")]
        public bool interactableOnFadein = true;

        [TabGroup("Interactive Panel")]
        public bool closeWithBackButton = true;

        [TabGroup("Interactive Panel")]
        [Tooltip("Use priority selection on popups or other things that should take priority when setting current selectable.")]
        public bool prioritySelection = false;

        [TabGroup("Interactive Panel")]
        public DUIView backTarget;

        [TabGroup("Interactive Panel")]
        public bool noBack = false;

        [TabGroup("Interactive Panel")]
        public GameObject defaultSelection; // when this window becomes active, which selectable object will be selected?

        protected Player player;
        protected bool fullyShowing = false;
        bool isTarget = true;


        protected override void Start()
        {
            base.Start();

            player = GameManager.Player();
            SetDefaultSelectable();

            if (fadeInOnStart) Show();
        }

        protected override void Update()
        {
            base.Update();

            if (player.GetButtonDown("cancel") && fullyShowing && closeWithBackButton) BackToTarget();
            SelectOnMove();
        }


        /// <summary>
        /// If the event system has no current selection and the user tries to use gamepad,
        /// resets the selection to the default. <para>This is called in the DUIView update, so if you overwrite update
        /// without having base.update, be sure this is in your child class update.</para>
        /// </summary>
        protected void SelectOnMove()
        {
            var eventSystem = UIManager.GetEventSystem();
            if (!eventSystem) return;

            if (eventSystem.currentSelectedGameObject != null) return;
            if (GamepadSelectionTried()) SetDefaultSelectable();
        }

        /// <summary>
        /// Is the user currently trying to select with the gamepad?
        /// </summary>
        /// <returns></returns>
        protected bool GamepadSelectionTried()
        {
            float i = Mathf.Abs(player.GetAxis("select X")) + Mathf.Abs(player.GetAxis("select Y"));

            return i > .05f;
        }


        /// <summary>
        /// General function for ending this window. If a target is defined, will re-activate that target.
        /// if not, just fades out and calls FadeoutComplete() at the end of the fade.
        /// </summary>
        public virtual void BackToTarget()
        {
            if (noBack) return;
            if (!isTarget)return;
            
            isTarget = false;

            StopAllCoroutines();

            if (backTarget) ShowAnother(backTarget);

            Hide();
        }

        /// <summary>
        /// Tells the event system to select this object's default selectable, if defined.
        /// </summary>
        protected virtual void SetDefaultSelectable()
        {
            if (defaultSelection == null) return;
            SetCurrentSelectable(defaultSelection);
        }

        /// <summary>
        /// Tells the event system to select the given game object. Will only set selectable if player isn't using the
        /// mouse.
        /// </summary>
        protected void SetCurrentSelectable(GameObject go)
        {
            if (UIManager.GetEventSystem().currentSelectedGameObject == go) return;

            // Check for player's most recent input type
            if (player == null) player = ReInput.players.GetPlayer(0);

            Controller lastUsedController = player.controllers.GetLastActiveController();

            if (lastUsedController != null)
                if (lastUsedController.type == ControllerType.Mouse) return;

            if (!prioritySelection)
                UIManager.GetEventSystem().SetSelectedGameObject(go);
            else
                StartCoroutine(SelectOnEndOfFrame(go));
        }

        IEnumerator SelectOnEndOfFrame(GameObject go)
        {
            // wait two frames
            yield return null;
            yield return null;

            UIManager.GetEventSystem().SetSelectedGameObject(go);
        }

        /// <summary>
        /// Sets active and fades in another DUI View
        /// </summary>
        /// <param name="window"></param>
        protected void ShowAnother(DUIView window)
        {
            Hide();
            window.gameObject.SetActive(true);
            window.Show();
        }

        /// <summary>
        /// Fades in the canvas group alpha at the given fade speed.
        /// </summary>
        public void Show()
        {
            isTarget = true;
            StartCoroutine(FadeIn(group, fadeSpeed));
        }

        public void Hide()
        {
            Debug.Log(name + "Hiding");
            StartCoroutine(FadeOut(group, fadeSpeed));
        }

        protected IEnumerator FadeIn(CanvasGroup cv, float speed)
        {
            alpha = 0;
            cv.blocksRaycasts = false;
            cv.interactable = false;

            yield return StartCoroutine(FadeToAlpha(1, speed));

            cv.blocksRaycasts = interactableOnFadein;
            cv.interactable = interactableOnFadein;

            fullyShowing = true;
            FadeinComplete();

            SetDefaultSelectable();
        }

        protected IEnumerator FadeOut(CanvasGroup cv, float speed, float delayTime = 0)
        {
            if (Time.timeScale > 0)
                yield return new WaitForSeconds(delayTime);

            fullyShowing = false;

            yield return StartCoroutine(FadeToAlpha(0, speed));

            cv.blocksRaycasts = false;
            cv.interactable = false;
            FadeoutComplete();
        }

        IEnumerator FadeToAlpha(float newAlpha, float speed)
        {
            float progress = 0;
            float initAlpha = alpha;

            while (progress < 1)
            {
                progress += Time.unscaledDeltaTime * speed;
                SnapAlpha(Mathf.Lerp(initAlpha, newAlpha, progress));
                yield return null;
            }

            alpha = newAlpha;
        }

        /// <summary>
        /// Places this element on top of all other elements within the same parent.
        /// </summary>
        protected void ShowOnTop()
        {
            if (transform.GetSiblingIndex() != transform.parent.childCount - 1)
                transform.SetAsLastSibling();
        }

        /// <summary>
        /// Gets called any time a fadeout is complete. The default action is to set this gameobject inactive.
        /// </summary>
        protected virtual void FadeoutComplete()
        {
            Destroy(gameObject);
            //gameObject.SetActive(false);
        }

        protected virtual void FadeinComplete() { }

        public override void End()
        {
            BackToTarget();
        }
    }
}