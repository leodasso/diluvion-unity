using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Diluvion;

namespace DUI
{
    /// <summary>
    /// Displays a list of the new charts that the player found, and the total reward they'll recieve.
    /// </summary>
    public class CartographerPanel : DUIView
    {

        public delegate void PlayerAcceptPayment();
        public PlayerAcceptPayment paymentAccepted;
        [Space]
        public DUILandmarkChart landmarkPanelPrefab;
        public VerticalLayoutGroup layoutGroup;
        public TextMeshProUGUI totalsText;
        public TextMeshProUGUI titleText;

        int totalReward = 0;

        public void Init(List<ChartInfo> chartInfoList)
        {
            GameManager.Freeze(this);
            
            // Show player's money
            DUIMoneyPanel.Show();

            // Show title text
            string title = SpiderWeb.Localization.GetFromLocLibrary("GUI/new_disc_f", "string not found");
            if (chartInfoList.Count > 0)
                title = SpiderWeb.Localization.GetFromLocLibrary("GUI/new_disc_t", "string not found");
            titleText.text = title;

            // Show the UI panel for each new discovery
            foreach (ChartInfo info in chartInfoList)
            {
                totalReward += info.reward;

                // Create the chart GUI
                DUILandmarkChart newPanel = Instantiate(landmarkPanelPrefab) as DUILandmarkChart;
                newPanel.transform.SetParent(layoutGroup.transform, false);
                newPanel.Init(info);
            }

            // Display the total reward
            totalsText.text = totalReward.ToString();
        }

        /// <summary>
        /// Player accepted the payments, continue.
        /// </summary>
        public void Accept()
        {
            if (paymentAccepted != null) paymentAccepted();
            BackToTarget();
        }

        protected override void FadeoutComplete()
        {
            DUIMoneyPanel.Hide();

            //TimeControl.SetPopup(false);
            GameManager.UnFreeze(this);

            Destroy(gameObject);
            //base.FadeoutComplete();
        }
    }
}