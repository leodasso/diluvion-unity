using UnityEngine.UI;
using TMPro;
using Diluvion.Ships;

namespace DUI
{
    public class ShipHeader : DUIPanel
    {

        public TextMeshProUGUI shipLevel;
        public TextMeshProUGUI flavorText;
        public TextMeshProUGUI shipName;
        public Image shipIcon;

        Button button;
        Shipyard parentShipyard;
        SubChassisData _chassisData;
        SubChassis _chassis;

        public void Init (SubChassisData newChassis, Shipyard shipyard, bool available = true)
        {

            _chassisData = newChassis;
            _chassis = _chassisData.ChassisObject();

            button = GetComponent<Button>();
            button.interactable = available;

            if (available) alpha = 1;
            else alpha = .4f;

            //Get parent shipyard, if any
            if (shipyard != null) parentShipyard = shipyard;

            //Apply all the text bits from the info set
            shipLevel.text = _chassis.shipLevel.ToString();
            flavorText.text = _chassis.locDetailedDescription.LocalizedText();
            shipName.text = _chassis.locName.LocalizedText();

            //apply the ship's icon
            if (_chassis.shipIcon != null)
                shipIcon.sprite = _chassis.shipIcon;
        }

        public void OnClick ()
        {

            // bring up ship comparison
            if (parentShipyard != null)
                parentShipyard.OpenComparison(_chassisData);
        }
    }
}