using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SpiderWeb;


namespace Diluvion
{

    public class GraveStone : MonoBehaviour
    {

        public PopupObject eulogyPopup;
        [TextArea(3, 6)]
        public string eulogy;
        public string eulogyTitle;

        float alpha = 1;
        SpriteRenderer sprite;
        List<string> names = new List<string>();

        public void Init(Character crew, bool visible)
        {

            sprite = GetComponentInChildren<SpriteRenderer>();

            sprite.enabled = visible;

            // Set up string
            /*
            eulogy = Localization.GetFromLocLibrary("popup_eulogy_body", "eulogy body");
            eulogyTitle = Localization.GetFromLocLibrary("popup_eulogy_title", "eulogy body");
            eulogy = eulogy.Replace("[n]", crew.GetLocalizedName());
            eulogyTitle = eulogyTitle.Replace("[n]", crew.GetLocalizedName());
            */

            names.Add(crew.GetLocalizedName());
        }

        public void OnClick()
        {

            eulogyPopup.CreateUI(names);
            StartCoroutine(FadeOut());
        }

        IEnumerator FadeOut()
        {

            while (alpha > 0)
            {

                alpha -= Time.deltaTime;
                sprite.color = new Color(1, 1, 1, alpha);

                yield return new WaitForEndOfFrame();
            }

            Destroy(gameObject, 1);
            yield break;
        }
    }
}