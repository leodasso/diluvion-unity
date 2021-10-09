using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Loot;

namespace DUI
{

    public class DUIDebugInventory : DUIDebug
    {
        public Button textButtonReference;
        public Transform scrollParent;
        int previousGold;
        List<GameObject> buttonList = new List<GameObject>();


        protected override void Start()
        {
            BuildItemButtons();
        }

        //TODO Wont take Typing for some reason
        public void FilterObjects(string name)
        {
            foreach (GameObject go in buttonList)
                if (go.name.Contains(name) || string.IsNullOrEmpty(name))
                    go.SetActive(true);
                else
                    go.SetActive(true);
        }


        public void BuildItemButtons()
        {
            buttonList.Clear();

            foreach (DItem item in ItemsGlobal.Get().allItems)
                buttonList.Add(AddItemButton(item, item.name));
        }

        Text buttonText;
        public GameObject AddItemButton(DItem item, string itemName)
        {
            Button buttonInstance = Instantiate<Button>(textButtonReference);
            buttonInstance.transform.SetParent(scrollParent);
            buttonInstance.GetComponent<Button>().onClick.AddListener(delegate { AddItem(item); });
            buttonInstance.GetComponentInChildren<Text>().text = itemName;
            return buttonInstance.gameObject;

        }

        public void AddItem(Loot.DItem item)
        {
            CheatMan().AddItem(item);
        }

        public void AddGold(int number)
        {
            previousGold = CheatManager.Get().GetGold();
            CheatManager.Get().SetGold(previousGold + number);
        }

        public void AddScrap(int number)
        {
            CheatManager.Get().AddScrap(number);
        }

        public void AddFood(int number)
        {
            CheatManager.Get().AddFood(number);
        }

        public void AddTorpedo(int number)
        {
            CheatManager.Get().AddTorpedo(number);
        }

        public void AddReinforced(int number)
        {
            CheatManager.Get().AddReinforcedPlates(number);
        }

        public void AddEngineParts(int number)
        {
            CheatManager.Get().AddEngineParts(number);
        }

        public void RefillAir()
        {
            CheatManager.Get().RefillAir();
        }

        void OnDestroy()
        {
            ClearButtonList();
        }


        void ClearButtonList()
        {
            foreach (GameObject go in buttonList)
                Destroy(go);
            buttonList.Clear();
        }
    }
}