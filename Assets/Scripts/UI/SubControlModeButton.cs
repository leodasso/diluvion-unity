using UnityEngine;
using UnityEngine.UI;
using Diluvion;
using Diluvion.Ships;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace DUI
{

    /// <summary>
    /// Button for selecting a type of submarine control.
    /// </summary>
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class SubControlModeButton : MonoBehaviour
    {
        [ReadOnly]
        public bool activeType;

        [Space]
        public Sprite selected;
        public Sprite notSelected;
        public ShipControls.ControlType controlType;

        Image image;
        List<SubControlModeButton> siblings = new List<SubControlModeButton>();

        Button button;

        private void Start ()
        {
            image = GetComponent<Image>();
            button = GetComponent<Button>();

            foreach (SubControlModeButton b in transform.parent.GetComponentsInChildren<SubControlModeButton>())
                siblings.Add(b);

            UpdateVisual();
        }

        public void SetControlType()
        {
            PlayerPrefs.SetInt(ShipControls.prefsControlType, (int)controlType);

            // apply to the player's ship controls
            if (PlayerManager.PlayerShip())

                if (PlayerManager.PlayerShip().GetComponent<ShipControls>())
                    PlayerManager.PlayerShip().GetComponent<ShipControls>().controlType = controlType;

            foreach (SubControlModeButton b in siblings) b.UpdateVisual();

        }

        public void SetButtonEnable(bool enabled)
        {
            button.enabled = enabled;
            UpdateVisual();
        }

        /// <summary>
        /// Updates visuals to reflect if this mode is selected or not
        /// </summary>
        void UpdateVisual()
        {
            if (controlType == (ShipControls.ControlType)PlayerPrefs.GetInt(ShipControls.prefsControlType, 1))
            {
                activeType = true;
                image.sprite = selected;
            }

            else
            {
                activeType = false;
                image.sprite = notSelected;
            }
        }
    }
}