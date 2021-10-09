using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Diluvion;

namespace DUI
{
    public class HPPanel : DUIPanel
    {
        public RectTransform hpRect;
        public TextMeshProUGUI healthText;
        public Image healthIcon;
        public Image healthIcon2;

        Vector2 _defaultHpRectSize;
        float _curHealth;
        float _oldHealth;
        Hull _hull;

        // Use this for initialization
        protected override void Start()
        {
            _defaultHpRectSize = hpRect.sizeDelta;
            base.Start();
            alpha = 0;
        }

        public void InitHullPanel(Hull h)
        {
            _hull = h;
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            hpRect.sizeDelta = Vector2.Lerp(hpRect.sizeDelta, _defaultHpRectSize, Time.deltaTime * 5);

            int HP = Mathf.CeilToInt(_hull.CurrentHP());
            HP = Mathf.Clamp(HP, 0, 100000);
            
            
            healthText.text = HP.ToString();

            _curHealth = _hull.CurrentHP() / _hull.maxHealth;

            //Change fill amount
            healthIcon.fillAmount = _curHealth;

            //animate color of health bar to turn red when taking damage
            if (_curHealth < _oldHealth) WiggleHPIndicator(Color.red);

            if (_curHealth > _oldHealth) WiggleHPIndicator(Color.green);

            //lerp to return to white over time
            healthIcon.color = Color.Lerp(healthIcon.color, Color.white, Time.deltaTime * 1);

            _oldHealth = _curHealth;
        }

        /// <summary>
        /// Set the HP indicator to visible and at the given color.
        /// </summary>
        void WiggleHPIndicator(Color color)
        {
            alpha = 1;
            group.alpha = 1;
            hpRect.sizeDelta = _defaultHpRectSize * 3;
            healthIcon.color = color;
            StartCoroutine(FightingHealthBar(_curHealth));
        }

        IEnumerator FightingHealthBar(float newHealth)
        {
            yield return new WaitForSeconds(3);

            float timer = 0;
            while (timer < 2)
            {
                healthIcon2.fillAmount = Mathf.Lerp(healthIcon2.fillAmount, newHealth, Time.deltaTime * 5);
                timer += Time.deltaTime;
                yield return null;
            }

            // Make HP invisible again if the health is good
            if (newHealth > .75f) alpha = 0;
        }
    }
}