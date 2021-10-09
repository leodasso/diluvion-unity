using System.Collections;
using System.Collections.Generic;
using DUI;
using UnityEngine;

namespace Diluvion
{

	[CreateAssetMenu(fileName = "change story state", menuName = "Diluvion/actions/story state")]
	public class ChangeStoryState : Action
	{

		[Range(.1f, 5)]
		public float transitionTime = .75f;

		public Color transitionColor = Color.black;
		
		[Tooltip("Sets the currently viewed interior's story state to this.")]
		public StoryState toState;
		
		public override bool DoAction(Object o)
		{
			UIManager.Clear<DialogBox>();
			UIManager.Clear<CaptainsTools>();
			
			GameManager.Freeze(this);
			FadeOverlay.FadeInThenOut(transitionTime, transitionColor, FadeoutDone);

			return true;
		}

		void FadeoutDone()
		{
			InteriorManager currentInterior = InteriorView.ViewedInterior();
			if (currentInterior == null)
			{
				Debug.LogError("No interior is currently being viewed!");
			}
			else
			{
				currentInterior.storyState = toState;
				currentInterior.RefreshStoryState();
			}
			
			GameManager.UnFreeze(this);
		}

		public override string ToString()
		{
			return "transitions to story state: " + toState;
		}

		protected override void Test()
		{
			DoAction(null);
		}
	}
}