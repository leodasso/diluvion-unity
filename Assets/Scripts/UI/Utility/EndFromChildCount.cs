using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DUI
{
	/// <summary>
	/// Checks the child count of the transform, and can end the attached duipanel component when it's less
	/// than a specified amount.
	/// </summary>
	public class EndFromChildCount : MonoBehaviour
	{

		[Tooltip("When the child count is less than this, this object will end.")]
		public int minChildren = 1;

		public DUIPanel panel;
		bool _consumed;

		// Use this for initialization
		void Start()
		{
			if (!panel) 
				panel = GetComponent<DUIPanel>();
		}

		// Update is called once per frame
		void Update()
		{
			if (!panel) return;
			if (_consumed) return;

			if (transform.childCount < minChildren)
			{
				panel.End();
				_consumed = true;
			}
		}
	}
}