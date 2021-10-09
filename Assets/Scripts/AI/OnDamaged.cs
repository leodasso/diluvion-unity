using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;


namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Callback for when AI is damaged")]
    [EventReceiver("Damaged")]
    public class OnDamaged : OnFoundContext{



    
        /// <summary>
        /// Callback when damaged
        /// </summary>
        /// <param name="ct"></param>
        public void Damaged(ContextTarget ct)
        {
            if (ct != null && ct.target != null)
            {
               // Debug.Log("Damaged by " + ct.target.name, agent.gameObject);
            }
            else return;
            ParseContextValues(ct);
            YieldReturn(true);
        }
    }
}