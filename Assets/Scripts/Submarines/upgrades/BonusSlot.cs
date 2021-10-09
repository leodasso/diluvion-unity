using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Diluvion;
using DUI;
using Sirenix.OdinInspector;

/// <summary>
/// The visual / UI component of the bonus slot, which appears in the ship interior. Can be clicked
/// on to apply a bonus chunk.
/// </summary>
public class BonusSlot : MonoBehaviour, IClickable {

    public Forging bonusChunk;

    SpriteRenderer _spriteRenderer;

    ForgeSlotInfo _uiPanel;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Set default sprite
        _spriteRenderer.sprite = LoadoutsGlobal.Get().defaultBonusSlot;

        BoxCollider2D boxCol = gameObject.AddComponent<BoxCollider2D>();
        var colSizer = gameObject.AddComponent<ColliderMatchSprite>();
        colSizer.col = boxCol;
    }

    /// <summary>
    /// Is this bonus slot empty and available?
    /// </summary>
    public bool Available()
    {
        return bonusChunk == null;
    }

    public void ApplyBonus(Forging newBonus)
    {
        if (!_spriteRenderer) _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;
        bonusChunk = newBonus;
        GameObject newVisuals = Instantiate(bonusChunk.visuals);
        newVisuals.transform.SetParent(transform, false);
        newVisuals.transform.localPosition = newVisuals.transform.localEulerAngles = Vector3.zero;
        newVisuals.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Cosmetic - removes the visuals of the current bonus chunk.
    /// </summary>
    public void RemoveBonus()
    {
        _spriteRenderer.enabled = true;
        if (bonusChunk == null) return;
        bonusChunk = null;

        // Remove the visuals
        SpiderWeb.GO.DestroyChildren(transform);
    }

    [Button]
    void Test()
    {
        if (!bonusChunk) return;
        ApplyBonus(bonusChunk);
    }

    public void OnClick()
    {
        //throw new System.NotImplementedException();
    }

    public void OnRelease()
    {
        //throw new System.NotImplementedException();
    }

    public void OnPointerEnter()
    {
        // show GUI
        if (_uiPanel) _uiPanel.End();
        
        if (bonusChunk)
            _uiPanel = ForgeSlotInfo.CreateInstance(bonusChunk, gameObject);
    }

    public void OnPointerExit()
    {
        // hide GUI
        if (_uiPanel) _uiPanel.End();
    }

    public void OnFocus()
    {
        //throw new System.NotImplementedException();
    }

    public void OnDefocus()
    {
        //throw new System.NotImplementedException();
    }
}