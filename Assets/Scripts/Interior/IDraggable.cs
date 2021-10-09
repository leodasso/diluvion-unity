using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDraggable : IClickable  {

    void OnDragStart();

    void OnDragEnd();

    void OnDrag(Vector2 dragAmt);
}
