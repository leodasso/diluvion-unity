using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DUI
{

    public class PanelPosition : MonoBehaviour
    {

        public horizontalPos hPos;
        public verticalPos vPos;

        public bool Match(horizontalPos horPos, verticalPos verPos)
        {
            if (horPos == hPos && vPos == verPos) return true;
            return false;
        }

    }
}