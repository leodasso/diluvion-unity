using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using DUI;
using NodeCanvas.BehaviourTrees;
using System.Collections.Generic;


namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Takes a reaction and gets the variables while running them")]
	public class DoReaction : ActionTask<AIMono>
	{

	  //  public BBParameter<Captain> cap;
        public BBParameter<Reaction> reaction;
        public BBParameter<BehaviourTree> mission;
        public BBParameter<BehaviourTree> attack;
        public BBParameter<BehaviourTree> flee;
        public BBParameter<AIAction> actionPath;
        public BBParameter<Action> usedAction;
        public BBParameter<Convo> usedConvo;

        public Object actionDoer;

		protected override string OnInit(){
			return null;
		}    

		protected override void OnExecute()
        {
            if(reaction.isNull){ // If we did not get a reaction, return false;
                
                EndAction(false);
                return;
            }

            if (actionDoer == null)
                actionDoer = agent.gameObject;

            BehaviourTree bt = reaction.value.overrideMission;

            actionPath.value = reaction.value.actionType;

            //If we have an override mission
            if (bt!=null)
            {
                switch(reaction.value.actionType)
                {
                    case AIAction.Fight:
                    {
                        attack.value = bt;
                        break;
                    }
                    case AIAction.Flee:
                    {
                        flee.value = bt;
                        break;
                    }
                    case AIAction.Mission:
                    {
                        mission.value = bt;
                        break;
                    }
                }           
            }
          
            agent.ResetMission();

            usedAction.value = reaction.value.GetAction;
            
            if(!usedAction.isNull)
                usedAction.value.DoAction(actionDoer);

            usedConvo.value = reaction.value.GetResponse;
       
            if(!usedConvo.isNull)
                DUIChatterBox.AddRadio(usedConvo.value);

            EndAction(true);
        }

		protected override void OnUpdate(){
			
		}

		protected override void OnStop(){
			
		}

		protected override void OnPause(){
			
		}
	}
}