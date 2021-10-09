using UnityEngine;
using System.Collections;
using System;
using Object = UnityEngine.Object;
using DUI;

namespace Diluvion
{
    [CreateAssetMenu(fileName = "show tutorial action", menuName = "Diluvion/actions/show tutorial", order = 0)]
    /// <summary>
    /// Shows the given tutorial. If the object has a transform, points at it with the tutorial.
    /// </summary>
    public class ShowTutorial : Action
    {
        public TutorialObject tut;

        public override bool DoAction(Object o)
        {
            Debug.Log(ToString() + " for " + o.name);
            Transform t = null;
            GameObject go = GetGameObject(o);
            Debug.Log(go.name);
            if (go) t = go.transform;
            TutorialPanel.ShowTutorial(tut, t);
            return true;
        }

        public override string ToString()
        {
            return "Shows tutorial " + tut.name;
        }


        protected override void Test()
        {
            Debug.Log(ToString());
            DoAction(testObject);
        }
    }
}
