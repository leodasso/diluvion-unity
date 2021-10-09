using System.Collections;
using System.Collections.Generic;
using Diluvion;
using Diluvion.Ships;
using UnityEngine;
using UnityEngine.UI;

public class AddItemButton : MonoBehaviour
{
    public Text text;
    public StackedItem itemToAdd;

    void Start()
    {
        if (text)
        {
            text.text = "Add " + itemToAdd.item.LocalizedName();
            if (itemToAdd.qty > 1) text.text += " X " + itemToAdd.qty;
        }
    }

    public void AddItem()
    {
        if (PlayerManager.PlayerInventory() == null)
        {
            Debug.Log("Player inventory couldnt be found!");
            return;
        }

        PlayerManager.PlayerInventory().AddItem(itemToAdd);
    }

    public void ForgeToPlayerSub()
    {
        if (itemToAdd.item == null) return;
        if (itemToAdd.item is Forging)
        {
            var chunk = itemToAdd.item as Forging;
            chunk.ApplyToPlayerShip();
        }
    }

    public void RemoveAllForgings()
    {
        if (PlayerManager.PlayerShip() == null) return;
        Bridge b = PlayerManager.pBridge;
        for (int i = b.bonusChunks.Count - 1; i >= 0; i--)
        {
            b.bonusChunks[i].RemoveFromShip(b, b.chassis);
        }
    }
}
