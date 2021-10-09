using UnityEngine;
using System.Collections;

public class UIHierarchyHelper : MonoBehaviour {

    public bool keepOnFront;
    public bool keepInRear;

    void Update()
    {
        if (keepOnFront) BringToFront();
        if (keepInRear) SendToBack();
    }


    /// <summary>
    /// Places this in front of other UI elements by setting it as last sibling in hierarchy.
    /// </summary>
	public void BringToFront()
    {
        transform.SetAsLastSibling();
    }

    /// <summary>
    /// Places this behind other UI elements by setting it as first sibling in hierarchy.
    /// </summary>
    public void SendToBack()
    {
        transform.SetAsFirstSibling();
    }
}
