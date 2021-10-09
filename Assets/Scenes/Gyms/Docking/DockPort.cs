using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using DUI;

namespace Diluvion
{

    public class DockPort : MonoBehaviour
    {
        public static List<DockPort> dockPorts = new List<DockPort>();
        public List<GameObject> hooks = new List<GameObject>();
        public bool showPort;
        public DockControl dockControl;

        DUIDock _dockHud;

        [Button()]
        void CreateHook()
        {
            GameObject newHook = new GameObject("hook " + hooks.Count);
            newHook.transform.parent = transform;
            newHook.transform.localEulerAngles = newHook.transform.localPosition = Vector3.zero;
            hooks.Add(newHook);
        }

        void OnDrawGizmos()
        {
            if (!showPort) return;
            Matrix4x4 rotMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.matrix = rotMatrix;
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(new Vector3(0, .5f, 0), new Vector3(.2f, 1, .2f));
        }

        private void Start()
        {
            dockPorts.Add(this);
            gameObject.layer = LayerMask.NameToLayer("Tools");
            GetHooks();
        }

        public void CreateHud()
        {
            _dockHud = DUIDock.MakeDockHud(this);
        }
        
        

        public void GetHooks()
        {
            if (hooks.Count == transform.childCount) return;

            hooks.Clear();

            if (transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                    hooks.Add(transform.GetChild(i).gameObject);
            }
            else hooks.Add(gameObject);
        }

        public Vector3 DirectionFromDockControl()
        {
            DockControl dc = GetComponentInParent<DockControl>();
            if (!dc) return Vector3.forward;
            return Vector3.Normalize(transform.position - dc.transform.position);
        }


        void OnDestroy()
        {
            dockPorts.Remove(this);
            RemoveGUI();
        }

        void RemoveGUI()
        {
            if (_dockHud) _dockHud.End();
        }
    }
}