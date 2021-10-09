//#define TK2D
using UnityEngine;

namespace I2.Loc
{
	#if TK2D

	public partial class Localize
	{
		#region Cache
		
		tk2dTextMesh 	mTarget_tk2dTextMesh;
		tk2dBaseSprite 	mTarget_tk2dBaseSprite;
		TextAnchor mOriginalAlignmentTk2d = TextAnchor.MiddleCenter;

		public void RegisterEvents_2DToolKit()
		{
			EventFindTarget += FindTarget_tk2dTextMesh;
			EventFindTarget += FindTarget_tk2dBaseSprite;
		}
		
		#endregion
		
		#region Find Target
		
		void FindTarget_tk2dTextMesh() 		{ FindAndCacheTarget (ref mTarget_tk2dTextMesh, SetFinalTerms_tk2dTextMesh, DoLocalize_tk2dTextMesh, true, true, false); }
		void FindTarget_tk2dBaseSprite()	{ FindAndCacheTarget (ref mTarget_tk2dBaseSprite, SetFinalTerms_tk2dBaseSprite, DoLocalize_tk2dBaseSprite, true, false, false); }

		#endregion
		
		#region SetFinalTerms
		
		void SetFinalTerms_tk2dTextMesh(string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII)
		{
			string second = (mTarget_tk2dTextMesh.font!=null ? mTarget_tk2dTextMesh.font.name : string.Empty);
			SetFinalTerms (mTarget_tk2dTextMesh.text, second,		out primaryTerm, out secondaryTerm, RemoveNonASCII);
			
		}
		
		public void SetFinalTerms_tk2dBaseSprite(string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII)
		{
			string second = (mTarget_tk2dBaseSprite.Collection!=null ? mTarget_tk2dBaseSprite.Collection.spriteCollectionName : string.Empty);
			string main = (mTarget_tk2dBaseSprite.CurrentSprite!=null ? mTarget_tk2dBaseSprite.CurrentSprite.name : string.Empty);
			SetFinalTerms (main, 	second,	out primaryTerm, out secondaryTerm, RemoveNonASCII);
		}

		#endregion
		
	#region DoLocalize
		
		public void DoLocalize_tk2dTextMesh(string MainTranslation, string SecondaryTranslation)
		{
			//--[ Localize Font Object ]----------
			tk2dFont newFont = GetSecondaryTranslatedObj<tk2dFont>(ref MainTranslation, ref SecondaryTranslation);
			if (newFont!=null && mTarget_tk2dTextMesh.font!=newFont)
			{
				mTarget_tk2dTextMesh.font = newFont.data;
			}


			if (mInitializeAlignment)
			{
				mInitializeAlignment = false;
				mOriginalAlignmentTk2d = mTarget_tk2dTextMesh.anchor;
			}

			if (MainTranslation != null && mTarget_tk2dTextMesh.text != MainTranslation)
			{
				if (Localize.CurrentLocalizeComponent.CorrectAlignmentForRTL)
				{
					int align = (int)mTarget_tk2dTextMesh.anchor;

					if (align % 3 == 0)
						mTarget_tk2dTextMesh.anchor = LocalizationManager.IsRight2Left ? mTarget_tk2dTextMesh.anchor + 2 : mOriginalAlignmentTk2d;
					else
					if (align % 3 == 2)
						mTarget_tk2dTextMesh.anchor = LocalizationManager.IsRight2Left ? mTarget_tk2dTextMesh.anchor - 2 : mOriginalAlignmentTk2d;
				}
				mTarget_tk2dTextMesh.text = MainTranslation;
			}
		}
		
		public void DoLocalize_tk2dBaseSprite(string MainTranslation, string SecondaryTranslation)
		{
			//--[ Localize Atlas ]----------
			tk2dSpriteCollection newCollection = GetSecondaryTranslatedObj<tk2dSpriteCollection>(ref MainTranslation, ref SecondaryTranslation);

			if (newCollection!=null) 
			{
				if (mTarget_tk2dBaseSprite.CurrentSprite.name != MainTranslation || mTarget_tk2dBaseSprite.Collection.name != SecondaryTranslation)
				mTarget_tk2dBaseSprite.SetSprite(newCollection.spriteCollection, MainTranslation);
			}
			else
			{
				if (mTarget_tk2dBaseSprite.CurrentSprite.name != MainTranslation)
					mTarget_tk2dBaseSprite.SetSprite(MainTranslation);
			}
		}


	#endregion	
	}
#else
	public partial class Localize
	{
		public static void RegisterEvents_2DToolKit()
		{
		}
	}
	#endif
}

