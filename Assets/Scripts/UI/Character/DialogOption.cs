using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;

namespace DUI
{
    //Class controlling a single option inside a dialogue poput
    public class DialogOption : DUIPanel
    {

        [ReadOnly]
        public Convo convo;

        [ReadOnly]
        public DialogBox myDialogBox;

        [Space]
        public TextMeshProUGUI text;
        public CanvasGroup questIcon;

        bool init = false;
        bool inConversation = false;

        public void Init (Dialogue forDialogue)
        {
            Show(convo.GivesQuest());
            init = true;
        }


        public void FadeOut ()
        {
            inConversation = false;
            alpha = 0;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        public void Show (bool givesQuest)
        {
            inConversation = false;
            text.text = convo.LocalizedQuestion();

            if (questIcon)
                questIcon.alpha = givesQuest ? 1 : 0;

            alpha = 1;
            group.blocksRaycasts = true;
            group.interactable = true;
        }

        public void SelectAsLog ()
        {
            Debug.Log("Showing as a dialog history.");
            DialogHistory.ShowDialogHistory(convo, myDialogBox.myDialogue.MyCharacter());
        }

        public void SelectThisDialog ()
        {

            if (!inConversation)
                StartCoroutine(WaitAndHide());
        }

        IEnumerator WaitAndHide ()
        {

            FadeOut();
            myDialogBox.HideOtherDialogOptions(this);
            //GetComponent<Animator>().SetTrigger("select");
            inConversation = true;

            yield return new WaitForSeconds(.6f);
            //Play Option Start sound
            GetComponent<AKTriggerCallback>().Callback();
            //show dialogue
            myDialogBox.DisplayDialog(convo);
        }
    }
}