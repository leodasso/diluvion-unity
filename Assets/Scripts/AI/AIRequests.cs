using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Diluvion.Ships;

namespace Diluvion.AI
{
    /// <summary>
    /// Holds a requestedThrottle and a priority value      
    /// </summary>  
    /// <para>For use when comparing what throttle we should be using at the moment</para>
    public class AIRequest
    {    
        public RequestPriority priorityModel;

        protected float priority = 0;      
        public float Priority
        {
            get
            {
                return priority;
            }
        }      

        public bool UpdatePriority(float urgency)
        {
            if (priorityModel == null) return false;
            priority = priorityModel.Priority(urgency);
            return true;
        }
     

        public AIRequest()
        {
            priority = 0;
            priorityModel = null;
        }

       
    }
}