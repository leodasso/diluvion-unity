using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Diluvion;
using Sirenix.OdinInspector;

namespace DUI
{

    public class DUICharacterBubbles : DUIPanel
    {
        [ReadOnly]
        public Character focusedCrew;

        [ReadOnly]
        public Dialogue dialogInstance;

        public bool hasNewDialog;

        public Image bubble;

        public Sprite hasDialog;
        public Sprite hasQuest;
        public Sprite merchant;
        public Sprite subBroker;
        public Sprite engineer;

        Animator _bubbleAnim;

        Vector3 _crewTopCenter;

        protected override void Start()
        {
            base.Start();
            _bubbleAnim = GetComponent<Animator>();
        }

        /// <summary>
        /// Create a new instance of the character bubbles UI element over the given character. Returns the created instance.
        /// </summary>
        public static DUICharacterBubbles CreateInstance (Character character, Dialogue d )
        {
            DUICharacterBubbles newInstance = UIManager.Create(UIManager.Get().characterBubbles as DUICharacterBubbles);

            newInstance.focusedCrew = character;
            newInstance.dialogInstance = d;
            newInstance.CheckDialogue();
            
            //newInstance.ShowBubbles(character);
            return newInstance;
        }

        void LateUpdate ()
        {
            bool showMe = ShouldShow();
            
            if (showMe)
                // track the crew member's position
                transform.position = FollowTransform(focusedCrew.TopCenter(), 5, InteriorView.InteriorCam());

            // Make invisible if the dialog panel is open
            alpha = showMe ? 1 : 0;
        }

        bool ShouldShow()
        {
            if (OrbitCam.CamMode() != CameraMode.Interior) return false;
            if (!focusedCrew) return false;
            if (!focusedCrew.gameObject.activeInHierarchy) return false;
            if (!focusedCrew.isActiveAndEnabled) return false;
            if (!focusedCrew.isVisible) return false;
            if (UIManager.GetPanel<DialogBox>()) return false;
            return true;
        }
        

        /// <summary>
        /// Shows the persistent bubbles for when the character is hovered
        /// </summary>
        public void ShowBubbles (bool show)
        {
            Debug.Log("Setting sprite to " + show);
            SetSprite();
            _bubbleAnim.SetBool("hovered", show);
        }

        /// <summary>
        /// Checks the dialogue instance to see if there's conversations to show.
        /// </summary>
        public void CheckDialogue()
        {
            if (!dialogInstance) return;
            
            if (_bubbleAnim == null) _bubbleAnim = GetComponent<Animator>();
            
            hasNewDialog = dialogInstance.HasNewDialogue();

            _bubbleAnim.SetBool("hasDialogue", hasNewDialog);
            SetSprite();
        }


        /// <summary>
        /// Sets the bubble sprite that best represents the given character. (quest giver, merchant, etc)
        /// </summary>
        void SetSprite ()
        {
            if (!dialogInstance) return;
            
            // Get components 
            Inventory shop = focusedCrew.GetComponent<Inventory>();
            ShipBroker shipBroker = focusedCrew.GetComponent<ShipBroker>();

            bubble.sprite = hasDialog;

            // Set sprite to show quest giver 
            if (dialogInstance.HasQuestToGive())
            {
                bubble.sprite = hasQuest;
                return;
            }
            
            // show merchant
            if (shop) bubble.sprite = merchant;

            // show ship broker
            if (shipBroker) bubble.sprite = subBroker;
        }

        public override void End ()
        {
            End(1.5f);
        }
    }
}