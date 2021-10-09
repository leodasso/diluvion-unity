using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		#region Variables
		private List<TranslationRequest> mTranslationRequests = new List<TranslationRequest> ();
        private bool mAppNameTerm_Expanded;

		#endregion

		void OnGUI_Languages()
		{
			//GUILayout.Space(5);

			OnGUI_ShowMsg();

			OnGUI_LanguageList();

            OnGUI_StoreIntegration();

            GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent("On Missing Translation:", "What should happen IN-GAME when a term is not yet translated to the current language?"), EditorStyles.boldLabel, GUILayout.Width(160));
                GUILayout.BeginVertical();
                    GUILayout.Space(7);
                    EditorGUILayout.PropertyField(mProp_OnMissingTranslation, GUITools.EmptyContent, GUILayout.Width(165));
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        #region GUI Languages

        void OnGUI_LanguageList()
		{
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
				GUILayout.FlexibleSpace();
				GUILayout.Label ("Languages:", EditorStyles.miniLabel, GUILayout.ExpandWidth(false));
				GUILayout.FlexibleSpace();
				GUILayout.Label ("Code:", EditorStyles.miniLabel, GUILayout.Width(76));
			GUILayout.EndHorizontal();
			
			//--[ Language List ]--------------------------

			int IndexLanguageToDelete = -1;
			int LanguageToMoveUp = -1;
			int LanguageToMoveDown = -1;
			mScrollPos_Languages = GUILayout.BeginScrollView( mScrollPos_Languages, EditorStyles.textArea, GUILayout.MinHeight (100), GUILayout.MaxHeight(Screen.height), GUILayout.ExpandHeight(false));

			List<string> codes = GoogleLanguages.GetAllInternationalCodes();
			codes.Sort();
			codes.Insert(0, string.Empty);

			for (int i=0, imax=mProp_Languages.arraySize; i<imax; ++i)
			{
				SerializedProperty Prop_Lang = mProp_Languages.GetArrayElementAtIndex(i);
				SerializedProperty Prop_LangName = Prop_Lang.FindPropertyRelative("Name");
                SerializedProperty Prop_LangCode = Prop_Lang.FindPropertyRelative("Code");
                SerializedProperty Prop_Flags    = Prop_Lang.FindPropertyRelative("Flags");
                bool isLanguageEnabled = (Prop_Flags.intValue & (int)eLanguageDataFlags.DISABLED)==0;

                GUI.color = isLanguageEnabled ? Color.white : new Color(1, 1, 1, 0.3f);
                GUILayout.BeginHorizontal();

                if (GUILayout.Button ("X", "toolbarbutton", GUILayout.ExpandWidth(false)))
				{
					IndexLanguageToDelete = i;
				}
				
				GUILayout.BeginHorizontal(EditorStyles.toolbar);

				GUI.changed = false;
				string LanName = EditorGUILayout.TextField(Prop_LangName.stringValue, GUILayout.ExpandWidth(true));
				if (GUI.changed && !string.IsNullOrEmpty(LanName))
				{
					Prop_LangName.stringValue = LanName;
					GUI.changed = false;
				}

				int Index = Mathf.Max(0, codes.IndexOf (Prop_LangCode.stringValue));
				GUI.changed = false;
				Index = EditorGUILayout.Popup(Index, codes.ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(60));
				if (GUI.changed && Index>=0)
				{
					Prop_LangCode.stringValue = codes[Index];
				}

				GUILayout.EndHorizontal();

				GUI.enabled = (i<imax-1);
				if (GUILayout.Button( "\u25BC", EditorStyles.toolbarButton, GUILayout.Width(18))) LanguageToMoveDown = i;
				GUI.enabled = i>0;
				if (GUILayout.Button( "\u25B2", EditorStyles.toolbarButton, GUILayout.Width(18))) LanguageToMoveUp = i;

                GUI.enabled = true;
                if (GUILayout.Button( new GUIContent("Show", "Preview all localizations into this language"), EditorStyles.toolbarButton, GUILayout.Width(35))) 
				{
					LocalizationManager.SetLanguageAndCode( LanName, Prop_LangCode.stringValue, false, true);
				}

				if (GUILayout.Button( new GUIContent("Translate", "Translate all empty terms"), EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) 
				{
					TranslateAllToLanguage( LanName );
				}
				GUI.enabled = true;

                EditorGUI.BeginChangeCheck();
                isLanguageEnabled = EditorGUILayout.Toggle(isLanguageEnabled, GUILayout.Width(15));
                if (EditorGUI.EndChangeCheck())
                {
                    Prop_Flags.intValue = (Prop_Flags.intValue & ~(int)eLanguageDataFlags.DISABLED) | (isLanguageEnabled ? 0 : (int)eLanguageDataFlags.DISABLED);
                }

                GUILayout.EndHorizontal();
			}
			
			GUILayout.EndScrollView();
			
			OnGUI_AddLanguage( mProp_Languages );

			if (mConnection_WWW!=null || mConnection_Text.Contains("Translating"))
			{
				// Connection Status Bar
				int time = (int)((Time.realtimeSinceStartup % 2) * 2.5);
				string Loading = mConnection_Text + ".....".Substring(0, time);
				GUI.color = Color.gray;
				GUILayout.BeginHorizontal(EditorStyles.textArea);
				GUILayout.Label (Loading, EditorStyles.miniLabel);
				GUI.color = Color.white;
                if (GUILayout.Button("Cancel", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                {
                    EditorApplication.update -= DelayedStartTranslation;
                    StopConnectionWWW();
                }
				GUILayout.EndHorizontal();
				Repaint();
			}
			
			if (IndexLanguageToDelete>=0)
			{
				mLanguageSource.RemoveLanguage( mLanguageSource.mLanguages[IndexLanguageToDelete].Name );
				serializedObject.Update();
                ParseTerms(true, false);
			}

			if (LanguageToMoveUp>=0)   SwapLanguages( LanguageToMoveUp, LanguageToMoveUp-1 );
			if (LanguageToMoveDown>=0) SwapLanguages( LanguageToMoveDown, LanguageToMoveDown+1 );
		}

		void SwapLanguages( int iFirst, int iSecond )
		{
			serializedObject.ApplyModifiedProperties();
			LanguageSource Source = mLanguageSource;

			SwapValues( Source.mLanguages, iFirst, iSecond );
			foreach (TermData termData in Source.mTerms)
			{
				SwapValues ( termData.Languages, iFirst, iSecond );
				SwapValues ( termData.Languages_Touch, iFirst, iSecond );
				SwapValues ( termData.Flags, iFirst, iSecond );
			}
			serializedObject.Update();
		}

		void SwapValues( List<LanguageData> mList, int Index1, int Index2 )
		{
			LanguageData temp = mList[Index1];
			mList[Index1] = mList[Index2];
			mList[Index2] = temp;
		}
		void SwapValues( string[] mList, int Index1, int Index2 )
		{
			string temp = mList[Index1];
			mList[Index1] = mList[Index2];
			mList[Index2] = temp;
		}
		void SwapValues( byte[] mList, int Index1, int Index2 )
		{
			byte temp = mList[Index1];
			mList[Index1] = mList[Index2];
			mList[Index2] = temp;
		}

		
		void OnGUI_AddLanguage( SerializedProperty Prop_Languages)
		{
			//--[ Add Language Upper Toolbar ]-----------------
			
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			mLanguages_NewLanguage = EditorGUILayout.TextField("", mLanguages_NewLanguage, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();

			GUI.enabled = !string.IsNullOrEmpty (mLanguages_NewLanguage);
			if (GUILayout.Button("Add", EditorStyles.toolbarButton, GUILayout.Width(50)))
			{
				Prop_Languages.serializedObject.ApplyModifiedProperties();
				mLanguageSource.AddLanguage( mLanguages_NewLanguage, GoogleLanguages.GetLanguageCode(mLanguages_NewLanguage) );
				Prop_Languages.serializedObject.Update();
				mLanguages_NewLanguage = "";
                GUI.FocusControl(string.Empty);
            }
            GUI.enabled = true;
			
			GUILayout.EndHorizontal();
			
			
			//--[ Add Language Bottom Toolbar ]-----------------
			
			GUILayout.BeginHorizontal();
			
			//-- Language Dropdown -----------------
			string CodesToExclude = string.Empty;
			foreach (var LanData in mLanguageSource.mLanguages)
				CodesToExclude = string.Concat(CodesToExclude, "[", LanData.Code, "]");

			List<string> Languages = GoogleLanguages.GetLanguagesForDropdown(mLanguages_NewLanguage, CodesToExclude);

			GUI.changed = false;
			int index = EditorGUILayout.Popup(0, Languages.ToArray(), EditorStyles.toolbarDropDown);

			if (GUI.changed && index>=0)
			{
				mLanguages_NewLanguage = GoogleLanguages.GetFormatedLanguageName( Languages[index] );
			}
			
			
			if (GUILayout.Button("Add", EditorStyles.toolbarButton, GUILayout.Width(50)) && index>=0)
			{
				Prop_Languages.serializedObject.ApplyModifiedProperties();
				mLanguages_NewLanguage = GoogleLanguages.GetFormatedLanguageName( Languages[index] );
				if (!string.IsNullOrEmpty(mLanguages_NewLanguage)) 
					mLanguageSource.AddLanguage( mLanguages_NewLanguage, GoogleLanguages.GetLanguageCode(mLanguages_NewLanguage) );
				Prop_Languages.serializedObject.Update();
				mLanguages_NewLanguage = "";
                GUI.FocusControl(string.Empty);
            }

            GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUI.color = Color.white;
		}


		void TranslateAllToLanguage (string lanName)
		{
			if (!GoogleTranslation.CanTranslate ()) 
			{
				ShowError ("WebService is not set correctly or needs to be reinstalled");
				return;
			}

			int LanIndex = mLanguageSource.GetLanguageIndex (lanName);
			string code = mLanguageSource.mLanguages [LanIndex].Code;

			mTranslationRequests.Clear ();
			foreach (var termData in mLanguageSource.mTerms) 
			{
				if (!string.IsNullOrEmpty((GUI_SelectedInputType==0 ? termData.Languages : termData.Languages_Touch)[LanIndex]))
					continue;
				
				string sourceCode, sourceText;
				FindTranslationSource( LanguageSource.GetKeyFromFullTerm(termData.Term), termData, code, out sourceText, out sourceCode );

				mTranslationRequests.Add( new TranslationRequest(){
					Term = termData.Term,
					Text=sourceText,
					LanguageCode=sourceCode,
					TargetLanguagesCode=new string[]{code}
				} );
			}

			mConnection_WWW = GoogleTranslation.GetTranslationWWW (mTranslationRequests);
            mConnection_Text = "Translating"; if (mTranslationRequests.Count > 1) mConnection_Text += " (" + mTranslationRequests.Count + ")";
			mConnection_Callback = OnLanguageTranslated;
			EditorApplication.update += CheckForConnection;
		}

		void OnLanguageTranslated( string Result, string Error )
		{
			//Debug.Log (Result);

            if (Result.Contains("Service invoked too many times"))
            {
                TimeStartTranslation = EditorApplication.timeSinceStartup + 1;
                EditorApplication.update += DelayedStartTranslation;
                mConnection_Text = "Translating (" + mTranslationRequests.Count + ")";
                return;
            }

			if (!string.IsNullOrEmpty(Error))/* || !Result.Contains("<i2>")*/
		    {
                Debug.LogError("WEB ERROR: " + Error);
				ShowError ("Unable to access Google or not valid request");
				return;
			}

			ClearErrors();
			Error = GoogleTranslation.ParseTranslationResult (Result, mTranslationRequests);
			if (!string.IsNullOrEmpty(Error))
			{
				ShowError (Error);
				return;
			}


			foreach (var request in mTranslationRequests)
			{
				if (request.Results == null)	// Handle cases where not all translations were valid
						continue;
								
				var termData = mLanguageSource.GetTermData(request.Term);
				if (termData==null)
					continue;

				string lastCode="";
				int lastcodeIdx= 0;

				for (int i=0; i<request.Results.Length; ++i)
				{
					//--[ most of the time is a single code, so this works as a cache
					if (lastCode!=request.TargetLanguagesCode[i])
					{
						lastCode = request.TargetLanguagesCode[i];
						lastcodeIdx = mLanguageSource.GetLanguageIndexFromCode( lastCode );
					}

					if (GUI_SelectedInputType==0)
						termData.Languages[lastcodeIdx] = request.Results[i];
					else
						termData.Languages_Touch[lastcodeIdx] = request.Results[i];
				}
			}
            mTranslationRequests.RemoveAll(x=>x.Results!=null && x.Results.Length>0);

            if (mTranslationRequests.Count>0)
            {
                TimeStartTranslation = EditorApplication.timeSinceStartup + 1;
                EditorApplication.update += DelayedStartTranslation;
                mConnection_Text = "Translating (" + mTranslationRequests.Count + ")";
            }
		}

        double TimeStartTranslation = 0;
        void DelayedStartTranslation()
        {
            if (EditorApplication.timeSinceStartup < TimeStartTranslation)
                return;
            EditorApplication.update -= DelayedStartTranslation;

            if (mTranslationRequests.Count <= 0)
                return;

            mConnection_WWW = GoogleTranslation.GetTranslationWWW(mTranslationRequests);
            mConnection_Text = "Translating (" + mTranslationRequests.Count + ")";
            mConnection_Callback = OnLanguageTranslated;
            EditorApplication.update += CheckForConnection;
        }

		#endregion

        #region Store Integration

        void OnGUI_StoreIntegration()
        {
            GUIStyle lstyle = new GUIStyle (EditorStyles.label);
            lstyle.richText = true;

            GUILayout.BeginHorizontal ();
                GUILayout.Label (new GUIContent("Store Integration:", "Setups the stores to detect that the game has localization, Android adds strings.xml for each language. IOS modifies the Info.plist"), EditorStyles.boldLabel, GUILayout.Width(160));
				GUILayout.FlexibleSpace();

					GUILayout.Label( new GUIContent( "<color=green><size=16>\u2713</size></color>  IOS", "Setups the stores to show in iTunes and the Appstore all the languages that this app supports, also localizes the app name if available" ), lstyle, GUILayout.Width( 90 ) );
					GUILayout.Label( new GUIContent( "<color=green><size=16>\u2713</size></color>  Android", "Setups the stores to show in GooglePlay all the languages this app supports, also localizes the app name if available" ), lstyle, GUILayout.Width( 90 ) );
            GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal();
                mAppNameTerm_Expanded = GUILayout.Toggle(mAppNameTerm_Expanded, new GUIContent( "App Name translations:", "How should the game be named in the devices based on their language" ), EditorStyles.foldout, GUILayout.Width( 160 ) );

                GUILayout.Label("", GUILayout.ExpandWidth(true));
                var rect = GUILayoutUtility.GetLastRect();
                TermsPopup_Drawer.ShowGUI( rect, mProp_AppNameTerm, GUITools.EmptyContent, mLanguageSource);

                if (GUILayout.Button("New Term", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    AddTerm("App_Name");
                    mProp_AppNameTerm.stringValue = "App_Name";
                    mAppNameTerm_Expanded = true;
                }
			GUILayout.EndHorizontal();

            if (mAppNameTerm_Expanded)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(10);

                GUILayout.BeginVertical("Box");
                    var termName = mProp_AppNameTerm.stringValue;
                    if (!string.IsNullOrEmpty(termName))
                    {
                        var termData = LocalizationManager.GetTermData(termName);
                        if (termData != null)
                            OnGUI_Keys_Languages(mProp_AppNameTerm.stringValue, ref termData, null, true, mLanguageSource);
                    }
                    GUILayout.Space(10);

                    GUILayout.BeginHorizontal();
                        GUILayout.Label("<b>Default App Name:</b>", lstyle, GUITools.DontExpandWidth);
                        GUILayout.Label(Application.productName);
                    GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }
		}

 		#endregion
	}
}