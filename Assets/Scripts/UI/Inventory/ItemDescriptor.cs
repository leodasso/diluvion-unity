using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SpiderWeb;
using TMPro;
using Loot;

namespace DUI {

/// <summary>
/// Component for a UI panel that displays an item.
/// </summary>
public class ItemDescriptor : DUIView
{

	public bool autoHide = true;
	
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDescription;
    public TextMeshProUGUI itemValue;
    public TextMeshProUGUI useInputText;
	[Tooltip("If unhovered for this many seconds, will destroy the panel")]
	public float hideTime = 1.5f;
	
    [Space]
    public Image itemIcon;
	
	[Space]
	public string useInputName;

    DItem _item;
	RectTransform _rect;
    Item _itemDisplay;
	bool _playerInventory;
	bool _adjustPosition;
    float _multiplier = 1;
    bool _used;
	float _hideTimer;
	bool _ending;

#region static functions
	
    /// <summary>
    /// Displays the 'important new item' panel for the given item.
    /// </summary>
    public static void ShowNewItemPanel(DItem newItem)
    {
	    ItemDescriptor newInstance = UIManager.Create(UIManager.Get().importantItemPanel as ItemDescriptor);
	    newInstance.transform.SetParent(ImportantItemLayout().GetComponentInChildren<VerticalLayoutGroup>().transform);
	    newInstance.ApplyToItem(newItem);
	    
    }

	static DUIPanel _importantItemLayout;
	/// <summary>
	/// Returns the instance of the layout that holds 'important new item' panels
	/// </summary>
	static DUIPanel ImportantItemLayout()
	{
		if (_importantItemLayout) return _importantItemLayout;
		_importantItemLayout = UIManager.Create(UIManager.Get().importantItemLayout);
		return _importantItemLayout;
	}


    /// <summary>
    /// Displays the panel for when player is hovering / selecting an item. If playerInv is true, gives option to use item.
    /// </summary>
    public static void DisplayForItem(Item i, bool playerInv)
    {
        GetItemDisplayPanel().ApplyToItem(i, playerInv);
    }

	/// <summary>
	/// If the item display calling for remove is the one currently being shown, clear it. This begins the cooldown for
	/// removal.
	/// </summary>
    public static void RemoveItemDisplay(Item item)
    {
	    // Make sure not to create an item display when askig for remove
	    if (!Exists()) return;

	    if (GetItemDisplayPanel()._ending) return;

	    if (GetItemDisplayPanel()._itemDisplay == item)
		    GetItemDisplayPanel()._itemDisplay = null;
    }

	static bool Exists()
	{
		return UIManager.GetPanel(UIManager.Get().itemDescription) != null;
	}

    /// <summary>
    /// Gets the current instance of the item descriptor panel (not the new item panel) and returns it. Creates a new one if none exist.
    /// </summary>
    static ItemDescriptor GetItemDisplayPanel()
    {
		ItemDescriptor panel = UIManager.GetPanel(UIManager.Get().itemDescription) as ItemDescriptor;
        if (panel) return panel;

        return UIManager.Create(UIManager.Get().itemDescription as ItemDescriptor);
    }
	
	#endregion


    protected override void Start()
    {
        base.Start();
    
        _rect = GetComponent<RectTransform>();
        StartCoroutine(SetCallback());

	    _hideTimer = hideTime;
    }


    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

	    if (_ending) return;

	    if (autoHide)
	    {
		    // Countdown to hiding. If no item display exists, then begin countdown.
		    if (_itemDisplay) _hideTimer = hideTime;
		    else _hideTimer -= Time.unscaledDeltaTime;

		    if (_hideTimer <= 0)
		    {
			    End();
			    _ending = true;
			    return;
		    }
	    }
	    
	    if (player.GetButtonDown("accept") || player.GetButtonDown("select"))
		    ButtonAction();

	    // Move the panel to the item being hovered
        if ( _adjustPosition && _itemDisplay != null )
            _rect.position = Vector2.Lerp(_rect.position, _itemDisplay.transform.position, Time.unscaledDeltaTime * 8);
    }

    /// <summary>
    /// Displays all the stats of the given DUI item display
    /// </summary>
    void ApplyToItem(Item duiItem, bool playersInv)
    {
        if (duiItem == null || duiItem.stack == null)
        {
			alpha = 0;
            return;
        }

        _used = false;
        _rect = GetComponent<RectTransform>();

        // If this is a new window, start off over the item
        if ( _itemDisplay == null ) 
	        _rect.position = duiItem.transform.position;

        _itemDisplay = duiItem;

        //StopAllCoroutines();

	    alpha = 1;

		_playerInventory = playersInv;
        _multiplier = duiItem.ValueMultiplier();
		ApplyToItem(duiItem.stack.item);

		_adjustPosition = true;

	    // for non-usable items, don't show the 'press x to use' text
	    if (!_item.useable || !CanUseAbilities())
	    {
		    useInputText.gameObject.SetActive(false);
		    return;
	    }

	    // Display 'use' text correctly
		useInputText.gameObject.SetActive(true);
	    string inputName = Controls.InputMappingName(useInputName);
	    useInputText.text = useInputText.text.Replace("#", inputName);
    }

    /// <summary>
    /// Displays stats for a given item.
    /// </summary>
	void ApplyToItem(DItem newItem) {
  
        _item = newItem;
		itemName.text 			= _item.LocalizedName();
		itemDescription.text 	= _item.LocalizedBody();
        itemValue.text          = Calc.NiceRounding(_item.goldValue * _multiplier, 1).ToString();
		itemIcon.sprite			= _item.icon;
	    if (useInputText) useInputText.gameObject.SetActive(false);

		alpha = 1;
	}


    protected override void FadeoutComplete()
    {
        Destroy(gameObject);
    }

	/// <summary>
	/// This gets called when the action button is pressed
	/// </summary>
	public void ButtonAction() 
	{
        Debug.Log("Button action.");

		if (_item == null) 		return;
		if (!CanUseAbilities()) return;
		if (!_itemDisplay) return;

		_hideTimer = hideTime;

        //Play Click Audio
		if (GetComponent<AKTriggerPositive>() != null)
        	GetComponent<AKTriggerPositive>().TriggerPos();

        if (_item.useable) UseItem();

        End();
	}

	public override void End()
	{
		Debug.Log("item descriptor is ending");
		base.End();
	}

	/// <summary>
	/// Determines if the player is trading / looting, and if not allows to use abilities.
	/// </summary>
	bool CanUseAbilities() {

		if (!_playerInventory) return false;

		TradePanel tradePanel = TradePanel.Get();
		if (tradePanel == null) return false;
		if (tradePanel.tradeMode == TradePanel.TradeMode.none) return true;

		return false;
	}

	/// <summary>
	/// Attempts to use the item currently showing. 
	/// </summary>
	void UseItem()
	{
		if (_item == null) return;
        if ( _used ) return;

		_item.Use();
        _used = true;

        //refresh inventory
        DUIInventory.RefreshAll();
	}
	
	IEnumerator SetCallback()
	{
		yield return new WaitForSeconds(0.1f);
		if ( GetComponent<AKTriggerCallback>() )
			GetComponent<AKTriggerCallback>().Callback();
	}
}
}