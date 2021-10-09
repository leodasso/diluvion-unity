using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Rewired;
using TMPro;
using Diluvion.Ships;
using Sirenix.OdinInspector;

namespace DUI
{
    public class DUIThrottle : DUIPanel
    {
        [ReadOnly, BoxGroup("Throttle")]
        public ShipMover shipMover;

        [BoxGroup("Throttle")]
        public bool leverControl;
        [BoxGroup("Throttle")]
        public int pixelsPerHeatTolerance = 25;

        public RectTransform heatToleranceBg;
        public FancyProgressBar heatAmountBar;

        [Space, BoxGroup("Throttle")]
        public bool autoHideThrottle;
        [Range(0, 1), BoxGroup("Throttle")]
        public float hiddenAlpha = .5f;
        [BoxGroup("Throttle")]
        public float[] throttleAngles;

        [Space]
        [BoxGroup("Throttle")]
        public Image throttleLever;
        [BoxGroup("Throttle")]
        public CanvasGroup overdriveText;
        [BoxGroup("Throttle")]
        public Transform mainThrottle;

        [Space]
        [BoxGroup("Throttle"), ColorPalette]
        public Color overdriveGood;
        [BoxGroup("Throttle"), ColorPalette]
        public Color overdriveRecharge;

        float _curThrottleAngle;
        Player player;


        // Use this for initialization
        protected override void Start()
        {
            base.Start();

            player = ReInput.players.GetPlayer(0);
            //alpha = .5f;
        }

        public void InitThrottle(ShipMover ship)
        {
            shipMover = ship;
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            if (shipMover == null) return;
            UpdateOverdrive();

            if (leverControl)  UpdateThrottle();
            
            heatToleranceBg.sizeDelta = new Vector2(heatToleranceBg.sizeDelta.x, shipMover.heatTolerance * pixelsPerHeatTolerance);
            heatAmountBar.value = shipMover.NormalizedOverheatAmount();
        }

        void UpdateThrottle()
        {
            if (animator) animator.enabled = false;

            int index = (int)shipMover.throttle;
            index = Mathf.Clamp(index, 0, throttleAngles.Length - 1);
            float finalAngle = throttleAngles[index];
            _curThrottleAngle = Mathf.Lerp(_curThrottleAngle, finalAngle, Time.deltaTime * 12);

            //Set the angle of the throttle lever
            throttleLever.transform.localEulerAngles = new Vector3(0, 0, _curThrottleAngle);

            //If the player has made any inputs, make the throttle visible, and reset the timer to fade it out.
            if (Math.Abs(player.GetAxis("throttle")) > .1f)
            {
                alpha = 1;
                StopAllCoroutines();

                if (autoHideThrottle) StartCoroutine(ThrottleFade());
            }
        }

        void UpdateOverdrive()
        {
            // TODO adjust overdrive visuals to reflect if able to overdrive or not, based on air avialable
        }

        IEnumerator ThrottleFade()
        {
            yield return new WaitForSeconds(1.5f);

            // Don't fade out if in overdrive
            while (shipMover.throttle == Throttle.overdrive)
                yield return new WaitForSeconds(.2f);

            alpha = hiddenAlpha;
        }
    }
}