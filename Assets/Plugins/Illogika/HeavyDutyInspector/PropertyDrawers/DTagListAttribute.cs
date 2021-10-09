//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------
using UnityEngine;

namespace Diluvion.Roll
{

	public class DTagListAttribute : PropertyAttribute {
		
		public bool canDeleteFirstElement
		{
			get;
			private set;
		}

        public System.Type type
        {
            get;
            private set;
        }

  

		/// <summary>
		/// Use with variables of type List<string>. Displays strings in a list using the tag drop down menu and adds the ability to delete tags from the list.
		/// </summary>
		/// <param name="canDeleteFirstElement">If set to <c>false</c> the first element in the list won't have the delete button.</param>
		public DTagListAttribute()
		{
			
		}
	}

}
