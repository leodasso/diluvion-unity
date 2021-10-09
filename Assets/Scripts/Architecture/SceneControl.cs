using UnityEngine;

namespace Diluvion
{
    public class SceneControl : MonoBehaviour
    {
        bool playerInCombat = false;

        [Space]
        public Transform mapOrigin;
        public Transform mapEnd;
        static SceneControl sc;

        public static SceneControl Get()
        {
            if (sc != null) return sc;
            sc = FindObjectOfType<SceneControl>();
            return sc;
        }

        void OnDrawGizmos()
        {
            // Draw the gizmos for the map bounds
            if (mapOrigin && mapEnd)
            {
                Vector3 mapCenter = (mapOrigin.position + mapEnd.position) / 2;
                Gizmos.DrawWireCube(mapCenter, (mapEnd.position - mapOrigin.position));
            }
        }
 
         /// <summary>
         /// Returns the size of the area the map represents
         /// </summary>
         public Vector2 MapSize()
         {
 
             if (mapEnd == null || mapOrigin == null)
             {
 
                 Debug.Log("Scene is missing map reference points. (mapEnd or mapOrigin)");
                 return Vector2.zero;
             }
 
             Vector3 size = mapEnd.position - mapOrigin.position;
             Vector2 mapSize = new Vector2(size.x, size.z);
             return mapSize;
         }
    }
}