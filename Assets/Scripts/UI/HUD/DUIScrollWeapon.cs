using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Diluvion;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(LayoutElement))]
public class DUIScrollWeapon : MonoBehaviour
{
    //public Weapon linkedWeapon;
    RectTransform myRect;

    /*
    //Gets the position of this mothafucka
    public int SiblingIndex()
    {
        if (myRect != null) return myRect.GetSiblingIndex();
        myRect = GetComponent<RectTransform>();
        return myRect.GetSiblingIndex();
    }
    */
}