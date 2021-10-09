using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Diluvion;
using TMPro;

namespace DUI
{
    public class CrewSelect : DUIView
    {
        public RectTransform layoutGroup;
        public CharacterBox characterBoxPrefab;
        public TextMeshProUGUI titleText;

        public delegate void CrewSelectDel (Character crew);
        public CrewSelectDel crewSelect;

        bool endIfEmpty;
        bool endAfterSelect;
        bool onlyOneTime;
        bool used;
        List<Character> crewList;
        string actionName;

        /// <summary>
        /// Creates a new crew selection panel, and returns the instance.
        /// </summary>
        /// <param name="newActionName">the name that appears on the buttons when hovered</param>
        /// <param name="closeAfterSelect">close the panel after a selection is made.</param>
        /// <param name="oneTime">only allow one crew selection.</param>
        /// <returns></returns>
        public static CrewSelect Create (List<Character> characters, string newActionName, bool closeAfterSelect = false, 
            bool oneTime = false, bool willEndIfEmpty = true)
        {
            CrewSelect panel = UIManager.Create(UIManager.Get().generalCrewSelect as CrewSelect);
            panel.Init(characters, newActionName, closeAfterSelect, oneTime, willEndIfEmpty);
            
            return panel;
        }
        
        
        public static CrewSelect CreateStandalone (List<Character> characters, string newActionName, string newTitle, bool closeAfterSelect = false, 
            bool oneTime = false, bool willEndIfEmpty = true)
        {
            CrewSelect panel = UIManager.Create(UIManager.Get().standaloneCrewSelect as CrewSelect);
            panel.Init(characters, newActionName, closeAfterSelect, oneTime, willEndIfEmpty);
            panel.titleText.text = newTitle;
            
            return panel;
        }


        public static CrewSelect Create (List<Character> characters, string newActionName, string title, bool closeAfterSelect = false, bool oneTime = false, bool willEndIfEmpty = true) 
        {
            CrewSelect instance = Create(characters, newActionName, closeAfterSelect, oneTime, willEndIfEmpty);
            instance.titleText.text = title;
            return instance;
        }


        void Init (List<Character> newCrewList, string newActionName, bool closeAfterSelect = false, bool oneTime = false, bool willEndIfEmpty = true) 
        {
            base.Start();

            endIfEmpty = willEndIfEmpty;
            onlyOneTime = oneTime;
            endAfterSelect = closeAfterSelect;

            Debug.Log("Created new crew select. End if empty: " + endIfEmpty);

            crewList = newCrewList;

            actionName = newActionName;
            Refresh();
        }


        protected override void SetDefaultSelectable ()
        {
            base.SetDefaultSelectable();

            Button nextBtn = layoutGroup.GetComponentInChildren<Button>();
            if (nextBtn == null)
                return;

            GameObject nextButton = nextBtn.gameObject;

            if (nextButton)
                SetCurrentSelectable(nextButton);
        }

        /// <summary>
        /// Refreshes the list of crew in the station.
        /// </summary>
        public void Refresh ()
        {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform t in layoutGroup)
                children.Add(t.gameObject);
            children.ForEach(child => Destroy(child));

            if (crewList.Count < 1 && endIfEmpty)
            {
                Debug.Log("Ending lol C");
                End();
                return;
            }

            foreach (Character crew in crewList)
                MakeCrewPanel(crew, actionName);

            SetDefaultSelectable();

        }

        void MakeCrewPanel (Character crew, string actionName)
        {
            //if (crew.myData.guest) return;
            CharacterBox newBox = Instantiate(characterBoxPrefab) as CharacterBox;
            newBox.transform.SetParent(layoutGroup, false);
            newBox.Init(crew, actionName);
        }


        public void OnSelectCrew (Character crew)
        {
            if (used && onlyOneTime) return;
            
            Debug.Log("Invoking crew select delegate");

            crewSelect?.Invoke(crew);
            Refresh();
            used = true;

            if (endAfterSelect)
                StartCoroutine(WaitAndEnd(1.5f));
        }

        IEnumerator WaitAndEnd (float waitTime)
        {
            Debug.Log("Ending lol B");
            yield return new WaitForSeconds(waitTime);
            End();
        }

        protected override void FadeoutComplete ()
        {
            Debug.Log("Ending lol");
            Destroy(gameObject);
        }
    }
}