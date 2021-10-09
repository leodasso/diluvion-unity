using UnityEngine;

namespace I2.Loc
{
    public class Example_LocalizedString : MonoBehaviour
    {
        public LocalizedString _MyLocalizedString;

        public string _NormalString;

        [TermsPopup]
        public string _StringWithTermPopup;

        void Start()
        {
            _MyLocalizedString = "Term1";
            Debug.Log( _MyLocalizedString );  // prints the translation of Term1 to the current language

            _MyLocalizedString = "Term2";     // Changes the term
            string s = _MyLocalizedString;    // Gets the translation of Term2 to the current language
            Debug.Log(s);
        }
    }
}