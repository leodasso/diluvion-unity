using UnityEngine;
using Diluvion;
using Rewired;
using UnityEngine.UI;

namespace DUI
{
    public class PauseMenu : DUIView
    {

        public RectTransform mainPanel;
        public Button saveGameButton;
        public CanvasGroup menu;
        public PopupObject quitConfirmPopup;

        /// <summary>
        /// Gets the instance if it exists, otherwise returns null.
        /// </summary>
        static PauseMenu Get ()
        {
            return UIManager.GetPanel(UIManager.Get().pauseMenu) as PauseMenu;
        }

        /// <summary>
        /// Resets the pause menu, selecting it's default button. Returns false if 
        /// there was no pause menu instance.
        /// </summary>
        public static bool Reset ()
        {
            if (!Get()) return false;
            Get().ResetPauseMenu();
            return true;
        }

        // Use this for initialization
        protected override void Start ()
        {
            base.Start();

            fullyShowing = true;

            if (Application.isEditor) Cursor.lockState = CursorLockMode.None;
            else Cursor.lockState = CursorLockMode.Confined;
            
            // Check if the save game button should be available
            saveGameButton.interactable = PlayerManager.PlayerIsInSafeArea();
        }

        protected override void Update ()
        {
            base.Update();
            if (player.GetButtonDown("pause") && fullyShowing) BackToTarget();
        }

        protected override void SetDefaultSelectable ()
        {
            if (InSubMenu()) return;

            group.interactable = true;
            base.SetDefaultSelectable();
        }

        void ResetPauseMenu ()
        {
            group.interactable = true;
            menu.interactable = true;

            Debug.Log("Menu is interactible: " + menu.interactable);

            if (InSubMenu())
                Debug.Log("Attempted to reset pause menu, but still in a pause menu.");


            SetDefaultSelectable();
        }

        #region button actions
        public void QuitToMenu ()
        {
            quitConfirmPopup.CreateUI(GameManager.MainMenu, null);
        }

        
        public void QuitToDesktop ()
        {
            quitConfirmPopup.CreateUI(CloseProgram, null);
        }

        void CloseProgram()
        {
            if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        public void ShowLoadMenu()
        {
            menu.interactable = false;
            UIManager.Create(UIManager.Get().loadGamePanel as LoadMenu);
        }

        /*
        public void ReloadCheckpoint ()
        {
            GameManager.ReloadFromFile();
        }
        */

        SaveGamePanel _saveGamePanel;
        public void SaveGameAs()
        {
            menu.interactable = false;
            _saveGamePanel = UIManager.Create(UIManager.Get().saveGamePanel as SaveGamePanel);
        }

        public void Resume ()
        {
            End();
        }

        public void OpenControls ()
        {

            group.interactable = false;
            UIManager.ShowControlMapper();
        }

        public void OpenQuality ()
        {
            menu.interactable = false;
            UIManager.Create(UIManager.Get().qualitySettings);
        }


        #endregion

        /// <summary>
        /// Returns true if there's another sub-menu active on top of the pause menu.
        /// </summary>
        bool InSubMenu ()
        {
            // Check if the control mapper is open 
            if (UIManager.GetControlMapper().isOpen) return true;

            // Check if the settings is open  
            if (UIManager.GetPanel(UIManager.Get().qualitySettings) != null) return true;

            if (_saveGamePanel) return true;

            return false;
        }

        public override void BackToTarget ()
        {
            // When back is called, wait until other sub-menus are removed

            // If the controls menu is open, close it. This is handled differently than the other menu
            // because the controls menu is a plugin. 
            if (UIManager.GetControlMapper().isOpen)
            {
                UIManager.HideControlMapper(true);
                return;
            }

            if (InSubMenu()) return;

            base.BackToTarget();
        }


        protected override void FadeoutComplete ()
        {
            GameManager.Resume();
            Destroy(gameObject);
        }
    }
}