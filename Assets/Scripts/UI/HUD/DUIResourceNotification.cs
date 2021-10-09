using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;

namespace DUI
{
    public class DUIResourceNotification : DUIView
    {
        public float duration = 5;
        public TextMeshProUGUI description;
        public Image arrow;
        public Image bgImage;
        public NicerOutline outline;

        public void Init(Loot.DItem item, Color color, string text)
        {
            bgImage.sprite = item.icon;
            bgImage.color = item.myColor;

            SetColor(color);

            description.text = text;

            StartCoroutine(WaitAndEnd());
        }

        void SetColor(Color color)
        {
            description.color = color;
            arrow.color = color;
            outline.effectColor = color;
        }

        IEnumerator WaitAndEnd()
        {
            yield return new WaitForSeconds(duration);
            BackToTarget();
        }

        protected override void FadeoutComplete()
        {
            base.FadeoutComplete();
            Destroy(gameObject);
        }
    }
}