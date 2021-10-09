using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Diluvion.Sonar;

namespace DUI
{
    /// <summary>
    /// The little icon to represent a single signature of a sonar stats UI.
    /// </summary>
    [ExecuteInEditMode]
    public class SignatureIcon : MonoBehaviour
    {
        public Image iconImage;
        public Color dotExposedColor = Color.green;
        public float radius = 25;
        [Range(0, 1)]
        public float value = 1;

        Signature _sig;
        Vector3 _scale = Vector3.zero;
        RectTransform _icon;
        RectTransform _rectTrans;
        float _dotAlpha = 1;
        float _angle;
        float _r;

        /// <summary>
        /// Alpha of the dot when the signature is exposed.
        /// </summary>
        float dotExposedAlpha = 0;

        // Use this for initialization
        void Start()
        {
            _rectTrans = GetComponent<RectTransform>();
            _icon = iconImage.GetComponent<RectTransform>();
            _icon.localScale = Vector3.zero;
        }

        // Update is called once per frame
        void Update()
        {
            float t = Time.unscaledDeltaTime * (5 * value + 2);

            if (_sig) value = _sig.revealStrengh;
            value = Mathf.Clamp(value, 0, .95f);

            float newAngle = -value * Mathf.PI * 2;
            newAngle += Mathf.PI / 2;

            _angle = Mathf.Lerp(_angle, newAngle, t);
            _r = Mathf.Lerp(_r, radius, t);

            Vector2 pos = _r * new Vector2(Mathf.Cos(_angle), Mathf.Sin(_angle));
            _rectTrans.anchoredPosition = pos;

            transform.localScale = Vector3.Lerp(transform.localScale, _scale, t);
        }

        /// <summary>
        /// Show or hide this signature icon.
        /// </summary>
        void Show(bool showing)
        {
            if (showing)  _scale = Vector3.one;
             
            else  _scale = Vector3.zero;
        }

        /// <summary>
        /// Tell the listener's power. Hides or shows the element based on that.
        /// </summary>
        public void SetPower(float newPower)
        {
            if (!_sig) return;
            if (newPower >= _sig.revealStrengh) Show(true);
            else Show(false);
        }

        public void SetSignature(Signature s)
        {
            _sig = s;
            if (_sig.icon) iconImage.sprite = _sig.icon;
        }
    }
}