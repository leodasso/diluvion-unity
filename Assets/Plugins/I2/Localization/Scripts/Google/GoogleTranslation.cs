using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace I2.Loc
{
	public struct TranslationRequest
	{
		public string Term;
		public string Text;
		public string LanguageCode;
		public string[] TargetLanguagesCode;
		public string[] Results;			// This is filled google returns the translations
	}

	public static class GoogleTranslation
	{
		public static bool CanTranslate ()
		{
			return (LocalizationManager.Sources.Count > 0 && 
					!string.IsNullOrEmpty (LocalizationManager.GetWebServiceURL()));
		}

		#region Single Translation

		// LanguageCodeFrom can be "auto"
		// After the translation is returned from Google, it will call OnTranslationReady(TranslationResult)
		// TranslationResult will be null if translation failed
		public static void Translate( string text, string LanguageCodeFrom, string LanguageCodeTo, Action<string> OnTranslationReady )
		{
			WWW www = GetTranslationWWW( text, LanguageCodeFrom, LanguageCodeTo );

			// Simulate a coroutine when in-editor
			#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					simulatedcoroutine_WWW = www;
					simulatedcoroutine_OnTranslationReady = OnTranslationReady;
					simulatedcoroutine_OriginalText = text;
					UnityEditor.EditorApplication.update += EditorCoroutine_WaitForTranslation;
					return;
				}
			#endif
			I2.CoroutineManager.pInstance.StartCoroutine(WaitForTranslation(www, OnTranslationReady, text));
		}

		#if UNITY_EDITOR
			static WWW simulatedcoroutine_WWW;
			static Action<string> simulatedcoroutine_OnTranslationReady;
			static string simulatedcoroutine_OriginalText;

			static void EditorCoroutine_WaitForTranslation()
			{
				bool finished = false;
				if (simulatedcoroutine_WWW == null)
					finished = true;
				else
				if (simulatedcoroutine_WWW.isDone)
				{
					finished = true;
					UseTranslation(simulatedcoroutine_WWW, simulatedcoroutine_OnTranslationReady, simulatedcoroutine_OriginalText);
				}

				if (finished)
					UnityEditor.EditorApplication.update -= EditorCoroutine_WaitForTranslation;
				else 
					UnityEditor.SceneView.RepaintAll(); // This is needed to make the editor call the EditorApplication.update in the next frame 
		}
#endif


		static IEnumerator WaitForTranslation(WWW www, Action<string> OnTranslationReady, string OriginalText)
		{
			yield return www;
			UseTranslation(www, OnTranslationReady, OriginalText);
		}

		static void UseTranslation(WWW www, Action<string> OnTranslationReady, string OriginalText)
		{
			if (!string.IsNullOrEmpty(www.error))
			{
				Debug.LogError (www.error);
				OnTranslationReady(string.Empty);
			}
			else
			{
                var bytes = www.bytes;
                var wwwText = Encoding.UTF8.GetString(bytes, 0, bytes.Length); //www.text
                string Translation = ParseTranslationResult(wwwText, OriginalText);
				OnTranslationReady( Translation );
			}
		}

		// Querry google for the translation and waits until google returns
		//public static string ForceTranslate ( string text, string LanguageCodeFrom, string LanguageCodeTo )
		//{
		//	WWW www = GetTranslationWWW( text, LanguageCodeFrom, LanguageCodeTo );
		//	while (!www.isDone);
			
		//	if (!string.IsNullOrEmpty(www.error))
		//	{
		//		Debug.LogError ("-- " + www.error);
		//		//foreach(KeyValuePair<string, string> entry in www.responseHeaders) 
		//		//	Debug.Log(entry.Value + "=" + entry.Key);
				
		//		return string.Empty;
		//	}
		//	else
		//	{
  //              var bytes = www.bytes;
  //              var wwwText = Encoding.UTF8.GetString(bytes, 0, bytes.Length); //www.text
  //              return ParseTranslationResult(wwwText, text);
		//	}
		//}

		public static WWW GetTranslationWWW(  string text, string LanguageCodeFrom, string LanguageCodeTo )
		{
			LanguageCodeFrom = GoogleLanguages.GetGoogleLanguageCode(LanguageCodeFrom);
			LanguageCodeTo = GoogleLanguages.GetGoogleLanguageCode(LanguageCodeTo);

			// Google has problem translating this "This Is An Example"  but not this "this is an example"
			// so I'm asking google with the lowercase version and then reverting back
			if (TitleCase(text)==text && text.ToUpper()!=text)
				text = text.ToLower();

			string url = string.Format("{0}?action=Translate&list={1}:{2}={3}", LocalizationManager.GetWebServiceURL(), LanguageCodeFrom, LanguageCodeTo, Uri.EscapeUriString( text ));
			//Debug.Log (url);
			WWW www = new WWW(url);
			return www;
		}

		public static string ParseTranslationResult( string html, string OriginalText )
		{
			try
			{
				//var iStart = html.IndexOf("<i2>");
				//var Translation = html.Substring(iStart+"<i2>".Length);
				var Translation = html;
				
				//if (OriginalText.ToUpper()==OriginalText)
					//Translation = Translation.ToUpper();
				//else
				//	if (UppercaseFirst(OriginalText)==OriginalText)
				//		Translation = UppercaseFirst(Translation);
				//else
					if (TitleCase(OriginalText)== OriginalText && OriginalText.ToUpper() != OriginalText)   // Google has problem translating this "This Is An Example"  but not this "this is an example"
						Translation = TitleCase(Translation);

				return Translation;
			}
			catch (Exception ex) 
			{ 
				Debug.LogError(ex.Message); 
				return string.Empty;
			}
		}
		/*static string ParseTranslationResult( string html, string OriginalText )
		{
			try
			{
				// This is a Hack for reading Google Translation while Google doens't change their response format
				int iStart = html.IndexOf("TRANSLATED_TEXT") + "TRANSLATED_TEXT='".Length;
				int iEnd = html.IndexOf("';INPUT_TOOL_PATH", iStart);
				
				string Translation = html.Substring( iStart, iEnd-iStart);
				
				// Convert to normalized HTML
				Translation = System.Text.RegularExpressions.Regex.Replace(Translation,
				                                                           @"\\x([a-fA-F0-9]{2})",
				                                                           match => char.ConvertFromUtf32(Int32.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber)));
				
				// Convert ASCII Characters
				Translation = System.Text.RegularExpressions.Regex.Replace(Translation,
				                                                           @"&#(\d+);",
				                                                           match => char.ConvertFromUtf32(Int32.Parse(match.Groups[1].Value)));
				
				Translation = Translation.Replace("<br>", "\n");
				
				if (OriginalText.ToUpper()==OriginalText)
					Translation = Translation.ToUpper();
				else
					if (UppercaseFirst(OriginalText)==OriginalText)
						Translation = UppercaseFirst(Translation);
				else
					if (TitleCase(OriginalText)==OriginalText)
						Translation = TitleCase(Translation);
				
				return Translation;
			}
			catch (System.Exception ex) 
			{ 
				Debug.LogError(ex.Message); 
				return string.Empty;
			}
		}*/

#endregion

#region Multiple Translations

		public static void Translate( List<TranslationRequest> requests, Action<List<TranslationRequest>> OnTranslationReady )
		{
			WWW www = GetTranslationWWW( requests );
			I2.CoroutineManager.pInstance.StartCoroutine(WaitForTranslation(www, OnTranslationReady, requests));
		}

		public static WWW GetTranslationWWW(  List<TranslationRequest> requests )
		{
			var sb = new StringBuilder ();

			bool first = true;
			foreach (var request in requests)
			{
				if (!first)
					sb.Append("<I2Loc>");
				sb.Append(request.LanguageCode);
				sb.Append(":");
				for (int i=0; i<request.TargetLanguagesCode.Length; ++i)
				{
					if (i!=0) sb.Append(",");
					sb.Append(request.TargetLanguagesCode[i]);
				}
				sb.Append("=");

                var text = (TitleCase(request.Text) == request.Text) ? request.Text.ToLowerInvariant() : request.Text;

                //sb.Append(text);
                sb.Append(Uri.EscapeUriString(text));
                first = false;
                if (sb.Length>4000)
                    break;
			}

            return new WWW(string.Format("{0}?action=Translate&list={1}", LocalizationManager.GetWebServiceURL(), sb.ToString()));
            //WWWForm form = new WWWForm();
            //form.AddField("action", "MultiTranslate");
            //form.AddField("data", sb.ToString());
				
            //WWW www = new WWW(LocalizationManager.GetWebServiceURL(), form);
            //return www;
		}
		
		static IEnumerator WaitForTranslation(WWW www, Action<List<TranslationRequest>> OnTranslationReady, List<TranslationRequest> requests)
		{
			yield return www;
			
			if (!string.IsNullOrEmpty(www.error))
			{
				Debug.LogError (www.error);
				OnTranslationReady(requests);
			}
			else
			{
                var bytes = www.bytes;
                var wwwText = Encoding.UTF8.GetString(bytes, 0, bytes.Length); //www.text
                ParseTranslationResult(wwwText, requests);
				OnTranslationReady( requests );
			}
		}

		public static string ParseTranslationResult( string html, List<TranslationRequest> requests )
		{
			//Debug.Log(html);
			// Handle google restricting the webservice to run
			if (html.StartsWith("<!DOCTYPE html>") || html.StartsWith("<HTML>"))
			{
                if (html.Contains("Service invoked too many times in a short time"))
                    return ""; // ignore and try again
                else
				    return "There was a problem contacting the WebService. Please try again later";
			}

			string[] texts = html.Split (new string[]{"<I2Loc>"}, StringSplitOptions.None);
			string[] splitter = new string[]{"<i2>"};
			for (int i=0; i<Mathf.Min (requests.Count, texts.Length); ++i)
			{
				var temp = requests[i];
				temp.Results = texts[i].Split (splitter, StringSplitOptions.None);

				// Google has problem translating this "This Is An Example"  but not this "this is an example"
				if (TitleCase(temp.Text)==temp.Text)
				{
					for (int j=0; j<temp.Results.Length; ++j)
						temp.Results[j] = TitleCase(temp.Results[j]);
				}
				requests[i] = temp;
			}
			return "";
		}

#endregion

		

		public static string UppercaseFirst(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			char[] a = s.ToLower().ToCharArray();
			a[0] = char.ToUpper(a[0]);
			return new string(a);
		}
		public static string TitleCase(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}

#if NETFX_CORE
			var sb = new StringBuilder(s);
			sb[0] = char.ToUpper(sb[0]);
			for (int i = 1, imax=s.Length; i<imax; ++i)
			{
				if (char.IsWhiteSpace(sb[i - 1]))
					sb[i] = char.ToUpper(sb[i]);
				else
					sb[i] = char.ToLower(sb[i]);
			}
			return sb.ToString();
#else
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s);
#endif
		}
	}
}

