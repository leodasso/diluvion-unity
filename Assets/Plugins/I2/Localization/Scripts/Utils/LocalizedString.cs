using System;

namespace I2.Loc
{
    [Serializable]
    public struct LocalizedString
    {
        public string mTerm;
        public bool mRTL_IgnoreArabicFix;
        public int  mRTL_MaxLineLength;
        public bool mRTL_ConvertNumbers;

        public static implicit operator string(LocalizedString s)
        {
            return s.ToString();
        }

        public static implicit operator LocalizedString(string term)
        {
            return new LocalizedString() { mTerm = term };
        }


        public override string ToString()
        {
            var translation = LocalizationManager.GetTermTranslation(mTerm, !mRTL_IgnoreArabicFix, mRTL_MaxLineLength, !mRTL_ConvertNumbers);
            LocalizationManager.ApplyLocalizationParams(ref translation, null);
            return translation;
        }
    }
}