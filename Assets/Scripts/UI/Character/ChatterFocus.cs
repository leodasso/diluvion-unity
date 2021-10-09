using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DUI
{

    /// <summary>
    /// The thing meant to draw player's attention to it. Pulls out of center right to bring focus to a new crew chatter.
    /// </summary>
    public class ChatterFocus : MonoBehaviour
    {

        public Image bg1;
        public CanvasGroup speechBubble;
        public TextMeshProUGUI inputText;
        public Text nameText;
        public Text chatter;
        public Image portraitImage;

        CanvasGroup canvasGroup;
        float x = 0;
        RectTransform rectTransform;
        Rect rect;
        Animator mainAnimator;
        float alpha;

        // Use this for initialization
        void Start()
        {

            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            alpha = 0;

            rectTransform = GetComponent<RectTransform>();
            mainAnimator = GetComponent<Animator>();
            rect = rectTransform.rect;
            End();
        }

        public void SetChatter(ChatterInfo info)
        {
            SetChatter(info.convo, info.dialogInstance);
        }

        public void SetChatter(Convo convo, Dialogue dInstance)
        {

            Debug.Log("setting chatter");

            inputText.text = SpiderWeb.Controls.InputMappingName("side view");

            // Set the correct color
            //if (convo.urgent) bg1.color = Color.red;
            //else 
            bg1.color = Color.black;

            // speech bubble on or off
            if (convo.speeches.Count > 0) speechBubble.alpha = 1;
            else speechBubble.alpha = 0;

            alpha = 1;

            // Animation for oscillating size of textbox
            Animator animator = GetComponentInChildren<Animator>();
            animator.speed = 0;
            //if (convo.urgent) animator.speed = 1;

            nameText.text = dInstance.MyCharacter().GetLocalizedName();
            chatter.text = convo.LocalizedChatter();
            if (dInstance.MyCharacter().GetAppearance())
            {
                portraitImage.sprite = dInstance.MyCharacter().GetAppearance().chatterPortrait;
            }
            else portraitImage.color = Color.clear;

            x = 0;

            mainAnimator.SetTrigger("begin");
        }

        public void End()
        {
            x = rect.width + 600;
            alpha = 0;
        }

        // Update is called once per frame
        void Update()
        {

            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, alpha, Time.unscaledDeltaTime * 8);

            rect = rectTransform.rect;
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, new Vector2(x, rectTransform.anchoredPosition.y), Time.deltaTime * 8);
        }
    }
}