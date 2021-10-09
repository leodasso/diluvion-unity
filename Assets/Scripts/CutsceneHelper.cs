using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

namespace Diluvion
{

    public class CutsceneHelper : MonoBehaviour
    {
        [Tooltip("Views the gameplay camera, but while maintaining the block on player controls.")]
        public bool usingGameplayCam = false;

        [ShowIf("usingGameplayCam")]
        public bool focusOnInhabitant = false;

        [ShowIf("focusOnInhabitant")]
        public CharacterInfo character;

        [Tooltip("Will play the end credits after this scene ends.")]
        public bool playCredits = false;

        public bool clearRenderSettingsFog;

        public bool autoEnd;
        [ShowIf("autoEnd")]
        public float secondsUntilEnd = 5;

        [Space]
        public bool startAnotherCutscene;

        [ShowIf("startAnotherCutscene")]
        public GameObject nextScene;
        [ShowIf("startAnotherCutscene")]
        public float nextSceneTransitionTime = 1;
        [ShowIf("startAnotherCutscene")]
        public Color nextSceneTransitionColor = Color.black;


        // Use this for initialization
        void Start()
        {
            if (autoEnd) StartCoroutine(WaitAndEnd(secondsUntilEnd));

            if (clearRenderSettingsFog) RenderSettings.fog = false;

            if (usingGameplayCam)
            {
                Diluvion.Cutscene.SetGameplayCams(true);

                if (focusOnInhabitant) OrbitCam.FocusOn(character);
            }
        }

        IEnumerator WaitAndEnd(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            EndCutscene();
            yield break;
        }

        /// <summary>
        /// Calls for the cutscene classs to end the cutscene.
        /// </summary>
        public void EndCutscene()
        {

            if (playCredits)
            {
                UIManager.Create(UIManager.Get().endCredits);
                return;
            }

            if (startAnotherCutscene && nextScene != null)
            {
                Diluvion.Cutscene.ShowCutscene(nextScene, nextSceneTransitionTime, nextSceneTransitionColor);
                return;
            }

            Diluvion.Cutscene.EndCutscene(1, Color.black);
        }
    }
}