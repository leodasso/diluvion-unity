using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace Diluvion
{
    [Serializable]
    public class Route
    {
        public List<Vector3> currentRoute;
        public int wpInd;  

        public Route(List<Vector3> points, bool debug = false)
        {
          
            currentRoute = points;
            if (debug)
            {
                for (int i = 0; i < currentRoute.Count; i++)
                {
                    if (i >= currentRoute.Count - 1) continue;
                    Debug.DrawLine(currentRoute[i],currentRoute[i+1], Color.yellow, 15);
                }
            }
            wpInd = 0;
        }

        [Button]
        void InspectRoute()
        {
            for (int i = 0; i < currentRoute.Count; i++)
            {
                if (i >= currentRoute.Count - 1) continue;
                Debug.DrawLine(currentRoute[i],currentRoute[i+1], Color.red, 15);
            }
        }
        
        
        public Vector3 Destination()
        {
            if (currentRoute == null) return Vector3.zero;
            return currentRoute.Last();
        }

        #region Checks

        public bool Finished()
        {
            return wpInd >= currentRoute.Count;
        }

        public bool Valid()
        {
            if (currentRoute == null) return false;
            if (currentRoute.Count < 1) return false;
            return true;
        }

        #endregion

        public Vector3 CurrentWP()
        {
            if (currentRoute == null) { Debug.LogError("No path was calculated. Check errors"); return Vector3.zero; }
            if (wpInd < currentRoute.Count-1)
                return currentRoute[wpInd];
            else
                return currentRoute[currentRoute.Count - 1];
        }

        public Vector3 PreviousWP()
        {
            int safeIndex = wpInd-1;
            if (safeIndex < 0)
                safeIndex = 0;
            return currentRoute[safeIndex];
        }

        public Vector3 NextWP()
        {
            int safeIndex = wpInd+1;
            if (safeIndex >= currentRoute.Count )
                safeIndex = currentRoute.Count - 1;
            return currentRoute[safeIndex];
        }

        #region movers
        public Vector3 MoveNext()
        {
            if (wpInd < currentRoute.Count)
                wpInd++;
            return CurrentWP();
        }

        public Vector3 MovePrevious()
        {
            if (wpInd < 1)
                wpInd--;
            return CurrentWP();
        }
        #endregion
    }
}