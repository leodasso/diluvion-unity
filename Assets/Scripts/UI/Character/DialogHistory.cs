using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Diluvion;

namespace DUI
{
    /// <summary>
    /// Displays any previously had conversation. Prefab should be located in resources/UI/dialog history panel.prefab
    /// </summary>
    public class DialogHistory : DUIView
    {
        public RectTransform speechesParent;
        public TextMeshProUGUI title;
        [Space]
        public DialogSpeech speechPanelPrefab;
        Convo conversation;
        Character speaker;


        public static void ShowDialogHistory(Convo c, Character ch)
        {
            UIManager.Create(UIManager.Get().dialogHistory as DialogHistory).Init(c, ch);
        }

        public static void RemoveInstance()
        {
            UIManager.Clear<DialogHistory>();
        }

        void Init(Convo c, Character ch)
        {
            conversation = c;
            speaker = ch;

            title.text = c.LocalizedQuestion();

            // Destroy any children in the speeches parent.
            List<Transform> children = new List<Transform>();
            foreach (Transform t in speechesParent.transform) children.Add(t);
            children.ForEach(child => Destroy(child.gameObject));

            // Add the new speeches
            foreach (Speech speech in c.speeches)
                UIManager.Create(speechPanelPrefab, speechesParent).Init(speaker, speech);
        }

        protected override void FadeoutComplete()
        {
            Destroy(gameObject);
        }
    }
}