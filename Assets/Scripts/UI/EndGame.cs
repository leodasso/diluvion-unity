using UnityEngine;
using System.Collections;
using Diluvion;

namespace DUI
{
    public class EndGame : DUIView
    {
        public bool restartGameOnExit = true;

        protected override void Start()
        {
            base.Start();

            GameManager.Freeze(this);
            Cursor.visible = true;
            if (Application.isEditor) Cursor.lockState = CursorLockMode.None; else Cursor.lockState = CursorLockMode.Confined;
            //StartCoroutine(EndDemotimer(endTime));
        }

        IEnumerator EndDemotimer(float time)
        {
            yield return new WaitForSeconds(time);
            EndGameToMenu();
        }

        public void EndGameToMenu()
        {
            GameManager.MainMenu();
        }

        public override void BackToTarget()
        {
            base.BackToTarget();
            if (restartGameOnExit) EndGameToMenu();
            GameManager.UnFreeze(this);
            Destroy(gameObject);
        }
    }
}