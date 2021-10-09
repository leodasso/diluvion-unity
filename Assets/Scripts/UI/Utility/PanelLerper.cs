using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(RectTransform))]
public class PanelLerper : MonoBehaviour
{
    public RectTransform nextPanel;

    [MinValue(1)]
    public float speed = 5;

    [ToggleLeft]
    public bool adjustAnchors;
    [ToggleLeft]
    public bool adjustPosition;
    [ToggleLeft]
    public bool adjustSize;

    RectTransform _prevRect;

    RectTransform _myRect;

    // Use this for initialization
    void Start()
    {
        _myRect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!nextPanel) return;

        float t = speed * Time.unscaledDeltaTime;

        LerpTo(nextPanel, t);
    }

    public void TeleportTo(RectTransform newRect)
    {
        //Debug.Log(name + " teleporting to " + newRect.name);
        LerpTo(newRect, 1);
    }

    public void LerpTo(RectTransform newRect, float progress)
    {
        if (_myRect == null) _myRect = GetComponent<RectTransform>();
        
        if (_prevRect != newRect)
        {
            _prevRect = newRect;
            
            if (newRect.parent != null)
                _myRect.SetParent(newRect.parent, true);
            
            _myRect.localScale = Vector3.one;
        }

        if (adjustPosition)
            _myRect.anchoredPosition = Vector2.Lerp(_myRect.anchoredPosition, newRect.anchoredPosition, progress);

        if (adjustAnchors)
        {
            _myRect.anchorMax = Vector2.Lerp(_myRect.anchorMax, newRect.anchorMax, progress);
            _myRect.anchorMin = Vector2.Lerp(_myRect.anchorMin, newRect.anchorMin, progress);
        }

        if (adjustSize)
            _myRect.sizeDelta = Vector2.Lerp(_myRect.sizeDelta, newRect.sizeDelta, progress);
    }
}
