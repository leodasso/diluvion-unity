using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

namespace Diluvion
{
    public class CameraStats : MonoBehaviour
    {
        public float cameraStartDistance = 15;
        [OnValueChanged("TryOverrideOffset")]
        public Vector2 baseOffset = new Vector2(0, 1);
        public bool overrideInteriorOffset;
        
        SideViewerStats sideView;

        InteriorManager _interior;

        void TryOverrideOffset()
        {
            if (_interior == null) _interior = GetComponentInChildren<InteriorManager>();
            if (_interior == null)
            {
                Debug.LogError("No interior could be found.");
                return;
            }

            _interior.defaultOffset = baseOffset;
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_interior);
            #endif
        }

        void OnDrawGizmosSelected ()
        {
            Vector3 offset = new Vector3(baseOffset.x, baseOffset.y, -cameraStartDistance);

            Quaternion rot = transform.rotation;

            SideViewerStats sv = GetComponent<SideViewerStats>();
            if (sv) rot = rot * Quaternion.Euler(sv.sideViewRotation);

            Gizmos.matrix = Matrix4x4.TRS(transform.position, rot, Vector3.one);
            Gizmos.DrawSphere(offset, .5f);
        }

        public SideViewerStats GetSideView()
        {
            if (sideView == null)
                sideView = GetComponent<SideViewerStats>();
            return sideView;
        }
    }
}