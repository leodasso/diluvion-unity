using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Diluvion;
using Sirenix.OdinInspector;
using TMPro;
using SpiderWeb;
using String = System.String;

namespace DUI
{

    public class DUIChatterItem : DUIPanel
    {
        [BoxGroup("local refs")]
        public CanvasGroup portraitGroup;

        [BoxGroup("local refs")]
        public Image bg;

        [BoxGroup("local refs")]
        public Image portrait;

        [BoxGroup("local refs")]
        public TextMeshProUGUI nameText;

        [BoxGroup("local refs")]
        public TalkyText text;

        [BoxGroup("local refs")] 
        public CanvasGroup questIcon;

        public float delay = 1;
        public float entrySpeed = 1;

        [MinValue(0)]
        [OnValueChanged("ApplyHeight")]
        public int extraLines;

        [OnValueChanged("ApplyHeight")]
        public float heightPerLine = 25;
        [OnValueChanged("ApplyHeight")]
        public float defaultHeight = 60;

        [ReadOnly]
        public ChatterInfo chatterInfo;

        RectTransform _rect;
        float _y;

        bool _showingCharacter = true;
        bool _showCharCache = true;

        List<DUIChatterItem> _siblings = new List<DUIChatterItem>();

        protected override void Start ()
        {
            base.Start();
            _rect = GetComponent<RectTransform>();
            alpha = 0;
            group.alpha = 0;
            
            text.Clear();

            StartCoroutine(FadeIn(delay, entrySpeed));
        }

        void GetSiblings ()
        {
            foreach (DUIChatterItem item in transform.parent.GetComponentsInChildren<DUIChatterItem>())
            {
                if (item == this) continue;
                _siblings.Add(item);
            }

        }

        void ApplyHeight()
        {
            Vector2 size = GetComponent<RectTransform>().sizeDelta;
            GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, TotalHeight());
        }

        float TotalHeight()
        {
            return defaultHeight + extraLines * heightPerLine;
        }

        IEnumerator FadeIn (float delay, float speed)
        {
            float d = 0;
            while (d < delay)
            {
                d += Time.unscaledDeltaTime;
                yield return null;
            }

            float progress = 0;
            while (progress < 1)
            {
                alpha = progress;
                progress += Time.unscaledDeltaTime * speed;
                yield return null;
            }

            alpha = 1;

            GetSiblings();
            foreach (DUIChatterItem chatter in _siblings)
                chatter.RefreshCharacter();
        }

        void RefreshCharacter ()
        {
            _showCharCache = _showingCharacter;
        }

        protected override void Update ()
        {
            base.Update();

            _rect.anchoredPosition = Vector2.Lerp(_rect.anchoredPosition,
                new Vector2(_rect.anchoredPosition.x, _y),
                Time.unscaledDeltaTime * 10);

            _rect.sizeDelta = Vector2.Lerp(_rect.sizeDelta, new Vector2(_rect.sizeDelta.x, TotalHeight()), Time.unscaledDeltaTime * 3);

            float portraitAlpha = 0;
            if (_showCharCache) portraitAlpha = 1;

            portraitGroup.alpha = Mathf.Lerp(portraitGroup.alpha, portraitAlpha, Time.unscaledDeltaTime * 12);
        }

        
        public void Display (ChatterInfo info)
        {
            chatterInfo = info;

            if (info.cType == ChatterType.radio)
            {
                // TODO radio stuff
            }

            else if (info.cType == ChatterType.crew)
            {

                if (chatterInfo.subject)
                {
                    portrait.sprite = chatterInfo.subject.GetAppearance().chatterPortrait;
                    nameText.text = chatterInfo.subject.GetLocalizedName();
                }
                else nameText.text = "-----";
            }
            
            bool hasQuest = info.convo.GivesQuest();
            questIcon.alpha = hasQuest ? 1 : 0;

            // add the chatter to the radio
            text.inputText = chatterInfo.convo.LocalizedChatter();
            
            if (chatterInfo.convo.showFullConvoInRadio)
            {
                foreach (Speech s in chatterInfo.convo.speeches)
                {
                    extraLines++;
                    text.inputText += "\n";
                    text.inputText += String.Join("\n", s.FormattedText());
                }
            }
            

            if (info.convo) PlayChatterSound(info.convo);
        }

        /// <summary>
        /// Sets the index of this chatter in the chatterbox
        /// </summary>
        /// <param name="needCharacter">Does this element need to show character portrait/name?</param>
        public void SetPosition (float yPos, bool needCharacter = false)
        {
            _y = yPos;

            _showingCharacter = needCharacter;

            /*
            if (index == 0)
                bg.sprite = highlightBG;
            else
                bg.sprite = normalBG;
                */
        }

        public float MyHeight()
        {
            if (!_rect) _rect = GetComponent<RectTransform>();
            return _rect.rect.height;
        }


        void PlayChatterSound (Convo convo)
        {
            if (convo.GivesQuest())
            {
                if (convo.speeches.Count < 1)
                    SpiderSound.MakeSound("Play_Urgent_Empty", gameObject);
                else
                    SpiderSound.MakeSound("Play_Urgent_Message", gameObject);
            }
            else
            {
                if (convo.speeches.Count < 1)
                    SpiderSound.MakeSound("Play_Chatter_Empty", gameObject);
                else
                    SpiderSound.MakeSound("Play_Chatter_Message", gameObject);

            }
        }
    }
}