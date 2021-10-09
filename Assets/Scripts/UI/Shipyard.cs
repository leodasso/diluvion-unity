using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Diluvion.SaveLoad;
using Diluvion.Ships;
using Diluvion;

namespace DUI
{
    /// <summary>
    /// UI Panel for shipyards. Swapping, upgrading, purchasing!
    /// </summary>
    public class Shipyard : DUIView {

	    public ShipBroker shipBroker;
	    public List<SubChassisData> displayedShips = new List<SubChassisData>();       

	    public RectTransform grid;
	    public TextMeshProUGUI nameText;
	    public TextMeshProUGUI shipyardLevel;
	    public RectTransform shopParent;
	    public RectTransform menuParent;
	    public Button swapButton;
	    public Button purchaseButton;
        public Button shipListBackButton;

        public PopupObject cantUpgradePopup;

	    CompareAction _mode;
        bool _mainMenu = true;
	    List<ShipHeader> _shipHeaders = new List<ShipHeader>();
	    float _gridAnchoredY;

        // Use this for initialization
        protected override void Start()
        {
            base.Start();

        //Place shop grid hidden out of bounds of screen
        shopParent.anchoredPosition = new Vector2(0, -2000);
		    _gridAnchoredY = -1000;
	    }
	    

	    public void Init(ShipBroker newShipBroker) 
	    {
		    shipBroker = newShipBroker;

		    foreach (SubChassis chassis in shipBroker.MyShips())
		    {
			    SubChassisData data = new SubChassisData(chassis);
			    foreach (var forging in shipBroker.defaultForgings)
			    {
				    data.appliedSlots.Add(forging.name);
			    }
			    displayedShips.Add(data);
		    }

		    // Display the name of the shipyard
            nameText.text = shipBroker.GetComponent<Character>() ? 
	            shipBroker.GetComponent<Character>().GetLocalizedName() : shipBroker.shipyardName;

		    // Display the level of the shipyard
		    shipyardLevel.text 	= shipyardLevel.text.Replace("#", shipBroker.engineerLevel.ToString());

		    //disable 'swap' button if the player only has one ship
		    if (DSave.current.playerShips.Count < 2) swapButton.interactable = false;

		    //disable 'purchase' button if the broker has no ships
		    if (displayedShips.Count < 1) purchaseButton.interactable = false;
	    }

        protected override void Update()
        {
            base.Update();

            //animate the ships grid
            shopParent.anchoredPosition = Vector2.Lerp(shopParent.anchoredPosition, new Vector2(0, _gridAnchoredY), Time.deltaTime * 8);
        }


        protected override void SetDefaultSelectable()
        {
            if ( _mainMenu )
                base.SetDefaultSelectable();

            else SetCurrentSelectable(shipListBackButton.gameObject);
        }

   

        /// <summary>
        /// Populate the list with ships
        /// </summary>
        void Populate(CompareAction mode) 
        {
		    ShowShips();
      
		    //Clear the previous population (if any)
		    ClearList();

            // Create list to populate ships with.  What goes in the list depends on compare action: swap, upgrade, buy.
            List<SubChassisData> subsList = new List<SubChassisData>();

		    // For purchasing, use the ship vendor's list
		    if (mode == CompareAction.purchase)
                subsList.AddRange(displayedShips);

		    //for swaps, use the list of player ships
		    if (mode == CompareAction.swap) {
                for (int i = 0; i < DSave.current.playerShips.Count; i++)
                {
                    // Skip the first index, because that's the player's current sub.
                    if (i == 0) continue;
                    subsList.Add(DSave.current.playerShips[i]);
                }
		    }

		    //For upgrades, use the players list of ships
		    if (mode == CompareAction.upgrade)
            {
                foreach (SubChassisData data in DSave.current.playerShips)
                    subsList.Add(data);
            }

            int index = 0;

		    // Instantiate a ship header for each ship, creating a list
		    foreach (SubChassisData sub in subsList) 
		    {
                if (sub == null) continue;
			    bool canUpgrade = true;
            
			    // If upgrading, disable ships that are too high a level for the ship broker to upgrade
			    if (mode == CompareAction.upgrade) {

                    if ( !sub.ChassisObject().hasUpgrade ) continue;
				    if (sub.ChassisObject().shipLevel >= shipBroker.engineerLevel) canUpgrade = false;		
              
			    }

			    ShipHeader infoPanel = UIManager.Create(UIManager.Get().shipHeader as ShipHeader, grid);
			    infoPanel.Init(sub, this, canUpgrade);
			    _shipHeaders.Add(infoPanel);

                // Set selection to the first ship panel
                if ( index == 0 ) SetCurrentSelectable(infoPanel.gameObject);

                index++;
		    }
	    }

	    /// <summary>
	    /// Removes all the ship panels from the list
	    /// </summary>
	    void ClearList() 
	    {
		    foreach (ShipHeader shipHeader in _shipHeaders) shipHeader.End();
		    _shipHeaders = new List<ShipHeader>();
	    }

	    public void OpenComparison(SubChassisData chassisData) 
	    {
            // For a purchase or swap, compare between currently equipped ship and the ship to buy
            if (_mode == CompareAction.purchase || _mode == CompareAction.swap)
                ShipComparison.CompareShips(DSave.current.playerShips[0], chassisData, _mode);

		    // for an upgrade, compare between the current level ship and the upgrade level
		    if (_mode == CompareAction.upgrade) {

			    //First we need to see if the ship player's trying to upgrade has an upgrade
			    if (!chassisData.ChassisObject().hasUpgrade) 
			    {
                    cantUpgradePopup.CreateUI();
				    return;
			    }

                //find the bridge for the next upgrade level
                SubChassis nextUpgrade = chassisData.ChassisObject().upgrade;
			    if (nextUpgrade == null) {
				    Debug.LogError("The selected ship is marked as having an upgrade, but the upgrade slot is empty.", chassisData.ChassisObject());
				    return;
			    }

			    SubChassisData newUpgradeData = new SubChassisData(chassisData);
			    newUpgradeData.Upgrade();

                // If all checks out, bring up the comparison between current ship and its upgrade
                ShipComparison.CompareShips(chassisData, newUpgradeData, _mode);
		    }
	    }

	    public void ShowPurchases() 
	    {
		    _mode = CompareAction.purchase;
		    Populate(CompareAction.purchase);
	    }

	    public void ShowSwaps() 
	    {
		    _mode = CompareAction.swap;
		    Populate(CompareAction.swap);
	    }

	    public void ShowUpgrades() 
	    {
		    _mode = CompareAction.upgrade;
		    Populate(CompareAction.upgrade);
	    }

	    /// <summary>
	    /// Shows the ships grid panel, hides the menu
	    /// </summary>
	    public void ShowShips()
        {
            GetComponent<AKTriggerPositive>().TriggerPos();

		    _gridAnchoredY = 0;

            // turn menu off
            menuParent.gameObject.SetActive(false);
            _mainMenu = false;
	    }

	    /// <summary>
	    /// Hides the ships grid panel, shows the menu
	    /// </summary>
	    public void HideShips()
        {
		    _gridAnchoredY = -2000;

            GetComponent<AKTriggerNegative>().TriggerNeg();

		    //disable 'swap' button if the player only has one ship
	        if (DSave.current != null)
		    	swapButton.interactable = DSave.current.playerShips.Count >= 2;

            // turn menu on
            menuParent.gameObject.SetActive(true);
            _mainMenu = true;

            SetDefaultSelectable();
	    }

        public override void BackToTarget()
        {
            if ( !_mainMenu ) {
                HideShips();
                return;
            }

            base.BackToTarget();
        }
    }
}