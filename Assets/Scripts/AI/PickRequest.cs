using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Pick highest priority request")]
	public class PickRequest<T> : ActionTask where T: AIRequest
    {
        public BBParameter<List<T>> requestsToCheck;
        List<T> orderedList = new List<T>();
        public BBParameter<T> pickedRequest;


        protected override string OnInit()
        {            
            orderedList = new List<T>();
            return null;
		}

		protected override void OnExecute()
        {
            if (requestsToCheck.isNull|| requestsToCheck.value.Count < 1)
            {
                EndAction(false);
                Debug.Log("No input list on " + this.name);
                return;
            }

           
            if (requestsToCheck.value.Count == 1)
            {
                pickedRequest = requestsToCheck.value.First() as T;
                EndAction(true);
                return;
            }

           
            orderedList = requestsToCheck.value.OrderBy(ar => ar.Priority).ToList();           
            pickedRequest.value = orderedList.First() as T;
         
            EndAction(pickedRequest.isNull);
        }

		protected override void OnUpdate(){
			
		}

		protected override void OnStop(){
			
		}

		protected override void OnPause(){
			
		}
	}
}