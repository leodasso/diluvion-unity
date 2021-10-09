using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Diluvion.AI{

	[Category("Diluvion")]
	[Description("Sets the attack vector relative of the target,")]
	public class SetAttackVector : ActionTask<AIMono>
	{
	    public BBParameter<Transform> trans;
        public BBParameter<Vector3> target;
        public BBParameter<float> range;
        public BBParameter<float> percentageOfRange;

       // public BBParameter<float> bravery;
        public BBParameter<bool> showRays = false;
        public BBParameter<float> directionSwitchTime = 10;

        [Range(0,1)]
        [Tooltip("How far to the side of the target will i try to go? (0-90)")]
        public BBParameter<float> sideOffset = 0;

        [Range(-1, 1)]
        [Tooltip("How far above/Below do i want to get to the target. (0-90)")]
        public BBParameter<float> upOffset = 0;

        [Tooltip("Am i passing the target or staying on my relative side")]
        public BBParameter<bool> passing = false;
        public BBParameter<bool> localDirections = false;
	    public BBParameter<bool> targetLocalDirections = false;
        
        
        public BBParameter<Vector3> attackVector;


		protected override string OnInit(){
			return null;
		}

		protected override void OnExecute()
        {
            upOffset.value = Mathf.Clamp(upOffset.value, -1, 1);
            
            sideOffset.value = Mathf.Clamp01(sideOffset.value);
            
            Vector3 targetPos = target.value;
            
            Vector3 targetVector = agent.transform.position - targetPos;
            
            Vector3 flatTargetvector = new Vector3(targetVector.x, 0 , targetVector.z);
            
            if (localDirections.value)
                flatTargetvector = targetVector;
            
        
            Vector3 targetVelocity = TargetVelocity();
            
            Vector3 targetOffsetVector = TargetOffsetVector(flatTargetvector, targetVelocity);

            Vector3 rangedLocalVector = targetOffsetVector * range.value * percentageOfRange.value;
            
            Quaternion lookTarget = Quaternion.LookRotation(flatTargetvector, Vector3.up);

            Vector3 rangedWorldVector = lookTarget * rangedLocalVector + targetVelocity; //Converts local vectors to world
            
            

            if (!ValidPosition(rangedWorldVector, targetVelocity))
            {
                MirrorVector(ref rangedLocalVector);
                rangedWorldVector = lookTarget * rangedLocalVector+ targetVelocity;
    
                if (showRays.value)
                    Debug.DrawRay(targetPos, rangedWorldVector, Color.red, 5);
            }
            else
            {
                if (showRays.value)
                    Debug.DrawRay(targetPos, rangedWorldVector, Color.green, 5);
            }
            //If its still out of Range
            if (!ValidPosition(rangedWorldVector, targetVelocity))
                rangedWorldVector = Vector3.zero;

            attackVector.value = rangedWorldVector;
			EndAction(true);
		}

        /// <summary>
        /// Test the input vector for collision
        /// </summary>
        bool ValidPosition(Vector3 testWorldOffset, Vector3 targetVelocity)
        {    

            Vector3 worldVector = target.value + targetVelocity + testWorldOffset;

            float distance = Vector3.Distance(worldVector, agent.transform.position);
            Ray worldRay = new Ray(agent.transform.position, worldVector - agent.transform.position);
            if(Physics.Raycast(worldRay, distance, LayerMask.GetMask("Terrain")))    
                return false;

            return true;
        }

        //flip the referenced vector around Y
        void MirrorVector(ref Vector3 vectorToFlip)
        {
            float storeZ = vectorToFlip.z;
            Vector3 flatVector = new Vector3(vectorToFlip.x, vectorToFlip.y, 0);
            vectorToFlip = Vector3.Reflect(-flatVector, Vector3.up);
            vectorToFlip.z = storeZ;
        }

        #if UNITY_EDITOR
        [ReadOnly]
        #endif
        public float switchTimer = 1;
#if UNITY_EDITOR
        [ReadOnly]
#endif
        public float targetRightLeftDot = 0;
#if UNITY_EDITOR
        [ReadOnly]
#endif
        public float rightLeftFlip = 1;
        /// <summary>
        /// Builds an approach location relative to the position of the target, includes their velocity to set intercept waypoints
        /// </summary>
        Vector3 TargetOffsetVector( Vector3 targetVector, Vector3 targetVelocity ,bool worldFlat = false) //TODO Add functionality for "flat" vectors planar with world directions
        {
            Vector3 relativeRight = Vector3.right;
            Vector3 targetRight = Vector3.right;
            if (targetLocalDirections.value&&!trans.isNull)
                targetRight = trans.value.transform.right;
            
            //Relative right position
            if (switchTimer > 0) // Stops excessive switching
            {
                switchTimer -= Time.deltaTime;
               // Debug.Log("Counting Down" + switchTimer + " / " + directionSwitchTime.value);
            }
            else
            {
              //  Debug.Log("Switching Direction");
                switchTimer = directionSwitchTime.value;

                relativeRight = Vector3.Cross( targetVector,Vector3.up);
                
                //Are we heading to the relative right or left of this target
                targetRightLeftDot = Vector3.Dot(agent.transform.forward, relativeRight);
                if (targetRightLeftDot < 0) // if we are heading to the left, flip the chosen horizontal vector
                    rightLeftFlip = -1;
            }

            //the left or right vector of this attack, picked based on the movement direction of the target, and the facing direction of this ship
            Vector3 horizontalVector;
            float moveDot = Vector3.Dot(agent.transform.forward, targetVelocity); //if they are moving the same direction

            if (moveDot > 0)
                horizontalVector = rightLeftFlip * targetRight* sideOffset.value; 
            else
                horizontalVector = rightLeftFlip * -targetRight* sideOffset.value;
       

            float upoffSetRange = upOffset.value * range.value;
            
            
            //The above or below vector of this attack (-1 to 1)
            Vector3 upVector = Vector3.up * upOffset.value;
            
            //If we already are above or below the target, pick a spot relatively closer to us

            //The forward or backward vector for this attack
            Vector3 forwardVector;
            if (passing.value)
                forwardVector = -Vector3.forward;
            else
                forwardVector = Vector3.forward;

            float forwardVectorReduction = (1 - Mathf.Abs(upOffset.value)) * (1 - sideOffset.value);
            
           // Debug.Log(forwardVectorReduction + " upOffset " + upOffset.value + " sideOffset " + sideOffset.value);
            //ForwardVector should go between 0 -1 
            forwardVector *= forwardVectorReduction; 

            //Create a relative attack position for this AI!
            Vector3 compositeVector = horizontalVector + upVector + forwardVector;
            
            float heightDifference = Mathf.Abs(agent.transform.position.y)-Mathf.Abs(target.value.y);

            float differenceNormalized = heightDifference / range.value;

            
            //Adds a offset based on our relative position to the target (if we are below, set slightly below and vice versa)
            if (differenceNormalized > compositeVector.y)
            {
                if (differenceNormalized > percentageOfRange.value)
                    differenceNormalized = percentageOfRange.value;
                compositeVector.y += differenceNormalized;
            }
            //Debug.Log(compositeVector + " composite vector");
            return compositeVector.normalized;
        }

        /// <summary>
        /// Gets the target velocity
        /// </summary>    
        Vector3 TargetVelocity()
        {
            if (target.isNull) return Vector3.zero;

            Transform targetTrans = target.varRef.value as Transform;
            if (targetTrans==null) return Vector3.zero;
       
             
            Rigidbody rb = targetTrans.GetComponent<Rigidbody>();

            if (rb == null) return Vector3.zero;
            return rb.velocity;
        }

    }
}