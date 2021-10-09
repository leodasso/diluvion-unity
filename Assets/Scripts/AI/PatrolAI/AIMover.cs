using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HeavyDutyInspector;
using SpiderWeb;
using Diluvion.Sonar;

namespace Diluvion.Ships
{

    [RequireComponent(typeof(CapsuleCollider))]
    public class AIMover : MonoBehaviour
    {

        //Value Representing how close a ship needs to get to a point to mark it as done, this will be bigger for bigger ships
        public float closeMoveDistance = 25;
        public float closeEnoughAngle = 45;
        public float turnStartDistance = 25;

        [Range(0, 1)]
        public float clampedEnginePower = 1;



        public float terrainCheckDistance = 5;

        [Tooltip("Time spent waiting for unstuck.")]
        public float stuckTimer = 15;


        public Throttle targetThrottle = Throttle.stop;
        public Throttle currentThrottle = Throttle.stop;
        ShipMover sMover;
        Hull myHull;
        int currentThrottleDelta;
        // int currentlyAvoiding = 0;
        Vector3 avoidOffset;
        bool avoiding = false;
        CapsuleCollider myCol;
        int slowDownSpeed = 5;
        int extremeSlowSpeed = 4;
        float smoothNextVectorValue = 10;
        float smoothLerpvalue = 25;
        float oldDistance = 0;
        float minCheckDist = 5;
        float stuckCount = 0;
        LayerMask terrainLayer;
        float currentCloseMoveDistance;
        Rigidbody bridgeRB;
        bool relocating;
        OrbitCam orbitCam;
        bool terrainInFront;
        bool terrainInBack;
        bool terrainBelow;
        bool terrainAbove;
        bool acuteAngleLock = false;
        float throttledivision = 1;
        Vector3 targetOverShootVector = Vector3.one;
        Bridge bridge;

        void Awake() {

            orbitCam = OrbitCam.Get();
            bridge = transform.GetComponentInParent<Bridge>();
            terrainLayer = Calc.IncludeLayer("Terrain").value;
            myCol = GetComponent<CapsuleCollider>();
            myHull = bridge.hull;

            // TODO
            //ShipMover().minVelForTurn = 0;
        }

        public void OnEnable()
        {
            targetThrottle = Throttle.slow;
            currentThrottle = Throttle.stop;
        }


        //Check if terrain is less than a boat length in front of us//TODO YAGNI hook up carefulness?
        void AboutToCollideTerrain(Vector3 dir)
        {
            // Get an overall check distance based on forward velocity
            float forwardVel = transform.InverseTransformDirection(bridge.GetComponent<Rigidbody>().velocity).z;
            float multipliedVel = forwardVel * terrainCheckDistance;

            float frontCheckDistance = Mathf.Clamp(multipliedVel, minCheckDist, 100);
            float rearCheckDistance = Mathf.Clamp(multipliedVel, -100, -minCheckDist);

            Ray frontRay = new Ray(transform.position, dir.normalized);
            //Ray upRay = new Ray(transform.position, Vector3.up);


            // Raycast behind and in front to tell if there's terrain there
            terrainInFront = Physics.Raycast(frontRay, frontCheckDistance, terrainLayer);
            terrainInBack = Physics.Raycast(frontRay, rearCheckDistance, terrainLayer);
            /*if (terrainInFront)
                Debug.Log("I hit : " + frontHit.collider.name + " in front");
            if (terrainInBack)
                Debug.Log("I hit : " + backHit.collider.name + " in the back!");
                */
            // Draw debug rays for front and rear collision
            // Color collidedColorF = terrainInFront ? Color.red : Color.yellow;      //Holy shit conditionals are neat
            //Color collidedColorB = terrainInBack ? Color.red : Color.yellow;      //Holy shit conditionals are neat
            //Debug.DrawRay(transform.position, frontRay.direction * frontCheckDistance, collidedColorF, 0.1f);
            //Debug.DrawRay(transform.position, frontRay.direction * rearCheckDistance, collidedColorB, 0.1f);    
        }

        public void SetDivision(float percentage)
        {
            throttledivision = percentage;
        }

        public void SetThrottle(Throttle throttle, float percentage = 1)
        {
            SetDivision(percentage);
            targetThrottle = throttle;
        }

        //Is the ships throttle this?
        public bool IsThrottle(Throttle comparer)
        {
            if (ShipMover().throttle == comparer || currentThrottle == comparer)
                return true;
            else
                return false;
        }


        //Gets the current operational speed
        public float GetSpeed()
        {
            // Debug.Log("Max Speed: " + ShipMover().MaxSpeed() + ", Real Speed Max: " + ShipMover().RealMaxSpeed() + ", CurrentSpeed(): " + ShipMover().CurrentSpeed());
            return ShipMover().ActualSpeed();
        }


        public Vector3 InterceptPosition(SonarStats target)
        {
            // TODO
            return target.transform.position;
            //return target.Lead(bridge.transform, bridgeRB.velocity * Time.deltaTime, GetSpeed());
        }

        ShipMover ShipMover()
        {
            if (sMover != null) return sMover;
            sMover = bridge.GetComponent<ShipMover>();
            return sMover;
        }

        Rigidbody BridgeRB()
        {
            if (bridgeRB != null) return bridgeRB;
            bridgeRB = bridge.GetComponent<Rigidbody>();
            return bridgeRB;
        }

        #region Avoidance    
        public void OnTriggerEnter(Collider other)
        {

            if (avoiding) return;
            Hull otherHull = other.GetComponent<Hull>();//TODO Come up with a system that does not require Hull
            if (otherHull == null) return;
            if (otherHull.Avoiding()) return;


            avoiding = true;
            myHull.Avoiding(avoiding);

            Vector3 colliderPos = other.transform.position;
            Vector3 forwardDir = transform.forward;
            Vector3 colliderForwardVector = Vector3.Project(colliderPos - transform.position, forwardDir);

            //TODO SET THE AVOIDANCE FOR A SHORT DURATION
            //  Debug.DrawRay(transform.position,colliderForwardVector, Color.red, 15);
            avoidOffset = -(colliderPos - (transform.position + colliderForwardVector));

            // Debug.DrawRay(transform.position + colliderForwardVector, avoidOffset, Color.cyan,15);        

        }


        //TODO YAGNI Create system for multiple ships
        public void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<Hull>())
            {
                if (avoiding)
                    StartCoroutine(AvoidTimer());
            }

        }

        public IEnumerator AvoidTimer()
        {
            yield return new WaitForSeconds(2);
            avoiding = false;
            myHull.Avoiding(false);

            avoidOffset = Vector3.zero;
        }
        #endregion





        #region Move Ship
        //returns true if object is close enough to its target
        public bool Move(Vector3 tSpot, Vector3 nSpot)
        {
            if (relocating) return false;
            if (!ShipMover().InControl()) return false;

            CheckForCurrent();
            CheckForStuck(tSpot);//checks if we are stuck or not          
            return MoveShip(tSpot, nSpot);
        }




        /* float XZangle;
         float turnRadius;
         float correctSpeed;*/
        //TODO YAGNI UP TO 3 SPOT TURNING, DEAD ZONE CHECK
        float DOT;
        float upDot;
        //Propels the ship towards its target, mspeed is a value from -1 to 1 which is from top reverse speed to top forward speed
        bool MoveShip(Vector3 tSpot, Vector3 nSpot)
        {
            Vector3 shipTPos = ShipMover().transform.position;

            Vector3 nextSpotVector = (tSpot - nSpot).normalized;//Vector pointing away from the nSpot    
            Vector3 shipTspotVector = (tSpot - shipTPos).normalized;

            Debug.DrawLine(transform.position, tSpot, Color.green, 0.1f);

            if (!acuteAngleLock)
            {
                targetOverShootVector = nextSpotVector;

                if (Vector3.Dot(nextSpotVector, shipTspotVector) > 0)
                    targetOverShootVector = -Vector3.Cross(Vector3.up, shipTspotVector);

                acuteAngleLock = true;
            }

            //Overshoot vector, calculates the turn place this ship  needs to target in order to do a smooth turn
            Vector3 targetVector = targetOverShootVector.normalized * smoothLerpvalue;

            // Debug.DrawRay(tSpot, targetVector, Color.green);
            smoothNextVectorValue = Mathf.Clamp((tSpot - shipTPos).magnitude / 1.5f, 0, turnStartDistance); //reduces the magnitude of the vector as  we get closer
            smoothLerpvalue = Mathf.Lerp(-turnStartDistance, turnStartDistance, smoothNextVectorValue / turnStartDistance);

            Vector3 forDir = ((tSpot + targetVector) - shipTPos);

            Vector3 flatTarget = new Vector3(forDir.x, 0, forDir.z);

            AboutToCollideTerrain(forDir); // checks for collision with terrain    

            Vector3 flatForward = new Vector3(ShipMover().transform.forward.normalized.x, 0, ShipMover().transform.forward.normalized.z);

            DOT = Vector3.Dot(flatForward.normalized, flatTarget.normalized);

            Debug.DrawRay(transform.position, flatForward.normalized * 3, Color.red, 0.1f);
            Debug.DrawRay(transform.position, flatTarget, Color.yellow, 0.1f);

            //Makes sure we dont constantly set speed when we dont need to   
            if (currentThrottle != targetThrottle || DOT < 0.99f)
            {
                currentThrottle = targetThrottle;
                //Throttle gets superceded by the helmsman if we are not pointing at target        

                if (DOT < 0.7f && !terrainInFront)
                {
                    currentThrottle = Throttle.half;
                    if (DOT < 0.3f)
                        currentThrottle = Throttle.slow;
                }

                if (DOT < -0.5f && !terrainInBack && !bridge.InCurrent())
                {
                    currentThrottle = Throttle.backHalf;
                    if (DOT < -0.8f)
                        currentThrottle = Throttle.backFull;
                }

                if (targetThrottle == Throttle.stop)
                {
                    //Debug.Log("Setting CurrentThrottle to Stop");
                    currentThrottle = Throttle.stop;
                }

                //Sets the speed on the shipmover
                ShipMover().SetThrottle(currentThrottle);//sets the speed to a number 0-8, corresponding to full back(0) and full forward(8), 4 is full stop
            }

            //Height Calculation
            upDot = Vector3.Dot(Vector3.up, shipTspotVector.normalized);
            //Changing this dot will affect when the AI will ballast(Closer to 0 means earlier ballasting)
            if (Mathf.Abs(upDot) > 0.1f)
                sMover.ChangeBallast(upDot / 2);//Sends the Ballast to the shipMover///TODO change for heavier ships         

            ShipMover().clampedEnginePower = clampedEnginePower;
            //Sends the movement to the shipMover

            if (IsAt(tSpot, tSpot + targetOverShootVector, currentCloseMoveDistance))
            {
                acuteAngleLock = false;
                ShipMover().ShipManouverGlobal(flatForward + shipTPos, avoidOffset.normalized);
                return true;
            }
            else
            {
                ShipMover().ShipManouverGlobal(flatTarget + shipTPos, avoidOffset.normalized);
                return false;
            }
        }

        public bool AmIstuck()
        {
            if (stuckCount > 0 || relocating)
                return true;
            else return false;
        }

        //If the distance between me and the target has not reduced itself by a meaningful amount in X seconds, do UNSTUCK behaviour
        void CheckForStuck(Vector3 tSpot)
        {
            if (!terrainInFront && !terrainInBack)
            {
                SetThrottle(Throttle.half);
                return;
            }

            //to prevent spawning when there is nothing around
            SetThrottle(Throttle.stop);
            //Debug.Log("Something stopped me!" + terrainInFront + terrainInBack);
            //Debug.DrawLine(transform.position, tSpot, Color.white, 45);
            float currentDistance = (transform.position - tSpot).sqrMagnitude;
            if (Mathf.Abs(currentDistance - oldDistance) > 9) //if the currentDistance is more than 3 units away(squared)
            {
                oldDistance = currentDistance;//update the old distance to this distance, we have moved
                stuckCount = 0;
            }
            else//if we havent moved more than 3 units, advance the timer by  a little bit        
                if (stuckCount < stuckTimer)
                stuckCount += Time.deltaTime;
            else
                StartCoroutine(Unstuck(tSpot));

        }

        //Resets the stuck mechanic
        public void BreakStuck()
        {
            relocating = false;
            stuckCount = 0;
            terrainInBack = false;
            terrainInFront = false;

        }
        /// <summary>
        /// Will teleport the ship if we are being watched
        /// </summary>
        /// <param name="tSpot"></param>
        /// <returns></returns>
        IEnumerator Unstuck(Vector3 tSpot)
        {
            //Debug.Log("Starting unstuck coroutine at " + Time.time);

            relocating = true;
            List<Vector3> targetAndShip = new List<Vector3>() { tSpot, transform.position };

            //While the player sees either the target or the ship, wait a second before checking again
            while (Cam.CanISeeAny(orbitCam.theCam, targetAndShip))
            {
                if (!relocating) break;
                yield return new WaitForSeconds(1);
            }
            if (!relocating) yield break;
            //If the player recently looked away, give it some seconds befor dissapearing
            yield return new WaitForSeconds(3);
            if (Cam.CanISeeAny(orbitCam.theCam, targetAndShip))//check again, if he's watching again, restart the loop and break this one
            {
                StartCoroutine(Unstuck(tSpot));
                yield break;
            }
            ShipMover().GetComponent<Rigidbody>().isKinematic = true;
            ShipMover().transform.position = tSpot;
            ShipMover().GetComponent<Rigidbody>().isKinematic = false;
            BreakStuck();
            SetThrottle(Throttle.full);
            yield break;
        }


        //Converts -1 to 1 into 1 to 8
        public int ConvertSpeed(float speed)
        {
            float tempSpeed = (speed + 1) * 3;
            //		Debug.Log (tempSpeed + " is the tempspeed");
            return Mathf.FloorToInt(tempSpeed);
        }

        public void Stop()
        {
            Debug.Log("Ordered to Stop");
            SetThrottle(Throttle.stop);
        }

        /// <summary>
        /// Forces the stop command
        /// </summary>
        public void ForceStop()
        {
            //Debug.Log("FORCE STOP!");
            if (IsThrottle(Throttle.stop)) return;
            SetThrottle(Throttle.stop);
            currentThrottle = Throttle.stop;
            ShipMover().SetThrottle(Throttle.stop);

        }
        #endregion

        #region Checkers


        public void CheckForCurrent()
        {
            if (bridge.InCurrent())
            {
                currentCloseMoveDistance = closeMoveDistance + 25;
                BridgeRB().angularDrag = 7;
            }
            else
            {
                currentCloseMoveDistance = closeMoveDistance;
                BridgeRB().angularDrag = .6f;
            }
        }

        public bool IsAt(Transform aTarget, Vector3 nextTarget, float closeDistance)
        {
            float dist = (transform.position - aTarget.position).sqrMagnitude;
            if (dist <= closeDistance * closeDistance)
                return true;

            Vector3 nextVector = (nextTarget - aTarget.position).normalized;
            Vector3 shipvector = (transform.position - aTarget.position).normalized;

            if (Vector3.Dot(nextVector, shipvector) > 0 && dist <= (closeDistance * 2) * (closeDistance * 2))
                return true;

            return false;
        }

        //Checks if the ship is at or beyond the next target
        public bool IsAt(Vector3 aPosition, Vector3 nextTarget, float closeDistance)
        {
            if (Calc.WithinDistance(closeDistance, transform.position, aPosition))
                return true;
            Vector3 nextVector = (aPosition - nextTarget).normalized;
            Vector3 shipvector = (transform.position - aPosition).normalized;

            if (Vector3.Dot(nextVector, shipvector) > 0 && Calc.WithinDistance(closeDistance * 5, transform.position, aPosition))
                return true;
            return false;
        }

        public bool IsFacing(Transform aTarget)
        {
            if (Vector3.Dot((transform.position - aTarget.position).normalized, transform.forward) >= closeEnoughAngle)
                return true;

            return false;
        }

        public bool IsFacing(Vector3 aPosition)
        {

            if (Vector3.Dot((transform.position - aPosition).normalized, transform.forward) >= closeEnoughAngle)
                return true;
            return false;
        }

        #endregion
    }
}