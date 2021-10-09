using UnityEngine;
using System.Collections;

namespace Diluvion
{
    [CreateAssetMenu(fileName = "popup action", menuName = "Diluvion/actions/open popup", order = 0)]
    public class OpenPopup : Action
    {
        [Space]
        public PopupObject popup;

        public override bool DoAction(Object o)
        {
            if (!popup) return false;
            popup.CreateUI();
            return true;
        }

        public override string ToString()
        {
            return "Open popup " + popup;
        }

        protected override void Test()
        {
            Debug.Log(ToString());
            DoAction(null);
        }
    }
}