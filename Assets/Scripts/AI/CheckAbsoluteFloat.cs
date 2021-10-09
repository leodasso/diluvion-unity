using NodeCanvas.Framework;
using ParadoxNotion.Design;
using ParadoxNotion;
using UnityEngine;

namespace Diluvion.AI{

    public class CheckAbsoluteFloat : ConditionTask
    {

        [BlackboardOnly]
        public BBParameter<float> valueA;
        public CompareMethod checkType = CompareMethod.EqualTo;
        public BBParameter<float> valueB;

        [SliderField(0, 0.1f)]
        public float differenceThreshold = 0.05f;

        protected override string info
        {
            get { return valueA + OperationTools.GetCompareString(checkType) + "Abs(" + valueB + ")"; }
        }

        protected override bool OnCheck()
        {
            return OperationTools.Compare((float)Mathf.Abs(valueA.value), (float)valueB.value, checkType, differenceThreshold);
        }
    }
}
