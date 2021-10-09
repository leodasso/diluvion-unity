using UnityEngine;
using TMPro;
using Diluvion;

namespace DUI
{
    public class DialogSpeech : DUIPanel
    { 
        public TextMeshProUGUI charName;
        public TextMeshProUGUI speechText;

        Character character;
        Speech speech;

        // Use this for initialization
        public void Init(Character c, Speech s)
        {
            speech = s;
            character = c;

            if (speech.speaker != null)
                charName.text = speech.speaker.GetLocalizedName();
            else
                charName.text = character.GetLocalizedName();

            speechText.text = speech.LocalizedText();

            speechText.text = speechText.text.Replace('@', '\n');
        }
    }
}