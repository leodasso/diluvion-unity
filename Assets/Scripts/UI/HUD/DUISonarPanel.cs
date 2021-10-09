using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Diluvion.Sonar;
using Diluvion.Ships;
using Diluvion;
using Sirenix.OdinInspector;
using TMPro;

namespace DUI
{

    public class DUISonarPanel : DUIPanel
    {
        [ReadOnly]
        public Pinger pinger;

        [ReadOnly] public Listener listener;

        [Range(0, 1), OnValueChanged("UpdateGuage")]
        public float charge = 0;

        [OnValueChanged("UpdateGuage")]
        public float maxChargeSize = 100;

        public float minCircleSize = 30;

        [Range(0, 1), OnValueChanged("UpdateGuage")]
        public float hailPercentage = .1f;
        
        [Range(0, 1), OnValueChanged("UpdateGuage")]
        public float SOSPercentage = .9f;

        [Space]
        public Color SOSColor = Color.white;
        public Color hailColor = Color.white;
        public Color pingColor = Color.white;

        [BoxGroup("Local objects")]
        public RectTransform circle;

        [BoxGroup("Local objects")]
        public RectTransform guage;

        [BoxGroup("Local objects")]
        public TextMeshProUGUI label;

        [BoxGroup("Local objects")]
        public RectTransform SOSGuage;

        [BoxGroup("Local objects")]
        public RectTransform hailGuage;

        Image circleImage;
        Image sosImage;
        Image hailImage;
        //Vector3 _initPos;
        // _gotoPos;

        public void SonarPanelSetup(Pinger p, Listener l)
        {
            pinger = p;
            listener = l;
            hailPercentage = Pinger.hailPercent;
            SOSPercentage =  Pinger.sosPercent;
        }

        // Use this for initialization
        protected override void Start()
        {
            base.Start();
            circleImage = circle.GetComponent<Image>();
            hailImage = hailGuage.GetComponent<Image>();
            sosImage = SOSGuage.GetComponent<Image>();
            transform.SetAsFirstSibling();
            //_initPos = transform.position;
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            if (!pinger) return;


            charge = pinger.NormalizedCharge();
            alpha = pinger.ShowGUI() ? 1 : 0;
            
            UpdateGuage();            
        }

        /// <summary>
        /// Makes the guage visual match the percentages of SOS, HAIL
        /// </summary>
        void UpdateGuage()
        {
            if (!hailGuage || !SOSGuage || !guage) return;
            

            // Set the size of the guage
            guage.sizeDelta = new Vector2(maxChargeSize / 2, guage.sizeDelta.y);

            hailGuage.anchorMax = new Vector2(hailPercentage, 1);
            SOSGuage.anchorMin = new Vector2(SOSPercentage, 0);

            // Set the size of the circle (based on current charge
            float size = charge * maxChargeSize;
            size = Mathf.Clamp(size, minCircleSize, 999);
            
            circle.sizeDelta = new Vector2(size, circle.sizeDelta.y);

            if (circleImage == null) circleImage = circle.GetComponent<Image>();

            if (charge <= hailPercentage)
            {
                circleImage.color = label.color = hailColor;
                label.text = "HAIL";
            }
            else if (charge > SOSPercentage)
            {
                circleImage.color = label.color = SOSColor;
                label.text = "SOS";
            }
            else
            {
                label.text = "PING";
                circleImage.color = label.color = pingColor;
            }
        }

        /*
        void LateUpdate()
        {
            //place the element over the hail target if there is one.
            if (listener)
            {
                if (listener.GetTargetedStats() != null)
                {
                    _gotoPos = FollowTransform(listener.GetTargetedStats().transform.position, 30, Camera.main);
                }
                else _gotoPos = _initPos;
            }

            transform.position = Vector3.Lerp(transform.position, _gotoPos, Time.deltaTime * 8);
        }
        */
    }
}