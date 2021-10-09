using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Diluvion
{

    public class ImpactableSurface : MonoBehaviour, Impactable
    {
        [AssetsOnly]
        public SurfaceMaterial surface;

        public void Impact(Vector3 vector, Vector3 point)
        {
            surface.NewImpact(vector, point).transform.parent = transform;
        }
    }
}