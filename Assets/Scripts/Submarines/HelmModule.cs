using UnityEngine;
using DUI;

namespace Diluvion.Ships
{

    [CreateAssetMenu(fileName = "helm module", menuName = "Diluvion/subs/modules/helm")]
    public class HelmModule : ShipModule
    {

        public override bool Disable(Bridge bridge, float disabledTime = 0)
        {
            if (!base.Disable(bridge, disabledTime)) return false;
            SetComponentsEnabled(bridge, false);
            return true;
        }

        public override bool Enable(Bridge bridge)
        {
            if (!base.Enable(bridge)) return false;
            SetComponentsEnabled(bridge, true);
            return true;
        }

        void SetComponentsEnabled(Bridge b, bool enabled)
        {
            ShipMover sm = b.GetComponent<ShipMover>();
            ShipAnimator sa = b.GetComponent<ShipAnimator>();
            if (sm) sm.enabled = enabled;
            if (sa) sa.enabled = enabled;
        }

        /// <summary>
        /// Creates and initializes the throttle UI panel
        /// </summary>
        public override DUIPanel CreateUI(Bridge forShip)
        {
            DUIPanel newPanel = base.CreateUI(forShip);
            DUIThrottle throttlePanel = newPanel as DUIThrottle;
            throttlePanel.InitThrottle(forShip.GetComponent<ShipMover>());
            return throttlePanel;
        }
    }
}