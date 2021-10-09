using UnityEngine;
using Diluvion;

namespace DUI
{

    /// <summary>
    /// The UI element for when you die. Shows game over and options for return to menu or restart.
    /// </summary>
    public class EndMenu : DUIView
    {
        protected override void Start()
        {
            base.Start();
            Show();

            GameManager.Get().currentState = GameState.Ending;
            if (Application.isEditor) Cursor.lockState = CursorLockMode.None; else Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        protected override void Update()
        {
            base.Update();
            if (Application.isEditor) Cursor.lockState = CursorLockMode.None; else Cursor.lockState = CursorLockMode.Confined;
        }

        public void RestartLevel()
        {
            GameManager.ReloadFromFile();
            End(1);
        }

        public void ReturnToMenu()
        {
            GameManager.MainMenu();
            End();
        }

        public void ExitGame()
        {
            GameManager.MainMenu();
        }

        public void CloseApplication()
        {
            Application.Quit();
        }
    }
}