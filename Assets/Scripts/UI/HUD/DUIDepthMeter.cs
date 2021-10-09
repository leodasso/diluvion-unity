using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Diluvion;
using TMPro;

namespace DUI
{

    public class DUIDepthMeter : DUIPanel
    {
        public RectTransform guageGraphic;
        public TextMeshProUGUI depthText;
        public float metersPerLoop = 10;
        public RectTransform crushDepthIndicator;
        
        Hull _targetHull;
        float _guageLoopHeight = 10;
        DUIPanel _crushDepthOverlay;

        protected override void Start()
        {
            base.Start();
            _crushDepthOverlay = UIManager.Create(UIManager.Get().crushDepthOverlay);
            _guageLoopHeight = guageGraphic.GetComponent<Image>().sprite.rect.height;

            _targetHull = PlayerManager.PlayerShip().GetComponent<Hull>();
        }


        protected override void Update()
        {
            base.Update();

            if (!_targetHull) return;
            
            float depth = _targetHull.transform.position.y;

            float displayDepth = -Mathf.RoundToInt(depth);
            displayDepth = Mathf.Clamp(displayDepth, 0, 99999);

            depthText.text = displayDepth.ToString();

            float adjustedDepth = depth * metersPerLoop;
            float clampedDepth = adjustedDepth % _guageLoopHeight;
            
            guageGraphic.anchoredPosition = new Vector2(guageGraphic.anchoredPosition.x, clampedDepth);

            //Move crush depth indicator
            float indicatorY = (depth - _targetHull.testDepth) * metersPerLoop;
            crushDepthIndicator.anchoredPosition = new Vector2(0, indicatorY);

            //Show crush depth overlay
            float dist = 10;
            float minWarnDepth = _targetHull.testDepth + dist;
            float currentDepth = _targetHull.transform.position.y;
            float currentOffset = minWarnDepth - currentDepth;
            float overlayAmount = currentOffset / (dist * 2.5f);
            overlayAmount = Mathf.Clamp01(overlayAmount);

            _crushDepthOverlay.SetAlpha(overlayAmount);
        }
        

        public override void End()
        {
            base.End();
            _crushDepthOverlay.End();
        }
    }
}