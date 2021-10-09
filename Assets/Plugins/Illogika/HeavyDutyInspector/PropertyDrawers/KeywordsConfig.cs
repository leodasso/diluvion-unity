//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	public class KeywordsConfig : ScriptableObject
	{

        
		public List<KeywordCategory> keyWordCategories = new List<KeywordCategory>();

        public KeywordCategory GetCategory(string categoryname)
        {
            foreach (KeywordCategory k in keyWordCategories)
                if (k.IsCategory(categoryname))
                    return k;
            return null;
        }

	}

	[System.Serializable]
	public class KeywordCategory : System.Object
	{
		public string name;

		[System.NonSerialized]
		public bool expanded;

		public List<string> keywords = new List<string>();

		public KeywordCategory()
		{
			name = "";
		}

		public KeywordCategory(string name)
		{
			this.name = name;
		}


        public bool IsCategory(string s)
        {
            if (name == s)
                return true;
            else
                return false;
        }
	}

}
