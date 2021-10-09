using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class StringOption: PropertyAttribute  
{
	public readonly List<string> options;
	
	public StringOption(List<string> options)
	{
		this.options = options;
		
	}
}
