using NodeCanvas.Framework;
using ParadoxNotion.Design;


namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Callback for losing sonar contacts")]
	[EventReceiver("LostInterest")]
	public class OnLostContact : OnFoundContext{

		protected override string OnInit(){
			return null;
		}

		protected override bool OnCheck(){
			return true;
		}
		
		/// <summary>
		/// FoundInterest
		/// </summary>
		/// <param name="ct"></param>
		public void LostInterest(ContextTarget ct)
		{
			if (ct != null && ct.target != null)
			{
				//Debug.Log(agent.gameObject.name + " by " + ct.target.name, agent.gameObject);
			}
			else return;

			ParseContextValues(ct);          
			YieldReturn(true);
		}        
	}
}