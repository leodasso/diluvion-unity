using UnityEngine;
using TMPro;
using System.Collections;
using Diluvion;

namespace DUI
{

    public class DUIMoneyPanel : DUIPanel
    {
        
        public TextMeshProUGUI goldAmtText;
        
        static DUIMoneyPanel _instance;
        static int _oldGold;
        static int _newGold;
        static bool _live;
        static Inventory _playerInv;
        RectTransform _rectTransform;
        

        /// <summary>
        /// Brings up the money panel to show a change in the player's money amount
        /// </summary>
        /// <param name="oldGold">the player's gold qty before the change</param>
        /// <param name="newGold">player's gold qty after the change</param>
        public static void ShowChange(int oldGold, int newGold)
        {
            Debug.Log("Showing money panel");
            _live = false;
            
            _oldGold = oldGold;
            _newGold = newGold;

            Instance().StartCoroutine(_instance.ShowMoneyChange(1, 1));
        }

        /// <summary>
        /// Brings up money panel to show current money
        /// </summary>
        public static void Show()
        {
            _live = true;
            Instance().transform.SetAsLastSibling();
            Instance().alpha = 1;
        }


        static DUIMoneyPanel Instance()
        {
            if (_instance) return _instance;
            
            _instance = UIManager.Create(UIManager.Get().moneyPanel as DUIMoneyPanel);
            return _instance;
        }
        
        public static void Hide()
        {
            _live = false;
            if (!_instance) return;
            _instance.alpha = 0;
        }

        protected override void Start()
        {
            base.Start();
            alpha = 1;
        }


        void UpdateText(float newAmount)
        {
            goldAmtText.text = Mathf.Round(newAmount).ToString();
        }

        protected override void Update()
        {
            base.Update();
            
            if (!_live) return;
            if (PlayerManager.PlayerInventory())
                UpdateText(PlayerManager.PlayerInventory().gold);
        }

        IEnumerator ShowMoneyChange(float delay, float time)
        {
            
            Debug.Log("Money change coroutine started");
            UpdateText(_oldGold);
            
            // make visible
            alpha = 1;
            _instance.transform.SetAsLastSibling();
            
            // wait for intro delay before animating money change
            yield return new WaitForSecondsRealtime(delay);
            
            float t = 0;
            int currentDisplayedGoldQty = _oldGold;

            //get amount of gold the player has - this will be the value that displays at the end of the animation
            int newGold = PlayerManager.pBridge.GetInventory().gold;

            while ( t < 1)
            {
                // lerp and round to animate the gold qty 'climbing'
                currentDisplayedGoldQty = (int)Mathf.Lerp(_oldGold, _newGold, t);
                //Display it
                UpdateText(currentDisplayedGoldQty);
                
                t += Time.unscaledDeltaTime / time;
                yield return null;
            }

            UpdateText(newGold);            
            yield return new WaitForSecondsRealtime(2);
            Hide();
        }

    }
}