using System.Collections;
using System.Collections.Generic;
using Diluvion;
using UnityEngine;

namespace Quests
{

	[RequireComponent(typeof(InteriorManager))]
	public class QuestActorStoryState : QuestActor
	{
		public StoryState storyState;
		InteriorManager _interior;
		
		protected override void OnActivate()
		{
			base.OnActivate();
			if (!_interior)_interior = GetComponent<InteriorManager>();

			_interior.ChangeStoryState(storyState);
		}
	}
}