using UnityEngine;
using Diluvion;

namespace Queries
{
    [CreateAssetMenu(fileName = "story state query", menuName = "Diluvion/queries/story state", order = 1)]
    public class StoryStateQuery : Query
    {
        [Space]
        public StoryState state;

        public override bool IsTrue(UnityEngine.Object o)
        {
            if (OrbitCam.Get() == null) return false;
            InteriorManager intMan = OrbitCam.Get().viewedInterior;
            if (intMan == null) return false;
            if (intMan.storyState != state) return false;

            return true;
        }

        protected override void Test()
        {
            Debug.Log(ToString() + ": " + IsTrue(null).ToString());
        }

        public override string ToString()
        {
            return "Current viewed interior in story state " + state.ToString() + " ";
        }
    }
}