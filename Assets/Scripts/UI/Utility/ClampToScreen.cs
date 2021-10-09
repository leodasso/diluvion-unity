using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace DUI
{

	[RequireComponent(typeof(RectTransform))]
	public class ClampToScreen : MonoBehaviour
	{
		[Tooltip("When not constrained by clamping, should I return to my init local position?")]
		public bool stayAtInitPos;
		
		
		[Tooltip("Clamp to a specific rect transform rather than the screen bounds?")]
		public bool clampToRect;
		[ShowIf("clampToRect")]
		public RectTransform clamperRect;
		
		RectTransform _rectTransform;
		Rect _rect;

		Vector3 _initLocalPos;

		// Use this for initialization
		void Start()
		{
			_rectTransform = GetComponent<RectTransform>();
			_initLocalPos = transform.localPosition;
		}

		// Update is called once per frame
		void LateUpdate()
		{
			if (stayAtInitPos) transform.localPosition = _initLocalPos;
			
			if (clampToRect && !clamperRect) return;
			
			RecalculateRect();

			// Default is clamping to the screen bounds
			float maxWidth = Screen.width;
			float minWidth = 0;
			float maxHeight = Screen.height;
			float minHeight = 0;

			// clamping to a specific rect transform
			if (clampToRect && clamperRect)
			{
				Rect newRect = RecalculateRect(clamperRect);
				maxWidth = newRect.xMax;
				minWidth = newRect.xMin;
				maxHeight = newRect.yMax;
				minHeight = newRect.yMin;
			}
			
			while (_rect.xMax > maxWidth)
			{
				Nudge(-1, 0);
			}

			while (_rect.xMin < minWidth)
			{
				Nudge(1, 0);
			}

			while (_rect.yMin < minHeight)
			{
				Nudge(0, 1);
			}

			while (_rect.yMax > maxHeight)
			{
				Nudge(0, -1);
			}
		}

		void Nudge(float x, float y)
		{
			Nudge(new Vector2(x, y));
		}

		void Nudge(Vector2 dir)
		{
			transform.position += (Vector3)dir;
			RecalculateRect();
		}

		void RecalculateRect()
		{
			_rect = RecalculateRect(_rectTransform);
		}

		/// <summary>
		/// Returns an accuract rect from the given rectTransform, taking scale into consideration
		/// </summary>
		static Rect RecalculateRect(RectTransform rectTransform)
		{
			Rect rect = rectTransform.rect;

			Vector2 pos = (Vector2)rectTransform.position + Vector2.Scale(rect.position, rectTransform.lossyScale);
			
			// add position to the rect
			return new Rect(pos, Vector2.Scale(rect.size, rectTransform.lossyScale));
		}
	}
}