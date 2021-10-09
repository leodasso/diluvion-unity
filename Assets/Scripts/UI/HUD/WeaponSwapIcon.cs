using System.Collections;
using System.Collections.Generic;
using Loot;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSwapIcon : MonoBehaviour
{
    public Vector2 anchoredPos;
    public float lerpSpeed = 10;
    public RectTransform rectTransform;
    public DItemWeapon weapon;
    public CanvasGroup canvasGroup;
    public Image image;

    public void ApplyWeapon(DItemWeapon w)
    {
        weapon = w;
        image.sprite = weapon.weaponIcon;
    }

    void Update()
    {
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, anchoredPos,
            Time.unscaledDeltaTime * lerpSpeed);
    }
}
