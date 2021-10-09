// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;

namespace FluffyUnderware.DevTools
{
    public class InspectorNote : MonoBehaviour 
    {

        [TextArea(5,20)]
        [SerializeField]
        string m_Note;
    }
}
