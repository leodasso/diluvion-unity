using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace I2.Loc
{
	public partial class LanguageSource : MonoBehaviour
	{
		public enum eInputSpecialization { PC, Touch, Controller }
		#if UNITY_EDITOR
			public static string[] InputSpecializationTooltips = {
					"Translations for devices using Mouse and Keyboard  (This allows using 'click' instead of 'tap')",
					"Translations for devices mobile and touch devices  (e.g. Tap to start)",
					"Translations for devices using a Controller  (e.g. Press [A] to Start)"
			};
		#endif
		
		public static eInputSpecialization GetCurrentInputType()
		{
			#if UNITY_ANDROID || UNITY_IOS || UNITY_WP8
				return eInputSpecialization.Touch;
			#else
				return Input.GetJoystickNames ().Length>0 ? eInputSpecialization.Controller : eInputSpecialization.PC;
			#endif
		}
	}

	public enum eTermType 
	{ 
		Text, Font, Texture, AudioClip, GameObject, Sprite, Material,
		#if NGUI
			UIAtlas, UIFont,
		#endif
		#if DFGUI
			dfFont, dfAtlas, 
		#endif
		#if TK2D
			TK2dFont, TK2dCollection,
		#endif
		#if TextMeshPro || TextMeshPro_Pre53
			TextMeshPFont,
		#endif
		#if SVG
			SVGAsset,
		#endif
		Object 
	}

	public enum TranslationFlag : byte
	{
		AutoTranslated_Normal = 1,
		AutoTranslated_Touch = 2,
		AutoTranslated_All = 255
	}

	public enum eTransTag_Plural { Any, Zero, One, Many };
	public enum eTransTag_Input { Any, PC, Touch,  };


	[Serializable]
	public class TermData
	{
		public string 			Term 			= string.Empty;
		public eTermType		TermType 		= eTermType.Text;
		public string 			Description	    = string.Empty;
		public string[]			Languages		= new string[0];
		public string[]			Languages_Touch = new string[0];
		public byte[]			Flags 			= new byte[0];	// flags for each translation

		public string GetTranslation ( int idx, eTransTag_Input input=eTransTag_Input.Any, eTransTag_Plural plural=eTransTag_Plural.Any )
		{
			if (IsTouchType())
			{
				return !string.IsNullOrEmpty(Languages_Touch[idx]) ? Languages_Touch[idx] : Languages[idx];
			}
			else
			{
				return !string.IsNullOrEmpty(Languages[idx]) ? Languages[idx] : Languages_Touch[idx];
			}
		}

		public bool IsAutoTranslated( int idx, bool IsTouch )
		{
			if (IsTouch)
				return (Flags[idx] & (byte)TranslationFlag.AutoTranslated_Touch) > 0;
			else
				return (Flags[idx] & (byte)TranslationFlag.AutoTranslated_Normal) > 0;
		}

		public bool HasTouchTranslations ()
		{
			for (int i=0, imax=Languages_Touch.Length; i<imax; ++i)
				if (!string.IsNullOrEmpty(Languages_Touch[i]) && !string.IsNullOrEmpty(Languages[i]) &&
					Languages_Touch[i]!=Languages[i])
					return true;
			return false;
		}

		public void Validate ()
		{
			int nLanguages = Mathf.Max(Languages.Length, 
							 Mathf.Max(Languages_Touch.Length, Flags.Length));

			if (Languages.Length != nLanguages) 		Array.Resize(ref Languages, nLanguages);
			if (Languages_Touch.Length != nLanguages) 	Array.Resize(ref Languages_Touch, nLanguages);
			if (Flags.Length!=nLanguages) 				Array.Resize(ref Flags, nLanguages);
		}

		public static bool IsTouchType()
		{
			#if UNITY_ANDROID || UNITY_IOS || UNITY_WP8
				return true;
			#else
				return false;
			#endif
		}

		public bool IsTerm( string name, bool allowCategoryMistmatch)
		{
			if (!allowCategoryMistmatch)
				return name == Term;

			return name == LanguageSource.GetKeyFromFullTerm (Term);
		}
	};

	public enum eLanguageDataFlags
	{
		DISABLED = 1
	}
	[Serializable]
	public class LanguageData
	{
		public string Name;
		public string Code;
		public byte Flags;      // eLanguageDataFlags

		[NonSerialized]
		public bool Compressed = false;  // This will be used in the next version for only loading used Languages

		public bool IsEnabled() { return (Flags & (int)eLanguageDataFlags.DISABLED) == 0; }
	}

	[AddComponentMenu("I2/Localization/Source")]
	public partial class LanguageSource : MonoBehaviour
	{
		#region Variables

		public List<TermData> mTerms = new List<TermData>();
		public List<LanguageData> mLanguages = new List<LanguageData>();

		public bool CaseInsensitiveTerms = false;

		//This is used to overcome the issue with Unity not serializing Dictionaries
		[NonSerialized] 
		public Dictionary<string, TermData> mDictionary = new Dictionary<string, TermData>(StringComparer.Ordinal);


		public Object[] Assets;	// References to Fonts, Atlasses and other objects the localization may need

		public bool NeverDestroy = true;  	// Keep between scenes (will call DontDestroyOnLoad )

		public bool UserAgreesToHaveItOnTheScene = false;
		public bool UserAgreesToHaveItInsideThePluginsFolder = false;

		public enum MissingTranslationAction { Empty, Fallback, ShowWarning };
		public MissingTranslationAction OnMissingTranslation = MissingTranslationAction.Fallback;

		public string mTerm_AppName;

		#endregion

		#region EditorVariables
#if UNITY_EDITOR

		public string Spreadsheet_LocalFileName;
		public string Spreadsheet_LocalCSVSeparator = ",";
		public string Spreadsheet_LocalCSVEncoding = "Unicode (UTF-8)";

		#endif
		#endregion

		#region Language

		void Awake()
		{
			if (NeverDestroy)
			{
				if (ManagerHasASimilarSource())
				{
					Destroy (this);
					return;
				}
				else
				{
					if (Application.isPlaying)
						DontDestroyOnLoad (gameObject);
				}
			}
			LocalizationManager.AddSource (this);
			UpdateDictionary();
		}

		public void UpdateDictionary( bool force = false )
		{
			if (!force && mDictionary != null && mDictionary.Count == mTerms.Count)
				return;

			StringComparer comparer = (CaseInsensitiveTerms ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
			if (mDictionary.Comparer != comparer)
				mDictionary = new Dictionary<string, TermData>(comparer);
			else
				mDictionary.Clear();
			for (int i=0, imax=mTerms.Count; i<imax; ++i)
			{
				ValidateFullTerm(ref mTerms[i].Term );
				if (mTerms[i].Languages_Touch==null || mTerms[i].Languages_Touch.Length != mTerms[i].Languages.Length)
					mTerms[i].Languages_Touch = new string[mTerms[i].Languages.Length];
				mDictionary[mTerms[i].Term]= mTerms[i];
				mTerms[i].Validate();
			}
		}

		public string GetSourceName()
		{
			string s = gameObject.name;
			Transform tr = transform.parent;
			while (tr)
			{
				s = string.Concat(tr.name, "_", s);
				tr = tr.parent;
			}
			return s;
		}


		public int GetLanguageIndex( string language, bool AllowDiscartingRegion = true, bool SkipDisabled = true)
		{
			// First look for an exact match
			for (int i=0, imax=mLanguages.Count; i<imax; ++i)
				if ((!SkipDisabled || mLanguages[i].IsEnabled()) && string.Compare(mLanguages[i].Name, language, StringComparison.OrdinalIgnoreCase)==0)
					return i;

			// Then allow matching "English (Canada)" to "english"
			if (AllowDiscartingRegion)
			{
				int MostSimilar = -1;
				int BestSimilitud = 0;
				for (int i=0, imax=mLanguages.Count; i<imax; ++i)
					if (!SkipDisabled || mLanguages[i].IsEnabled())
					{
						int commonWords = GetCommonWordInLanguageNames(mLanguages[i].Name, language);
						if (commonWords>BestSimilitud)
						{
							BestSimilitud = commonWords;
							MostSimilar = i;
						}
						//if (AreTheSameLanguage(mLanguages[i].Name, language))
						//	return i;
					}
				if (MostSimilar>=0)
					return MostSimilar;
			}
			return -1;
		}

		public int GetLanguageIndexFromCode( string Code, bool exactMatch=true )
		{
			for (int i=0, imax=mLanguages.Count; i<imax; ++i)
				if (string.Compare(mLanguages[i].Code, Code, StringComparison.OrdinalIgnoreCase)==0)
					return i;

			if (!exactMatch)
			{
				// Find any match without using the Regions
				for (int i=0, imax=mLanguages.Count; i<imax; ++i)
					if (string.Compare( mLanguages[i].Code, 0, Code, 0, 2, StringComparison.OrdinalIgnoreCase )==0)
						return i;
			}

			return -1;
		}

		public static int GetCommonWordInLanguageNames(string Language1, string Language2)
		{
			if (string.IsNullOrEmpty (Language1) || string.IsNullOrEmpty (Language2))
					return 0;
			var separators = "( )-/\\".ToCharArray();
			string[] Words1 = Language1.Split(separators);
			string[] Words2 = Language2.Split(separators);

			int similitud = 0;
			foreach (var word in Words1)
				if (!string.IsNullOrEmpty(word) && Words2.Contains(word))
					similitud++;

			foreach (var word in Words2)
				if (!string.IsNullOrEmpty(word) && Words1.Contains(word))
					similitud++;

			return similitud;
		}

		public static bool AreTheSameLanguage(string Language1, string Language2)
		{
			Language1 = GetLanguageWithoutRegion(Language1);
			Language2 = GetLanguageWithoutRegion(Language2);
			return (string.Compare(Language1, Language2, StringComparison.OrdinalIgnoreCase)==0);
		}

		public static string GetLanguageWithoutRegion(string Language)
		{
			int Index = Language.IndexOfAny("(/\\[,{".ToCharArray());
			if (Index<0)
				return Language;
			else
				return Language.Substring(0, Index).Trim();
		}

		public void AddLanguage( string LanguageName, string LanguageCode )
		{
			if (GetLanguageIndex(LanguageName, false)>=0)
				return;

			LanguageData Lang = new LanguageData();
				Lang.Name = LanguageName;
				Lang.Code = LanguageCode;
			mLanguages.Add (Lang);

			int NewSize = mLanguages.Count;
			for (int i=0, imax=mTerms.Count; i<imax; ++i)
			{
				Array.Resize(ref mTerms[i].Languages, NewSize);
				Array.Resize(ref mTerms[i].Languages_Touch, NewSize);
				Array.Resize(ref mTerms[i].Flags, NewSize);
			}
			#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(this);
			#endif
		}

		public void RemoveLanguage( string LanguageName )
		{
			int LangIndex = GetLanguageIndex(LanguageName);
			if (LangIndex<0)
				return;

			int nLanguages = mLanguages.Count;
			for (int i=0, imax=mTerms.Count; i<imax; ++i)
			{
				for (int j=LangIndex+1; j<nLanguages; ++j)
				{
					mTerms[i].Languages[j-1] = mTerms[i].Languages[j];
					mTerms[i].Languages_Touch[j-1] = mTerms[i].Languages_Touch[j];
					mTerms[i].Flags[j-1] = mTerms[i].Flags[j];
				}
				Array.Resize(ref mTerms[i].Languages, nLanguages-1);
				Array.Resize(ref mTerms[i].Languages_Touch, nLanguages-1);
				Array.Resize(ref mTerms[i].Flags, nLanguages-1);
			}
			mLanguages.RemoveAt(LangIndex);
			#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(this);
			#endif
		}

		public List<string> GetLanguages( bool skipDisabled = true)
		{
			List<string> Languages = new List<string>();
			for (int j = 0, jmax = mLanguages.Count; j < jmax; ++j)
			{
				if (!skipDisabled || mLanguages[j].IsEnabled())
					Languages.Add(mLanguages[j].Name);
			}
			return Languages;
		}

		public List<string> GetLanguagesCode(bool allowRegions = true, bool skipDisabled = true)
		{
			List<string> Languages = new List<string>();
			for (int j = 0, jmax = mLanguages.Count; j < jmax; ++j)
			{
				if (skipDisabled && !mLanguages[j].IsEnabled())
					continue;

				var code = mLanguages[j].Code;

				if (!allowRegions && code != null && code.Length > 2)
					code = code.Substring(0, 2);

				if (!string.IsNullOrEmpty(code) && !Languages.Contains(code))
					Languages.Add(code);
			}
			return Languages;
		}

		public bool IsLanguageEnabled(string Language)
		{
			int idx = GetLanguageIndex(Language, false);
			return idx >= 0 && mLanguages[idx].IsEnabled();
		}

		public string GetTermTranslation (string term)
		{
			string Translation;
			if (TryGetTermTranslation(term, out Translation))
				return Translation;

			return string.Empty;
		}

		public bool TryGetTermTranslation (string term, out string Translation)
		{
			int Index = GetLanguageIndex(LocalizationManager.CurrentLanguage, SkipDisabled:false);
			if (Index>=0/* && mLanguages[Index].IsEnabled()*/) 
			{
				TermData data = GetTermData(term);
				if (data!=null)
				{
					Translation = data.GetTranslation(Index);

					// "---" is a code to define that the translation is meant to be empty
					if (Translation == "---")
					{
						Translation = string.Empty;
						return true;
					}
					else
					if (!string.IsNullOrEmpty(Translation))
					{
						// has a valid translation
						return true;
					}
					else
						Translation = null;
				}

				if (OnMissingTranslation == MissingTranslationAction.ShowWarning)
				{
					Translation = string.Format("<!-Missing Translation [{0}]-!>", term);
					return false;
				}
				else
				if (OnMissingTranslation == MissingTranslationAction.Fallback && data!=null)
				{
					for (int i = 0; i < mLanguages.Count; ++i)
						if (i != Index && mLanguages[i].IsEnabled())
						{
							Translation = data.GetTranslation(i);
							if (Translation != null)
								return true;
						}
				}
			}

			Translation = null;
			return false;
		}

		public TermData AddTerm( string term )
		{
			return AddTerm (term, eTermType.Text);
		}

		public TermData GetTermData( string term, bool allowCategoryMistmatch = false )
		{
			if (string.IsNullOrEmpty(term))
				return null;
			
			if (mDictionary.Count==0)// != mTerms.Count)
				UpdateDictionary();

			TermData data;
			if (mDictionary.TryGetValue(term, out data))
				return data;

			TermData d = null;
			if (allowCategoryMistmatch)
			{
				var keyPart = GetKeyFromFullTerm (term);
				foreach (var kvp in mDictionary)
					if (kvp.Value.IsTerm (keyPart, true))
					{
						if (d == null)
							d = kvp.Value;
						else
							return null;
					}
			}
			return d;
		}

		public bool ContainsTerm(string term)
		{
			return (GetTermData(term)!=null);
		}

		public List<string> GetTermsList ( string Category = null )
		{
			if (mDictionary.Count != mTerms.Count)
				UpdateDictionary();
			if (string.IsNullOrEmpty( Category ))
				return new List<string>( mDictionary.Keys );
			else
			{
				var terms = new List<string>();
				for (int i=0; i<mTerms.Count; ++i)
				{
					var term = mTerms[i];
					if (GetCategoryFromFullTerm( term.Term ) == Category)
						terms.Add( term.Term );
				}
				return terms;
			}
		}

		public  TermData AddTerm( string NewTerm, eTermType termType, bool SaveSource = true )
		{
			ValidateFullTerm( ref NewTerm );
			NewTerm = NewTerm.Trim ();

			if (mLanguages.Count == 0) 
				AddLanguage ("English", "en");

			// Don't duplicate Terms
			TermData data = GetTermData(NewTerm);
			if (data==null) 
			{
				data = new TermData();
				data.Term = NewTerm;
				data.TermType = termType;
				data.Languages = new string[ mLanguages.Count ];
				data.Languages_Touch = new string[ mLanguages.Count ];
				data.Flags = new byte[ mLanguages.Count ];
				mTerms.Add (data);
				mDictionary.Add ( NewTerm, data);
				#if UNITY_EDITOR
				if (SaveSource)
				{
					UnityEditor.EditorUtility.SetDirty (this);
					UnityEditor.AssetDatabase.SaveAssets();
				}
				#endif
			}

			return data;
		}

		public void RemoveTerm( string term )
		{
			for (int i=0, imax=mTerms.Count; i<imax; ++i)
				if (mTerms[i].Term==term)
				{
					mTerms.RemoveAt(i);
					mDictionary.Remove(term);
					return;
				}
		}

		public static void ValidateFullTerm( ref string Term )
		{
			Term = Term.Replace('\\', '/');
			Term = Term.Trim();
			if (Term.StartsWith(EmptyCategory, StringComparison.Ordinal))
			{
				if (Term.Length>EmptyCategory.Length && Term[EmptyCategory.Length]=='/')
					Term = Term.Substring(EmptyCategory.Length+1);
			}
		}


		public bool IsEqualTo( LanguageSource Source )
		{
			if (Source.mLanguages.Count != mLanguages.Count)
				return false;

			for (int i=0, imax=mLanguages.Count; i<imax; ++i)
				if (Source.GetLanguageIndex( mLanguages[i].Name ) < 0)
					return false;

			if (Source.mTerms.Count != mTerms.Count)
				return false;

			for (int i=0; i<mTerms.Count; ++i)
				if (Source.GetTermData(mTerms[i].Term)==null)
					return false;

			return true;
		}

		internal bool ManagerHasASimilarSource()
		{
			for (int i=0, imax=LocalizationManager.Sources.Count; i<imax; ++i)
			{
				LanguageSource source = (LocalizationManager.Sources[i] as LanguageSource);
				if (source!=null && source.IsEqualTo(this) && source!=this)
					return true;
			}
			return false;
		}

		public void ClearAllData()
		{
			mTerms.Clear ();
			mLanguages.Clear ();
			mDictionary.Clear();
		}

		#endregion

		#region Assets
		
		public Object FindAsset( string Name )
		{
			if (Assets!=null)
			{
				for (int i=0, imax=Assets.Length; i<imax; ++i)
					if (Assets[i]!=null && Name.EndsWith( Assets[i].name, StringComparison.OrdinalIgnoreCase))
						return Assets[i];
			}
			return null;
		}
		
		public bool HasAsset( Object Obj )
		{
			return Array.IndexOf (Assets, Obj) >= 0;
		}

		public void AddAsset( Object Obj )
		{
			Array.Resize (ref Assets, Assets.Length + 1);
			Assets [Assets.Length - 1] = Obj;
		}

		
		#endregion
	}
}