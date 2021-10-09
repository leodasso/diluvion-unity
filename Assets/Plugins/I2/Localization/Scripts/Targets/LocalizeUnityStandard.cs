using UnityEngine;

namespace I2.Loc
{
	public partial class Localize
	{
		#region Cache

		GUIText 	mTarget_GUIText;
		TextMesh 	mTarget_TextMesh;
		AudioSource mTarget_AudioSource;
		GUITexture 	mTarget_GUITexture;
		GameObject  mTarget_Child;
		SpriteRenderer mTarget_SpriteRenderer;
		bool mInitializeAlignment = true;
		TextAlignment mAlignmentStd_LTR = TextAlignment.Left;
        TextAlignment mAlignmentStd_RTL = TextAlignment.Right;

        public void RegisterEvents_UnityStandard()
		{
			EventFindTarget += FindTarget_GUIText;
			EventFindTarget += FindTarget_TextMesh;
			EventFindTarget += FindTarget_AudioSource;
			EventFindTarget += FindTarget_GUITexture;
			EventFindTarget += FindTarget_Child;
			EventFindTarget += FindTarget_SpriteRenderer;
		}

		#endregion

		#region Find Target

		void FindTarget_GUIText() 		{ FindAndCacheTarget (ref mTarget_GUIText, 			SetFinalTerms_GUIText,			DoLocalize_GUIText,			true, true, false); }
		void FindTarget_TextMesh() 		{ FindAndCacheTarget (ref mTarget_TextMesh,			SetFinalTerms_TextMesh,			DoLocalize_TextMesh,		true, true, false); }
		void FindTarget_AudioSource()	{ FindAndCacheTarget (ref mTarget_AudioSource,		SetFinalTerms_AudioSource,		DoLocalize_AudioSource,		false,false, false);}
		void FindTarget_GUITexture() 	{ FindAndCacheTarget (ref mTarget_GUITexture,		SetFinalTerms_GUITexture,		DoLocalize_GUITexture,		false,false, false);}
		void FindTarget_Child() 		{ FindAndCacheTarget (ref mTarget_Child,			SetFinalTerms_Child,			DoLocalize_Child,			false,false, false);}
		void FindTarget_SpriteRenderer(){ FindAndCacheTarget (ref mTarget_SpriteRenderer,	SetFinalTerms_SpriteRenderer,	DoLocalize_SpriteRenderer,	false,false, false);}

		#endregion

		#region SetFinalTerms

		public void SetFinalTerms_GUIText ( string Main, string Secondary, out string PrimaryTerm, out string secondaryTranslation, bool RemoveNonASCII )
		{
			if (string.IsNullOrEmpty(Secondary) && mTarget_GUIText.font!=null)
				Secondary = mTarget_GUIText.font.name;
			SetFinalTerms (mTarget_GUIText.text, 	Secondary, 	out PrimaryTerm, out secondaryTranslation, RemoveNonASCII);
		}

		public void SetFinalTerms_TextMesh ( string Main, string Secondary, out string PrimaryTerm, out string secondaryTranslation, bool RemoveNonASCII )
		{
			string second = (mTarget_TextMesh.font!=null ? mTarget_TextMesh.font.name : string.Empty);
			SetFinalTerms (mTarget_TextMesh.text, 	second, out PrimaryTerm, out secondaryTranslation, RemoveNonASCII);
		}

		public void SetFinalTerms_GUITexture ( string Main, string Secondary, out string PrimaryTerm, out string secondaryTranslation, bool RemoveNonASCII )
		{
			if (!mTarget_GUITexture || !mTarget_GUITexture.texture) 
			{
				SetFinalTerms( string.Empty, string.Empty, out PrimaryTerm, out secondaryTranslation, false );
			}
			else
			{
				SetFinalTerms (mTarget_GUITexture.texture.name,	string.Empty, 		out PrimaryTerm, out secondaryTranslation, false);
			}
		}

		public void SetFinalTerms_AudioSource ( string Main, string Secondary, out string PrimaryTerm, out string secondaryTranslation, bool RemoveNonASCII )
		{
			if (!mTarget_AudioSource || !mTarget_AudioSource.clip)
			{
				SetFinalTerms( string.Empty, string.Empty, out PrimaryTerm, out secondaryTranslation, false );
			}
			else
			{
				SetFinalTerms (mTarget_AudioSource.clip.name,string.Empty, 		out PrimaryTerm, out secondaryTranslation, false);
			}
		}

		public void SetFinalTerms_Child ( string Main, string Secondary, out string PrimaryTerm, out string secondaryTranslation, bool RemoveNonASCII )
		{
			SetFinalTerms (mTarget_Child.name,	string.Empty, 	out PrimaryTerm, out secondaryTranslation, false);
		}

		public void SetFinalTerms_SpriteRenderer ( string Main, string Secondary, out string PrimaryTerm, out string secondaryTranslation, bool RemoveNonASCII )
		{
			SetFinalTerms (mTarget_SpriteRenderer.sprite!=null ? mTarget_SpriteRenderer.sprite.name : string.Empty,	string.Empty, 	out PrimaryTerm, out secondaryTranslation, false);
		}

		#endregion

		#region DoLocalize

		void DoLocalize_GUIText( string mainTranslation, string secondaryTranslation )
		{
			//--[ Localize Font Object ]----------
			Font newFont = GetSecondaryTranslatedObj<Font>(ref mainTranslation, ref secondaryTranslation);
			if (newFont!=null && mTarget_GUIText.font != newFont) 
				mTarget_GUIText.font = newFont;

			//--[ Localize Text ]----------
			if (mInitializeAlignment)
			{
				mInitializeAlignment = false;

                mAlignmentStd_LTR = mAlignmentStd_RTL = mTarget_GUIText.alignment;

                if (LocalizationManager.IsRight2Left && mAlignmentStd_RTL == TextAlignment.Right)
                    mAlignmentStd_LTR = TextAlignment.Left;
                if (!LocalizationManager.IsRight2Left && mAlignmentStd_LTR == TextAlignment.Left)
                    mAlignmentStd_RTL = TextAlignment.Right;

			}
			if (mainTranslation!=null && mTarget_GUIText.text != mainTranslation)
			{
				if (CurrentLocalizeComponent.CorrectAlignmentForRTL && mTarget_GUIText.alignment!=TextAlignment.Center)
					mTarget_GUIText.alignment = LocalizationManager.IsRight2Left ? mAlignmentStd_RTL : mAlignmentStd_LTR;
				
				mTarget_GUIText.text = mainTranslation;
			}
		}
		
		void DoLocalize_TextMesh( string mainTranslation, string secondaryTranslation )
		{
			//--[ Localize Font Object ]----------
			Font newFont = GetSecondaryTranslatedObj<Font>(ref mainTranslation, ref secondaryTranslation);
			if (newFont!=null && mTarget_TextMesh.font != newFont)
			{
				mTarget_TextMesh.font = newFont;
				GetComponent<Renderer>().sharedMaterial = newFont.material;
			}
			
			//--[ Localize Text ]----------
			if (mInitializeAlignment)
			{
				mInitializeAlignment = false;
                mAlignmentStd_LTR = mAlignmentStd_RTL = mTarget_TextMesh.alignment;

                if (LocalizationManager.IsRight2Left && mAlignmentStd_RTL == TextAlignment.Right)
                    mAlignmentStd_LTR = TextAlignment.Left;
                if (!LocalizationManager.IsRight2Left && mAlignmentStd_LTR == TextAlignment.Left)
                    mAlignmentStd_RTL = TextAlignment.Right;
			}
			if (mainTranslation!=null && mTarget_TextMesh.text != mainTranslation)
			{
				if (CurrentLocalizeComponent.CorrectAlignmentForRTL && mTarget_TextMesh.alignment!=TextAlignment.Center)
                    mTarget_TextMesh.alignment = LocalizationManager.IsRight2Left ? mAlignmentStd_RTL : mAlignmentStd_LTR;

				mTarget_TextMesh.text = mainTranslation;
			}
		}

		void DoLocalize_AudioSource( string mainTranslation, string secondaryTranslation )
		{
			bool bIsPlaying = mTarget_AudioSource.isPlaying;
			AudioClip OldClip = mTarget_AudioSource.clip;
			AudioClip NewClip = FindTranslatedObject<AudioClip> (mainTranslation);
			if (OldClip != NewClip)
				mTarget_AudioSource.clip = NewClip;

			if (bIsPlaying && mTarget_AudioSource.clip) 
				mTarget_AudioSource.Play();
			
			// If the old clip is not in the translatedObjects, then unload it as it most likely was loaded from Resources
			//if (!HasTranslatedObject(OldClip))
			//	Resources.UnloadAsset(OldClip);
		}
		
		void DoLocalize_GUITexture( string mainTranslation, string secondaryTranslation )
		{
			Texture Old = mTarget_GUITexture.texture;
			if (Old!=null && Old.name!=mainTranslation)
				mTarget_GUITexture.texture = FindTranslatedObject<Texture>(mainTranslation);
			
			// If the old value is not in the translatedObjects, then unload it as it most likely was loaded from Resources
			//if (!HasTranslatedObject(Old))
			//	Resources.UnloadAsset(Old);
		}
		
		void DoLocalize_Child( string mainTranslation, string secondaryTranslation )
		{
			if (mTarget_Child && mTarget_Child.name==mainTranslation)
				return;

			GameObject current = mTarget_Child;
			GameObject NewPrefab = FindTranslatedObject<GameObject>(mainTranslation);
            if (NewPrefab)
            {
                mTarget_Child = Instantiate(NewPrefab);
                Transform mNew = mTarget_Child.transform;
                Transform bBase = (current ? current.transform : NewPrefab.transform);

                mNew.SetParent(transform);
                mNew.localScale = bBase.localScale;
                mNew.localRotation = bBase.localRotation;
                mNew.localPosition = bBase.localPosition;
            }

			if (current)
			{
				#if UNITY_EDITOR
					if (Application.isPlaying)
                        Destroy(current);
                    else
                        DestroyImmediate (current);
				#else
					Destroy (current);
				#endif
			}
		}

		void DoLocalize_SpriteRenderer(string mainTranslation, string secondaryTranslation)
		{
			
			Sprite Old = mTarget_SpriteRenderer.sprite;
			if (Old==null || Old.name!=mainTranslation)
				mTarget_SpriteRenderer.sprite = FindTranslatedObject<Sprite>(mainTranslation);

			// If the old value is not in the translatedObjects, then unload it as it most likely was loaded from Resources
			//if (!HasTranslatedObject(Old))
			//	Resources.UnloadAsset(Old);
		}


		#endregion
	}
}