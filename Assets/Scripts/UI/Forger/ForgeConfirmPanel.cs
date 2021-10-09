using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DUI
{
    /// <summary>
    /// A panel that confirms a player's selection from the <see cref="ForgeItemPanel"/>. 
    /// </summary>
    [RequireComponent(typeof(ForgeItemPanel))]
    public class ForgeConfirmPanel : DUIPanel
    {
        public Button upgradeButton;
        public GridLayoutGroup itemReqGrid;

        [Space] 
        public DUIItemRequiredPanel itemReqPrefab;

        [Tooltip("If true, the confirm button will be inactive unless the player is able to give all the items " +
                 "listed in the bonus chunk's synth materials.")]
        public bool requireItems;

        [Tooltip("Show only how many items would be salvaged when this is dismantled. Takes into account the " +
                 "game mode's dismantle percentage.")]
        public bool showSalvagedItems;

        ForgeItemPanel _forgeItemPanel;
        Forging _myBonusChunk;

        Func<Forging, bool> myConfirmMethod;


        /// <summary>
        /// Initializes the confirmation panel for selecting an upgrade. 
        /// </summary>
        /// <param name="chunk">The selected upgrade chunk</param>
        /// <param name="onPlayerConfirm">Function to evoke when the player selects the confirm button</param>
        /// <param name="requireItems">If true, the confirm button will be inactive unless the player is able to give all
        /// the items listed in the bonus chunk's synth materials\</param>
        public void InitForgePanel(Forging chunk, Func<Forging, bool> onPlayerConfirm)
        {
            _forgeItemPanel = GetComponent<ForgeItemPanel>();

            if (!_forgeItemPanel)
            {
                Debug.LogError("Forge confirm panel needs to have a forge item panel attached.", gameObject);
                return;
            }

            myConfirmMethod = onPlayerConfirm;

            _myBonusChunk = chunk;
            
            _forgeItemPanel.ApplyChunk(chunk);
            
            // clear out old children from item grid
            SpiderWeb.GO.DestroyChildren(itemReqGrid.transform);

            bool canSynth = true;

            List<StackedItem> displayList = chunk.synthesisMaterials;
            if (showSalvagedItems) displayList = chunk.DismantleMaterials();
            
            // set up the item required panels
            foreach (StackedItem stackedItem in displayList)
            {
                DUIItemRequiredPanel instance = Instantiate(itemReqPrefab, itemReqGrid.transform);
                instance.Init(stackedItem, requireItems);
                if (!instance.HasEnough()) canSynth = false;
            }
            

            // Check if this panel requires items to confirm. If it does, turn off the button if the player doesn't have
            // the required items.
            if (requireItems) upgradeButton.interactable = canSynth;
        }

        public void OnConfirm()
        {
            myConfirmMethod(_myBonusChunk);
            ForgerPanel.Refresh();
            End();
        }

        public void Cancel()
        {
            End();
        }
    }
}