using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BankHolder : MonoBehaviour
{

	public static BankHolder _instance;

	public static BankHolder Get()
	{
		if (_instance != null) return _instance;
		_instance = GameObject.FindObjectOfType<BankHolder>();
		return _instance;
	}

	private void Awake()
	{
		
		if(_instance!=null)
		{
			Destroy(this.gameObject);
			return;
		}
		_instance = this;
		DontDestroyOnLoad(this.gameObject);
	}
}
