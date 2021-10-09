using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion.Ships;

namespace Diluvion.AI
{ 
    /// <summary>
    /// Holds a requestedThrottle and a priority value      
    /// </summary>
    /// <para>For use when comparing what throttle we should be using at the moment</para>
    public class ThrottleRequest : AIRequest
    {
        public Throttle throttle;

        public ThrottleRequest()
        {
            throttle = Throttle.full;
            priorityModel = null;

        }

        public ThrottleRequest(Throttle throt)
        {
            throttle = throt;
            priorityModel = null;
        }
        
        public ThrottleRequest(RequestPriority pm, float urgency, Throttle t)
        {
            throttle = t;
            priorityModel = pm;
            UpdatePriority(urgency);
        }


        public override string ToString()
        {
            return throttle.ToString();
        }


    }
}
