using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Globalization;
using System.Collections;

namespace I2.Loc
{
	public static class LocalizationManager
	{
		#region Variables: CurrentLanguage
		
		public static string CurrentLanguage
		{
			get {
				InitializeIfNeeded();
				return mCurrentLanguage;
			}
			set {
				InitializeIfNeeded();
				string SupportedLanguage = GetSupportedLanguage( value );
				if (!string.IsNullOrEmpty(SupportedLanguage) && mCurrentLanguage != SupportedLanguage)
				{
					SetLanguageAndCode( SupportedLanguage, GetLanguageCode(SupportedLanguage));
				}
			}
		}
		public static string CurrentLanguageCode
		{
			get { 
				InitializeIfNeeded();
				return mLanguageCode; }
			set {
				InitializeIfNeeded();
				if (mLanguageCode!=value)
				{
					string LanName = GetLanguageFromCode( value );
					if (!string.IsNullOrEmpty(LanName))
						SetLanguageAndCode( LanName, value );
				}
			}
		}

		// "English (United States)" (get returns "United States") 
		// when set "Canada", the new language code will be "English (Canada)"
		public static string CurrentRegion
		{
			get { 
				var Lan = CurrentLanguage;
				int idx = Lan.IndexOfAny ("/\\".ToCharArray ());
				if (idx>0)
					return Lan.Substring(idx+1);

				idx = Lan.IndexOfAny ("[(".ToCharArray ());
				int idx2 = Lan.LastIndexOfAny ("])".ToCharArray ());
				if (idx>0 && idx!=idx2)
					return Lan.Substring(idx+1, idx2-idx-1);
				else
					return string.Empty;
			}
			set {
				var Lan = CurrentLanguage;
				int idx = Lan.IndexOfAny ("/\\".ToCharArray ());
				if (idx>0)
				{
					CurrentLanguage = Lan.Substring(idx+1) + value;
					return;
				}
				
				idx = Lan.IndexOfAny ("[(".ToCharArray ());
				int idx2 = Lan.LastIndexOfAny ("])".ToCharArray ());
				if (idx>0 && idx!=idx2)
					Lan = Lan.Substring(idx);

				CurrentLanguage = Lan + "(" + value +")";
			}
		}

		// "en-US" (get returns "US") (when set "CA", the new language code will be "en-CA")
		public static string CurrentRegionCode
		{
			get { 
					var code = CurrentLanguageCode;
					int idx = code.IndexOfAny (" -_/\\".ToCharArray ());
					return idx < 0 ? string.Empty : code.Substring (idx + 1);
				}
			set {
				var code = CurrentLanguageCode;
				int idx = code.IndexOfAny (" -_/\\".ToCharArray ());
				if (idx > 0)
					code = code.Substring (0, idx);

				CurrentLanguageCode = code + "-" + value;
			}
		}

		static string mCurrentLanguage;
		static string mLanguageCode;
		static bool mChangeCultureInfo = false;

		public static bool IsRight2Left = false;

		static void InitializeIfNeeded()
		{
			if (string.IsNullOrEmpty(mCurrentLanguage) || Sources.Count==0)
			{
				UpdateSources();
				SelectStartupLanguage();
			}
		}

		public static void SetLanguageAndCode( string LanguageName, string LanguageCode, bool RememberLanguage = true, bool Force = false )
		{
			if (mCurrentLanguage != LanguageName || mLanguageCode != LanguageCode || Force)
			{
				if (RememberLanguage)
					PlayerPrefs.SetString ("I2 Language", LanguageName);
				mCurrentLanguage = LanguageName;
				mLanguageCode = LanguageCode;
				if (mChangeCultureInfo)
					SetCurrentCultureInfo();
				else
					IsRight2Left = IsRTL(mLanguageCode);
				LocalizeAll(Force);
			}
		}

		static CultureInfo GetCulture( string code )
		{
			#if !NETFX_CORE
				try
				{
					return CultureInfo.CreateSpecificCulture(code);
				}
				catch(System.Exception)
				{
					return CultureInfo.InvariantCulture;
				}
			#else
				return CultureInfo.InvariantCulture;
			#endif
		}

		public static void EnableChangingCultureInfo( bool bEnable )
		{
			if (!mChangeCultureInfo && bEnable)
				SetCurrentCultureInfo();
			mChangeCultureInfo = bEnable;
		}

		static void SetCurrentCultureInfo()
		{
			#if NETFX_CORE
				IsRight2Left = IsRTL (mLanguageCode);
			#else
				System.Threading.Thread.CurrentThread.CurrentCulture = GetCulture(mLanguageCode);
				IsRight2Left = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft;  //IsRTL (mLanguageCode);
			#endif
		}


		static void SelectStartupLanguage()
		{
			// Use the system language if there is a source with that language, 
			// or pick any of the languages provided by the sources

			string SavedLanguage = PlayerPrefs.GetString ("I2 Language", string.Empty);
			string SysLanguage = Application.systemLanguage.ToString();
			if (SysLanguage=="ChineseSimplified") SysLanguage = "Chinese (Simplified)";
			if (SysLanguage=="ChineseTraditional") SysLanguage = "Chinese (Traditional)";

			// Try selecting the System Language
			// But fallback to the first language found  if the System Language is not available in any source

			if (HasLanguage (SavedLanguage, Initialize:false))
			{
				SetLanguageAndCode(SavedLanguage, GetLanguageCode(SavedLanguage));
				return;
			}

			// Check if the device language is supported. 
			// Also recognize when not region is set ("English (United State") will be used if sysLanguage is "English")
			string ValidLanguage = GetSupportedLanguage(SysLanguage);
			if (!string.IsNullOrEmpty(ValidLanguage))
			{
				SetLanguageAndCode( ValidLanguage, GetLanguageCode(ValidLanguage), false);
				return;
			}

			//--[ Use first language that its not disabled ]-----------
			for (int i=0, imax=Sources.Count; i<imax; ++i)
				if (Sources[i].mLanguages.Count>0)
				{
					for (int j = 0; j < Sources[i].mLanguages.Count; ++j)
						if (Sources[i].mLanguages[j].IsEnabled())
						{
							SetLanguageAndCode(Sources[i].mLanguages[j].Name, Sources[i].mLanguages[j].Code, false);
							return;
						}
				}
		}

#endregion

		#region Variables: Misc

		//public static Dictionary<string, string> Terms = new Dictionary<string, string>();
		public static List<LanguageSource> Sources = new List<LanguageSource>();
		public static string[] GlobalSources = {"I2Languages"};

		public delegate void OnLocalizeCallback ();
		public static event OnLocalizeCallback OnLocalizeEvent;

		public static List<ILocalizationParamsManager> ParamManagers = new List<ILocalizationParamsManager>();
		static bool mLocalizeIsScheduled = false;
		static bool mLocalizeIsScheduledWithForcedValue = false;
		#endregion

		#region Localization

		public static string GetTermTranslation(string Term) { return GetTermTranslation(Term, IsRight2Left, 0, true, false, null); }
		public static string GetTermTranslation(string Term, bool FixForRTL) { return GetTermTranslation(Term, FixForRTL, 0, true, false, null); }
		public static string GetTermTranslation(string Term, bool FixForRTL, int maxLineLengthForRTL) { return GetTermTranslation(Term, FixForRTL, maxLineLengthForRTL, true, false, null); }
		public static string GetTermTranslation(string Term, bool FixForRTL, int maxLineLengthForRTL, bool ignoreRTLnumbers) { return GetTermTranslation(Term, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, false, null); }
		public static string GetTermTranslation(string Term, bool FixForRTL, int maxLineLengthForRTL, bool ignoreRTLnumbers, bool applyParameters, GameObject localParametersRoot)
		{
			string Translation = null;
			TryGetTermTranslation(Term, out Translation, FixForRTL, maxLineLengthForRTL, ignoreRTLnumbers, applyParameters, localParametersRoot);

			return Translation;
		}

		public static bool TryGetTermTranslation(string Term, out string Translation) { return TryGetTermTranslation(Term, out Translation, false, 0, true, false, null); }
		public static bool TryGetTermTranslation(string Term, out string Translation, bool FixForRTL ) { return TryGetTermTranslation (Term, out Translation, FixForRTL, 0, true, false, null); }
		public static bool TryGetTermTranslation(string Term, out string Translation, bool FixForRTL, int maxLineLengthForRTL, bool ignoreRTLnumbers, bool applyParameters, GameObject localParametersRoot )
		{
			Translation = null;
			if (string.IsNullOrEmpty(Term))
				return false;

			InitializeIfNeeded();

			for (int i=0, imax=Sources.Count; i<imax; ++i)
				if (Sources[i].TryGetTermTranslation (Term, out Translation))
				{
					if (applyParameters)
						ApplyLocalizationParams(ref Translation, localParametersRoot);

					if (IsRight2Left && FixForRTL)
						Translation = ApplyRTLfix(Translation, maxLineLengthForRTL, ignoreRTLnumbers);
					return true;
				}

			return false;
		}

		public static string GetAppName( string languageCode )
		{
			if (!string.IsNullOrEmpty( languageCode ))
			{
				for (int i=0; i<Sources.Count; ++i)
				{
					if (string.IsNullOrEmpty( Sources[i].mTerm_AppName ))
						continue;

					int langIdx = Sources[i].GetLanguageIndexFromCode( languageCode, false );
					if (langIdx<0)
						continue;

					var termData = Sources[i].GetTermData( Sources[i].mTerm_AppName );
					if (termData==null)
						continue;

					var appName = termData.GetTranslation( langIdx );
					if (!string.IsNullOrEmpty(appName))
						return appName;
				}
			}

			return Application.productName;
		}

		static bool FindNextTag( string line, int iStart, out int tagStart, out int tagEnd)
		{
			tagStart = -1;
			tagEnd = -1;
			int len = line.Length;

			// Find where the tag starts
			for (tagStart = iStart; tagStart < len; ++tagStart)
				if (line[tagStart] == '[' || line[tagStart] == '(' || line[tagStart] == '{')
					break;

			if (tagStart == len)
				return false;

			bool isArabic = false;
			for (tagEnd = tagStart + 1; tagEnd < len; ++tagEnd)
			{
				char c = line[tagEnd];
				if (c == ']' || c == ')' || c == '}')
				{
					if (isArabic) return FindNextTag(line, tagEnd + 1, out tagStart, out tagEnd);
						     else return true;
				}
				if (c > 255) isArabic = true;
			}

			// there is an open, but not close character
			return false;
		}

		public static string ApplyRTLfix( string line ) { return ApplyRTLfix (line, 0, true); }
		public static string ApplyRTLfix( string line, int maxCharacters, bool ignoreNumbers )
		{
			if (string.IsNullOrEmpty(line))
				return line;

			// Fix !, ? and . signs not set correctly
			char firstC = line[0];
			if (firstC == '!' || firstC == '.' || firstC == '?')
				line = line.Substring(1) + firstC;

			int tagStart = -1, tagEnd = 0;
			// Find any non-RTL character at the beggining because those need to be converted to RTL direction (place them at the end)
			//int ifirst = 0;
			//while (ifirst<line.Length && line[ifirst] < 255)
			//	ifirst++;

			// unless there is tag there
			//if (FindNextTag(line, 0, out tagStart, out tagEnd))
			//	ifirst = ifirst < tagStart ? ifirst : tagStart;

			//string linePrefix = ifirst>0 ? line.Substring(0, ifirst) : string.Empty;
			//line = line.Substring(ifirst);

			//int ilast = line.Length-1;
			//string linePostfix = "";
			//while (ilast > 0 && line[ilast] < 255)
			//{
			//	char c = line[ilast];
			//	switch (c)
			//	{
			//		case '(': c = ')'; break;
			//		case ')': c = '('; break;
			//		case '[': c = ']'; break;
			//		case ']': c = '['; break;
			//		case '{': c = '}'; break;
			//		case '}': c = '{'; break;
			//	}
			//	linePostfix += c;
			//	ilast--;
			//}
			//line = line.Substring(0, ilast);



			// Find all Tags (and Numbers if ignoreNumbers is true)
			int tagBase = 40000;
			tagEnd = 0;
			var tags = new List<string>();
			while (FindNextTag(line, tagEnd, out tagStart, out tagEnd))
			{
				string tag = "@@"+(char)(tagBase + tags.Count)+"@@";
				tags.Add(line.Substring(tagStart, tagEnd - tagStart + 1));

				line = line.Substring(0, tagStart) + tag + line.Substring(tagEnd + 1);
				tagEnd = tagStart + 5;
			}

			// Split into lines and fix each line

			if (maxCharacters <= 0)
			{
				line = ArabicSupport.ArabicFixer.Fix(line, true, !ignoreNumbers);
				//line = linePrefix + linePostfix + line;
			}
			else
			{
				// Split into lines of maximum length
				var regex = new Regex(".{0," + maxCharacters + "}(\\s+|$)", RegexOptions.Multiline);
				line = line.Replace("\r\n", "\n");
				line = regex.Replace(line, "$0\n");

				line = line.Replace("\n\n", "\n");
				line = line.TrimEnd('\n');
				//if (line.EndsWith("\n\n"))
				//  line = line.Substring(0, line.Length - 2);

				// Apply the RTL fix for each line
				var lines = line.Split('\n');
				for (int i = 0, imax = lines.Length; i < imax; ++i)
					lines[i] = ArabicSupport.ArabicFixer.Fix(lines[i], true, !ignoreNumbers);
				//lines[lines.Length-1] = linePrefix + linePostfix + lines[lines.Length - 1];

				line = string.Join("\n", lines);
			}


			// Restore all tags

			for (int i = 0; i < tags.Count; i++)
			{
				var len = line.Length;
				for (int j = 0; j < len; ++j)
					if (line[j] == '@' && line[j+1] == '@' && line[j+2]>= tagBase && line[j+3] == '@' && line[j + 4] == '@')
					{
						int idx = line[j + 2] - tagBase;
						if (idx % 2 == 0) idx++;
									 else idx--;
						if (idx >= tags.Count) idx = tags.Count - 1;

						line = line.Substring(0, j) + tags[idx] + line.Substring(j + 5);

						break;
					}
			}
			return line;
		}

		public static string ApplyRTLfix1(string line, int maxCharacters, bool ignoreNumbers)
		{
			// pattern = ignoreNumbers ? (all non ascii characters + numbers) : (all non ASCII characters)   
			// tip: the character before the 0 is /  and the character after the 9 is :   (that's why the second pattern is [0x0-/  and  :-0xff]
			string pattern = ignoreNumbers ? @"(\s|[^\x00-\xff])+" : @"(\s|[^\x00-\/:-\xff])+";
			var regexPattern = new Regex(pattern);

			if (maxCharacters <= 0)
			{
				// Apply arabic fixer to all non ascii characters
				line = regexPattern.Replace(line, (m) => ReverseText(ArabicSupport.ArabicFixer.Fix(m.Value)));

			}
			else
			{
				// Split into lines of maximum length
				var regex = new Regex(".{0," + maxCharacters + "}(\\s+|$)", RegexOptions.Multiline);
				line = line.Replace("\r\n", "\n");
				line = regex.Replace(line, "$0\n");

				line = line.Replace("\n\n", "\n");
				line = line.TrimEnd('\n');
				//if (line.EndsWith("\n\n"))
				//  line = line.Substring(0, line.Length - 2);

				// Apply the RTL fix for each line
				var lines = line.Split('\n');
				for (int i = 0, imax = lines.Length; i < imax; ++i)
					lines[i] = regexPattern.Replace(lines[i], (m) => ReverseText(ArabicSupport.ArabicFixer.Fix(m.Value)));
				line = string.Join("\n", lines);
			}

			return line;
		}


		static string ReverseText(string source)
		{
			return source;
			/*var len = source.Length;
			var output = new char[len];
			for (var i = 0; i < len; i++)
			{
				output[(len - 1) - i] = source[i];
			}
			return new string(output);*/
		}

public static string FixRTL_IfNeeded(string text, int maxCharacters = 0, bool ignoreNumber=false)
		{
			if (IsRight2Left)
				return ApplyRTLfix(text, maxCharacters, ignoreNumber);
			else
				return text;
		}

		public static void LocalizeAll(bool Force = false)
		{
			if (!Application.isPlaying)
			{
				DoLocalizeAll(Force);
				return;
			}
			mLocalizeIsScheduledWithForcedValue |= Force;
			if (mLocalizeIsScheduled)
				return;
			I2.CoroutineManager.Start(Coroutine_LocalizeAll());
		}

		static IEnumerator Coroutine_LocalizeAll()
		{
			mLocalizeIsScheduled = true;
			yield return null;
			mLocalizeIsScheduled = false;
			DoLocalizeAll(mLocalizeIsScheduledWithForcedValue);
			mLocalizeIsScheduledWithForcedValue = false;
		}

		static void DoLocalizeAll(bool Force = false)
		{
			Localize[] Locals = (Localize[])Resources.FindObjectsOfTypeAll( typeof(Localize) );
			for (int i=0, imax=Locals.Length; i<imax; ++i)
			{
				Localize local = Locals[i];
				//if (ObjectExistInScene (local.gameObject))
				local.OnLocalize(Force);
			}
			if (OnLocalizeEvent != null)
				OnLocalizeEvent ();
			ResourceManager.pInstance.CleanResourceCache();
		}



		internal static void ApplyLocalizationParams( ref string translation, GameObject root )
		{
			if (translation == null)
				return;

			var regex = new Regex(@"{\[(.*?)\]}");
			var regexMatches = regex.Matches(translation);
			for (int i = 0, nMatches = regexMatches.Count; i < nMatches; ++i)
			{
				var match = regexMatches[i];
				var param = match.Groups[match.Groups.Count - 1].Value;
				var result = GetLocalizationParam(param, root);
				if (result!=null)
					translation = translation.Replace(match.Value, result);
			}
		}

		internal static string GetLocalizationParam(string ParamName, GameObject root)
		{
			string result = null;
			if (root)
			{
				var components = root.GetComponents<MonoBehaviour>();
				for (int i=0, imax=components.Length; i<imax; ++i)
				{
					var manager = components[i] as ILocalizationParamsManager;
					if (manager != null)
					{
						result = manager.GetParameterValue(ParamName);
						if (result != null)
							return result;
					}
				}
			}

			for (int i = 0, imax = ParamManagers.Count; i < imax; ++i)
			{
				result = ParamManagers[i].GetParameterValue(ParamName);
				if (result!=null)
					return result;
			}

			return null;
		}

#endregion

		#region Sources

		public static bool UpdateSources()
		{
			UnregisterDeletededSources();
			RegisterSourceInResources();
			RegisterSceneSources();
			return Sources.Count>0;
		}

		static void UnregisterDeletededSources()
		{
			// Delete sources that were part of another scene and not longer available
			for (int i=Sources.Count-1; i>=0; --i)
				if (Sources[i] == null)
					RemoveSource( Sources[i] );
		}

		static void RegisterSceneSources()
		{
			LanguageSource[] sceneSources = (LanguageSource[])Resources.FindObjectsOfTypeAll( typeof(LanguageSource) );
			for (int i=0, imax=sceneSources.Length; i<imax; ++i)
				if (!Sources.Contains(sceneSources[i]))
				{
					AddSource( sceneSources[i] );
				}
		}		

		static void RegisterSourceInResources()
		{
			// Find the Source that its on the Resources Folder
			foreach (string SourceName in GlobalSources)
			{
				GameObject Prefab = (ResourceManager.pInstance.GetAsset<GameObject>(SourceName));
				LanguageSource GlobalSource = (Prefab ? Prefab.GetComponent<LanguageSource>() : null);
				
				if (GlobalSource && !Sources.Contains(GlobalSource))
					AddSource( GlobalSource );
			}
		}		

		internal static void AddSource ( LanguageSource Source )
		{
			if (Sources.Contains (Source))
				return;

			Sources.Add( Source );
#if !UNITY_EDITOR || I2LOC_AUTOSYNC_IN_EDITOR
			if (Source.HasGoogleSpreadsheet() && Source.GoogleUpdateFrequency != LanguageSource.eGoogleUpdateFrequency.Never)
			{
				Source.Import_Google_FromCache();
				if (Source.GoogleUpdateDelay > 0)
						CoroutineManager.pInstance.StartCoroutine( Delayed_Import_Google(Source, Source.GoogleUpdateDelay) );
				else
					Source.Import_Google();
			}
#endif

			if (Source.mDictionary.Count==0)
				Source.UpdateDictionary(true);
		}

		static IEnumerator Delayed_Import_Google ( LanguageSource source, float delay )
		{
			yield return new WaitForSeconds( delay );
			source.Import_Google();
		}

		internal static void RemoveSource (LanguageSource Source )
		{
			//Debug.Log ("RemoveSource " + Source+" " + Source.GetInstanceID());
			Sources.Remove( Source );
		}

		public static bool IsGlobalSource( string SourceName )
		{
			return System.Array.IndexOf(GlobalSources, SourceName)>=0;
		}

		public static bool HasLanguage( string Language, bool AllowDiscartingRegion = true, bool Initialize=true, bool SkipDisabled=true )
		{
			if (Initialize)
				InitializeIfNeeded();

			// First look for an exact match
			for (int i=0, imax=Sources.Count; i<imax; ++i)
				if (Sources[i].GetLanguageIndex(Language, false, SkipDisabled) >=0)
					return true;

			// Then allow matching "English (Canada)" to "english"
			if (AllowDiscartingRegion)
			{
				for (int i=0, imax=Sources.Count; i<imax; ++i)
					if (Sources[i].GetLanguageIndex(Language, true, SkipDisabled) >=0)
						return true;
			}
			return false;
		}

		// Returns the provided language or a similar one without the Region 
		//(e.g. "English (Canada)" could be mapped to "english" or "English (United States)" if "English (Canada)" is not found
		public static string GetSupportedLanguage( string Language )
		{
			// First look for an exact match
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				int Idx = Sources[i].GetLanguageIndex(Language, false);
				if (Idx>=0)
					return Sources[i].mLanguages[Idx].Name;
			}
			
			// Then allow matching "English (Canada)" to "english"
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				int Idx = Sources[i].GetLanguageIndex(Language, true);
				if (Idx>=0)
					return Sources[i].mLanguages[Idx].Name;
			}

			return string.Empty;
		}

		public static string GetLanguageCode( string Language )
		{
			if (Sources.Count==0)
				UpdateSources();
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				int Idx = Sources[i].GetLanguageIndex(Language);
				if (Idx>=0)
					return Sources[i].mLanguages[Idx].Code;
			}
			return string.Empty;
		}

		public static string GetLanguageFromCode( string Code, bool exactMatch=true )
		{
			if (Sources.Count==0)
				UpdateSources();
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				int Idx = Sources[i].GetLanguageIndexFromCode(Code, exactMatch);
				if (Idx>=0)
					return Sources[i].mLanguages[Idx].Name;
			}
			return string.Empty;
		}


		public static List<string> GetAllLanguages ( bool SkipDisabled = true )
		{
			if (Sources.Count==0)
				UpdateSources();
			List<string> Languages = new List<string> ();
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				Languages.AddRange(Sources[i].GetLanguages(SkipDisabled).Where(x=>!Languages.Contains(x)));
			}
			return Languages;
		}

		public static List<string> GetAllLanguagesCode(bool allowRegions=true, bool SkipDisabled = true)
		{
			List<string> Languages = new List<string>();
			for (int i = 0, imax = Sources.Count; i < imax; ++i)
			{
				Languages.AddRange(Sources[i].GetLanguagesCode(allowRegions, SkipDisabled).Where(x => !Languages.Contains(x)));
			}
			return Languages;
		}

		public static bool IsLanguageEnabled(string Language)
		{
			for (int i = 0, imax = Sources.Count; i < imax; ++i)
				if (!Sources[i].IsLanguageEnabled(Language))
					return false;
			return true;
		}


		public static List<string> GetCategories ()
		{
			List<string> Categories = new List<string> ();
			for (int i=0, imax=Sources.Count; i<imax; ++i)
				Sources[i].GetCategories(false, Categories);
			return Categories;
		}



		public static List<string> GetTermsList ( string Category = null )
		{
			if (Sources.Count==0)
				UpdateSources();

			if (Sources.Count==1)
				return Sources[0].GetTermsList(Category);

			HashSet<string> Terms = new HashSet<string> ();
			for (int i=0, imax=Sources.Count; i<imax; ++i)
				Terms.UnionWith( Sources[i].GetTermsList(Category) );
			return new List<string>(Terms);
		}

		public static TermData GetTermData( string term )
		{
            InitializeIfNeeded();

			TermData data;
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				data = Sources[i].GetTermData(term);
				if (data!=null)
					return data;
			}

			return null;
		}

		public static LanguageSource GetSourceContaining( string term, bool fallbackToFirst = true )
		{
			if (!string.IsNullOrEmpty(term))
			{
				for (int i=0, imax=Sources.Count; i<imax; ++i)
				{
					if (Sources[i].GetTermData(term) != null)
						return Sources[i];
				}
			}
			
			return ((fallbackToFirst && Sources.Count>0) ? Sources[0] :  null);
		}
		

#endregion

		public static Object FindAsset (string value)
		{
			for (int i=0, imax=Sources.Count; i<imax; ++i)
			{
				Object Obj = Sources[i].FindAsset(value);
				if (Obj)
					return Obj;
			}
			return null;
		}

		public static string GetVersion()
		{
			return "2.7.0 f1";
		}

		public static int GetRequiredWebServiceVersion()
		{
			return 4;
		}

		public static string GetWebServiceURL( LanguageSource source = null )
		{
			if (source != null && !string.IsNullOrEmpty(source.Google_WebServiceURL))
				return source.Google_WebServiceURL;

			for (int i = 0; i < Sources.Count; ++i)
				if (Sources[i] != null && !string.IsNullOrEmpty(Sources[i].Google_WebServiceURL))
					return Sources[i].Google_WebServiceURL;
			return string.Empty;
		}

#region Left to Right Languages

		static string[] LanguagesRTL = {"ar-DZ", "ar","ar-BH","ar-EG","ar-IQ","ar-JO","ar-KW","ar-LB","ar-LY","ar-MA","ar-OM","ar-QA","ar-SA","ar-SY","ar-TN","ar-AE","ar-YE",
										"he","ur","ji"};

		public static bool IsRTL(string Code)
		{
			return System.Array.IndexOf(LanguagesRTL, Code)>=0;
		}

#endregion

#if UNITY_EDITOR
		// This function should only be called from within the Localize Inspector to temporaly preview that Language

		public static void PreviewLanguage(string NewLanguage)
		{
			mCurrentLanguage = NewLanguage;
			mLanguageCode = GetLanguageCode(mCurrentLanguage);
			IsRight2Left = IsRTL(mLanguageCode);

		}
#endif
	}

	public class TermsPopup : PropertyAttribute {}
}