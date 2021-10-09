using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace DUI
{
    public class DUIFollower : DUIPanel
    {

        public GameObject targetToFollow;
        public Text titleObj;
        public virtual void AddTarget(GameObject ttf)
        {
            targetToFollow = ttf;
            titleObj.text = "Debug_" + ttf.name;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (targetToFollow == null) return;

            transform.position = FollowTransform(targetToFollow.transform.position, 50, Camera.main);
        }
    }
}