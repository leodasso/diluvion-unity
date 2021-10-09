using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Loot;
using Sirenix.OdinInspector;

namespace DUI
{

    public class DUIResources : DUIPanel
    {
        static DUIResources _duiResources;
        
        [Tooltip("These items will be shown in the hud from the start")]
        public List<DItem> defaultResources = new List<DItem>();

        public RectTransform layoutPanel;
        
        public DUIResource resourcePrefab;
        [ReadOnly]
        public List<DUIResource> resources = new List<DUIResource>();

        static DUIResources Get()
        {
            if (_duiResources) return _duiResources;
            _duiResources = UIManager.Create(UIManager.Get().resourcesPanel as DUIResources);
            return _duiResources;
        }

        protected override void Start()
        {
            // Display the default items
            foreach (var item in defaultResources)
            {
                DisplayItemHUD(item);
            }
        }

        /// <summary>
        /// Adds a resource display for the given item, which shows how many of them are in the player inventory.
        /// </summary>
        public static void DisplayItemHUD(DItem theItem)
        {
            Get().AddResourceToInstance(theItem);
        }
        

        void AddResourceToInstance(DItem forItem)
        {
            //Check if this item has already been added to resource display
            foreach (DUIResource resource in resources)
            {
                if (resource.myItem == forItem) return;
            }

            DUIResource newResource = Instantiate(resourcePrefab);
            newResource.transform.SetParent(layoutPanel, false);

            newResource.Init(forItem);
            resources.Add(newResource);
        }

        /// <summary>
        /// Returns the transform of the UI element that's displaying for the given item. If no resources
        /// are displaying for the given item, returns null.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Transform DisplayForItem(DItem item)
        {
            foreach (DUIResource resource in resources)
            {
                if (resource.myItem == item) return resource.transform;
            }

            return null;
        }
    }
}