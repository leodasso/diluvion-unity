using UnityEngine;
using UnityEngine.UI;
using Diluvion;
using Diluvion.Ships;
using Sirenix.OdinInspector;

namespace DUI
{

    public class DUIReticule : DUIPanel
    {

        [MinMaxSlider(0, 1)]
        public Vector2 minMaxRadiusAlpha;
        public float radiusVisibleDist = 30;
        
        public RectTransform reticule;
        public RectTransform reticuleRadius;
        public CanvasGroup circle;

        [ReadOnly]
        public ShipControls playerControls;

        public float pixelsPerDegree = 25;

        Vector3 _aimPos;
        float _aimAngle;

        public static Vector3 PlayerAimPos()
        {
            DUIReticule r = UIManager.GetPanel<DUIReticule>();
            return r == null ? Vector3.zero : r.reticule.position;
        }
        
        /// <summary>
        /// The distance the given position is from the player aim reticule (in GUI space)
        /// </summary>
        public static float DistFromAim(Vector3 pos)
        {
            return ((Vector2)pos - (Vector2)PlayerAimPos()).magnitude;
        }

        protected override void Start()
        {
            base.Start();
            
            _aimAngle = PlayerPrefs.GetFloat("_maxAimAngle", 25);
            playerControls = PlayerManager.PlayerShip().GetComponent<ShipControls>();
            alpha = 1;
        }


        protected override void Update()
        {
            base.Update();
            
            _aimAngle = PlayerPrefs.GetFloat("_maxAimAngle", 25);
            float size = pixelsPerDegree * _aimAngle;
            reticuleRadius.sizeDelta = new Vector2(size * 2, size * 2);
            
            _aimPos = playerControls.AimPosition(.1f, 100);
            reticule.position = FollowTransform(_aimPos, 20, Camera.main);

            // adjust circle alpha 
            float closeness = size - reticule.localPosition.magnitude;

            if (closeness < radiusVisibleDist)
            {
                float amount = (radiusVisibleDist - closeness) / radiusVisibleDist;
                amount = Mathf.Clamp01(amount);
                circle.alpha = minMaxRadiusAlpha.y;

                float newAlpha = Mathf.Lerp(minMaxRadiusAlpha.x, minMaxRadiusAlpha.y, amount);
                circle.alpha = newAlpha;
            }
            else circle.alpha = minMaxRadiusAlpha.x;
        }
    }
}