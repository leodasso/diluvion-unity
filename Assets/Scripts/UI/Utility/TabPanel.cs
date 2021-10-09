using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using SpiderWeb;

namespace DUI
{

    [ExecuteInEditMode]
    public class TabPanel : MonoBehaviour
    {
        public bool autoArrange;
        [Tooltip("The index of this tab from left to right"), MinValue(0), HideIf("autoArrange")]
        public int tabIndex;

        [Tooltip("Is this the tab that shows at start? There should only be one of these per group."), HideIf("autoArrange")]
        public bool defaultTab;
        public RectTransform tabRect;
        public Image tabImage;
        public Sprite activeSprite;
        public Sprite inactiveSprite;

        [Tooltip("All the contents of this tab (minus the tab button itself). This is used to set the contents as non" +
                 "-interactive when the tab group isn't active.")]
        public List<CanvasGroup> tabContents;

        

        [ShowInInspector, ReadOnly]
        bool activeTab;

        public List<TabPanel> siblings = new List<TabPanel>();

        RectTransform rect;
        Vector2 initAnchorPos;
        int totalTabs;
        float tabSize;

        [Space]
        public float leftPadding = 15;
        public float rightPadding = 17;

        [Space]
        public float leftExtension = 15;
        public float rightExtension = 15;
        

        // Use this for initialization
        void Start ()
        {
            tabImage.sprite = inactiveSprite;

            rect = GetComponent<RectTransform>();
            initAnchorPos = rect.anchoredPosition;

            GetSiblings();

            foreach (TabPanel t in siblings) t.GetSiblings();

            // If this is the default tab, set it active
            SetTabActive(defaultTab);
        }


        /// <summary>
        /// Find all my transform siblings.
        /// </summary>
        void GetSiblings()
        {
            siblings.Clear();

            foreach (TabPanel tab in transform.parent.GetComponentsInChildren<TabPanel>())
            {
                if (tab == this) continue;
                siblings.Add(tab);
            }
            
            if (autoArrange)
            {
                tabIndex = transform.GetSiblingIndex();
                defaultTab = tabIndex == 0;
            }

            //index = transform.GetSiblingIndex();
        }


        void Update ()
        {
            if (!autoArrange) return;

            totalTabs = siblings.Count + 1;

            tabSize = 1 / (float)totalTabs;

            tabRect.anchorMin = new Vector2(tabSize * tabIndex, tabRect.anchorMin.y);
            tabRect.anchorMax = new Vector2(tabSize * (tabIndex + 1), tabRect.anchorMax.y);

            if (tabIndex == 0 && totalTabs == 1)
            {
                tabRect.anchoredPosition = new Vector2(leftPadding, tabRect.anchoredPosition.y);
                tabRect.sizeDelta = new Vector2(-rightPadding - leftPadding, tabRect.sizeDelta.y);
                return;
            }

            // left tab
            if (tabIndex == 0)
            {
                tabRect.anchoredPosition = new Vector2(leftPadding, tabRect.anchoredPosition.y);
                tabRect.sizeDelta = new Vector2(rightExtension, tabRect.sizeDelta.y);
            }

            // right tab
            else if (tabIndex == totalTabs - 1)
            {
                tabRect.anchoredPosition = new Vector2(-leftExtension, tabRect.anchoredPosition.y);
                tabRect.sizeDelta = new Vector2(-rightPadding + leftExtension, tabRect.sizeDelta.y);
            }

            // Middle tabs
            else
            {
                tabRect.sizeDelta = new Vector2(rightExtension + leftExtension, tabRect.sizeDelta.y);
                tabRect.anchoredPosition = new Vector2(-leftExtension, tabRect.anchoredPosition.y);
            }
        }


        [Button]
        public void ActivateTab()
        {
            SetTabActive(true);
        }


        void SetTabActive (bool active)
        {
            // prevent obsolete changes
            if (activeTab == active) return;

            activeTab = active;

            // change sort order
            if (active)
            {
                transform.SetAsLastSibling();
                tabImage.sprite = activeSprite;
                
                // Set my contents to be interactive
                foreach (var c in tabContents) c.interactable = true;

                // Set other tabs inactive
                foreach (TabPanel sibling in siblings) sibling.SetTabActive(false);
                
                SpiderSound.MakeSound("Play_Inventory_Tabs", gameObject);
            }
            else
            {
                transform.SetAsFirstSibling();
                tabImage.sprite = inactiveSprite;
                rect.anchoredPosition = initAnchorPos;
                
                // Set my contents to be non-interactive
                foreach (var c in tabContents) c.interactable = false;
            }
        }

    }
}