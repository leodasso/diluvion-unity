//----------------------------------------------
//            Heavy-Duty Inspector
//      Copyright © 2014 - 2015  Illogika
//----------------------------------------------
using UnityEngine;
using System.Linq;

namespace HeavyDutyInspector
{
	[System.Serializable]
	public class Scene : System.Object
	{

		[SerializeField]
		[HideInInspector]
		private string _name;

		public Scene()
		{
			_name = "";
		}

		private Scene(string name)
		{
			_name = name;
		}

		public static implicit operator string(Scene scene)
		{
			return scene._name.Split('/').Last().Replace(".unity", "");
		}

		public static implicit operator Scene(string path)
		{
			return new Scene(path);
		}

		public string fullPath
		{
			get { return _name; }
		}
	}

}
