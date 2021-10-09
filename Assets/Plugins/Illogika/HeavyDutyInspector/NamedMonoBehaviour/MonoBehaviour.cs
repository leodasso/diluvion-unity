//----------------------------------------------
//
//         Copyright © 2014  Illogika
//----------------------------------------------
using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
/*
public class MonoBehaviour : UnityEngine.MonoBehaviour, ISerializationCallbackReceiver {
	
	public MonoBehaviour() : base()
	{
		typeName = GetType().ToString();
	}
	
#pragma warning disable 414
	[SerializeField]
	[HideInInspector]
	private string typeName;
#pragma warning restore 414
	
	private Transform cachedTransform;

	public new Transform transform
	{
		get
		{
			if(cachedTransform == null)
			{
				cachedTransform = base.transform;
			}
			return cachedTransform;
		}
	}

	public void OnAfterDeserialize()
	{
	/*	foreach(FieldInfo field in GetType().GetFields())
		{
			foreach(System.Attribute attribute in field.GetCustomAttributes(true))
			{
				if(typeof(HeavyDutyInspector.DictionaryAttribute).IsAssignableFrom(attribute.GetType()))
				{
					HeavyDutyInspector.DictionaryAttribute dictionaryAttribute = attribute as HeavyDutyInspector.DictionaryAttribute;
					FieldInfo keysField = field;
					FieldInfo valuesField = GetType().GetField(dictionaryAttribute.valuesListName);
					FieldInfo dictionaryField = GetType().GetField(dictionaryAttribute.dictionaryName);
					if(keysField != null && valuesField != null && dictionaryField != null)
					{
						IList keysCollection = keysField.GetValue(this) as IList;
						IList valuesCollection = valuesField.GetValue(this) as IList;
						IDictionary dictionaryCollection = dictionaryField.GetValue(this) as IDictionary;
						if(keysCollection != null && valuesCollection != null && dictionaryCollection != null)
						{
							for(int i = 0; i < keysCollection.Count && i < valuesCollection.Count; ++i)
							{
								if(!dictionaryCollection.Contains(keysCollection[i]))
									dictionaryCollection.Add(keysCollection[i], valuesCollection[i]);
							}
						}
					}
				}
			}
		}
	}

	public void OnBeforeSerialize()
	{
		// Do Nothing.
	}
}*/
