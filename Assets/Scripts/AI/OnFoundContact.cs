using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion.Design;
using UnityEngine;
using System.Collections.Generic;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Callback for pings")]
    [EventReceiver("FoundInterest")]
    public class OnFoundContact : OnFoundContext
	{
	
        protected override string OnInit(){
			return null;
		}

		protected override bool OnCheck()
        {
			return false;
		}

	    /// <summary>
	    /// FoundInterest
	    /// </summary>
	    /// <param name="ct"></param>
	    public void FoundInterest(ContextTarget ct)
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