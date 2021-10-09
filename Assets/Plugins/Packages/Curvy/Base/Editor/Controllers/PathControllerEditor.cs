// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using FluffyUnderware.Curvy.Controllers;

namespace FluffyUnderware.CurvyEditor.Controllers
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PathController), true)]
    public class PathControllerEditor : CurvyControllerEditor<PathController>
    {
    }
}
