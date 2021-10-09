// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using FluffyUnderware.CurvyEditor;
using FluffyUnderware.Curvy.Controllers;

namespace FluffyUnderware.CurvyEditor.Controllers
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SplineController),true)]
    public class SplineControllerEditor : CurvyControllerEditor<SplineController>
    {
    }

}

