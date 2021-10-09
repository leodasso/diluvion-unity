using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion.Design;
using UnityEngine;
using Diluvion.Sonar;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Callback for pings")]
    [EventReceiver("Pinged")]
    public class OnPinged : OnFoundContext
    {
        /// <summary>
        /// CallBack when hit by a  ping
        /// </summary>
        /// <param name="ct"></param>
        public void Pinged(ContextTarget ct)
        {
            if (ct != null && ct.target != null)
                Debug.Log(agent.gameObject.name +" "+ ct.MyActionContext+ " by " + ct.target.name, agent.gameObject);
            else return;

            ParseContextValues(ct);
            YieldReturn(true);
        }        
    }
}