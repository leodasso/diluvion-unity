using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion.Roll
{ 
    /// <summary>
    /// If there is no valid transform, ignore this tag
    /// </summary>
    [CreateAssetMenu(fileName = "new depthTag", menuName = "Diluvion/RollTags/Depth")]
    public class DepthTag : Tag {

    public float shallowest = -10;
    public float deepest = -1000;
      

    public override bool IsValid(IRoller roller)
    {
        ITransformRoller tRoller = roller as ITransformRoller;
       
        if (tRoller!=null&&tRoller.Roller() != null)
        {
            float depth = tRoller.Roller().position.y;

            if (depth > shallowest) return false;
            if (depth < deepest) return false;
        }
        return true;

    }

    public override string ToString()
    {
        throw new NotImplementedException();
    }
    }
}
