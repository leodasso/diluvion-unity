using UnityEngine;
using System.Collections;
using TMPro;

namespace Diluvion.Ships
{

    /// <summary>
    /// A level attached to a station, used for adding or removing crew.
    /// </summary>
    public class StationLever : InteriorElement, IClickable
    {

        public static GameObject leverTextPrefab;
        public enum LeverType { addSailor, removeSailor };
        public LeverType leverType;
        public Sprite normal;
        public Sprite hover;
        public Sprite clicked;

        TextMeshPro controlText;
        SpriteRenderer spriteRenderer;
        Station station;

        // Use this for initialization
        protected override void Start()
        {

            base.Start();

            station = GetComponentInParent<Station>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = normal;

            // Instantiate lever text
            if (LeverTextPrefab() == null) return;
            GameObject leverTextInstance = Instantiate(LeverTextPrefab(), transform.position, transform.rotation) as GameObject;
            leverTextInstance.transform.SetParent(transform.parent, true);
            controlText = leverTextInstance.GetComponent<TextMeshPro>();

            ShowControl(false);
        }

        GameObject LeverTextPrefab()
        {
            if (leverTextPrefab != null) return leverTextPrefab;

            leverTextPrefab = Resources.Load("lever text") as GameObject;
            return leverTextPrefab;
        }

        public void SetControltext(string text)
        {
            if (controlText == null) return;
            controlText.text = text;
        }

        public void ShowControl(bool show)
        {
            if (!controlText) return;
            controlText.gameObject.SetActive(show);
        }

        bool IsActive()
        {
            if (!station) return false;
            if (!station.operational) return false;
            return true;
        }

        public void OnPointerEnter()
        {
            if (!IsActive()) return;
            station.hovered = true;
            //station.ShowStatsPreview();
            spriteRenderer.sprite = hover;
        }

        public void OnPointerExit()
        {
            if (!IsActive()) return;
            station.hovered = false;
            //station.HideStatsPreview();
            spriteRenderer.sprite = normal;
        }

        public void OnClick()
        {
            if (!IsActive()) return;
            spriteRenderer.sprite = clicked;
        }

        public void OnRelease()
        {
            if (!IsActive()) return;
            //if (leverType == LeverType.addSailor) station.IncreaseCrewCount();
            //else station.DecreaseCrewCount();
            spriteRenderer.sprite = normal;
        }

        public void OnFocus() { }
        public void OnDefocus() { }

        public void Click()
        {
            StartCoroutine(LeverClick());
        }

        IEnumerator LeverClick()
        {
            OnClick();
            yield return new WaitForSeconds(.2f);
            OnRelease();
        }
    }
}