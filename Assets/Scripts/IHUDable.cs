using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pardon the ugly name. This is an interface for any component that needs to display a HUD element.
/// </summary>
public interface IHUDable  {

    void CreateUI();

    void RemoveUI();
}
