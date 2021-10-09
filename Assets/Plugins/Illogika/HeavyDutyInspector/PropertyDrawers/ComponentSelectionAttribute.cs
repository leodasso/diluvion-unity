//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2013 - 2015  Illogika
//----------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	public class ComponentSelectionAttribute : PropertyAttribute {

		public System.Type componentType
		{
			get;
			private set;
		}

		public string fieldName
		{
			get;
			private set;
		}

		public string[] requiredValues
		{
			get;
			private set;
		}

		public string defaultObject
		{
			get;
			private set;
		}

		public bool isPrefab
		{
			get;
			private set;
		}

		/// <summary>
		/// Display a Component reference as a reference to a GameObject and a drop down menu listing all Components matching your variable's type on this object. Components are numbered, NamedMonoBehaviours display their names.
		/// </summary>
		public ComponentSelectionAttribute()
		{
			fieldName = "";
		}

		/// <summary>
		/// Display a Component reference as a reference to a GameObject and a drop down menu listing all Components matching your variable's type on this object. Components and display their type and numbering, NamedMonoBehaviours display their names, followed by the value contained within the specified field.
		/// </summary>
		/// <param name="fieldName">Name of the field whose content to display after the component's type and numbering.</param>
		public ComponentSelectionAttribute(string fieldName)
		{
			this.fieldName = fieldName;
		}

		/// <summary>
		/// Display a Component reference as a reference to a GameObject and a drop down menu listing all Components matching your variable's type on this object. Components and display their type and numbering, NamedMonoBehaviours display their names, followed by the value contained within the specified field.
		/// </summary>
		/// <param name="fieldName">Name of the field whose content to display after the component's type and numbering.</param>
		/// <param name="requiredValues">The values this field has to have to be in the component list (for enums this is the string value of the enum, not its int value).</param>
		public ComponentSelectionAttribute(string fieldName, params string[] requiredValues)
		{
			this.fieldName = fieldName;
			this.requiredValues = requiredValues;
		}

		/// <summary>
		/// Display a Component reference as a reference to a GameObject and a drop down menu listing all Components matching your variable's type on this object. Components and display their type and numbering, NamedMonoBehaviours display their names, followed by the value contained within the specified field.
		/// </summary>
		/// <param name="componentType">The type of component to find on the target object.</param>
		/// <param name="defaultObject">The name of a GameObject to select by default.</param>
		/// <param name="isPrefab">Whether or not the default GameObject is a prefab (must be located in a Resources folder).</param>
		public ComponentSelectionAttribute(string defaultObject, bool isPrefab)
		{
			this.defaultObject = defaultObject;
			this.isPrefab = isPrefab;
		}

		/// <summary>
		/// Display a Component reference as a reference to a GameObject and a drop down menu listing all Components matching your variable's type on this object. Components and display their type and numbering, NamedMonoBehaviours display their names, followed by the value contained within the specified field.
		/// </summary>
		/// <param name="componentType">The type of component to find on the target object.</param>
		/// <param name="defaultObject">The name of a GameObject to select by default.</param>
		/// <param name="isPrefab">Whether or not the default GameObject is a prefab (must be located in a Resources folder).</param>
		/// <param name="fieldName">Name of the field whose content to display after the component's type and numbering.</param>
		public ComponentSelectionAttribute(string defaultObject, bool isPrefab, string fieldName)
		{
			this.fieldName = fieldName;
			this.defaultObject = defaultObject;
			this.isPrefab = isPrefab;
		}

		/// <summary>
		/// Display a Component reference as a reference to a GameObject and a drop down menu listing all Components matching your variable's type on this object. Components and display their type and numbering, NamedMonoBehaviours display their names, followed by the value contained within the specified field.
		/// </summary>
		/// <param name="componentType">The type of component to find on the target object.</param>
		/// <param name="defaultObject">The name of a GameObject to select by default.</param>
		/// <param name="isPrefab">Whether or not the default GameObject is a prefab (must be located in a Resources folder).</param>
		/// <param name="fieldName">Name of the field whose content to display after the component's type and numbering.</param>
		/// <param name="requiredValues">The values this field has to have to be in the component list (for enums this is the string value of the enum, not its int value).
		public ComponentSelectionAttribute(string defaultObject, bool isPrefab, string fieldName, params string[] requiredValues )
		{
			this.fieldName = fieldName;
			this.requiredValues = requiredValues;
			this.defaultObject = defaultObject;
			this.isPrefab = isPrefab;
		}
	}

}

