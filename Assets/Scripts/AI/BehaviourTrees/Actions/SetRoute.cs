using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Diluvion.Ships;

namespace Diluvion.AI
{
    [Name("Set Route")]
    [Category("Diluvion")]  
    public class SetRoute : ActionTask<AIMono>
    {
        //TODO Add Offsets from SHIPSTEER
        
        public BBParameter<Vector3> destination;
        public BBParameter<Vector3> offsetVector;
        public BBParameter<Vector3> startWaypoint;
        public BBParameter<Route> newRoute;
       
        protected override string info => "Set new route to " + destination.ToString();

        protected override void OnExecute()
        {   
            newRoute.value = agent.MyNav.SetCourse(destination.value + offsetVector.value);     // We pass the ENDACTION in to ensure we get a callback when we are done   
          
            Debug.DrawLine(agent.transform.position, destination.value+ offsetVector.value, Color.cyan,1);
                
            startWaypoint.value = newRoute.value.CurrentWP();
             
            Debug.DrawLine(destination.value+ offsetVector.value, startWaypoint.value, Color.yellow,1);
            
            EndAction(!newRoute.isNull);
        }
    }
}