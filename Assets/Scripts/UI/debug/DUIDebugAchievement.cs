using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Diluvion.Achievements;
struct ButtonAchievementPair
{
    public CanvasGroup group;
    public SpiderAchievement achievement;

    public ButtonAchievementPair(CanvasGroup c, SpiderAchievement sao)
    {
        group = c;
        achievement = sao;
    }
}

namespace DUI
{

    public class DUIDebugAchievement : DUIDebug
    {
        public Button textButtonReference;
        public Transform scrollParent;
        List<ButtonAchievementPair> addButtonList = new List<ButtonAchievementPair>();
        List<ButtonAchievementPair> removeButtonList = new List<ButtonAchievementPair>();

        bool clicked = false;

        protected override void Start()
        {
            BuildAchievementButtons();
            SpiderAchievementHandler.Get().checkedAchievement += RefreshButtons;
            RefreshAchievements();
        }

        public void DoAchievement(SpiderAchievement ach)
        {
            if (clicked) return;
            clicked = true;
            ach.IncreaseProgress(1);
            RefreshAchievements();
        }

        public void ClearAchievement(SpiderAchievement ach)
        {
            if (clicked) return;
            clicked = true;
            SpiderAchievementHandler.Get().ClearAchievement(ach);
            RefreshAchievements();
        }

        public void ClearAchievements()
        {
            if (clicked) return;
            clicked = true;
            SpiderAchievementHandler.Get().ClearAchievements();
            RefreshAchievements();
        }

        public void RefreshAchievements()
        {
            SpiderAchievementHandler.Get().RefreshAchievements();
            clicked = false;
        }

        public void RefreshButtons()
        {
            Debug.Log("refereshing buttons");
            foreach (ButtonAchievementPair bap in addButtonList)
            {
                if (bap.achievement.completed)
                    bap.group.alpha = 0;
                else
                    bap.group.alpha = 1;

                bap.group.interactable = !bap.achievement.completed;
            }

            foreach (ButtonAchievementPair bap in removeButtonList)
            {
                if (bap.achievement.completed)
                    bap.group.alpha = 1;
                else
                    bap.group.alpha = 0;

                bap.group.interactable = bap.achievement.completed;
            }
            clicked = false;
        }

        public void BuildAchievementButtons()
        {
            addButtonList.Clear();
            removeButtonList.Clear();
            foreach (SpiderAchievement sa in SpiderAchievementHandler.Get().AllAchievements())
            {
                if (sa == null) continue;
                addButtonList.Add(new ButtonAchievementPair(AddAchievementButton(sa, sa.name), sa));
                removeButtonList.Add(new ButtonAchievementPair(AddClearButton(sa, sa.name), sa));
            }
        }



        Text buttonText;
        public CanvasGroup AddAchievementButton(SpiderAchievement ach, string itemName)
        {
            Button buttonInstance = Instantiate<Button>(textButtonReference);
            buttonInstance.transform.SetParent(scrollParent);
            buttonInstance.GetComponent<Button>().onClick.AddListener(delegate { DoAchievement(ach); });
            buttonInstance.GetComponentInChildren<Text>().text = itemName;

            return buttonInstance.GetComponent<CanvasGroup>();


        }

        public CanvasGroup AddClearButton(SpiderAchievement ach, string itemName)
        {
            Button buttonInstance = Instantiate<Button>(textButtonReference);
            buttonInstance.transform.SetParent(scrollParent);
            var colors = buttonInstance.colors;
            colors.normalColor = Color.red;
            buttonInstance.colors = colors;
            buttonInstance.GetComponent<Button>().onClick.AddListener(delegate { ClearAchievement(ach); });
            buttonInstance.GetComponentInChildren<Text>().text = itemName;

            return buttonInstance.GetComponent<CanvasGroup>();
        }
    }
}