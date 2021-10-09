using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using SpiderWeb;

namespace DUI
{
    public class TalkyText : MonoBehaviour
    {
        [MinValue(1), OnValueChanged("ResetInterval")]
        public int charactersPerSecond;
        public float delay;

        [ShowInInspector, ReadOnly]
        float _interval = 1;

        /// <summary>
        /// The full text to display
        /// </summary>
        [MultiLineProperty(5), HideLabel, BoxGroup("Text")]
        public string inputText = "";

        [Tooltip("Enter this character to have the talky text pause.")]
        public string pauseCharacter = ".";
        public float pauseTime = .2f;

        /// <summary>
        /// Does the text in the textbox match the text to be shown?
        /// </summary>
        [ReadOnly]
        public bool fullyShowing;

        /// <summary>
        /// The text being shown this frame
        /// </summary>
        [MultiLineProperty(5), ReadOnly, HideLabel, BoxGroup("Text")]
        public string formattedOutput = "";

        float _intervalTimer;
        bool _needClosingTag;

        TextMeshProUGUI _textBox;
        Text _textBoxLegacy;
        string _outputText = "";
        int _memCharPerSec;
        bool _resetWhenDone;
        
        /// <summary>
        /// How many characters have appeared since the last sfx
        /// </summary>
        int _charCount;
        /// <summary>
        /// How many characters appear between sfx
        /// </summary>
        int _sfxChars = 1;

        float _t;
        float _audioT;

        // Use this for initialization
        void Start()
        {
            _textBox = GetComponent<TextMeshProUGUI>();
            if (_textBox != null) _textBox.parseCtrlCharacters = true;
            _textBoxLegacy = GetComponent<Text>();
            
            Clear();
            UpdateText();
        }

        // Update is called once per frame
        void Update()
        {
            // If fully showing, the only case when we should update is if the input text changes. 
            if (fullyShowing)
            {
                // If input text changes, clear the output text.
                if (_outputText != inputText) Clear();
                return;
            }

            if (inputText == null)
            {
                Clear();
                return;
            }
            
            
            if (inputText.Length < _outputText.Length) Clear();

            // delay
            if (_t < delay)
            {
                _t += Time.unscaledDeltaTime;
                return;
            }

            if (_outputText == inputText)
            {
                if (_resetWhenDone)
                {
                    charactersPerSecond = _memCharPerSec;
                    _resetWhenDone = false;
                }

                fullyShowing = true;
            }
            else fullyShowing = false;

            //Determine when to add a new (text) character to active dialogue box
            ResetInterval();
            _intervalTimer += Time.unscaledDeltaTime;

            if (_intervalTimer >= _interval)
            {
                // Support adding multiple characters per frame
                int charTimes = Mathf.CeilToInt(Time.unscaledDeltaTime / _interval);
                for (int i = 0; i < charTimes; i++) UpdateText();

                _intervalTimer = 0;
                  
                DoSFX();
            }
        }

        void DoSFX()
        {
            if (_charCount >= _sfxChars)
            {
                SpiderSound.MakeSound("Play_Conversation_Text_Solo", gameObject);
            }
            
            _charCount++;
            if (_charCount > _sfxChars) _charCount = 0;
        }

        void ResetInterval()
        {
            _interval = 1 / (float)charactersPerSecond;
        }

        /// <summary>
        /// Clears output text, and resets stuff to begin typing again.
        /// </summary>
        public void Clear()
        {
            _outputText = formattedOutput = "";
            fullyShowing = false;
            _t = 0;
        }

        /// <summary>
        /// This function is called to add a new character to the currently displayed inputText every period (about 10 times a second)
        /// to give the effect of the text appearing gradually. Bulk of it is to support rich character tags '<>'
        /// </summary>
        void UpdateText()
        {
            int nextCharIndex = _outputText.Length;
            string newCharacters = string.Empty;

            if (nextCharIndex < inputText.Length)
            {
                // Get the next character to show
                newCharacters += inputText[nextCharIndex];

                // Check the new text being added for rich formatting tags
                if (newCharacters.Contains("<"))
                {
                    // If it contains a tag '<', add the full thing '<b>'
                    int f = 1;
                    while (!newCharacters.Contains(">") && f < 20)
                    {
                        newCharacters += inputText[nextCharIndex + f];
                        f++;
                    }
                    _needClosingTag = true;
                }

                // treat newline characters
                if (newCharacters.Contains(@"\"))
                {
                    int f = 1;
                    while (!newCharacters.Contains("n") && f < 20)
                    {
                        newCharacters += inputText[nextCharIndex + f];
                        f++;
                    }
                }
                _outputText += newCharacters;
            }

            formattedOutput = _outputText;

            // If the current visible text has rich formatting tags that haven't been closed, close them off
            if (_needClosingTag)
            {
                // Split the string along all the formatting tags so we can check only the text after the last formatting tag
                string[] splitString = _outputText.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);

                if (splitString.Length > 0)
                {
                    // Get the string of all text after the most recent > character
                    string sinceLastTag = splitString[splitString.Length - 1];

                    if (sinceLastTag.Contains("/")) _needClosingTag = false;
                    else
                    {
                        if (sinceLastTag.Contains("<i>")) formattedOutput += "</i>";
                        if (sinceLastTag.Contains("<b>")) formattedOutput += "</b>";
                        if (sinceLastTag.Contains("<color")) formattedOutput += "</color>";
                        if (sinceLastTag.Contains("<size")) formattedOutput += "</size>";
                    }
                }
            }

            if (_textBox) _textBox.text = formattedOutput;
            if (_textBoxLegacy) _textBoxLegacy.text = formattedOutput;

            if (newCharacters.Contains(pauseCharacter)) _intervalTimer += pauseTime;
        }

        [Button]
        public void ShowFull()
        {
            if (fullyShowing) return;
            _memCharPerSec = charactersPerSecond;
            _resetWhenDone = true;
            charactersPerSecond = 200;
        }
    }
}