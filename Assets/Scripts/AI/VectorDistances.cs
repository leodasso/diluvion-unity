using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using Diluvion.Ships;

namespace Diluvion.AI{

	[Category("Diluvion/Movement")]
	[Description("Gets the distances from the input vector.")]
	public class VectorDistances : ActionTask<ShipMover>{

        [RequiredField]
        public BBParameter<Vector3> waypointVector;
	    [RequiredField]
	    public BBParameter<Vector3> tetherVector;
        [BlackboardOnly]
        public BBParameter<float> linearDistance;
	    [BlackboardOnly]
	    public BBParameter<float> forwardDot;
        [BlackboardOnly]
        public BBParameter<float> flatYDistance;
        [BlackboardOnly]
        public BBParameter<float> heightDot;
        [BlackboardOnly]
        public BBParameter<float> momentum;
        public BBParameter<float> lookAhead = 2;
        [BlackboardOnly]
        public BBParameter<float> overShoot;
        [BlackboardOnly]
        public BBParameter<float> forwardSpeed;
		public BBParameter<float> maxSpeed;
		public BBParameter<float> normalizedSpeed;
	   
      

        Rigidbody rigidbody;
        Rigidbody RigidBody
        {
            get
            {
                if (rigidbody != null) return rigidbody;
                if (agent.GetComponent<Rigidbody>())
                {
                    rigidbody = agent.GetComponent<Rigidbody>();    
                }
                else
                {
                    rigidbody = agent.gameObject.AddComponent<Rigidbody>();
                }
                return rigidbody;
            }
            set
            {
                rigidbody = value;
            }
        }
      
		protected override void OnExecute()
        {
	        //Ship Values
	        forwardSpeed.value = agent.transform.InverseTransformDirection(RigidBody.velocity).z;
	        momentum.value = forwardSpeed.value * RigidBody.mass * (1- Time.fixedDeltaTime*RigidBody.drag);
	        //Tether Values
            flatYDistance.value = new Vector3(tetherVector.value.x, 0, tetherVector.value.z).magnitude;
            heightDot.value = Vector3.Dot(tetherVector.value.normalized, Vector3.up);
	        forwardDot.value = Vector3.Dot(agent.transform.forward.normalized, tetherVector.value.normalized);
	        maxSpeed = agent.MaxSpeedPerSecond();
	        
	        //Waypoint Values
	        linearDistance.value = waypointVector.value.magnitude;
	        normalizedSpeed.value = forwardSpeed.value / maxSpeed.value;
	        
            overShoot.value = (forwardSpeed.value * lookAhead.value)/linearDistance.value;
            EndAction(true);
		}
	}
}