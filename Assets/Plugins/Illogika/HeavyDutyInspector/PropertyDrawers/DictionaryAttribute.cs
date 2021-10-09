//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------
using UnityEngine;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	public class DictionaryAttribute : PropertyAttribute {

		public string valuesListName
		{
			get;
			private set;
		}

		public KeywordsConfig keywordConfig
		{
			get;
			private set;
		}

		/// <summary>
		/// Displays two lists as a single dictionary and synchronizes them together.
		/// You need to implement ISerializationCallbackReceiver to create your actual dictionary when your asset is loaded, before its Awake function.
		/// </summary>
		/// <param name="valuesListName">Name of the list containing the values for the dictionary.</param>
		public DictionaryAttribute(string valuesListName)
		{
			keywordConfig = null;
			this.valuesListName = valuesListName;
		}

		/// <summary>
		/// Displays two lists as a single dictionary and synchronizes them together.
		/// This overload is for use when your list of Keys is a list of keywords.
		/// You need to implement ISerializationCallbackReceiver to create your actual dictionary when your asset is loaded, before its Awake function.
		/// </summary>
		/// <param name="valuesListName">Name of the list containing the values for the dictionary.</param>
		/// <param name="keywordsConfigFile">Path of your KeywordConfig file relative to a Resources folder.</param>
		public DictionaryAttribute(string valuesListName, string keywordsConfigFile)
		{
			keywordConfig = Resources.Load(keywordsConfigFile) as KeywordsConfig;
			this.valuesListName = valuesListName;
		}
	}	

	public static class DictionaryHelper
	{
		public static Dictionary<T, U> InitDictionary<T, U>(List<T> keys, List<U> values)
		{
			Dictionary<T, U> dictionary = new Dictionary<T, U>();
			try
			{
				for(int i = 0; i < keys.Count; ++i)
				{
					if(!dictionary.ContainsKey(keys[i]))
						dictionary.Add(keys[i], values[i]);
				}
			}
			catch { }
			return dictionary;
		} 
	}
}
	