using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion.Roll
{

    [CreateAssetMenu(fileName = "new tag", menuName = "Diluvion/RollTags/Default")]
    [System.Serializable]
    public class Tag : ScriptableObject
    {
        /// <summary>
        /// Returns true if this tag is valid for the given placer.
        /// </summary>
        public virtual bool IsValid(IRoller checkObject)
        {
            return (checkObject.RollingTags.Contains(this));
        }

        //public abstract bool ValidPosition(Vector3 pos);

        public override string ToString() { return name; }

    }
}
