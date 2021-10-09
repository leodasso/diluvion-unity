using UnityEngine;
using Diluvion;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace DUI {

    public class DUIInteriorQuestWaypoint : DUIPanel
    {
        [HorizontalGroup, LabelWidth(50)]
        public Sprite active;
        [HorizontalGroup, LabelWidth(50)]
        public Sprite inactive;
        public Image waypointImage;
        public GameObject linkedObject;
        [ReadOnly]
        public QuestActor wp;
        InteriorManager _interior;
        Renderer _renderer;

        public static DUIInteriorQuestWaypoint Create(GameObject forObject, QuestActor waypoint)
        {
            DUIInteriorQuestWaypoint newWP =
                UIManager.Create(UIManager.Get().interiorWP as DUIInteriorQuestWaypoint);

            newWP.linkedObject = forObject;
            newWP.wp = waypoint;

            newWP._renderer = forObject.GetComponent<Renderer>();
            if (!newWP._renderer) newWP._renderer = forObject.GetComponentInChildren<Renderer>();
            if (!newWP._renderer)
            {
                Debug.LogError("No renderer found on " + forObject + ", so 2D waypoint will be positioned on center.");
            }

            return newWP;
        }

        
        InteriorManager LinkedInterior()
        {
            if (_interior) return _interior;

            if (!linkedObject) return null;
            _interior = linkedObject.GetComponentInParent<InteriorManager>();
            if (_interior != null) return _interior;
            
            Debug.LogError("No interior could be found! " + name, gameObject);
            return null;
        }

        bool LinkedObjectIsValid()
        {
            if (!linkedObject) return false;
            float scaleMag = linkedObject.transform.lossyScale.magnitude;
            return !(scaleMag < .05);
        }
 
        // Update is called once per frame
        void LateUpdate()
        {
            if (!LinkedObjectIsValid())
            {
                alpha = 0;
                return;
            }

            if (InteriorView.ViewedInterior() == LinkedInterior()) alpha = 1;
            else alpha = 0;
            
            waypointImage.sprite = wp == QuestManager.MainWaypoint() ? active : inactive;

            Vector3 pos = linkedObject.transform.position;

            if (_renderer)
            {
                Bounds b = _renderer.bounds;
                pos = new Vector3(b.center.x, b.center.y + b.extents.y, b.center.z);
            }
            
            transform.position = FollowTransform(pos, 20, InteriorView.Get().localCam);
        }
    }
}