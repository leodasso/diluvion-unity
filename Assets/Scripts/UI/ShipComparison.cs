using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Diluvion;
using Diluvion.SaveLoad;
using Diluvion.Ships;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public enum CompareAction { swap, purchase, upgrade}; 	//THESE MATCH I2 LOCALIZATION KEYS, DONT CHANGE THEM

namespace DUI
{
    public class ShipComparison : DUIView
    {

	    public PopupObject notDepthSafe;
	    [ReadOnly]
	    public SubChassisData ship1;
	    [ReadOnly]
	    public SubChassisData ship2;
	    [Space]
	    public DUIHappyUpgradePanel upgradeSuccessPanel;
	    public DUIUpgradeReqPanel requirementsPanel;
	    public CanvasGroup costsGroup;
	    [Space]
	    public HorizontalLayoutGroup shipInfoParent;
	    public TextMeshProUGUI actionButtonText;
	    public Button actionsButton;

	    [DrawWithUnity] public UnityEvent onShipChanged;
	    
	    ShipUpgradeInfo ship1Info;
	    ShipUpgradeInfo ship2Info;

	    CompareAction compareMode;

	    static ShipComparison _instance;

        public static void CompareShips(SubChassisData sub1, SubChassisData sub2, CompareAction mode)
        {
            _instance = UIManager.Create(UIManager.Get().shipComparison as ShipComparison);
            _instance.Init(sub1, sub2, mode);
        }

	    public static ShipComparison GetInstance()
	    {
		    return _instance;
	    }


	    void Init(SubChassisData newShip1, SubChassisData newShip2, CompareAction mode) 
	    {
            Show();

		    compareMode = mode;

		    // Don't show requirements if just swapping between two owned ships
		    if (compareMode == CompareAction.swap)
		    {
			    costsGroup.alpha = 0;
			    actionsButton.interactable = true;
		    }
		    else
		    {
			    costsGroup.alpha = 1;

			    // Initialize the requirements panel
			    requirementsPanel.Init(newShip2, mode);

			    // Activate / deactivate button based on if purchase requirements are met
			    actionsButton.interactable = requirementsPanel.RequirementsMet();
		    }

		    // show action button text based on the purpose of this comparison
		    actionButtonText.text = "sample text"; // TODO fix me I2.Loc.ScriptLocalization.Get("GUI/" + mode);

		    ship1 = newShip1;
		    ship2 = newShip2;

            //Instantiate the first ship's info (this is usually the player's current ship)
            ship1Info = UIManager.Create(UIManager.Get().shipFullPanel as ShipUpgradeInfo);
		    ship1Info.transform.SetParent( shipInfoParent.transform, false);
		    ship1Info.Init(newShip1);

		    //Instantiate the second ship's info
		    ship2Info = UIManager.Create(UIManager.Get().shipFullPanel as ShipUpgradeInfo);
            ship2Info.transform.SetParent( shipInfoParent.transform, false);
		    ship2Info.Init(newShip2);
	    }

	    public void ActionButton()
	    {
		    if (!SwapIsDepthSafe(compareMode))
		    {
			    Debug.Log("This swap isn't safe!!");
			    return;
		    }
		    
		    DSave.current.SaveLocation(PlayerManager.PlayerTransform());
            //DSave.Save();
		    
		    UIManager.Clear<DialogBox>();
		    UIManager.Clear<Shipyard>();
		    
		    OrbitCam.RequestTransition(false);

		    if (compareMode == CompareAction.upgrade) 
		    {
                PlayerManager.UpgradeSub(ship1);

                SpiderWeb.SpiderSound.MakeSound("Play_Stinger_Upgraded",gameObject);

                // bring in the congrats panel
                CongratsPanel(compareMode);
            }


		    if(compareMode == CompareAction.purchase) 
		    {
                SpiderWeb.SpiderSound.MakeSound("Play_Stinger_Purchased", gameObject);

                //Switch the subs.
                PlayerManager.PurchaseSub(ship2);
                // bring in the congrats panel
                CongratsPanel(compareMode);
		    }

            if (compareMode == CompareAction.swap)
                PlayerManager.InstantiatePlayerSub(ship2);


            // If the player is purchasing or upgrading
            if (compareMode == CompareAction.purchase || compareMode == CompareAction.upgrade) 
		    {

                ShipCosts cost = ship2.ChassisObject().cost;

                Inventory playerInv = PlayerManager.pBridge.GetInventory();
                playerInv.SpendGold(cost.goldCost);
                playerInv.UpdateGoldAchievement();

                // deduct items
                foreach (StackedItem itemStack in cost.requiredItems) 
			    {
                    playerInv.RemoveItem(itemStack);
			    }
			    
			    onShipChanged.Invoke();
		    }

            End();
	    }

	    /// <summary>
	    /// Returns false if the player's trying to swap into a sub that can't handle the depth they're at.
	    /// </summary>
	    bool SwapIsDepthSafe(CompareAction swapType)
	    {
		    // Upgrades are always safe because the depth upgrade slots get carried over to the new ship
		    if (swapType == CompareAction.upgrade) return true;

		    if (ship2 == null)
		    {
			    Debug.LogError("While checking if sub swap is safe, the sub to be swapped into couldn't be found!");
			    return false;
		    }
		    
		    float depth = PlayerManager.PlayerShip().transform.position.y;
		    float depthOfSwap = ship2.CrushDepth();

		    if (depth < depthOfSwap)
		    {
			    notDepthSafe.ShowPopup();
			    return false;
		    }
		    return true;
	    }

	    void CongratsPanel(CompareAction swapType) 
	    {
		    DUIHappyUpgradePanel congratsPanel = Instantiate(upgradeSuccessPanel);
		    congratsPanel.transform.SetParent(transform.parent, false);
		    congratsPanel.Init(ship1.ChassisObject(), ship2.ChassisObject(), swapType);
	    }

        protected override void FadeoutComplete()
        {
            Destroy(gameObject);
        }
    }
}