// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using System;

#if UNITY_5_0 || UNITY_4_6
namespace UnityEngine
{
    
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class HelpURLAttribute : Attribute
    {
    
        public string URL
        {
            get;
            private set;
        }
    
        public HelpURLAttribute(string url)
        {
            this.URL = url;
        }
    }
}
#endif
