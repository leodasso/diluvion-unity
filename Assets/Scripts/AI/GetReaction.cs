using NodeCanvas.Framework;
using ParadoxNotion.Design;
using NodeCanvas.BehaviourTrees;

namespace Diluvion.AI
{

	[Category("Diluvion")]
	[Description("Gets a reaction from this tree's Captain")]
	public class GetReaction : ActionTask<AIMono>
	{
	
		public BBParameter<ContextTarget> reactingTo;
		public BBParameter<Reaction> myReaction;

		/* public BBParameter<BehaviourTree> mission;
		 public BBParameter<Action> action;
		 public BBParameter<Convo> response;*/

		protected override string OnInit()
		{
			return null;
		}

		/// <summary>
		/// Return null if something is wrong, or nothing to react to
		/// </summary>
		protected override void OnExecute()
		{
			
			if (reactingTo.isNull)
			{
				EndAction(false);
				return;
			}
			
			myReaction.value = agent.TargetReaction(reactingTo.value);
			
			if (myReaction.isNull) EndAction(false);
			/* mission.value = reaction.value.mission;
			 action.value = reaction.value.GetAction;
			 response.value = reaction.value.GetResponse;*/

			EndAction(true);
		}
	}
}   