using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SpiderWeb;
using Diluvion;
using Sirenix.OdinInspector;

namespace DUI
{

    public class DUIPopup : DUIView
    {

        [ReadOnly]
        public PopupObject popupPrefab;
        [Space]
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;

        public GameObject okGroup;
        public GameObject choiceGroup;

        public delegate void choiceDelegate();

        public choiceDelegate acceptDelegate;
        public choiceDelegate denyDelegate;

        bool ending = false;

        public void Init(PopupObject popup)
        {
            Init(popup, new List<string>());
        }

        public void Init(PopupObject popup, List<string> replacements)
        {
            popupPrefab = popup;
            
            titleText.text = popup.LocalizedTitle();
            descriptionText.text = popup.LocalizedMainText();

            titleText.text = String.Format(titleText.text, replacements);
            descriptionText.text = String.Format(descriptionText.text, replacements);

            GameManager.Freeze(this);

            if (popup.buttonSetup == PopupObject.ButtonSetup.okayButton)
            {
                okGroup.SetActive(true);
                choiceGroup.SetActive(false);
            }else
            {
                okGroup.SetActive(false);
                choiceGroup.SetActive(true);
            }

            //turn on cursor
            if (Application.isEditor) Cursor.lockState = CursorLockMode.None;
            else Cursor.lockState = CursorLockMode.Confined;
            Show();
        }

        // wait until animation is complete to stop time
        protected override void FadeinComplete()
        {
            GameManager.Freeze(this);
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            SelectOnMove();

            // hit exit button to exit
            if (player.GetButtonDown("pause"))
            {
                End();
                return;
            }

            ShowOnTop();
        }

        public void Accept()
        {
            if (acceptDelegate != null) acceptDelegate();
            End();
        }

        public void Deny()
        {
            if (denyDelegate != null) denyDelegate();
            End();
        }

        public override void End()
        {
            GameManager.UnFreeze(this);
            base.End();
        }

        protected override void FadeoutComplete ()
        {
            Destroy(gameObject);
        }
    }
}