using UnityEngine;
using System;
using System.Collections;

namespace I2.Loc
{
	public partial class LanguageSource
	{
		#region Variables

		public string Google_WebServiceURL;
		public string Google_SpreadsheetKey;
		public string Google_SpreadsheetName;
		public string Google_LastUpdatedVersion;

		public enum eGoogleUpdateFrequency { Always, Never, Daily, Weekly, Monthly, OnlyOnce }
		public eGoogleUpdateFrequency GoogleUpdateFrequency = eGoogleUpdateFrequency.Weekly;

		public float GoogleUpdateDelay = 5; // How many second to delay downloading data from google (to avoid lag on the startup)

		public event Action<LanguageSource, bool, string> Event_OnSourceUpdateFromGoogle;    // (LanguageSource, bool ReceivedNewData, string errorMsg)
		
		#endregion

		#region Connection to Web Service 

		public static void FreeUnusedLanguages()
		{
			var source    = LocalizationManager.Sources[0];
			int langIndex = source.GetLanguageIndex(LocalizationManager.CurrentLanguage);

			for (int i=0; i<source.mTerms.Count; ++i)
			{
				var term = source.mTerms[i];
				for (int j=0; j<term.Languages.Length; j++)
				{
					if (j != langIndex)
						term.Languages[j] = term.Languages_Touch[j] = null;
				}
			}
		}

		public void Import_Google_FromCache()
		{
			if (GoogleUpdateFrequency==eGoogleUpdateFrequency.Never)
				return;
			
			if (!Application.isPlaying)
					return;
					
			string PlayerPrefName = GetSourcePlayerPrefName();
			string I2SavedData = PlayerPrefs.GetString("I2Source_"+PlayerPrefName, null);
			if (string.IsNullOrEmpty (I2SavedData))
				return;

			//--[ Compare with current version ]-----
			bool shouldUpdate = false;
			string savedSpreadsheetVersion = Google_LastUpdatedVersion;
			if (PlayerPrefs.HasKey("I2SourceVersion_"+PlayerPrefName))
			{
				savedSpreadsheetVersion = PlayerPrefs.GetString("I2SourceVersion_"+PlayerPrefName, Google_LastUpdatedVersion);
//				Debug.Log (Google_LastUpdatedVersion + " - " + savedSpreadsheetVersion);
				shouldUpdate = IsNewerVersion(Google_LastUpdatedVersion, savedSpreadsheetVersion);
			}

			if (!shouldUpdate)
			{
				PlayerPrefs.DeleteKey("I2Source_"+PlayerPrefName);
				PlayerPrefs.DeleteKey("I2SourceVersion_"+PlayerPrefName);
				return;
			}

			if (savedSpreadsheetVersion.Length > 19) // Check for corruption from previous versions
				savedSpreadsheetVersion = string.Empty;
			Google_LastUpdatedVersion = savedSpreadsheetVersion;

			Debug.Log ("[I2Loc] Using Saved (PlayerPref) data in 'I2Source_"+PlayerPrefName+"'" );
			Import_Google_Result(I2SavedData, eSpreadsheetUpdateMode.Replace);
		}

		bool IsNewerVersion( string currentVersion, string newVersion )
		{
			if (string.IsNullOrEmpty (newVersion))			// if no new version
				return false;
			if (string.IsNullOrEmpty (currentVersion))		// there is a new version, but not a current one
				return true;
			
			long currentV, newV;
			if (!long.TryParse (newVersion, out newV) || !long.TryParse (currentVersion, out currentV))	// if can't parse either, then force get the new one
				return true;

			return newV > currentV;
		}

		public void Import_Google( bool ForceUpdate = false)
		{
			if (!ForceUpdate && GoogleUpdateFrequency==eGoogleUpdateFrequency.Never)
				return;

			#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
			#endif

			string PlayerPrefName = GetSourcePlayerPrefName();
			if (!ForceUpdate && GoogleUpdateFrequency!=eGoogleUpdateFrequency.Always)
			{
				string sTimeOfLastUpdate = PlayerPrefs.GetString("LastGoogleUpdate_"+PlayerPrefName, "");
				DateTime TimeOfLastUpdate;
				if (DateTime.TryParse(sTimeOfLastUpdate, out TimeOfLastUpdate))
				{
					double TimeDifference = (DateTime.Now-TimeOfLastUpdate).TotalDays;
					switch (GoogleUpdateFrequency)
					{
						case eGoogleUpdateFrequency.Daily 	: if (TimeDifference<1) return;
																break;
						case eGoogleUpdateFrequency.Weekly 	: if (TimeDifference<8) return;
																break;
						case eGoogleUpdateFrequency.Monthly : if (TimeDifference<31) return;
																break;
						case eGoogleUpdateFrequency.OnlyOnce: return;
					}										
				}
			}
			PlayerPrefs.SetString("LastGoogleUpdate_"+PlayerPrefName, DateTime.Now.ToString());

			//--[ Checking google for updated data ]-----------------
			CoroutineManager.pInstance.StartCoroutine(Import_Google_Coroutine());
		}

		string GetSourcePlayerPrefName()
		{
			// If its a global source, use its name, otherwise, use the name and the level it is in
			if (Array.IndexOf(LocalizationManager.GlobalSources, name)>=0)
				return name;
			else
			{
				#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
				return Application.loadedLevelName + "_" + name;
				#else
				return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name+"_"+name;
				#endif
			}
		}

		IEnumerator Import_Google_Coroutine()
		{
			WWW www = Import_Google_CreateWWWcall();
			if (www==null)
				yield break;

			while (!www.isDone)
				yield return null;

			//Debug.Log ("Google Result: " + www.text);
			bool notError = string.IsNullOrEmpty(www.error);
			string wwwText = null;

			if (notError)
			{
				var bytes = www.bytes;
				wwwText = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length); //www.text
			}

			if (notError && !string.IsNullOrEmpty(wwwText) && wwwText != "\"\"")
			{
				var errorMsg = Import_Google_Result(wwwText, eSpreadsheetUpdateMode.Replace, true);
				if (string.IsNullOrEmpty(errorMsg))
				{
					if (Event_OnSourceUpdateFromGoogle != null)
						Event_OnSourceUpdateFromGoogle(this, true, www.error);

					LocalizationManager.LocalizeAll(true);
					Debug.Log("Done Google Sync");
				}
				else
				{
					if (Event_OnSourceUpdateFromGoogle != null)
						Event_OnSourceUpdateFromGoogle(this, false, www.error);

					Debug.Log("Done Google Sync: source was up-to-date");
				}
			}
			else
			{
				if (Event_OnSourceUpdateFromGoogle != null)
					Event_OnSourceUpdateFromGoogle(this, false, www.error);

				Debug.Log("Language Source was up-to-date with Google Spreadsheet");
			}
		}

		public WWW Import_Google_CreateWWWcall( bool ForceUpdate = false )
		{
			#if UNITY_WEBPLAYER
			Debug.Log ("Contacting google translation is not yet supported on WebPlayer" );
			return null;
			#else

			if (!HasGoogleSpreadsheet())
				return null;

			string savedVersion = PlayerPrefs.GetString("I2SourceVersion_"+GetSourcePlayerPrefName(), Google_LastUpdatedVersion);
			if (savedVersion.Length > 19) // Check for corruption
				savedVersion= string.Empty;

			if (IsNewerVersion(savedVersion, Google_LastUpdatedVersion))
				Google_LastUpdatedVersion = savedVersion;

			string query =  string.Format("{0}?key={1}&action=GetLanguageSource&version={2}", 
										  LocalizationManager.GetWebServiceURL(this),
										  Google_SpreadsheetKey,
										  ForceUpdate ? "0" : Google_LastUpdatedVersion);
			WWW www = new WWW(query);
			return www;
			#endif
		}

		public bool HasGoogleSpreadsheet()
		{
			return !string.IsNullOrEmpty(LocalizationManager.GetWebServiceURL(this)) && !string.IsNullOrEmpty(Google_SpreadsheetKey) && !string.IsNullOrEmpty(Google_SpreadsheetName);
		}

		public string Import_Google_Result( string JsonString, eSpreadsheetUpdateMode UpdateMode, bool saveInPlayerPrefs = false )
		{
			string ErrorMsg = string.Empty;
			if (string.IsNullOrEmpty(JsonString) || JsonString == "\"\"")
			{
				return ErrorMsg;
			}

			int idxV = JsonString.IndexOf("version=", StringComparison.Ordinal);
			int idxSV = JsonString.IndexOf("script_version=", StringComparison.Ordinal);
			if (idxV<0 || idxSV<0)
			{
				return "Invalid Response from Google, Most likely the WebService needs to be updated";
			}

			idxV += "version=".Length;
			idxSV += "script_version=".Length;
			
			string newSpreadsheetVersion = JsonString.Substring(idxV, JsonString.IndexOf(",",idxV, StringComparison.Ordinal) -idxV);
			var scriptVersion = int.Parse( JsonString.Substring(idxSV, JsonString.IndexOf(",",idxSV, StringComparison.Ordinal) -idxSV));

			if (newSpreadsheetVersion.Length > 19) // Check for corruption
				newSpreadsheetVersion= string.Empty;

			if (scriptVersion!=LocalizationManager.GetRequiredWebServiceVersion())
			{
				return "The current Google WebService is not supported.\nPlease, delete the WebService from the Google Drive and Install the latest version.";
			}

			//Debug.Log (Google_LastUpdatedVersion + " - " + newSpreadsheetVersion);
			if (saveInPlayerPrefs && !IsNewerVersion( Google_LastUpdatedVersion, newSpreadsheetVersion) )
			#if UNITY_EDITOR
				return "";
			#else
				return "LanguageSource is up-to-date";
			#endif
			
			if (saveInPlayerPrefs)
			{
				string PlayerPrefName = GetSourcePlayerPrefName();
				PlayerPrefs.SetString("I2Source_"+PlayerPrefName, JsonString);
				PlayerPrefs.SetString("I2SourceVersion_"+PlayerPrefName, newSpreadsheetVersion);
				PlayerPrefs.Save();
			}
			Google_LastUpdatedVersion = newSpreadsheetVersion;
			
			if (UpdateMode == eSpreadsheetUpdateMode.Replace)
					ClearAllData();

			int CSVstartIdx = JsonString.IndexOf("[i2category]", StringComparison.Ordinal);
			while (CSVstartIdx>0)
			{
				CSVstartIdx += "[i2category]".Length;
				int endCat = JsonString.IndexOf("[/i2category]", CSVstartIdx, StringComparison.Ordinal);
				string category = JsonString.Substring(CSVstartIdx, endCat-CSVstartIdx);
				endCat += "[/i2category]".Length;

				int endCSV = JsonString.IndexOf("[/i2csv]", endCat, StringComparison.Ordinal);
				string csv = JsonString.Substring(endCat, endCSV-endCat);

				CSVstartIdx = JsonString.IndexOf("[i2category]", endCSV, StringComparison.Ordinal);

				Import_I2CSV( category, csv, UpdateMode );
				
				// Only the first CSV should clear the Data
				if (UpdateMode == eSpreadsheetUpdateMode.Replace)
					UpdateMode = eSpreadsheetUpdateMode.Merge;
			}

#if UNITY_EDITOR
			if (!string.IsNullOrEmpty(ErrorMsg))
				UnityEditor.EditorUtility.SetDirty(this);
#endif
			return ErrorMsg;
		}

		#endregion
	}
}