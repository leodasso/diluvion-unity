using System.Collections;
using System.Collections.Generic;
using Diluvion.Ships;
using UnityEngine;
using UnityEngine.UI;
using Diluvion.Sonar;
using Sirenix.OdinInspector;
using System.Diagnostics;
using System.Linq;
using Diluvion;

namespace DUI
{
    public class SignatureHUD : DUIPanel
    {
        [Tooltip("The signature is explosed to the player")]
        public bool visible;
        [Tooltip("The signature is focused by the player, showing more info.")]
        public bool focused;

        [Tooltip("This signature is near enough the reticule to show the lead.")]
        public bool nearReticule;

        public Color neutral = Color.white;
        public Color friendly = Color.green;
        public Color hostile = Color.red;

        [Range(0, 1)]
        public float power;
        public RectTransform leadIcon;
        public GameObject leadGroup;

        public UIMeshLine leadLine;
        public Image leadIconImage;
       
        public List<Image> signatureIcons = new List<Image>();
        public Image hpGuage;
        public CanvasGroup hpGroup;

        [AssetsOnly]
        public Sprite defaultSigSprite;

        IAlignable _alignable;

        [Tooltip("Value is a percentage of the width of the screen. If the signature hud is within this value to the player's reticle, it will show the lead.")]
        [Range(0, 1)] public float leadArea = .3f;

        [Space, ReadOnly]
        public Listener listener;
        
        [ReadOnly]
        public SonarStats target;

        [ReadOnly]
        public bool showHp;

        RectTransform _rect;
        Animator _anim;
        /// <summary>
        /// The signatures ordered by the power they unlock at
        /// </summary>
        List<Signature> orderedSignatures = new List<Signature>();

        IDamageable _damageableTarget;

        WeaponSystem _lastFired;
        

        /// <summary>
        /// Creates & returns a HUD element for the given sonar stats.
        /// </summary>
        public static SignatureHUD CreateHUD(SonarStats ss, Listener l)
        {
            SignatureHUD newSig = UIManager.Create(UIManager.Get().sonarContact as SignatureHUD);
            newSig.listener = l;
            newSig.target = ss;
            newSig.visible = false;
            newSig.CreateOrderedSigList();
            newSig._damageableTarget = ss.GetComponent<IDamageable>();
            newSig.showHp = newSig._damageableTarget != null;
            newSig._alignable = ss.GetComponent<IAlignable>();
            
            SignatureHudManager.RegisterInstance(newSig);
           
            
            // get the weapon system which was most recently fired and also enables leading
            newSig._lastFired = newSig.ListenerBridge().LastFiredLeadingWeaponSystem();
            
            return newSig;
        }

        /// <summary>
        /// Create a list of the signatures of the target in order
        /// </summary>
        void CreateOrderedSigList()
        {
            if (!target) return;
            orderedSignatures = target.OrderedSignatures();
        }

        // Use this for initialization
        protected override void Start()
        {
            base.Start();

            _anim = GetComponent<Animator>();
            _rect = GetComponent<RectTransform>();

            SnapAlpha(0);
            alpha = 0;
        }

        
        // Update is called once per frame
        Vector2 _diff;
        Color _leadColor;
        protected override void Update()
        {
            base.Update();

            if (!listener || !target || !target.isActiveAndEnabled)
            {
                alpha = 0;
                return;
            }

            // Check visibility of the contact. If it's not visible, make this element invisible and return.
            visible = listener.ContactExposed(target);

            // If this contact isn't exposed, don't take any action.
            if (!visible)
            {
                alpha = 0;
                return;
            }
            
            alpha = 1;
            
            // Show or hide HP guage
            hpGroup.alpha = showHp ? 1 : 0;

            if (showHp && _damageableTarget != null) 
                hpGuage.fillAmount = Mathf.Lerp(hpGuage.fillAmount, _damageableTarget.NormalizedHp(), Time.deltaTime * 10);

            // Only display lead for damageable targets
            if (_damageableTarget != null)
            {
                // Check if near the player's aim reticle. This determines if we will show the lead or not.
                float pixelDist = Screen.width * leadArea / 2;
                nearReticule = DUIReticule.DistFromAim(transform.position) < pixelDist;

                if (_lastFired != null)
                {
                    // Set the color of the lead reticule based on aim
                    _leadColor = Vector3.Distance(listener.transform.position, target.transform.position) < _lastFired.Range() 
                        ? InRangeColor() : Color.gray;
                }

                leadIconImage.color = _leadColor;
                leadLine.color = _leadColor;
            }
            else nearReticule = false;
            

            // Check if this particular instance is the focus
            focused = SignatureHudManager.IsFocus(this);
            
            // set animator stats
            _anim.SetBool("focused", focused);
            
            power = listener.PowerOfSignature(target);

            // Display the signatures of this sonar contact
            for (int i = 0; i < signatureIcons.Count; i++)
            {
                // for icons beyond the signature count of the target, make them invisible
                if (i >= orderedSignatures.Count)
                {
                    signatureIcons[i].color = Color.clear;
                    continue;
                }
                
                // for icons that haven't been discovered yet, set them as the default sprite.
                if (orderedSignatures[i].revealStrengh > power)
                {
                    signatureIcons[i].sprite = defaultSigSprite;
                    signatureIcons[i].color = new Color(0, 0, 0, .5f);
                    continue;
                }
                
                // display the signature
                signatureIcons[i].sprite = orderedSignatures[i].icon;
                signatureIcons[i].color = Color.white;
            }
        }

        Color InRangeColor()
        {
            if (_alignable == null) return neutral;

            if (_alignable.getAlignment() == AlignmentToPlayer.Friendly) return friendly;
            if (_alignable.getAlignment() == AlignmentToPlayer.Hostile) return hostile;
            return neutral;
        }

        public float DistFromAim()
        {
            return DUIReticule.DistFromAim(transform.position);
        }
        
        
        Bridge _bridge;
        Bridge ListenerBridge()
        {
            if (_bridge) return _bridge;
            if (!listener) return null;
            _bridge = listener.GetComponent<Bridge>();

            return _bridge;
        }

        Vector3 _leadPos;
        Vector3 _screenSpaceLeadPos;
        void UpdateLead()
        {
            if (!listener) return;
            
            // get the weapon system which was most recently fired and also enables leading
            _lastFired = ListenerBridge().LastFiredLeadingWeaponSystem();
            
            // check if the weapon system shows a lead
            if (!_lastFired.module.showLeadGUI)
            {
                SetLeadActive(false);
                return;
            }
            
            SetLeadActive(true);

            // Get the lead position for the target of the last fired weapon system
            _leadPos = _lastFired.LeadPosition(target);
            
            // convert that into a UI position
            _screenSpaceLeadPos = FollowTransform(_leadPos, 40, Camera.main);
            leadIcon.position = Vector3.Lerp(leadIcon.position, _screenSpaceLeadPos, Time.deltaTime * 10);
        }

        void SetLeadActive(bool isActive)
        {
            if (leadGroup.activeInHierarchy == isActive) return;
            leadGroup.SetActive(isActive);
        }

        void LateUpdate()
        {
            if (!visible || !target || !listener)
            {
                SetLeadActive(false);
                return;
            }
            
            // Update the lead position
            if (nearReticule) UpdateLead();
            else SetLeadActive(false);
            
            if (listener && target && Camera.main)
                _rect.transform.position = FollowTransform(target.transform.position, 30, Camera.main);
        }
    }
}