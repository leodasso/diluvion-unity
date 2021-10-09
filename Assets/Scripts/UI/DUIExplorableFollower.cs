using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SpiderWeb;
using Diluvion;

namespace DUI
{

    public class DUIExplorableFollower : DUIFollower
    {
        public DebugExplorable targetExplorable;
        public Image exImage;

        public override void AddTarget(GameObject ttf)
        {
            base.AddTarget(ttf);
            targetExplorable = ttf.GetComponent<DebugExplorable>();
            gameObject.name = "DUI " + targetExplorable.name;
            SetHostile();
        }

        public void SetHostile()
        {
            if (!targetExplorable) return;
            if (!targetExplorable.instanceRef) return;

        }

        public void Activate()
        {
            targetExplorable.Activate(true);
        }

        public void Hide()
        {
            targetExplorable.ResetAndDeactivate();
        }

        public void Reset()
        {
            targetExplorable.Reset();
        }

        public void Teleport()
        {
            CheatManager.Get().WarpPlayerTo(targetExplorable.transform.position - (OrbitCam.Get().transform.forward * 10));
        }
    }
}