using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion.Roll
{

    public abstract class SpawnCheck : ScriptableObject
    {
        public abstract bool ValidCheck(Vector3 checkStart, float radius, ref Vector3 position, ref Quaternion rotation);
        public abstract override string ToString();
    }
}

