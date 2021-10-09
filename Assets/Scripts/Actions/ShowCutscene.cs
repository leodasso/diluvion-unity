using UnityEngine;

namespace Diluvion
{
    [CreateAssetMenu(fileName = "show cutscene", menuName = "Diluvion/actions/show cutscene")]
    public class ShowCutscene : Action
    {
        public GameObject cutscene;
        public float transitionTime = 1;
        public Color transitionColor = Color.black;
        [Space]
        public bool focusOnMe;
        public bool immediatelyStopControls;

        public override bool DoAction(UnityEngine.Object o)
        {
            Cutscene.ShowCutscene(cutscene, transitionTime, transitionColor);

            GameObject GO = GetGameObject(o);
            if (focusOnMe && GO) OrbitCam.FocusOn(GO);

            if (immediatelyStopControls) Cutscene.playerControl = false;
            return true;
        }

        protected override void Test()
        {
            Debug.Log(ToString());
            DoAction(null);
        }

        public override string ToString()
        {
            return "shows cutscene " + cutscene.name;
        }
    }
}