using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Quests;
using SpiderWeb;

namespace Diluvion
{
    /// <summary>
    /// An interior element such as a door, chest, etc which can be locked depending on quest status.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(ColliderMatchSprite))]
    [RequireComponent(typeof(SpriteRenderer))]
    
    public class InteriorSwitch : QuestActor, IClickable
    {
        [TitleGroup("Switch", indent:true)]
        [AssetsOnly, ValidateInput("HasSprite", "Interior switch needs a sprite set.", InfoMessageType.Warning)]
        public SpriteSet spriteSet;

        [TitleGroup("Switch")]
        [ToggleLeft, Tooltip("Is the switch on or off")]
        public bool on;

        [TitleGroup("Switch")]
        [ToggleLeft, InfoBox("If locked, you can set a quest status above that will unlock it.")]
        public bool locked;

        bool HasSprite (SpriteSet set)    {return set != null;}

        Color _initColor;

        protected Collider2D myCol;
        protected ColliderMatchSprite colMatch;
        protected Color lockedColor = Color.grey;
        protected SpriteRenderer spriteRenderer;

        protected override void Tick()
        {
            if (!locked) return;
            base.Tick();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            locked = false;
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            locked = true;
        }

        // Use this for initialization
        protected virtual void Start ()
        {
            gameObject.layer = LayerMask.NameToLayer("Interior");
            spriteRenderer = GetComponent<SpriteRenderer>();
            _initColor = spriteRenderer.color;

            if (spriteSet) spriteRenderer.sprite = SwitchSprite();

            myCol = GetComponent<Collider2D>();

            BoxCollider2D boxCol = GetComponent<BoxCollider2D>();

            colMatch = GO.MakeComponent<ColliderMatchSprite>(gameObject);
            colMatch.spriteRenderer = spriteRenderer;
            colMatch.col = boxCol;
        }

        // Update is called once per frame
        void Update ()
        {
            if (!spriteRenderer) return;
            if (locked) spriteRenderer.color = lockedColor;
            else spriteRenderer.color = _initColor;
        }


        public virtual void OnClick () {}

        public virtual void OnRelease () { }

        public virtual void OnPointerEnter ()
        {
            if (locked) QuestManager.Tick();
            
            if (!locked)
                SetSprite(spriteSet.hover);
        }

        public virtual void OnPointerExit ()
        {
            if (!locked)
                SetSprite(SwitchSprite());
        }

        public virtual void OnFocus () { }

        public virtual void OnDefocus () { }

        /// <summary>
        /// Returns sprite set 'normal' for on, 'empty' for off
        /// </summary>
        protected Sprite SwitchSprite()
        {
            if (!spriteSet) return null;
            if (on) return spriteSet.normal;
            else return spriteSet.empty;
        }

        /// <summary>
        /// Efficiently change sprite to the given sprite
        /// </summary>
        protected void SetSprite (Sprite newSprite)
        {
            if (!spriteRenderer) return;
            if (spriteRenderer.sprite != newSprite)
                spriteRenderer.sprite = newSprite;
        }
    }
}