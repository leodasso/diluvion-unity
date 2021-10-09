using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Diluvion.SaveLoad;

namespace DUI
{
    public class QualitySettings : DUIView
    {
        public Toggle fullscreenToggle;
        public Toggle tutorialToggle;
        public Toggle showMapPosToggle;

        //public static string prefsFOVkey = "_fovAdjust";
        public static string prefsMapKey = "_showMapPos";

        // Use this for initialization
        protected override void Start ()
        {
            base.Start();

            // load players pref on seeing their position on map
            showMapPosToggle.isOn = PlayerPrefs.GetInt(prefsMapKey, 1) == 1;

            fullscreenToggle.isOn = Screen.fullScreen;

            if (DSave.current != null) tutorialToggle.isOn = !DSave.current.tutorialSkipped;
        }

        protected override void Update ()
        {
            base.Update();
            if (player.GetButtonDown("pause")) BackToTarget();
        }

        static QualitySettings _instance;
        public static QualitySettings Get()
        {
            if (_instance) return _instance;

            _instance = UIManager.GetPanel<QualitySettings>();
            return _instance;
        }

        public static void Reset()
        {
            if (Get()) Get().@group.interactable = true;
        }


        public void ShowControls()
        {
            group.interactable = false;
            UIManager.ShowControlMapper();
        }

        public void SetTutorialStatus (bool isOn)
        {
            if (DSave.current != null)
                DSave.current.tutorialSkipped = !isOn;
        }

        public void SetMapPosToggle (bool isOn)
        {
            int setting = isOn ? 1 : 0;
            PlayerPrefs.SetInt(prefsMapKey, setting);

            Debug.Log("Prefs value is now: " + PlayerPrefs.GetInt(prefsMapKey, 1));
        }

        public void SetFullscreen(bool isOn)
        {
            Screen.fullScreen = isOn;
        }


        public void ApplyAll ()
        {
            foreach (SliderExtension s in GetComponentsInChildren<SliderExtension>())
                s.Apply();

            SaveLanguage();
        }

        /// <summary>
        /// Saves the current language to player prefs. Called whenever a language button is clicked
        /// </summary>
        public void SaveLanguage ()
        {
            PlayerPrefs.SetString("language", I2.Loc.LocalizationManager.CurrentLanguage);
        }

        public override void End ()
        {
            SaveLanguage();
            PauseMenu.Reset();
            base.End();
        }

        protected override void FadeoutComplete ()
        {
            PauseMenu.Reset();
            Destroy(gameObject);
        }
    }
}