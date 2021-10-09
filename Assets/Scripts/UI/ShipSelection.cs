using UnityEngine;
using System.Collections;
using Cinemachine;
using Diluvion.Ships;
using DUI;
using UnityStandardAssets.ImageEffects;

namespace Diluvion
{

    public class ShipSelection : MonoBehaviour
    {

        public string descriptionLocKey;
        public CinemachineVirtualCamera vCam;

        public SubChassis chassis;
        public SubLoadout loadout;
        public Transform focalTransform;
        public DepthOfField depthOfField;
        
        CanvasGroup gradient;

        float alpha;

        void Start()
        {
            gradient = GetComponentInChildren<CanvasGroup>();
            gradient.alpha = 0;
        }

        void Update()
        {
            gradient.alpha = Mathf.Lerp(gradient.alpha, alpha, Time.unscaledDeltaTime * 8);
        }

        public void Defocus()
        {
            vCam.Priority = 5;
            alpha = 0;
        }

        public void Focus()
        {
            vCam.Priority = 10;
            alpha = 1;
        }

        public void SelectMe ()
        {
            DUIShipSelect shipSelect = GetComponentInParent<DUIShipSelect>();
            shipSelect.SelectAShip(this);
            depthOfField.focalTransform = focalTransform;
        }
    }
}