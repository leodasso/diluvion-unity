	using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Diluvion.AI{

	[Category("Diluvion/Movement")]
	[Description("Have we arrived at the point")]
	public class ArrivedAtPoint : ConditionTask<AIMono>
    {
        public BBParameter<Route> myRoute;
        public BBParameter<float> progress;
        public BBParameter<float> checkDistance = new BBParameter<float>(5f);
        public BBParameter<float> passedWPpercentage = new BBParameter<float>(0.9f);
	    
	    public BBParameter<Vector3> offsetPoint = new BBParameter<Vector3>();
     
        public BBParameter<float> tetherOffset = new BBParameter<float>(15);
	    public BBParameter<Vector3> routeTetherPoint = new BBParameter<Vector3>();
        
		protected override string OnInit()
		{
			checkDistance.value = Mathf.Clamp(agent.MyAvoider.noseDistance * 3,3,200);
			tetherOffset.value = Mathf.Clamp(agent.MyAvoider.noseDistance * 3, 3,210);
			return null;
		}

	  

        public float distanceToCurrent;
        private float clampedTether;
		protected override bool OnCheck()
		{
		    distanceToCurrent = Vector3.Distance(agent.transform.position, myRoute.value.CurrentWP());
            if(!myRoute.isNull)
            {
                if (!myRoute.value.Valid()) return false;
	            clampedTether = Mathf.Clamp(tetherOffset.value, 0, distanceToCurrent);
                progress.value = WaypointProgress(myRoute.value, clampedTether);
	            
	            Debug.DrawLine(myRoute.value.CurrentWP(), myRoute.value.PreviousWP(), Color.blue, 0.1f);
	            
	            Debug.DrawLine(agent.transform.position, routeTetherPoint.value, Color.yellow, 0.1f);
               
                if (progress.value< passedWPpercentage.value) return false;
            }
		
            return distanceToCurrent < checkDistance.value;
		}

	 

        /// <summary>
        /// Records the linear progress on a waypoint from its previous waypoint
        /// </summary>
        /// <param name="wp"></param>
        /// <returns></returns>
        float prog = 0;
        float WaypointProgress(Route wp, float offset)
        {
	        Vector3 tempA = agent.transform.position - wp.PreviousWP();
	        
	        Vector3 tetherDir = tempA.normalized * offset;
	        
            Vector3 a = tempA+tetherDir;

            Vector3 b = wp.CurrentWP() - wp.PreviousWP();
	        
	        Debug.DrawLine(wp.CurrentWP(), wp.PreviousWP(), Color.blue, 0.1f);
	        
	        routeTetherPoint.value = wp.CurrentWP();

	        if (b == Vector3.zero || a == Vector3.zero)
		        return 1;

            //To know if we have passed the upcoming waypoint we need to find out how much of b is a1
            //a1 = (a.b / |b|^2) * b
            //a1 = progress * b -> progress = a1 / b -> progress = (a.b / |b|^2)
            prog = (a.x * b.x + a.y * b.y + a.z * b.z) / (b.x * b.x + b.y * b.y + b.z * b.z);
	        routeTetherPoint.value = wp.PreviousWP() + b * Mathf.Clamp(Mathf.Abs(prog), 0.05f, 2);
            return prog;
        }


    }
}