using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion.Ships;
using Sirenix.OdinInspector;

namespace Diluvion.AI
{ 
 
    /// <summary>
    /// Data class that holds an urgency graph, 0 being the lowest, 1 being highest
    /// </summary>
    [CreateAssetMenu(fileName = "new throttle priority", menuName = "Diluvion/AI/RequestPriority")]
    public class RequestPriority : ScriptableObject
    {
        [Tooltip("Time is urgency of the request")]
        public AnimationCurve requestImportance = new AnimationCurve(new Keyframe[2] { new Keyframe(0, 0), new Keyframe(1, 100) });

        public float Priority(float urgency)
        {
            if (requestImportance == null) { Debug.Log("No animation curve set up on " + name, this); return 0; }
            return requestImportance.Evaluate(Mathf.Clamp01(urgency));           
        }
    }
}
