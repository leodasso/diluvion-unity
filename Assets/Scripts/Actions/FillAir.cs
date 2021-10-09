using UnityEngine;
using System.Collections;

namespace Diluvion
{
    [CreateAssetMenu(fileName = "fill air action", menuName = "Diluvion/actions/fill air", order = 0)]
    public class FillAir : Action
    {
        [Space]
        public int maxAir = 3;
        public int minAir = 1;
        public int setAmtAir = 1;

        public override bool DoAction(Object o)
        {
            // TODO
            return true;
        }

        public override string ToString()
        {
            // TODO
            string rString = "fill air ";
            return rString;
        }

        protected override void Test()
        {
            Debug.Log(ToString());
            if (testObject)
                DoAction(testObject);
        }
    }
}