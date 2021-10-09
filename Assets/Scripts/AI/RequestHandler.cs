using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
namespace Diluvion.AI
{
    public abstract class RequestHandler<T> : ActionTask where T : AIRequest
    {

        public BBParameter<float> urgencyValue;

        [SerializeField]
        public T editRequest;

        [ParadoxNotion.Design.ForceObjectField]
        public BBParameter<T> outRequest;

        #if UNITY_EDITOR
        [ReadOnly]
        #endif
        public float currentPriority;
   
        protected override void OnExecute()
        {
            if(outRequest.isNull)
            {
                outRequest = new BBParameter<T>();
            }
            CalculatePriority();
        }

        void CalculatePriority()
        {
            if (editRequest == null) { EndAction(false); return; }
            if (!editRequest.UpdatePriority(urgencyValue.value)) { EndAction(false); return; }

            outRequest.value = editRequest;
            currentPriority = outRequest.value.Priority;
            EndAction(true);
        }


    }
}