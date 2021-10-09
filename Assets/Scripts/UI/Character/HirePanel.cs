using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using SpiderWeb;
using Diluvion;
using Diluvion.Ships;

namespace DUI
{
    /// <summary>
    /// UI Element that pops up to allow player to hire a character.
    /// </summary>
    public class HirePanel : DUIView
    {
        public TextMeshProUGUI costToHire;
        public Animator starAnimator;
        public Button noBtn;
        public CharacterBox characterBoxPrefab;
        public Transform characterBoxParent;
        public PopupObject noMoneyPopup;

        bool hired = false;
        Character crewToHire;
        RectTransform rect;
        Vector2 targetPosition = Vector2.zero;

        public void Init(Character ch)
        {
            rect = GetComponent<RectTransform>();
            crewToHire = ch;
            hired = false;

            //turn off gui dialogue window if it's up
            UIManager.Clear<DialogBox>();

            // Spawn in the character box
            UIManager.Create(characterBoxPrefab, characterBoxParent).Init(ch, "", true);

            costToHire.text = CostToHire(ch).ToString();

            starAnimator.speed = 0;

            //focus on the crew
            OrbitCam.FocusOn(ch.gameObject);

            //turn on money panel 
            DUIMoneyPanel.Show();
        }

        float CostToHire(Character ch)
        {
            float cost = 0;
            Sailor crew = ch as Sailor;
            if (crew) cost = crew.costToHire;
            return cost;
        }

        public void Hire()
        {
            if (hired) return;

            //Check if player can afford the cost
            Inventory playerInv = PlayerManager.pBridge.GetInventory();

            //If the player can't afford to hire dis
            if (playerInv.gold < CostToHire(crewToHire))
            {
                noMoneyPopup.CreateUI();
                End();
                return;
            }

            CrewManager playerCrew = PlayerManager.PlayerCrew();

            //deduct cost for hire
            playerInv.SpendGold((int)CostToHire(crewToHire));

            //play hired sound
            SpiderSound.MakeSound("Play_Stinger_Hired", gameObject);

            starAnimator.speed = 1;
            playerCrew.AddCrewman(crewToHire);

            //wait and end to allow for star animation to play3
            hired = true;
            StartCoroutine(WaitAndEnd(1));
        }

        /// <summary>
        /// Makes it mandatory to hire this person.
        /// </summary>
        /// <param name="mandatory">If set to <c>true</c> mandatory.</param>
        public void SetMandatory(bool mandatory)
        {
            noBtn.interactable = !mandatory;
        }

        public void Decline()
        {
            SpiderSound.MakeSound("Play_Stinger_Declined", gameObject);
            End();
        }

        public override void End()
        {
            OrbitCam.ClearFocus();

            //turn off money panel
            DUIMoneyPanel.Hide();

            base.End();
        }

        protected override void FadeoutComplete()
        {
            Destroy(gameObject);
        }

        IEnumerator WaitAndEnd(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            End();
            yield break;
        }
    }
}