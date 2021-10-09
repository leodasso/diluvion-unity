using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diluvion
{
    /// <summary>
    /// Can go on any component that will have reactions to docking.
    /// </summary>
    public interface iDockable
    {
        void DockSuccess(DockControl other);

        void UnDock(DockControl other);
    }
}