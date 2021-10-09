using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Diluvion;
using Sirenix.OdinInspector;
using TMPro;

namespace DUI {
public class QtySelect : DUIView {

    [ReadOnly]
    public TradePanel tradePanel;
    
    [ReadOnly]
    public DUIInventory inventoryPanel;
    
    public float joySelectionInterval = .2f;
    public float joyThreshhold = .7f;
    [Space]
    public TextMeshProUGUI numberText;
    public TextMeshProUGUI valueText;

    int _amount;
    float _valuePer;
    Item _item;
    bool _selectionReady = true;     // used for timed changes of qty using gamepad

    static QtySelect _instance;

    public static bool Exists()
    {
        return _instance != null;
    }
    
    /// <summary>
    /// Adds panel for selecting a quantity of items out of a stack
    /// </summary>
    public static QtySelect AddQtySelection (Item forItem, DUIInventory inv, TradePanel tradePanel = null)
    {
        // remove any previous instance
        if (_instance) _instance.DeselectAndEnd();

        // check for null item ref
        if (forItem.stack == null)
        {
            Debug.LogError("Attempting to show quantity panel for " + forItem + " but the item stack is null.");
            return null;
        }

        // create new instance
        _instance = UIManager.Create(UIManager.Get().qtySelectPanel as QtySelect); 

        // position the instance correctly
        Vector2 pos = forItem.transform.position;
        _instance.GetComponent<RectTransform>().position = pos;
        _instance.transform.SetAsLastSibling();

        _instance.tradePanel = tradePanel;
        _instance.inventoryPanel = inv;

        // Get the base value per item
        float valuePerItem = forItem.stack.item.goldValue;

        // if trading, check for the merchant's weighted trade values
        if (tradePanel)
        {
            if (tradePanel.tradeMode == TradePanel.TradeMode.cash)
            {
                bool selling = !inv.playersInventory;

                // display value of the transaction
                if (selling) valuePerItem *= tradePanel.NonPlayerInventory().invGenerator.sellPercentage;
                else         valuePerItem *= tradePanel.NonPlayerInventory().invGenerator.buyPercentage;
            }
        }

        _instance.Init(forItem, valuePerItem);
        return _instance;
    }

    public void Init(Item newItem, float valuePerItem)
    {
        _item = newItem;
        _amount = _item.stack.qty;
        _valuePer = valuePerItem;

        Refresh();
    }

    void Refresh()
    {
        if (_item == null) return;
        _amount = Mathf.Clamp(_amount, 0, _item.stack.qty);

        string formattedAmount = string.Format("{0:000}", _amount);
        numberText.text = formattedAmount;

        float value = Mathf.Round(_valuePer * _amount);
        valueText.text = value.ToString();
    }

    protected override void Update()
    {
        base.Update();

        if ( OrbitCam.CamMode() != CameraMode.Interior ) End();
        

        // allow cancelling with button
        if ( player.GetButton("cancel") ) End();

        int newQty = 0;

        if ( !_selectionReady ) return;

        // Allow player to select quantities by using gamepad axis'
        if ( player.GetAxisRaw("select Y") > joyThreshhold ) newQty = 1;
        if ( player.GetAxisRaw("select Y") < -joyThreshhold ) newQty = -1;
        if ( player.GetAxisRaw("select X") > joyThreshhold ) newQty = 10;
        if ( player.GetAxisRaw("select X") < -joyThreshhold ) newQty = -10;

        if (newQty != 0) {
            ChangeAmount(newQty);
            Cooldown();
        }
    }

    
    void Cooldown()
    {
        StartCoroutine(SelectionCooldown());
    }

    IEnumerator SelectionCooldown()
    {
        _selectionReady = false;
        yield return new WaitForSeconds(.2f);
        _selectionReady = true;
    }
    

    public void ChangeAmount(int amt)
    {
        _amount += amt;
        Refresh();
    }

    public void ApplyAmount()
    {
        //create a new item stack for the amount selected
        StackedItem newStack = new StackedItem(_item.stack.item);
        newStack.qty = _amount;

        // if this is through a trade panel, do traditional trade
        if (tradePanel)
            tradePanel.Trade(inventoryPanel, newStack);

        // otherwise just do direct transaction to give the stack to the player's inventory.
        else
        {
            inventoryPanel.myInventory.Transaction(PlayerManager.PlayerInventory(), newStack, false, 1);
            inventoryPanel.Refresh();
        }

        End();
    }

    public void DeselectAndEnd()
    {
        End();
    }


    protected override void FadeoutComplete()
    {
        Destroy(gameObject);
    }
}
}