using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Diluvion.Ships;

namespace Diluvion.AI
{
    [Name("Ship Move To Target Position")]
    [Category("Movement")]
    public class AIMove : ActionTask<ShipMover>
    {
        public BBParameter<Vector3> destination;
        public BBParameter<Vector3> waypointPosition;
        public BBParameter<List<Vector3>> currentPath;
        public BBParameter<Throttle> speed = Throttle.half;
        public float keepDistance = 0.1f;
     
        Navigation nav;

        protected override string info
        {
            get { return "Navigation to " + destination.ToString(); }
        }

        protected override void OnExecute()
        {
            Debug.Log("Started navigation");
            if(destination.isNull)
            {
                EndAction(false);
                return;
            }

            if (currentPath == null || currentPath.isNull)
                currentPath.value = Navigation().SetCourse(destination.value).currentRoute;

            if(currentPath.isNull)                
            {
                EndAction(false);
                return;
            }

            agent.SetThrottle(speed.value);

            if (Vector3.Distance(agent.transform.position, destination.value)<keepDistance)
            {
                EndAction(true);
                return;
            }

            Go();
        }

        Navigation Navigation()
        {
            if (nav != null) return nav;
            if (agent.GetComponent<Navigation>())
                return nav = agent.GetComponent<Navigation>();
            else
                return nav = agent.gameObject.AddComponent<Navigation>();
        }

        Vector3 SafeTargetPosition()
        {
            return waypointPosition.value = Navigation().Waypoint();
        }

        protected override void OnUpdate()
        {
            Go();
        }

        void Go()
        {
            agent.ShipManouverGlobal(SafeTargetPosition());
            if (Vector3.Distance(agent.transform.position, destination.value) < keepDistance)//TODO YAGNI Register a callback from navigation instead of measuring distance
            {
                EndAction(true);
                return;
            }
        }

        protected override void OnStop()
        {
            if (agent.gameObject.activeSelf)
            {
                agent.NullThrottle();
                currentPath.value = null;
            }
            

        }

        protected override void OnPause()
        {
            OnStop();
        }
    }
}