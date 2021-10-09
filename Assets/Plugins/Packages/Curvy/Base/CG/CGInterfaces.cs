// =====================================================================
// Copyright 2013-2017 Fluffy Underware
// All rights reserved
// 
// http://www.fluffyunderware.com
// =====================================================================

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FluffyUnderware.DevTools;

namespace FluffyUnderware.Curvy.Generator
{
    /// <summary>
    /// For modules that don't process anything
    /// </summary>
    public interface INoProcessing
    {
    }

    /// <summary>
    /// For modules that rely on external input (Splines, Meshes etc..)
    /// </summary>
    public interface IExternalInput
    {
        /// <summary>
        /// Whether the module currently supports an IPE session
        /// </summary>
        bool SupportsIPE { get; }
    }


    public interface IOnRequestProcessing
    {
        CGData[] OnSlotDataRequest(CGModuleInputSlot requestedBy, CGModuleOutputSlot requestedSlot, params CGDataRequestParameter[] requests);
    }

    /// <summary>
    /// For modules that process unrasterized data on demand
    /// </summary>
    public interface IOnRequestPath : IOnRequestProcessing
    {
        float PathLength { get; }
        bool PathIsClosed { get; }
    }

    /// <summary>
    /// Resource Loader Interface
    /// </summary>
    public interface ICGResourceLoader
    {
        Component Create(CGModule module, string context);
        void Destroy(CGModule module, Component obj, string context, bool kill);
    }

    /// <summary>
    /// Resource Collection interface
    /// </summary>
    public interface ICGResourceCollection
    {
        int Count { get; }
        Component[] ItemsArray { get; }
    }
}
