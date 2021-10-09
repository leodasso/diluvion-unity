using UnityEngine;

namespace BlendModes
{
	public class DemoObject : MonoBehaviour
	{
		private BlendModeEffect blendModeEffect;

		private void Awake ()
		{
			blendModeEffect = GetComponent<BlendModeEffect>();
		}

		private void Update ()
		{
			if (DemoBlendModePicker.SelectedBlendMode != blendModeEffect.BlendMode)
				blendModeEffect.BlendMode = DemoBlendModePicker.SelectedBlendMode;
		}
	}
}
