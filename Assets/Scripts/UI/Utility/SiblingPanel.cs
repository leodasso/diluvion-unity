using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DUI
{
	
	/// <summary>
	/// Can take action based on my position in the hierarchy. Stuff like making only the first sibling active, controlling alpha, etc.
	/// </summary>
	[RequireComponent(typeof(CanvasGroup)), ExecuteInEditMode]
	public class SiblingPanel : MonoBehaviour
	{
		
		public enum InteractiveType {First, Last, All, None}
		
		[Tooltip("Determines when this will be interactive based on its position in the hierarchy.")]
		public InteractiveType interactivePosition = InteractiveType.All;

		[Tooltip("The alpha of the first sibling.")]
		public float firstAlpha = 1;
		[Tooltip("The alpha of the last sibling. All siblings in between lerp between these values.")]
		public float lastAlpha = .25f;
		
		CanvasGroup _canvasGroup;

		// Use this for initialization
		void Start()
		{
			_canvasGroup = GetComponent<CanvasGroup>();
		}

		// Update is called once per frame
		void Update()
		{
			if (!_canvasGroup) return;

			// Set the appropriate alpha
			_canvasGroup.alpha = Mathf.Lerp(firstAlpha, lastAlpha, NormalizedSiblingPosition());

			switch (interactivePosition)
			{
				case InteractiveType.All:
					SetCanvasGroup(true);
					break;
					
				case InteractiveType.None:
					SetCanvasGroup(false);
					break;
					
				case InteractiveType.First:
					SetCanvasGroup(transform.GetSiblingIndex() == 0);
					break;
					
				case InteractiveType.Last:
					SetCanvasGroup(IsLastSibling());
					break;
					
				default:
					SetCanvasGroup(_canvasGroup.interactable = true);
					break;
					
			}
		}

		void SetCanvasGroup(bool setActive)
		{
			_canvasGroup.interactable = setActive;
			_canvasGroup.blocksRaycasts = setActive;
		}

		bool IsLastSibling()
		{
			if (transform.parent == null) return false;
			if (transform.parent.childCount <= 1) return false;

			return transform.parent.childCount == transform.GetSiblingIndex() + 1;
		}

		float NormalizedSiblingPosition()
		{
			if (transform.parent == null) return 0;

			int siblings = transform.parent.childCount - 1;
			if (siblings < 1) return 0;
			
			return transform.GetSiblingIndex() / (float)siblings;
		}
	}
}