using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using ParadoxNotion.Design;
using UnityEngine;
using Diluvion.Sonar;
using System.Collections.Generic;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Base Callback Class")]
    [EventReceiver("FoundContext")]
    public class OnFoundContext : ConditionTask<AIMono>
    {
        public BBParameter<ContextTarget> newContextTarget;
        public BBParameter<TargetContext> targetContext;
        public BBParameter<ActionContext> actionContext;
        public BBParameter<float> contextAttitude;

        protected override bool OnCheck()
        {
            return false;
        }
        
        /// <summary>
        /// Basic foundInterest call, populates the individual values with the contextTarget's
        /// </summary>
        /// <param name="ct"></param>
        public void ParseContextValues(ContextTarget ct)
        {
            newContextTarget.value = ct;
            actionContext.value = ct.MyActionContext;
            targetContext.value = ct.MyTargetContext;
            contextAttitude.value = ct.relationPriority;            
        }        
    }
}