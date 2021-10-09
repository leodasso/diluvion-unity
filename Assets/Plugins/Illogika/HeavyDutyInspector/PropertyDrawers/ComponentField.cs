//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	[System.Serializable]
	public class ComponentField : System.Object
	{
		public Component component;

		public List<string> propertyPath = new List<string>();

		public bool isProperty;

		public FieldInfo GetFieldInfo(out System.Object parentObject)
		{
			return GetMemberInfo(out parentObject) as FieldInfo;
		}

		public PropertyInfo GetPropertyInfo(out System.Object parentObject)
		{
			 return GetMemberInfo(out parentObject) as PropertyInfo;
		}

		private MemberInfo GetMemberInfo(out System.Object parentObject)
		{
			propertyPath.RemoveAll((x) => string.IsNullOrEmpty(x));

			MemberInfo mi = component.GetType().GetField(propertyPath[0], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if(mi == null)
				mi = component.GetType().GetProperty(propertyPath[0], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			parentObject = component;
			for(int i = 1; i < propertyPath.Count; ++i)
			{
				object newObj = null;
				try
				{
					newObj = (mi as FieldInfo).GetValue(parentObject);
				}
				catch
				{
					newObj = (mi as PropertyInfo).GetValue(parentObject, null);
				}
				parentObject = newObj;
				mi = parentObject.GetType().GetField(propertyPath[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if(mi == null)
					mi = parentObject.GetType().GetProperty(propertyPath[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
			return mi;
		}
	}

}
