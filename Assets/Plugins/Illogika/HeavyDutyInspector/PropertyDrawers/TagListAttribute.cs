//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------
using UnityEngine;

namespace HeavyDutyInspector
{

	public class TagListAttribute : PropertyAttribute {
		
		public bool canDeleteFirstElement
		{
			get;
			private set;
		}

		/// <summary>
		/// Use with variables of type List<string>. Displays strings in a list using the tag drop down menu and adds the ability to delete tags from the list.
		/// </summary>
		/// <param name="canDeleteFirstElement">If set to <c>false</c> the first element in the list won't have the delete button.</param>
		public TagListAttribute(bool canDeleteFirstElement = true)
		{
			this.canDeleteFirstElement = canDeleteFirstElement;
		}
	}

}
