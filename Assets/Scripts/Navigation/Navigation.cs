using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SpiderWeb;
using System;

namespace Diluvion
{    

    public class Navigation : MonoBehaviour
    {
        [TabGroup("Values")]
        [SerializeField, ReadOnly]
        float wayPointDistance = 10;

        [TabGroup("Values")] public bool useLos = true;
      
        Action<bool?> endedNav; //void delegate, inputs true when the route has been completed, inputs false if we stop our current route for any reason
        Action<bool?> clearedWaypoint;
        Vector3 finalPoint;
        LayerMask mask;

        #region debug

        [TabGroup("Debug")] 
        public bool routeIsValid;

        [TabGroup("Debug")] 
        public bool nextWaypointIsLos;

        [TabGroup("Debug")]
        public Transform debugTargetPoint;
        
        [TabGroup("Debug")]
        [SerializeField]
        Route route;
        
        [TabGroup("Debug")]
        [Button("Test Path")]
        void SetCourseButton()
        {
            SetCourse(debugTargetPoint.position, null);
        }
        
        [TabGroup("Debug")]
        [SerializeField]
        bool debugLines;
        Vector3 debugPos;

        void OnDrawGizmos()
        {
            if (!debugLines) return;
            Gizmos.DrawWireSphere(debugPos, 3);
        }

        void OnDrawGizmosSelected()
        {
            if (!debugLines) return;
            if (route == null) return;
            if (route.currentRoute == null|| route.currentRoute.Count < 1) return;
            Gizmos.color = Color.red;
            for (int i =0; i < route.currentRoute.Count - 1; i++)
            {             
                Gizmos.DrawLine(route.currentRoute[i] + Vector3.up+Vector3.right, route.currentRoute[i + 1] - Vector3.up - Vector3.right);
            }

            if (debugTargetPoint == null) return;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, debugTargetPoint.position);

            
        }

        #endregion

        void Start()
        {
            mask = LayerMask.GetMask("Terrain");
        }

        /// <summary>
        /// Gets the most up-to-date waypoint
        /// </summary>
        public Vector3 Waypoint()//TODO extra processing to take into account early turning to get to the next point from the previous point (construct a smoother path based on turnrate)
        {
            if (route == null || !route.Valid())
            {
                endedNav?.Invoke(false);
                return transform.position;
            }
            
            if (!ClearedWaypoint(route))
            {        
                return route.CurrentWP();
            }

            if (route.Finished())
            {
                endedNav?.Invoke(true);
                return transform.position;
            }
            
            Vector3 currentWP = route.MoveNext();
            clearedWaypoint?.Invoke(true);
            return currentWP;
        }

        /// <summary>
        /// The conditions for clearing a way point
        /// </summary>
        bool ClearedWaypoint(Route r)
        {
            routeIsValid = r.Valid();
            if (!routeIsValid) return false;

            if (DistanceToTarget(r.CurrentWP()) > wayPointDistance)
            {
                return false;
            }

            if (WaypointProgress(route) < 0.99f)
            {
                return false;
            }

            if (!Calc.IsLOS(transform.position, r.NextWP(), mask) && useLos)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Records the linear progress on a waypoint from its previous waypoint
        /// </summary>
        float _progress = 0;
        float WaypointProgress(Route wp)
        {
            Vector3 a = transform.position - wp.PreviousWP();
            Vector3 b = wp.CurrentWP() - wp.PreviousWP();

            _progress = (a.x * b.x + a.y * b.y + a.z * b.z) / (b.x * b.x + b.y * b.y + b.z * b.z);

            if(debugLines)
                debugPos = _progress * b + wp.PreviousWP();

            return _progress;
        }

        /// <summary>
        /// Linear Distance to target
        /// </summary>
        /// <param name="targetLocation"></param>
        /// <returns></returns>
        float DistanceToTarget(Vector3 targetLocation)
        {
            return Vector3.Distance(transform.position, targetLocation);
        }

        /// <summary>
        /// Sets a course to the given landmark, Can register a void method as callback for when the route is finished
        /// </summary>
        public Route SetCourse(Vector3 target, Action<bool?> endedCallback=  null)
        {
            //Debug.Log("Setting new Course to " + target);
            endedNav?.Invoke(false);
            
            if(endedCallback != null) endedNav += endedCallback;

            Vector3 startPoint = transform.position;
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb) startPoint += rb.velocity;
            
            List<Vector3> path = new PathFind().OpenPathToVector3(target, startPoint);
            
            if (path == null)
            {
                Debug.LogError("No path found for " + gameObject.name + " adding my own position.", gameObject);
                path = new List<Vector3>() { transform.position };
            }
            route = new Route(path, true);
        
            return route;
        }


        public void StopNavigation()
        {        
            route = null;
        }      

        public bool Navigating()
        {
            if (route == null) return false;
            if (!route.Valid()) return false;
            if (route.Finished()) return false;
            return true;
        }
    }
}