using System;
using UnityEngine;
using Object = UnityEngine.Object;

// localize: Subtitle

namespace I2.Loc
{
	[AddComponentMenu("I2/Localization/Localize")]
	public partial class Localize : MonoBehaviour 
	{
		#region Variables: Term
		public string Term 
		{ 
			get { return mTerm; } 
			set { SetTerm(value); }
		}
		public string SecondaryTerm 
		{ 
			get { return mTermSecondary; } 
			set { SetTerm(null, value); }
		}

		public string mTerm = string.Empty,  		  // if Target is a Label, this will be the text,  if sprite, this will be the spriteName, etc
					  mTermSecondary =  string.Empty; // if Target is a Label, this will be the font Name,  if sprite, this will be the Atlas name, etc

		// This are the terms actually used (will be mTerm/mSecondaryTerm or will get them from the objects if those are missing. e.g. Labels' text and font name)
		// This are set when the component starts
		[NonSerialized] public string FinalTerm, FinalSecondaryTerm;

		public enum TermModification {DontModify, ToUpper, ToLower, ToUpperFirst, ToTitle/*, CustomRange*/}
		public TermModification PrimaryTermModifier = TermModification.DontModify, 
								SecondaryTermModifier = TermModification.DontModify;
        public string TermPrefix, TermSuffix;

		public bool LocalizeOnAwake = true;

		string LastLocalizedLanguage;	// Used to avoid Localizing everytime the object is Enabled

		#if UNITY_EDITOR
			public LanguageSource Source;	// Source used while in the Editor to preview the Terms
		#endif

		#endregion

		#region Variables: Target

		// This is the Object/Component that should be localized
		public Object mTarget;

		public event Action EventFindTarget;

		public delegate void DelegateSetFinalTerms ( string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII );
		public DelegateSetFinalTerms EventSetFinalTerms;

		public delegate void DelegateDoLocalize(string primaryTerm, string secondaryTerm);
		public DelegateDoLocalize EventDoLocalize;

		public bool CanUseSecondaryTerm = false;

		public bool AllowMainTermToBeRTL = false;	//	Whatever or not this localize should Fix the MainTranslation on Right To Left Languages
		public bool AllowSecondTermToBeRTL = false; // Same for secondary Translation
		public bool IgnoreRTL = false;	// If false, no Right To Left processing will be done
		public int  MaxCharactersInRTL = 0;     // If the language is RTL, the translation will be split in lines not longer than this amount and the RTL fix will be applied per line
        public bool IgnoreNumbersInRTL = true; // If the language is RTL, the translation will not convert numbers (will preserve them like: e.g. 123)

        public bool CorrectAlignmentForRTL = true;	// If true, when Right To Left language, alignment will be set to Right

		#endregion

		#region Variables: References

		public Object[] TranslatedObjects;	// For targets that reference objects (e.g. AudioSource, UITexture,etc) 
											// this keeps a reference to the possible options.
											// If the value is not the name of any of this objects then it will try to load the object from the Resources

		#endregion

		#region Variable Translation Modifiers

		public EventCallback LocalizeCallBack = new EventCallback();	// This allows scripts to modify the translations :  e.g. "Player {0} wins"  ->  "Player Red wins"	
		public static string MainTranslation, SecondaryTranslation;		// The callback should use and modify this variables
		public static string CallBackTerm, CallBackSecondaryTerm;		// during the callback, this will hold the FinalTerm and FinalSecondary  to know what terms are originating the translation
		public static Localize CurrentLocalizeComponent;				// while in the LocalizeCallBack, this points to the Localize calling the callback

		public bool AlwaysForceLocalize = false;			// Force localization when the object gets enabled (useful for callbacks and parameters that change the localization even through the language is the same as in the previous time it was localized)

		#endregion

		#region Variables: Editor Related
		public bool mGUI_ShowReferences = false;
		public bool mGUI_ShowTems = true;
		public bool mGUI_ShowCallback = false;
		#endregion

		#region Localize

		void Awake()
		{
			RegisterTargets();
			if (HasTargetCache())
				EventFindTarget(); // Finds a new target if mTarget is null. Also caches the target into the mTarget_XXX variables

			if (LocalizeOnAwake)
				OnLocalize();
		}

		void RegisterTargets()
		{
			if (EventFindTarget!=null)
				return;
			RegisterEvents_NGUI();
			RegisterEvents_DFGUI();
			RegisterEvents_UGUI();
			RegisterEvents_2DToolKit();
			RegisterEvents_TextMeshPro();
			RegisterEvents_UnityStandard();
			RegisterEvents_SVG();
		}

		void OnEnable()
		{
			OnLocalize ();
		}

		public void OnLocalize( bool Force = false )
		{
			if (!Force && (!enabled || gameObject==null || !gameObject.activeInHierarchy))
				return;

			if (string.IsNullOrEmpty(LocalizationManager.CurrentLanguage))
				return;

			if (!AlwaysForceLocalize && !Force && !LocalizeCallBack.HasCallback() && LastLocalizedLanguage==LocalizationManager.CurrentLanguage)
				return;
			LastLocalizedLanguage = LocalizationManager.CurrentLanguage;

			if (!HasTargetCache()) 
				FindTarget();

			if (!HasTargetCache()) return;
			
			// These are the terms actually used (will be mTerm/mSecondaryTerm or will get them from the objects if those are missing. e.g. Labels' text and font name)
			if (string.IsNullOrEmpty(FinalTerm) || string.IsNullOrEmpty(FinalSecondaryTerm))
				GetFinalTerms( out FinalTerm, out FinalSecondaryTerm );


			bool hasCallback = Application.isPlaying && LocalizeCallBack.HasCallback();

			if (!hasCallback && string.IsNullOrEmpty (FinalTerm) && string.IsNullOrEmpty (FinalSecondaryTerm))
				return;

			CallBackTerm = FinalTerm;
			CallBackSecondaryTerm = FinalSecondaryTerm;
			MainTranslation = string.IsNullOrEmpty(FinalTerm) || FinalTerm=="-" ? null : LocalizationManager.GetTermTranslation (FinalTerm, false);
			SecondaryTranslation = string.IsNullOrEmpty(FinalSecondaryTerm) || FinalSecondaryTerm == "-" ? null : LocalizationManager.GetTermTranslation (FinalSecondaryTerm, false);

			if (!hasCallback && /*string.IsNullOrEmpty (MainTranslation)*/ string.IsNullOrEmpty(FinalTerm) && string.IsNullOrEmpty (SecondaryTranslation))
				return;

			CurrentLocalizeComponent = this;
			if (Application.isPlaying) 
			{
				LocalizeCallBack.Execute (this);  // This allows scripts to modify the translations :  e.g. "Player {0} wins"  ->  "Player Red wins"
				LocalizationManager.ApplyLocalizationParams (ref MainTranslation, gameObject);
			}
            bool applyRTL = LocalizationManager.IsRight2Left && !IgnoreRTL;
            if (applyRTL)
			{
				if (AllowMainTermToBeRTL && !string.IsNullOrEmpty(MainTranslation))   
					MainTranslation = LocalizationManager.ApplyRTLfix(MainTranslation, MaxCharactersInRTL, IgnoreNumbersInRTL);
				if (AllowSecondTermToBeRTL && !string.IsNullOrEmpty(SecondaryTranslation)) 
					SecondaryTranslation = LocalizationManager.ApplyRTLfix(SecondaryTranslation);
			}

			if (PrimaryTermModifier != TermModification.DontModify)
					MainTranslation = MainTranslation ?? string.Empty;

			switch (PrimaryTermModifier)
			{
				case TermModification.ToUpper 		: MainTranslation = MainTranslation.ToUpper(); break;
				case TermModification.ToLower 		: MainTranslation = MainTranslation.ToLower(); break;
				case TermModification.ToUpperFirst 	: MainTranslation = GoogleTranslation.UppercaseFirst(MainTranslation); break;
				case TermModification.ToTitle 		: MainTranslation = GoogleTranslation.TitleCase(MainTranslation); break;
			}

			if (SecondaryTermModifier != TermModification.DontModify)
				SecondaryTranslation = SecondaryTranslation ?? string.Empty;

			switch (SecondaryTermModifier)
			{
				case TermModification.ToUpper 		: SecondaryTranslation = SecondaryTranslation.ToUpper();  break;
				case TermModification.ToLower 		: SecondaryTranslation = SecondaryTranslation.ToLower();  break;
				case TermModification.ToUpperFirst 	: SecondaryTranslation = GoogleTranslation.UppercaseFirst(SecondaryTranslation); break;
				case TermModification.ToTitle 		: SecondaryTranslation = GoogleTranslation.TitleCase(SecondaryTranslation); break;
			}
            if (!string.IsNullOrEmpty(TermPrefix))
                MainTranslation = applyRTL ? MainTranslation + TermPrefix : TermPrefix + MainTranslation;
            if (!string.IsNullOrEmpty(TermSuffix))
                MainTranslation = applyRTL ? TermSuffix + MainTranslation : MainTranslation + TermSuffix;

            EventDoLocalize( MainTranslation, SecondaryTranslation);
			CurrentLocalizeComponent = null;
		}

		#endregion

		#region Finding Target

		public bool FindTarget()
		{
			if (HasTargetCache())
				return true;
			
			if (EventFindTarget==null)
				RegisterTargets();

			EventFindTarget();
			return HasTargetCache();
		}

		public void FindAndCacheTarget<T>( ref T targetCache, DelegateSetFinalTerms setFinalTerms, DelegateDoLocalize doLocalize, bool UseSecondaryTerm, bool MainRTL, bool SecondRTL ) where T : Component
		{
			if (mTarget!=null)
				targetCache = (mTarget as T);
			else
				mTarget = targetCache = GetComponent<T>();

			if (targetCache != null)
			{
				EventSetFinalTerms = setFinalTerms;
				EventDoLocalize = doLocalize;

				CanUseSecondaryTerm = UseSecondaryTerm;
				AllowMainTermToBeRTL 	= MainRTL;
				AllowSecondTermToBeRTL	= SecondRTL;
			}
		}

		void FindAndCacheTarget( ref GameObject targetCache, DelegateSetFinalTerms setFinalTerms, DelegateDoLocalize doLocalize, bool UseSecondaryTerm, bool MainRTL, bool SecondRTL )
		{
			if (mTarget!=targetCache && targetCache)
			{
#if UNITY_EDITOR
				DestroyImmediate (targetCache);
#else
				Destroy (targetCache);
#endif
			}

			if (mTarget!=null)
				targetCache = (mTarget as GameObject);
			else
			{
				Transform mThis = transform;
				mTarget = targetCache = (mThis.childCount<1 ? null : mThis.GetChild(0).gameObject);
			}
			if (targetCache != null)
			{
				EventSetFinalTerms = setFinalTerms;
				EventDoLocalize = doLocalize;

				CanUseSecondaryTerm = UseSecondaryTerm;
				AllowMainTermToBeRTL 	= MainRTL;
				AllowSecondTermToBeRTL	= SecondRTL;
			}
		}

		bool HasTargetCache() { return EventDoLocalize!=null; }

		#endregion

		#region Finding Term
		
		// Returns the term that will actually be translated
		// its either the Term value in this class or the text of the label if there is no term
		public void GetFinalTerms( out string primaryTerm, out string secondaryTerm )
		{
            if (EventSetFinalTerms==null || (!mTarget && !HasTargetCache()))
				FindTarget();

			primaryTerm 	= string.Empty;
			secondaryTerm 	= string.Empty;



			// if either the primary or secondary term is missing, get them. (e.g. from the label's text and font name)
			if (mTarget!=null && (string.IsNullOrEmpty(mTerm) || string.IsNullOrEmpty(mTermSecondary)))
			{
				if (EventSetFinalTerms!=null)
					EventSetFinalTerms(mTerm, mTermSecondary,	out primaryTerm, out secondaryTerm, true );  // If no term is set, use the text from the label, the spritename from the Sprite, etc
			}

			// If there are values already set, go with those
			if (!string.IsNullOrEmpty(mTerm))
				primaryTerm = mTerm;

			if (!string.IsNullOrEmpty(mTermSecondary))
				secondaryTerm = mTermSecondary;

            if (primaryTerm != null)
                primaryTerm = primaryTerm.Trim();
            if (secondaryTerm != null)
                secondaryTerm = secondaryTerm.Trim();
        }

        public string GetMainTargetsText( bool RemoveNonASCII )
		{
			string primary = null, secondary = null;
			if (EventSetFinalTerms!=null)
				EventSetFinalTerms( null, null, out primary, out secondary, RemoveNonASCII );  // If no term is set, use the text from the label, the spritename from the Sprite, etc

			return string.IsNullOrEmpty(primary) ? mTerm : primary;
		}
		
		void SetFinalTerms( string Main, string Secondary, out string primaryTerm, out string secondaryTerm, bool RemoveNonASCII )
		{
			primaryTerm = (RemoveNonASCII && !string.IsNullOrEmpty(Main))? 	System.Text.RegularExpressions.Regex.Replace(Main, @"[^a-zA-Z0-9_ ]+", " ") : Main;
			secondaryTerm = Secondary;
		}
		
		#endregion

		#region Misc

		public void SetTerm (string primary)
		{
			if (!string.IsNullOrEmpty(primary))
				FinalTerm = mTerm = primary;

			OnLocalize (true);
		}

        public void SetTerm(string primary, string secondary )
        {
            if (!string.IsNullOrEmpty(primary))
                FinalTerm = mTerm = primary;
            FinalSecondaryTerm = mTermSecondary = secondary;

            OnLocalize(true);
        }

		T GetSecondaryTranslatedObj<T>( ref string mainTranslation, ref string secondaryTranslation ) where T: Object
		{
            string newMain, newSecond;

			//--[ Allow main translation to override Secondary ]-------------------
            DeserializeTranslation(mainTranslation, out newMain, out newSecond);

            T obj = null;

            if (!string.IsNullOrEmpty(newSecond))
            {
                obj = GetObject<T>(newSecond);
                if (obj != null)
                {
                    mainTranslation = newMain;
                    secondaryTranslation = newSecond;
                }
            }

            if (obj == null)
                obj = GetObject<T>(secondaryTranslation);

			return obj;
		}

		T GetObject<T>( string Translation ) where T: Object
		{
			if (string.IsNullOrEmpty (Translation))
				return null;
			T obj = GetTranslatedObject<T>(Translation);
			
			if (obj==null)
			{
				// Remove path and search by name
				//int Index = Translation.LastIndexOfAny("/\\".ToCharArray());
				//if (Index>=0)
				//{
				//	Translation = Translation.Substring(Index+1);
					obj = GetTranslatedObject<T>(Translation);
				//}
			}
			return obj;
		}

		T GetTranslatedObject<T>( string Translation ) where T: Object
		{
			T Obj = FindTranslatedObject<T>(Translation);
			/*if (Obj == null) 
				return null;
			
			if ((Obj as T) != null) 
				return Obj as T;
			
			// If the found Obj is not of type T, then try finding a component inside
			if (Obj as Component != null)
				return (Obj as Component).GetComponent(typeof(T)) as T;
			
			if (Obj as GameObject != null)
				return (Obj as GameObject).GetComponent(typeof(T)) as T;
			*/
			return Obj;
		}


		// translation format: "[secondary]value"   [secondary] is optional
		void DeserializeTranslation( string translation, out string value, out string secondary )
		{
			if (!string.IsNullOrEmpty(translation) && translation.Length>1 && translation[0]=='[')
			{
				int Index = translation.IndexOf(']');
				if (Index>0)
				{
					secondary = translation.Substring(1, Index-1);
					value = translation.Substring(Index+1);
					return;
				}
			}
			value = translation;
			secondary = string.Empty;
		}
		
		public T FindTranslatedObject<T>( string value ) where T : Object
		{
			if (string.IsNullOrEmpty(value))
				return null;

			if (TranslatedObjects!=null)
            {
			    for (int i=0, imax=TranslatedObjects.Length; i<imax; ++i)
                    if (TranslatedObjects[i] is T && value.EndsWith(TranslatedObjects[i].name, StringComparison.OrdinalIgnoreCase))
                    {
                        // Check if the value is just the name or has a path
                        if (string.Compare(value, TranslatedObjects[i].name, StringComparison.OrdinalIgnoreCase)==0)
                            return (T) TranslatedObjects[i];

                        // Check if the path matches
                        //Resources.get TranslatedObjects[i].
                    }
            }

			T obj = LocalizationManager.FindAsset(value) as T;
			if (obj)
				return obj;

			obj = ResourceManager.pInstance.GetAsset<T>(value);
			return obj;
		}

		public bool HasTranslatedObject( Object Obj )
		{
			if (Array.IndexOf (TranslatedObjects, Obj) >= 0) 
				return true;
			return ResourceManager.pInstance.HasAsset(Obj);

		}

		public void AddTranslatedObject( Object Obj )
		{
			Array.Resize (ref TranslatedObjects, TranslatedObjects.Length + 1);
			TranslatedObjects [TranslatedObjects.Length - 1] = Obj;
		}

		#endregion
	
		#region Utilities
		// This can be used to set the language when a button is clicked
		public void SetGlobalLanguage( string Language )
		{
			LocalizationManager.CurrentLanguage = Language;
		}

		#endregion
	}
}