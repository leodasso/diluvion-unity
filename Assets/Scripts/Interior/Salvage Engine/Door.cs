using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using SpiderWeb;

namespace Diluvion
{
    /// <summary>
    /// Doors can be placed in interiors, and will lead to another room when opened.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D)), 
        RequireComponent(typeof(SpriteRenderer)), 
        RequireComponent(typeof(ColliderMatchSprite))]

    public class Door : InteriorSwitch
    {
        [ReadOnly]
        public Room parentRoom;

        public bool requireBoardingParty;

        public delegate void DoorAction();
        public DoorAction onOpen;

        float _cooldown = 0;

        protected override void Start ()
        {
            base.Start();
            on = true;

            parentRoom = GetComponentInParent<Room>();

            SetSprite(SwitchSprite());
        }

        void Update()
        {
            if (_cooldown > 0) _cooldown -= Time.unscaledDeltaTime;
        }

        public override void OnClick ()
        {
            base.OnClick();

            if (locked) return;
            spriteRenderer.sprite = spriteSet.empty;
        }

        IEnumerator OpenDoor()
        {
            if (locked)
            {
                Debug.Log("Can't open door " + name + " because it's locked.");
                yield break;
            }

            if (_cooldown > 0)
            {
                Debug.Log("Door is still cooling down. Time remaining: " + _cooldown);
                yield break;
            }
            _cooldown = 5;

            // run the delegate for onOpen
            onOpen?.Invoke();
            
            // Play audio
            SpiderSound.MakeSound("Play_Door_Open", gameObject);

            yield return new WaitForSeconds(.2f);

            parentRoom.ShowNextRoom();
        }

        public void TurnDoorOff()
        {
            GetComponent<Collider2D>().enabled = false;
            on = false;
            SetSprite(spriteSet.empty);
        }

        public override void OnRelease ()
        {
            base.OnRelease();
            StartCoroutine(OpenDoor());
        }

        bool HasBoardingParty()
        {
            return PlayerManager.BoardingParty().Count > 0;
        }
    }
}