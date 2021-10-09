using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Diluvion.Ships;
using Diluvion.Sonar;
using Diluvion;

namespace DUI
{

    public class DUIHappyUpgradePanel : DUIPanel
    {

        [Space]
        public Text shipName;
        public Text shipDescription;
        public Text shipLevel;
        public Image oldShipIcon;
        public Image newShipIcon;

        Animator anim;


        public void Init(SubChassis oldSub, SubChassis newSub, CompareAction newShipMode)
        {
            shipName.text = newSub.locName.LocalizedText();
            shipDescription.text = newSub.locDetailedDescription.LocalizedText();

            shipLevel.text = newSub.shipLevel.ToString();
            oldShipIcon.sprite = oldSub.shipIcon;
            newShipIcon.sprite = newSub.shipIcon;

            StartCoroutine(InitSequence(newShipMode));
        }

        IEnumerator InitSequence(CompareAction shipMode)
        {

            group.alpha = 0;
            alpha = 1;

            anim = GetComponent<Animator>();
            anim.speed = 0;

            yield return new WaitForSeconds(1);

            //TimeControl.SetPopup(true);
            GameManager.Freeze(this);

            // Fade in the panel
            while (group.alpha < .98f) yield return null;

            animator.speed = 1;
            if (shipMode == CompareAction.purchase) anim.SetTrigger("purchase");
            else anim.SetTrigger("upgrade");
        }

        public void AnimEnd()
        {
            End();
        }

        public override void End()
        {
            GameManager.UnFreeze(this);
            base.End();
        }

        public void PlaySong()
        {
            //GetComponent<AKTriggerCallback>().Callback();
        }
    }
}