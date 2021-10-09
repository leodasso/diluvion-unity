using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Diluvion
{

    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("DQuest/Quest actor animator")]
    public class QuestActorAnimator : QuestActor
    {
        [Tooltip("Trigger to set when this quest actor is activated.")]
        public string triggerOnActivate;

        [Tooltip("Trigger to set when this quest actor is de-activated.")]
        public string triggerOnDeactivate;
        public string boolName;

        /// <summary>
        /// if true, the bool value will be the opposite of this quest actor's status.
        /// </summary>
        [Tooltip("if true, the bool value will be the opposite of this quest actor's status.")]
        public bool invertBool;

        public Animator animator;

        private void Start ()
        {
            animator = GetComponent<Animator>();
        }

        protected override void OnActivate ()
        {
            base.OnActivate();

            if (!string.IsNullOrEmpty(triggerOnActivate)) animator.SetTrigger(triggerOnActivate);

            if (!string.IsNullOrEmpty(boolName))
                animator.SetBool(boolName, !invertBool);
        }

        protected override void OnDeactivate ()
        {
            base.OnDeactivate();

            if (!string.IsNullOrEmpty(triggerOnDeactivate)) animator.SetTrigger(triggerOnDeactivate);

            if (!string.IsNullOrEmpty(boolName))
                animator.SetBool(boolName, invertBool);
        }
    }
}