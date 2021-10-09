using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpiderWeb;
using Diluvion.Ships;
using UnityEngine.Events;
using Sirenix.OdinInspector;

namespace Diluvion
{

    public class DockControl : MonoBehaviour
    {
        public List<Action> actionsOnDock = new List<Action>();
        public List<Action> actionsOnUndock = new List<Action>();

        [DrawWithUnity]
        public UnityEvent onDock;

        [DrawWithUnity]
        public UnityEvent onUnDock;
        
        [Tooltip("Can this thing be docked to?")]
        public bool dockActive = true;

        [Tooltip("Should rigidbodies docked to my ports be set to kinematic? If this thing moves, probably not.")]
        public bool kinematicPorts = true;

        [Tooltip("Has this docked with the player in the current play session?"), ReadOnly]
        public bool dockedWithPlayer;

        /// <summary>
        /// Callback happens on both docks simultaneously
        /// </summary>
        /// <param name="otherDock"></param>
        public delegate void DockDelegate(DockControl otherDock);
        public DockDelegate docked;
        public DockDelegate unDocked;

        DockingRope _rope;
        GameObject _failedDockRope;
        readonly List<GameObject>    _dockRopeInstances = new List<GameObject>();
        readonly List<DockPort>      _myDockPorts = new List<DockPort>();
        readonly List<SpringJoint>   _springJoints = new List<SpringJoint>();
        DockControl _dockedTo;
        Rigidbody _rb;
        ShipMover _shipMover;
        bool _docking;

        public static float dockRange = 15;
        static float connectionRange = 1;
        static float finalizeRange = .3f;

        float dockingForce = 4f;
        float attachSpeed = 8;
        bool _kinematic;
        float _drag;
        float _angularDrag;

        void Awake()
        {
            MyRigidbody();
            
            _drag = _rb.drag;
            _angularDrag = _rb.angularDrag;
            _kinematic = _rb.isKinematic;

            _shipMover = GetComponent<ShipMover>();

            // Get all dockports
            _myDockPorts.Clear();
            _myDockPorts.AddRange(GetComponentsInChildren<DockPort>());
        }

        Rigidbody MyRigidbody()
        {
            if (_rb) return _rb;
            
            // Set up rigidbody
            _rb = GetComponent<Rigidbody>();
            if (!_rb) _rb = GetComponentInParent<Rigidbody>();
            if (!_rb)
            {
                // add a kinematic rigidbody if none exist
                _rb = gameObject.AddComponent<Rigidbody>();
                _rb.mass = 10;
                _rb.isKinematic = true;
                _rb.useGravity = false;
            }

            return _rb;
        }

        IEnumerator Start()
        {
            yield return new WaitForSeconds(.1f);
            
            while (PlayerManager.PlayerShip() == null)
            {
                yield return null;
            }
            
            // Create UI for dock ports
            if (PlayerManager.PlayerDocks() == this) yield break;

            foreach (var d in _myDockPorts)
            {
                d.dockControl = this;
                d.CreateHud();
            }
        }

        GameObject DockBubblePrefab()
        {
            return Resources.Load("effects/docking steam") as GameObject;
        }

        /// <summary>
        /// Sets if this can be docked with or not
        /// </summary>
        public void SetDockable(bool able)
        {
            dockActive = able;
            // break any current dockings
            if (!able) BreakDocking();
        }

        /// <summary>
        /// Toggles docking. This is what gets called when player presses 'dock' 
        /// </summary>
        public void DockToggle()
        {
            // If already docking, break from it.
            if (Undock()) return;

            // search for any dockports near me
            DockPort nearestDock = NearestDock(DockPort.dockPorts, transform);
            if (!WithinDistance(nearestDock)) return;

            DockTo(nearestDock);
        }

        /// <summary>
        /// Returns the nearest dock port of any in the scene to this dock control.
        /// </summary>
        public DockPort NearestDockPort()
        {
            return NearestDock(DockPort.dockPorts, transform);
        }

        /// <summary>
        /// Cancel any current or in progress docking. Returns true if docking was cancelled.
        /// </summary>
        public bool Undock()
        {
            if (!_docking)
            {
              //  Debug.Log("Can't undock while not already docking.");
                return false;
            }
            
            SpiderSound.MakeSound("Stop_Ship_DockingHook_ReelingIn", gameObject);
            
            BreakDocking();
            StopCoroutine("DockingProcedure");
            return true;
        }

        void OnDestroy()
        {
            SpiderSound.MakeSound("Stop_Ship_DockingHook_ReelingIn", gameObject);
        }

        /// <summary>
        /// Called when this is docked to by another dock control.
        /// </summary>
        void DockPassive()
        {
            DockActions();
        }

        /// <summary>
        /// Called when another dock control undocks from this.
        /// </summary>
        void UndockPassive()
        {
            UndockActions();
            ResetDrag();
            SetCollisionDamage(true);
        }


        bool WithinDistance(DockPort other)
        {
            if (other == null) return false;
            if (Vector3.Distance(transform.position, other.transform.position) < dockRange) return true;
            return false;
        }

        public bool DockTo(DockPort port)
        {
           // Debug.Log("Attempting to dock to " + port.name);
            DockControl otherDockControl = port.GetComponentInParent<DockControl>();

            if (!dockActive) return false;
            if (!otherDockControl.dockActive) return false;
            if (!otherDockControl._rb)
            {
                Debug.LogError("No rigidbody on " + otherDockControl.name);
                return false;
            }

            // Choose my dockport that's best aligned
            DockPort myPort = BestAlignedPort(port);
            if (myPort == null) return false;

            StartCoroutine(DockingProcedure(myPort, port, otherDockControl));
            return true;
        }

        #region docking movements

        void ResetDrag()
        {
            if (!_rb) return;
            _rb.drag = _drag;
            _rb.angularDrag = _angularDrag;
        }

        void HighDrag()
        {
            if (!_rb) return;
            _rb.drag = 3;
            _rb.angularDrag = 5;
        }

        IEnumerator DockingProcedure(DockPort myPort, DockPort otherPort, DockControl otherDock)
        {
            _docking = true;
            HighDrag();
            otherDock.HighDrag();

            OrbitCam.transitionLock = true;

            // turn off the throttle
            if (_shipMover) _shipMover.NullThrottle();
            
            // turn off collision damage
            SetCollisionDamage(false);
            otherDock.SetCollisionDamage(false);

            // Get the other dock's rigidbody
            Rigidbody otherRB = otherDock.MyRigidbody();

            // Connect all the ropes
            int hookNum = Mathf.Min(myPort.hooks.Count, otherPort.hooks.Count);
            for (int i = 0; i < hookNum; i++)
            {
                // Define the hooks that are attaching
                GameObject myHook = myPort.hooks[i];
                GameObject theirHook = otherPort.hooks[i];
                ConnectRopes(myHook, theirHook);
            }

            // Begin the pull
            for (int i = 0; i < hookNum; i++)
            {
                // Define the hooks that are attaching
                GameObject myHook = myPort.hooks[i];
                GameObject theirHook = otherPort.hooks[i];
                ConnectSprings(myHook, theirHook, otherRB, hookNum);
            }
            SetSpringForce(0);

            ShipMover mover = GetComponent<ShipMover>();
            if (mover) mover.enabled = false;
            
            // begin playing 'reeling in' sfx
            SpiderSound.MakeSound("Play_Ship_DockingHook_ReelingIn", gameObject);

            // Gradually increase spring force
            float f = 0;
            while (f < 1)
            {
                f += Time.deltaTime;
                SetSpringForce(_rb.mass * dockingForce * f);
                yield return null;
            }

            // wait for the springs to pull us close
            float t = 0;
            while (Vector3.Distance(myPort.transform.position, otherPort.transform.position) > connectionRange && t < 1.5f)
            {
                t += Time.deltaTime;
                yield return null;
            }

            // shake dat camera
            OrbitCam.ShakeCam(.05f, transform.position);
            
            // set the springs force to be lower now that we're closer to the other dock
            SetSpringForce(_rb.mass / 2);
            
            SpiderSound.MakeSound("Stop_Ship_DockingHook_ReelingIn", gameObject);

            // switch rigidbody to kinematic so we can lerp to the thing
            _rb.isKinematic = true;

            OrbitCam.transitionLock = false;
            //Change camera target
            OrbitCam.SetTarget(otherDock.transform, true, .5f);
            
            docked?.Invoke(otherDock);
            otherDock.docked?.Invoke(this);

            // final stage does a kinematic lerp so the dockports kiss
            t = 0;
            while (LerpToPort(myPort.transform, otherPort.transform) > finalizeRange && t < 1)
            {
                t += Time.deltaTime;
                yield return null;
            }

            // docked sound
            SpiderSound.MakeSound("Play_Dock_Complete", gameObject);
            SpiderSound.PauseSound("Play_Critical_Health", gameObject, 0.1f);

            OrbitCam.ShakeCam(.05f, transform.position);

            foreach (iDockable d in otherDock.GetComponents<iDockable>())
                d.DockSuccess(this);

            // docking actions
            DockActions();
            otherDock.DockPassive();

            // Make the airlock particle effects
            for (int j = 0; j < 3; j++)
                MakeDockBubbles(myPort.transform);

            // record the docking in the quest system
            QuestManager.Tick();
            
            // Reset my rigidbody now that lerping is complete
            _rb.isKinematic = otherDock.kinematicPorts;

            _dockedTo = otherDock;
        }

        void MakeDockBubbles(Transform port)
        {
            GameObject newBubble = Instantiate(DockBubblePrefab(), port);
            newBubble.transform.localEulerAngles = new Vector3(Random.Range(-30, 30), Random.Range(0, 360), 0);
            newBubble.transform.localPosition = Vector3.zero;
            Destroy(newBubble, 5);
        }

        /// <summary>
        /// Lerps the ship into position so the two dockports kiss. Returns the difference between the two ports (both position and rotation)
        /// </summary>
        float LerpToPort(Transform myPort, Transform theirPort)
        {
            Quaternion rotDelta = Quaternion.FromToRotation(myPort.transform.up, theirPort.transform.up * -1);
            Quaternion targetRot = rotDelta * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, attachSpeed * Time.deltaTime);

            Vector3 diff = myPort.transform.position - theirPort.transform.position;
            Vector3 finalPos = transform.position - diff;
            transform.position = Vector3.Lerp(transform.position, finalPos, attachSpeed * Time.deltaTime);

            float dot = Vector3.Dot(myPort.transform.up.normalized, theirPort.transform.up.normalized * -1);
            dot = Mathf.Clamp01(dot);

            return diff.magnitude + (1 - dot);
        }
        #endregion

        /// <summary>
        /// Cancels any current docks, and if already docked to something, undocks from it.
        /// </summary>
        public void BreakDocking()
        {
            StopAllCoroutines();

            //transform.parent = null;
            ClearHooks();
            _docking = false;
            SetCollisionDamage(true);
            if (_rb) _rb.isKinematic = _kinematic;
            
            ResetDrag();

            ShipMover mover = GetComponent<ShipMover>();
            if (mover) mover.enabled = true;

            if (gameObject != PlayerManager.PlayerShip()) return;

            // Player only stuff is below

            SpiderSound.MakeSound("Play_DockRelease", gameObject);
            SpiderSound.ResumeSound("Play_Critical_Health", gameObject, 0.1f);
            if (GetComponent<ShipMover>()) GetComponent<ShipMover>().StartAmbientSounds();

            if (_dockedTo)
            {
                _dockedTo.dockedWithPlayer = true;
                _dockedTo.UndockPassive();
                foreach (iDockable d in _dockedTo.GetComponents<iDockable>())
                    d.UnDock(this);
            }

            QuestManager.Tick();

            // Call our own undock delegate
            unDocked?.Invoke(_dockedTo);

            // Call the undocked delegate on the dock we're docked to
            if (_dockedTo)
                _dockedTo.unDocked?.Invoke(this);

            UndockActions();

            OrbitCam.SetTarget(gameObject.transform, false, .5f);
        }

        #region ropes and links

        /// <summary>
        /// Connects the given hooks with a docking rope.
        /// </summary>
        void ConnectRopes(GameObject myHook, GameObject theirHook)
        {
            //Get the rotation from this hook to the other
            Quaternion ropeRot = Quaternion.LookRotation(theirHook.transform.position - myHook.transform.position);

            DockingRope ropeInstance = InstantiateDockingRope(myHook.transform.position, ropeRot);
            ropeInstance.FireDockRope(theirHook.transform);
            _dockRopeInstances.Add(ropeInstance.gameObject);

            //rotate rope randomly
            ropeInstance.transform.Rotate(new Vector3(0, 0, Random.Range(-90, 90)));

            //parent the base to my hook
            ropeInstance.transform.SetParent(myHook.transform);
        }


        /// <summary>
        /// Connects the given hooks with a spring joint
        /// </summary>
        void ConnectSprings(GameObject myHook, GameObject theirHook, Rigidbody theirRB, int connectedHooks)
        {
            // Create the spring joint
            SpringJoint spring = _rb.gameObject.AddComponent<SpringJoint>();
            spring.autoConfigureConnectedAnchor = false;
            spring.anchor = transform.InverseTransformPoint(myHook.transform.position);
            spring.connectedBody = theirRB;
            spring.enableCollision = true;
            spring.connectedAnchor = theirRB.transform.InverseTransformPoint(theirHook.transform.position);
            _springJoints.Add(spring);
        }

        void SetSpringForce(float force)
        {
            if (_springJoints.Count < 1) return;
            foreach (SpringJoint s in _springJoints) s.spring = force / _springJoints.Count;
        }

        void ClearHooks()
        {
            foreach (SpringJoint sj in _springJoints)
            {
                if (sj == null) continue;
                Destroy(sj);
            }
            _springJoints.Clear();

            foreach (GameObject rope in _dockRopeInstances) Destroy(rope);
            _dockRopeInstances.Clear();
        }

        /// <summary>
        /// Instantiates a docking rope.
        /// </summary>
        DockingRope InstantiateDockingRope(Vector3 pos, Quaternion rot)
        {
            //Spawn a rope from my hook lerping to that hook
            if (_rope == null)
            {
                GameObject ropeGO = Resources.Load("dockingRope") as GameObject;
                _rope = ropeGO.GetComponent<DockingRope>();
            }

            return Instantiate(_rope, pos, rot) as DockingRope;
        }

        /// <summary>
        /// Returns the 'failed rope' object
        /// </summary>
        GameObject FailedDockSnap()
        {
            if (_failedDockRope != null) return _failedDockRope;
            _failedDockRope = (GameObject)Resources.Load("dockingRopeSnap");
            return _failedDockRope;
        }

        #endregion

        /// <summary>
        /// Turn on or off collision damage on attached hull
        /// </summary>
        void SetCollisionDamage(bool active)
        {
            Hull h = GetComponent<Hull>();
            if (h == null) return;
            h.CollisionDamage(active);
        }


        /// <summary>
        /// Returns the child dock port that's nearest to the given transform t
        /// </summary>
        public DockPort ClosestDock(Transform t)
        {
            return NearestDock(_myDockPorts, t, false);
        }

        List<DockPort> _nearestDocks = new List<DockPort>();
         /// <summary>
        /// Returns the dock from the list that's nearest to transform t.
        /// </summary>
        DockPort NearestDock(List<DockPort> dockports, Transform t, bool excludeMyDockports = true)
        {
            if (dockports==null||dockports.Count < 1) return null;
            if (t == null) return null;
            //List<DockPort> newList = new List<DockPort>();
            _nearestDocks.Clear();
            
            for (int i = 0; i < dockports.Count; i++)
            {
                if (dockports[i] == null) continue;
                if (!dockports[i].isActiveAndEnabled) continue;
                if (excludeMyDockports && _myDockPorts.Contains(dockports[i])) continue;

                _nearestDocks.Add(dockports[i]);
            }
            
            if (_nearestDocks.Count < 1) return null;
            _nearestDocks = _nearestDocks.OrderBy(d => Vector3.Distance(t.position, d.transform.position)).ToList();
            return _nearestDocks.First();
        }


        /// <summary>
        /// Get the DockPort that aligns the target dock best
        /// </summary>
        DockPort BestAlignedPort(DockPort targetDock)
        {
            // the dockport with a dot product nearest to -1 (opposite direction) will be a good fit.
            List<DockPort> sortingList = new List<DockPort>();
            sortingList = _myDockPorts.OrderBy(dock => Vector3.Dot((dock.transform.position - targetDock.transform.position), dock.DirectionFromDockControl())).ToList();
            return sortingList[0];
        }

        /// <summary>
        /// Runs through all the actions on undock.
        /// </summary>
        void UndockActions()
        {
            foreach (Action a in actionsOnUndock) a.DoAction(gameObject);
            onUnDock.Invoke();
        }

        /// <summary>
        /// Runs through all the actions on dock.
        /// </summary>
        void DockActions()
        {
            foreach (Action a in actionsOnDock) a.DoAction(gameObject);
            onDock.Invoke();
        }
    }
}