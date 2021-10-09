// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy
{

    #region ### CG related ###

    /// <summary>
    /// CG Resource Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class CGResourceManagerAttribute : DTPropertyAttribute
    {
        public readonly string ResourceName;
        public bool ReadOnly;

        public CGResourceManagerAttribute(string resourceName)
        {
            ResourceName = resourceName;
        }
    }

    /// <summary>
    /// CG Resource Collection Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class CGResourceCollectionManagerAttribute : CGResourceManagerAttribute
    {
        public bool ShowCount;

        public CGResourceCollectionManagerAttribute(string resourceName)
            : base(resourceName)
        {
            ReadOnly = true;
        }
    }

    /// <summary>
    /// CG Data Reference Selector Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class CGDataReferenceSelectorAttribute : DTPropertyAttribute
    {
        public readonly System.Type DataType;

        public CGDataReferenceSelectorAttribute(System.Type dataType)
        {
            DataType = dataType;
        }
    }

    #endregion



}
