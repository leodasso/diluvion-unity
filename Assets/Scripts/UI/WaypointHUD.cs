using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Diluvion;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace DUI
{

    /// <summary>
    /// Hud element that shows above 3D waypoint 
    /// </summary>
    public class WaypointHUD : DUIPanel
    {
        [Tooltip("If within this radius to the reticule, will become transparent as to not obscure the player's aim.")]
        public float reticuleRadius = 100;

        [Range(0, 1), Tooltip("When near the reticule, alpha is set to this.")]
        public float transparentState = .3f;
        public QuestActor wp;
        [HorizontalGroup, LabelWidth(70)]
        public Sprite active;
        [HorizontalGroup, LabelWidth(70)]
        public Sprite inactive;
        public Image waypointImage;
        GameObject player;
        
        
        static List<WaypointHUD> allWaypointHuds = new List<WaypointHUD>();

        /// <summary>
        /// Create a new waypoint over the given target.
        /// </summary>
        public static WaypointHUD CreateWaypoint(QuestActor target)
        {
            if (target.hideHud) return null;

            WaypointHUD instance = UIManager.Create(UIManager.Get().mainWaypoint as WaypointHUD);
            
            allWaypointHuds.Add(instance);
            
            instance.wp = target;
            return instance;
        }

        /// <summary>
        /// Removes all waypoints associated with the given quest actor
        /// </summary>
        public static void RemoveWaypoint(QuestActor target)
        {
            WaypointHUD wpToRemove = null;
            
            foreach (var waypoint in allWaypointHuds)
            {
                if (waypoint.wp == target) 
                {
                    wpToRemove = waypoint;
                    break;
                }
            }

            if (wpToRemove != null)
            {
                allWaypointHuds.Remove(wpToRemove);
                wpToRemove.End();
            }
        }

        protected override void Update ()
        {
            base.Update();
            if (!wp || !Camera.main)
            {
                alpha = 0;
                return;
            }

            // Find the player ship
            player = PlayerManager.PlayerShip();
            if (!player)
            {
                alpha = 0;
                return;
            }
            
            
            // If within the game mode's distance, show the waypoint. Otherwise, hide it.
            float dist = Vector3.Distance(player.transform.position, wp.transform.position);
            if (dist > GameManager.Mode().waypointVisibleDistance) alpha = 0;
            else
            {
                waypointImage.sprite = wp == QuestManager.MainWaypoint() ? active : inactive;

                // Set the alpha - if it's near the reticule, it'll be transparent. otherwise it's full opaque.
                alpha = DUIReticule.DistFromAim(transform.position) < reticuleRadius ? transparentState : 1;
            }
        }


        void LateUpdate ()
        {
            if (!wp || !Camera.main) return;

            // follow the position of the waypoint
            transform.position = FollowTransform(wp.transform.position, 20, Camera.main);
        }
    }
}