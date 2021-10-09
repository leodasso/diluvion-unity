//#define NGUI

using UnityEngine;

namespace I2.Loc
{
	#if NGUI
	public partial class Localize
	{
		#region Cache

		UILabel 	mTarget_UILabel;
		UISprite 	mTarget_UISprite;
		UITexture	mTarget_UITexture;
        NGUIText.Alignment mAlignmentNGUI_LTR = NGUIText.Alignment.Left;
        NGUIText.Alignment mAlignmentNGUI_RTL = NGUIText.Alignment.Right;

        public void RegisterEvents_NGUI()
		{
			EventFindTarget += FindTarget_UILabel;
			EventFindTarget += FindTarget_UISprite;
			EventFindTarget += FindTarget_UITexture;
		}

		#endregion

		#region Find Target

		void FindTarget_UILabel() 	{ FindAndCacheTarget (ref mTarget_UILabel, SetFinalTerms_UIlabel, DoLocalize_UILabel, true, true, false); }
		void FindTarget_UISprite()	{ FindAndCacheTarget (ref mTarget_UISprite, SetFinalTerms_UISprite, DoLocalize_UISprite, true, false, false); }
		void FindTarget_UITexture() { FindAndCacheTarget (ref mTarget_UITexture, SetFinalTerms_UITexture, DoLocalize_UITexture, false, false, false); }

		#endregion

		#region SetFinalTerms

		void SetFinalTerms_UIlabel(string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII)
		{
			string second = (mTarget_UILabel.ambigiousFont!=null ? mTarget_UILabel.ambigiousFont.name : string.Empty);
			SetFinalTerms (mTarget_UILabel.text, second,		out primaryTerm, out secondaryTerm, RemoveNonASCII);
			
		}

		public void SetFinalTerms_UISprite(string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII)
		{
			string second = (mTarget_UISprite.atlas!=null ? mTarget_UISprite.atlas.name : string.Empty);
			SetFinalTerms (mTarget_UISprite.spriteName, 	second,	out primaryTerm, out secondaryTerm, false);
			
		}

		public void SetFinalTerms_UITexture(string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII)
		{
			SetFinalTerms (mTarget_UITexture.mainTexture.name, 	null,	out primaryTerm, out secondaryTerm, false);
			
		}

		#endregion

	#region DoLocalize

		public void DoLocalize_UILabel(string mainTranslation, string secondaryTranslation)
		{
			//--[ Localize Font Object ]----------
			Font newFont = GetSecondaryTranslatedObj<Font>(ref mainTranslation, ref secondaryTranslation);
			if (newFont!=null) 
			{
				if (newFont != mTarget_UILabel.ambigiousFont)
					mTarget_UILabel.ambigiousFont = newFont;
			}
			else
			{
				UIFont newUIFont = GetSecondaryTranslatedObj<UIFont>(ref mainTranslation, ref secondaryTranslation);
				if (newUIFont!=null && mTarget_UILabel.ambigiousFont != newUIFont)
					mTarget_UILabel.ambigiousFont = newUIFont;
			}

			if (mInitializeAlignment)
			{
				mInitializeAlignment = false;
                mAlignmentNGUI_LTR = mAlignmentNGUI_RTL = mTarget_UILabel.alignment;

                if (LocalizationManager.IsRight2Left && mAlignmentNGUI_RTL == NGUIText.Alignment.Right)
                    mAlignmentNGUI_LTR = NGUIText.Alignment.Left;
                if (!LocalizationManager.IsRight2Left && mAlignmentNGUI_LTR == NGUIText.Alignment.Left)
                    mAlignmentNGUI_RTL = NGUIText.Alignment.Right;
			}

			UIInput input = NGUITools.FindInParents<UIInput>(mTarget_UILabel.gameObject);
			if (input != null && input.label == mTarget_UILabel) 
			{
				if (mainTranslation!=null && input.defaultText != mainTranslation) 
				{
                    if (CurrentLocalizeComponent.CorrectAlignmentForRTL && (input.label.alignment == NGUIText.Alignment.Left || input.label.alignment == NGUIText.Alignment.Right))
                        input.label.alignment = LocalizationManager.IsRight2Left ? mAlignmentNGUI_RTL : mAlignmentNGUI_LTR;

					input.defaultText = mainTranslation;
				}
			}
			else 
			{
				if (mainTranslation!=null && mTarget_UILabel.text != mainTranslation) 
				{
					if (CurrentLocalizeComponent.CorrectAlignmentForRTL && (mTarget_UILabel.alignment==NGUIText.Alignment.Left || mTarget_UILabel.alignment == NGUIText.Alignment.Right))
                        mTarget_UILabel.alignment = LocalizationManager.IsRight2Left ? mAlignmentNGUI_RTL : mAlignmentNGUI_LTR;

					mTarget_UILabel.text = mainTranslation;
				}
			}
		}

		public void DoLocalize_UISprite(string mainTranslation, string secondaryTranslation)
		{
			if (mTarget_UISprite.spriteName == mainTranslation)
				return;
			
			//--[ Localize Atlas ]----------
			UIAtlas newAtlas = GetSecondaryTranslatedObj<UIAtlas>(ref mainTranslation, ref secondaryTranslation);
			bool bChanged = false;
			if (newAtlas!=null && mTarget_UISprite.atlas != newAtlas)
			{
				mTarget_UISprite.atlas = newAtlas;
				bChanged = true;
			}

			if (mTarget_UISprite.spriteName != mainTranslation && mTarget_UISprite.atlas.GetSprite(mainTranslation)!=null)
			{
				mTarget_UISprite.spriteName = mainTranslation;
				bChanged = true;
			}
			if (bChanged)
				mTarget_UISprite.MakePixelPerfect();
		}
		
		public void DoLocalize_UITexture(string mainTranslation, string secondaryTranslation)
		{
			Texture Old = mTarget_UITexture.mainTexture;
			if (Old!=null && Old.name!=mainTranslation)
				mTarget_UITexture.mainTexture = FindTranslatedObject<Texture>(mainTranslation);
			
			// If the old value is not in the translatedObjects, then unload it as it most likely was loaded from Resources
			//if (!HasTranslatedObject(Old))
			//	Resources.UnloadAsset(Old);
		}
		
	#endregion	
	}
#else
	public partial class Localize
	{
		public static void RegisterEvents_NGUI()
		{
		}
	}
	#endif
}

