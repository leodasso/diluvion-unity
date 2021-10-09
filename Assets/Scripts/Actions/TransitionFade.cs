using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion
{

    /// <summary>
    /// Brings in a transition fade (fade out, then fade back in)
    /// </summary>
    [CreateAssetMenu(fileName = "new transition", menuName = "Diluvion/actions/transition")]
    public class TransitionFade : Action
    {

        public bool freezeTime = true;
        [Range(.1f, 5)]
        public float transitionLength = 1;
        public Color transitionColor = Color.black;

        public override bool DoAction (UnityEngine.Object o)
        {
            if (freezeTime) GameManager.Freeze(this);

            FadeOverlay.FadeInThenOut(transitionLength, transitionColor, UnFreeze);

            return true;
        }

        void UnFreeze()
        {
            GameManager.UnFreeze(this);
        }

        protected override void Test ()
        {
            if (!Application.isPlaying) return;
            DoAction(null);
        }

        public override string ToString ()
        {
            return "Starts a fade to " + transitionColor.ToString() + " transition for " + transitionLength + " seconds";
        }


    }
}