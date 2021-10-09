using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

namespace Diluvion
{

    [AddComponentMenu("DQuest/Quest actor disable children")]
    public class QuestActorDisableChildren : QuestActor
    {

        [ToggleLeft]
        public bool disableOnAwake;

        void Awake()
        {
            if (disableOnAwake) SetChildrenActive(false);
        }

       /* private void OnEnable()
        {
            if(disableOnAwake) SetChildrenActive(false);
        }*/

        protected override void Tick ()
        {
            base.Tick();
            SetChildrenActive(isActive);
        }

        void SetChildrenActive(bool newActive)
        {
            foreach (Transform child in transform)
            {
                if (child.parent == transform) child.gameObject.SetActive(newActive);
            }
        }

        public override string ToString ()
        {
            string test = "Will enable children if ";
            test += base.ToString();
            test += " and vice versa.";
            return test;
        }
    }
}