using System;
using UnityEngine;

namespace I2.Loc
{
	#if TextMeshPro || TextMeshPro_Pre53
	public partial class Localize
	{
		TMPro.TextMeshPro 	mTarget_TMPLabel;
		TMPro.TextMeshProUGUI mTarget_TMPUGUILabel;
        TMPro.TextAlignmentOptions mAlignmentTMPro_RTL = TMPro.TextAlignmentOptions.TopRight;
        TMPro.TextAlignmentOptions mAlignmentTMPro_LTR = TMPro.TextAlignmentOptions.TopLeft;
		bool mAlignmentTMPwasRTL;

		[NonSerialized]public string TMP_previewLanguage;  // this is used because when in the editor, TMPro disables the inspector for a frame when changing fonts
		
		public void RegisterEvents_TextMeshPro()
		{
			EventFindTarget += FindTarget_TMPLabel;
			EventFindTarget += FindTarget_TMPUGUILabel;
		}
		
		void FindTarget_TMPLabel() 	{ FindAndCacheTarget (ref mTarget_TMPLabel, SetFinalTerms_TMPLabel, DoLocalize_TMPLabel, true, true, false); }

		void FindTarget_TMPUGUILabel() 	{ FindAndCacheTarget (ref mTarget_TMPUGUILabel, SetFinalTerms_TMPUGUILabel, DoLocalize_TMPUGUILabel, true, true, false); }

		void SetFinalTerms_TMPLabel(string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII)
		{
			string second = (mTarget_TMPLabel.font!=null ? mTarget_TMPLabel.font.name : string.Empty);
			SetFinalTerms( mTarget_TMPLabel.text, second, out primaryTerm, out secondaryTerm, RemoveNonASCII );
		}

		void SetFinalTerms_TMPUGUILabel ( string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII )
		{
			string second = (mTarget_TMPUGUILabel.font!=null ? mTarget_TMPUGUILabel.font.name : string.Empty);
			SetFinalTerms( mTarget_TMPUGUILabel.text, second, out primaryTerm, out secondaryTerm, RemoveNonASCII );
		}
		
		public void DoLocalize_TMPLabel(string mainTranslation, string secondaryTranslation)
		{

			// there its a problem with TMPro that disables and renables the inspector when changing fonts, and that breaks the current preview flow
			if (!Application.isPlaying)
			{
				#if UNITY_EDITOR
				if (UnityEditor.Selection.activeGameObject == gameObject)
				{
					if (string.IsNullOrEmpty(TMP_previewLanguage))
						TMP_previewLanguage = LocalizationManager.CurrentLanguage;
				}
				#endif
			}

			//--[ Localize Font Object ]----------
			{
				#if TextMeshPro_Pre53
				TMPro.TextMeshProFont newFont = GetSecondaryTranslatedObj<TMPro.TextMeshProFont>(ref mainTranslation, ref secondaryTranslation);
				#else
				TMPro.TMP_FontAsset newFont = GetSecondaryTranslatedObj<TMPro.TMP_FontAsset>(ref mainTranslation, ref secondaryTranslation);
				#endif


				if (newFont != null)
				{
					if (mTarget_TMPLabel.font != newFont)
						mTarget_TMPLabel.font = newFont;
				}
				else
				{
					//--[ Localize Font Material ]----------
					Material newMat = GetSecondaryTranslatedObj<Material>(ref mainTranslation, ref secondaryTranslation);
					if (newMat != null && mTarget_TMPLabel.fontMaterial != newMat) 
					{
						if (!newMat.name.StartsWith (mTarget_TMPLabel.font.name, StringComparison.Ordinal)) 
						{
							newFont = GetTMPFontFromMaterial( secondaryTranslation.EndsWith( newMat.name, StringComparison.Ordinal ) ? secondaryTranslation : newMat.name );
							if (newFont!=null)
								mTarget_TMPLabel.font = newFont;
						}

						mTarget_TMPLabel.fontSharedMaterial/* fontMaterial*/ = newMat;
					}
				}
			}
			if (mInitializeAlignment)
			{
				mInitializeAlignment = false;
				mAlignmentTMPwasRTL = LocalizationManager.IsRight2Left;
				InitAlignment_TMPro(mAlignmentTMPwasRTL, mTarget_TMPLabel.alignment, out mAlignmentTMPro_LTR, out mAlignmentTMPro_RTL);
			}
			else
			{
				TMPro.TextAlignmentOptions alignRTL, alignLTR;
				InitAlignment_TMPro(mAlignmentTMPwasRTL, mTarget_TMPLabel.alignment, out alignLTR, out alignRTL);

				if ((mAlignmentTMPwasRTL && mAlignmentTMPro_RTL != alignRTL) ||
					(!mAlignmentTMPwasRTL && mAlignmentTMPro_LTR != alignLTR))
				{
					mAlignmentTMPro_LTR = alignLTR;
					mAlignmentTMPro_RTL = alignRTL;
				}
				mAlignmentTMPwasRTL = LocalizationManager.IsRight2Left;
			}

			if (mainTranslation!=null && mTarget_TMPLabel.text != mainTranslation)
			{
				if (CurrentLocalizeComponent.CorrectAlignmentForRTL)
				{
                    mTarget_TMPLabel.alignment = LocalizationManager.IsRight2Left ? mAlignmentTMPro_RTL : mAlignmentTMPro_LTR;
					mTarget_TMPLabel.isRightToLeftText = LocalizationManager.IsRight2Left;
					if (LocalizationManager.IsRight2Left) mainTranslation = ReverseText(mainTranslation);
				}

				mTarget_TMPLabel.text = mainTranslation;
				//mTarget_TMPLabel.SetText( mainTranslation, 0 );
			}
		}

        void InitAlignment_TMPro(bool isRTL, TMPro.TextAlignmentOptions alignment, out TMPro.TextAlignmentOptions alignLTR, out TMPro.TextAlignmentOptions alignRTL )
        {
            alignLTR = alignRTL = alignment;

            if (isRTL)
            {
                switch (alignment)
                {
                    case TMPro.TextAlignmentOptions.TopRight: alignLTR=TMPro.TextAlignmentOptions.TopLeft; break;
                    case TMPro.TextAlignmentOptions.Right: alignLTR=TMPro.TextAlignmentOptions.Left; break;
                    case TMPro.TextAlignmentOptions.BottomRight: alignLTR=TMPro.TextAlignmentOptions.BottomLeft; break;
                    case TMPro.TextAlignmentOptions.BaselineRight: alignLTR=TMPro.TextAlignmentOptions.BaselineLeft; break;
                    case TMPro.TextAlignmentOptions.MidlineRight: alignLTR=TMPro.TextAlignmentOptions.MidlineLeft; break;
                    case TMPro.TextAlignmentOptions.CaplineRight: alignLTR=TMPro.TextAlignmentOptions.CaplineLeft; break;

					case TMPro.TextAlignmentOptions.TopLeft: alignLTR = TMPro.TextAlignmentOptions.TopRight; break;
					case TMPro.TextAlignmentOptions.Left: alignLTR = TMPro.TextAlignmentOptions.Right; break;
					case TMPro.TextAlignmentOptions.BottomLeft: alignLTR = TMPro.TextAlignmentOptions.BottomRight; break;
					case TMPro.TextAlignmentOptions.BaselineLeft: alignLTR = TMPro.TextAlignmentOptions.BaselineRight; break;
					case TMPro.TextAlignmentOptions.MidlineLeft: alignLTR = TMPro.TextAlignmentOptions.MidlineRight; break;
					case TMPro.TextAlignmentOptions.CaplineLeft: alignLTR = TMPro.TextAlignmentOptions.CaplineRight; break;

				}
			}
            else
            {
                switch (alignment)
                {
					case TMPro.TextAlignmentOptions.TopRight: alignRTL = TMPro.TextAlignmentOptions.TopLeft; break;
					case TMPro.TextAlignmentOptions.Right: alignRTL = TMPro.TextAlignmentOptions.Left; break;
					case TMPro.TextAlignmentOptions.BottomRight: alignRTL = TMPro.TextAlignmentOptions.BottomLeft; break;
					case TMPro.TextAlignmentOptions.BaselineRight: alignRTL = TMPro.TextAlignmentOptions.BaselineLeft; break;
					case TMPro.TextAlignmentOptions.MidlineRight: alignRTL = TMPro.TextAlignmentOptions.MidlineLeft; break;
					case TMPro.TextAlignmentOptions.CaplineRight: alignRTL = TMPro.TextAlignmentOptions.CaplineLeft; break;

					case TMPro.TextAlignmentOptions.TopLeft: alignRTL=TMPro.TextAlignmentOptions.TopRight; break;
                    case TMPro.TextAlignmentOptions.Left: alignRTL=TMPro.TextAlignmentOptions.Right; break;
                    case TMPro.TextAlignmentOptions.BottomLeft: alignRTL=TMPro.TextAlignmentOptions.BottomRight; break;
                    case TMPro.TextAlignmentOptions.BaselineLeft: alignRTL=TMPro.TextAlignmentOptions.BaselineRight; break;
                    case TMPro.TextAlignmentOptions.MidlineLeft: alignRTL=TMPro.TextAlignmentOptions.MidlineRight; break;
                    case TMPro.TextAlignmentOptions.CaplineLeft: alignRTL=TMPro.TextAlignmentOptions.CaplineRight; break;
                }
            }
        }

 
		public void DoLocalize_TMPUGUILabel(string mainTranslation, string secondaryTranslation)
		{
			{			
				//--[ Localize Font Object ]----------
				#if TextMeshPro_Pre53
				TMPro.TextMeshProFont newFont = GetSecondaryTranslatedObj<TMPro.TextMeshProFont>(ref mainTranslation, ref secondaryTranslation);
				#else
				TMPro.TMP_FontAsset newFont = GetSecondaryTranslatedObj<TMPro.TMP_FontAsset>(ref mainTranslation, ref secondaryTranslation);
				#endif

				if (newFont != null)
				{
					if (mTarget_TMPUGUILabel.font != newFont)
						mTarget_TMPUGUILabel.font = newFont;
				}
				else
				{
					//--[ Localize Font Material ]----------
					Material newMat = GetSecondaryTranslatedObj<Material>(ref mainTranslation, ref secondaryTranslation);
					if (newMat != null && mTarget_TMPUGUILabel.fontMaterial != newMat) 
					{
						if (!newMat.name.StartsWith (mTarget_TMPUGUILabel.font.name, StringComparison.Ordinal)) 
						{
							newFont = GetTMPFontFromMaterial ( secondaryTranslation.EndsWith(newMat.name, StringComparison.Ordinal) ? secondaryTranslation : newMat.name );
							if (newFont!=null)
								mTarget_TMPUGUILabel.font = newFont;
						}
						mTarget_TMPUGUILabel.fontSharedMaterial = newMat;
					}
				}
			}

			if (mInitializeAlignment)
			{
				mInitializeAlignment = false;
				mAlignmentTMPwasRTL = LocalizationManager.IsRight2Left;
				InitAlignment_TMPro(mAlignmentTMPwasRTL, mTarget_TMPUGUILabel.alignment, out mAlignmentTMPro_LTR, out mAlignmentTMPro_RTL);
			}
			else
			{
				TMPro.TextAlignmentOptions alignRTL, alignLTR;
				InitAlignment_TMPro(mAlignmentTMPwasRTL, mTarget_TMPUGUILabel.alignment, out alignLTR, out alignRTL);

				if ((mAlignmentTMPwasRTL && mAlignmentTMPro_RTL != alignRTL) ||
					(!mAlignmentTMPwasRTL && mAlignmentTMPro_LTR != alignLTR))
				{
					mAlignmentTMPro_LTR = alignLTR;
					mAlignmentTMPro_RTL = alignRTL;
				}
				mAlignmentTMPwasRTL = LocalizationManager.IsRight2Left;
			}

			if (mainTranslation!=null && mTarget_TMPUGUILabel.text != mainTranslation)
			{
                if (CurrentLocalizeComponent.CorrectAlignmentForRTL)
                {
                    mTarget_TMPUGUILabel.alignment = LocalizationManager.IsRight2Left ? mAlignmentTMPro_RTL : mAlignmentTMPro_LTR;
					mTarget_TMPUGUILabel.isRightToLeftText = LocalizationManager.IsRight2Left;
					if (LocalizationManager.IsRight2Left) mainTranslation = ReverseText(mainTranslation);
				}

				mTarget_TMPUGUILabel.text = mainTranslation;
				//mTarget_TMPUGUILabel.SetText(mainTranslation, 0);
			}
		}

		string ReverseText(string source)
		{
			var len = source.Length;
			var output = new char[len];
			for (var i = 0; i < len; i++)
			{
				output[(len - 1) - i] = source[i];
			}
			return new string(output);
		}

#if TextMeshPro_Pre53
   		TMPro.TextMeshProFont GetTMPFontFromMaterial( string matName )
		{
			int idx = matName.IndexOf (" SDF");
			if (idx>0)
			{
				var fontName = matName.Substring (0, idx + " SDF".Length);
				return GetObject<TMPro.TextMeshProFont>(fontName);
			}
			return null;
		}
#else
		TMPro.TMP_FontAsset GetTMPFontFromMaterial( string matName )

		{
			string splitChars = " .\\/-[]()";
			for (int i=matName.Length-1; i>0;)
			{
				// Find first valid character
				while (i>0 && splitChars.IndexOf(matName[i])>=0)
					i--;

				if (i<=0) break;

				var fontName = matName.Substring (0, i+1);
				var obj = GetObject<TMPro.TMP_FontAsset>(fontName);
				if (obj!=null)
					return obj;


				// skip this word
				while (i>0 && splitChars.IndexOf(matName[i])<0)
					i--;
			}

			return null;


			/*int idx = matName.IndexOf (" SDF");
			if (idx>0)
			{
				var fontName = matName.Substring (0, idx + " SDF".Length);
				return GetObject<TMPro.TMP_FontAsset>(fontName);
			}
			return null;*/
		}
    #endif
    }

#else
        public partial class Localize
	{
		public static void RegisterEvents_TextMeshPro()
		{
		}
	}
#endif
}