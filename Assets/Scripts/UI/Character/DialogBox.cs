using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using SpiderWeb;
using Diluvion;
using Sirenix.OdinInspector;
using TMPro;

namespace DUI
{
    //Function for creating a dialogue box (the lower part of what comes up when a dialogue is displayed)
    public class DialogBox : DUIView
    {
        [TabGroup("dialog", "setup")]
        public DialogOption dialogOptionPrefab;

        [ReadOnly]
        [TabGroup("dialog", "debug")]
        public Dialogue myDialogue;

        [TabGroup("dialog", "setup")]
        public DialogOption logOptionPrefab;

        [TabGroup("dialog", "setup")]
        public CharacterBox characterBoxPrefab;

        [Space]
        [TabGroup("dialog", "setup")]
        public CanvasGroup dialogBubbleGroup;

        [TabGroup("dialog", "setup")]
        public CanvasGroup dialogOptionGroup;

        [Space]

        [TabGroup("dialog", "setup")]
        public TalkyText talkyText;

        [TabGroup("dialog", "setup")]
        public TextMeshProUGUI dialogText;

        [TabGroup("dialog", "setup")]
        public RectTransform panel;

        [TabGroup("dialog", "setup")]
        public CanvasGroup nextButton;

        [TabGroup("dialog", "setup")]
        public Transform characterBoxParent;

        [TabGroup("dialog", "setup")]
        public Transform optionsLayout;

        public enum DialogState { List = 0, Dialog = 2 }

        [ReadOnly, ShowInInspector, TabGroup("dialog", "debug")]
        DialogState _myState;

        [ReadOnly, ShowInInspector, TabGroup("dialog", "debug")]
        Character _focusedSubject;

        [ReadOnly, ShowInInspector, TabGroup("dialog", "debug")]
        CharacterBox _characterBox;

        [ReadOnly, ShowInInspector, TabGroup("dialog", "debug")]
        Convo _selectedConv;

        [ReadOnly, ShowInInspector, TabGroup("dialog", "debug")]
        List<string> _dialogStrings = new List<string>();

        [ReadOnly, ShowInInspector, TabGroup("dialog", "debug")]
        List<DialogOption> _dialogOptions;

        [ReadOnly, ShowInInspector, TabGroup("dialog", "debug")]
        bool _showingOptions;

        int _speechIndex;

        // Use this for initialization
        protected override void Start()
        {
            base.Start();
            dialogBubbleGroup.alpha = 0;
            Show();
        }

        protected override void Update()
        {
            base.Update();

            if (GameManager.Player().GetButtonDown("dialog next"))
            {
                if (!_showingOptions) AdvanceDialog();
            }
        }

        public void Init(Dialogue dialogue)
        {
            //get the dialogue
            myDialogue = dialogue;
            CreateCharacterBox(myDialogue.MyCharacter());
            InitOptions();
        }


        /// <summary>
        /// Refreshes the character box displaying the stats / portrait of the given crew.
        /// </summary>
        void CreateCharacterBox(Character crew)
        {
            if (_characterBox != null)
            {
                // if the characterbox already is setup for this character, just return.
                if (_characterBox.myCrew == crew) return;
                
                // otherwise clear it out.
                Destroy(_characterBox.gameObject);
            }

            _characterBox = UIManager.Create(characterBoxPrefab, characterBoxParent);
            _characterBox.Init(crew);
        }


        /// <summary>
        /// Returns false if currently in an urgent conversation.
        /// </summary>
        public bool CanExit()
        {
            if (_selectedConv == null) return true;
            return true;
        }


        /// <summary>
        /// Will create option box for each valid dialogue bit.
        /// </summary>
        void InitOptions()
        {
            if (myDialogue == null) return;

            // Clear out the old options (if any)
            List<GameObject> children = new List<GameObject>();
            foreach (Transform t in optionsLayout) children.Add(t.gameObject);
            children.ForEach(child => Destroy(child));

            _showingOptions = true;

            //init list of dialog options
            _dialogOptions = new List<DialogOption>();

            List<Convo> options = myDialogue.GetNewDialog();
            foreach (Convo c in myDialogue.GetLogDialog())
            {
                if (options.Contains(c)) continue;
                options.Add(c);
            }

            
            int optionIndex = 0;
            foreach (Convo c in options)
            {
                bool hasBeenRead = c.BeenRead(myDialogue.MyCharacter());
                //if (c.dontLog) hasBeenRead = false;
                
                DialogOption newOption = CreateNewOption(c, hasBeenRead);
                // Place cursor on the first option
                if (optionIndex == 0)
                    SetCurrentSelectable(newOption.GetComponentInChildren<Button>().gameObject);

                //iterate index
                optionIndex++;
            }
        }

        /// <summary>
        /// Creates a new option and places it in the list.
        /// </summary>
        /// <param name="c">The conversation</param>
        /// <param name="isNew">If it's new, this will be created as a speech. If not, will be a log.</param>
        DialogOption CreateNewOption(Convo c, bool beenRead)
        {
            // Select the correct prefab based on if this is a new conversation.
            DialogOption prefab = dialogOptionPrefab;
            if (beenRead) prefab = logOptionPrefab;

            //instantiate an option box
            DialogOption newOption = Instantiate(prefab) as DialogOption;

            newOption.transform.SetParent(optionsLayout, false);
            newOption.myDialogBox = this;
            newOption.convo = c;
            newOption.Init(myDialogue);

            //add to list
            _dialogOptions.Add(newOption);
            return newOption;
        }

        void EnableCanvasGroup(CanvasGroup group, bool enabled)
        {
            if (enabled) group.alpha = 1;
            else group.alpha = 0;
            group.blocksRaycasts = enabled;
            group.interactable = enabled;
        }

        /// <summary>
        /// Change states between list of options, or displaying the dialog
        /// </summary>
        public void SetState(DialogState newState)
        {
            _myState = newState;

            if (_myState == DialogState.List)
            {
                EnableCanvasGroup(nextButton, false);

                EnableCanvasGroup(dialogBubbleGroup, false);
                EnableCanvasGroup(dialogOptionGroup, true);
            }

            if (_myState == DialogState.Dialog)
            {
                EnableCanvasGroup(nextButton, true);

                EnableCanvasGroup(dialogBubbleGroup, true);
                EnableCanvasGroup(dialogOptionGroup, false);
                
                SetCurrentSelectable(nextButton.gameObject);
            }
        }


        public void DisplayDialog(Convo conv)
        {
            _selectedConv = conv;
            //Reset the speech index
            _speechIndex = 0;
            PrepDialog(_selectedConv.speeches[0]);
        }

        Speech DisplayedSpeech()
        {
            if (_selectedConv == null)
            {
                Debug.LogError("No speech found because no convo is selected.");
                return null;
            }

            return _selectedConv.speeches[_speechIndex];
        }
        
        

        void PrepDialog(Speech speech)
        {
            /*
            //Remove hashtags
            string newText = speech.LocalizedText();

            //Make an array of the sub dialogue pieces
            string[] pendingArray = newText.Split("@"[0]);
            _dialogStrings = new List<string>();

            //Make a list of the bits of dialogue
            _dialogStrings.AddRange(pendingArray);
            _dialogStrings = StringOps.RemoveEmptyStrings(_dialogStrings);
            */

            _dialogStrings = speech.FormattedText();

            SetState(DialogState.Dialog);

            _focusedSubject = myDialogue.MyCharacter();

            // change portrait and camera target based on subject
            if (speech.speaker != null)
            {
                // check if that character is in the currently viewed interior
                _focusedSubject = OrbitCam.Get().viewedInterior.GetInhabitant(speech.speaker);

                //If the subject isn't here
                if (_focusedSubject == null)
                {
                    // Skip this speech
                    SpeechComplete();
                    return;
                }
            }

            //Default to showing the main character
            FocusOn(_focusedSubject);

            // Animate the subject
            if (speech.animTool.animationType == AnimationType.whenDisplayed)
                speech.animTool.ApplyAnimation(_focusedSubject);

            AdvanceDialog();
        }


        void FocusOn(Character crew)
        {
            //Change the displayed portrait & info (overriding the display name is optional)
            CreateCharacterBox(crew);
            OrbitCam.FocusOn(crew.gameObject);
        }

        /// <summary>
        /// Advances the dialogue 1 step.
        /// </summary>
        public void AdvanceDialog()
        {
            //PLAY NEXT PAGE SOUND
            GetComponent<AKTriggerPositive>().TriggerPos();

            //If clicked while text is still printing, show the full text
            if (!talkyText.fullyShowing) talkyText.ShowFull();

            //If there's still dialogue strings remaining
            else if (_dialogStrings.Count > 0)
            {
                string nextText = _dialogStrings[0];
                talkyText.inputText = nextText;

                //remove string
                _dialogStrings.RemoveAt(0);
            }

            else SpeechComplete();
        }

        /// <summary>
        /// The current speech is done displaying.  Checks if there's any more speeches,
        /// and if so it will display them.
        /// </summary>
        void SpeechComplete()
        {
            // Do the completed speech's action
            Speech completedSpeech = DisplayedSpeech();
            if (completedSpeech == null) return;
            if (completedSpeech.action != null) completedSpeech.action.DoAction(this);
            
            _speechIndex++;

            if (_focusedSubject) _focusedSubject.SetDefaultAnimation();

            if (_selectedConv == null)
            {
                DialogueComplete();
                return;
            }

            //If that was the last speech
            if (_speechIndex >= _selectedConv.speeches.Count)
            {
                DialogueComplete();
                return;
            }

            //Otherwise, prepare the next speech in line.
            PrepDialog(_selectedConv.speeches[_speechIndex]);
        }

        /// <summary>
        /// Hides any options except for the given one
        /// </summary>
        /// <param name="notHidden"></param>
        public void HideOtherDialogOptions(DialogOption notHidden)
        {
            _showingOptions = false;

            foreach (DialogOption option in _dialogOptions)
            {
                //don't hide the selected one
                if (option == notHidden) continue;
                option.FadeOut();
            }
        }

        public void ReturnToOptions()
        {
            SetState(DialogState.List);

            //Default to showing the main character
            FocusOn(myDialogue.MyCharacter());

            talkyText.inputText = "";
            InitOptions();

            _selectedConv = null;
            _speechIndex = 0;
        }

        //Function to find any urgent chatter related ot a completed dialogue and remove it
        public void DialogueComplete()
        {
            //Play Ending sound of dialogue
            GetComponent<AKTriggerNegative>().TriggerNeg();

            //if already showing uptions, end the dialogue box
            if (_showingOptions) End();

            if (_selectedConv != null && !_showingOptions)
            {
                //Tell dialogue instance that this bit has been shown
                _selectedConv.SetAsRead(myDialogue.MyCharacter());
                ReturnToOptions();
            }
        }

        protected override void SetDefaultSelectable()
        {
            // Check for next button
            if (nextButton.interactable)
            {
                SetCurrentSelectable(nextButton.gameObject);
                return;
            }

            // Check for dialog options
            if (_dialogOptions != null)
            {
                if (_dialogOptions.Count > 0)
                {
                    SetCurrentSelectable(_dialogOptions[0].GetComponentInChildren<Button>().gameObject);
                    return;
                }
            }

            // Check for any other buttons (i.e. remove button)
            Button b = characterBoxParent.GetComponentInChildren<Button>();
            if (b != null) SetCurrentSelectable(b.gameObject);
        }

        public override void BackToTarget()
        {
            if (!CanExit()) return;

            if (_focusedSubject) _focusedSubject.LeaveMe();
            
            UIManager.Clear<DialogHistory>();

            //remove focus of camera
            OrbitCam.ClearFocus(null);
            base.BackToTarget();
        }

        public override void End()
        {
            OrbitCam.ClearFocus();
            QuestManager.Tick();
            base.End();
        }

        protected override void FadeoutComplete()
        {
            Destroy(gameObject);
        }
    }
}