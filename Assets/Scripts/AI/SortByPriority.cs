	using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections.Generic;
using System.Linq;
	using Diluvion.Sonar;
	using UnityEngine;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Updates/Adds the context target in memory")]
	public class SortByPriority : ActionTask<AIMono>{

        public BBParameter<ContextTarget> topPriority;
		
		//public BBParameter<List<SonarStats>> friendList;
		
        public BBParameter<List<ContextTarget>> targetList;
	
        protected override string info => "Sorts the input list by its priority value, returns false if no interesting thing is found";

		protected override string OnInit(){
			return null;
		}

	    
        List<ContextTarget> invalidContexts = new List<ContextTarget>();
        protected override void OnExecute()
        {
            //Order by the inverse Distance * priority          
            if (targetList.isNull) { EndAction(false); return; }
	        if (targetList.value.Count<1)
	        {
		        topPriority.value = null; EndAction(true); return; 
	        }
	        
            //Debug.Log("Setting target list priority");
            invalidContexts.Clear();

            foreach (ContextTarget ct in targetList.value)
            {
                if (ct == null || ct.target == null || !ct.target.gameObject.activeInHierarchy) { invalidContexts.Add(ct); continue; }//Remove Null entries, null targets and inactive targets
				if(ct.MyTargetContext == TargetContext.Friend)
					agent.AddFriend(ct.target);
                float distance = Vector3.Distance(ct.target.transform.position, agent.transform.position);

                ct.DistancePriority(distance);
            }

	        foreach (ContextTarget ct in invalidContexts)
	        {
		        agent.RemoveFriend(ct.target);
		        targetList.value.Remove(ct);
	        }

	        if (targetList.isNull || targetList.value.Count < 1) { EndAction(false); return; }

	        targetList.value.Sort();
	        ContextTarget topCandidate = targetList.value.First();
	        
	      	if(topCandidate==null){ EndAction(false); return; }
	        
	        //If any of my other targets has maching signatures (RIGHT NOW ONLY ADDS TO THE TOP PRIORITY TARGET)
			topCandidate.GetFriends(targetList.value);
	        
	        topPriority.value = topCandidate;
          
            EndAction(!topPriority.isNull);
		}

	
	}
	
}