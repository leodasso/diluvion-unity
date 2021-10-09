using UnityEngine;

namespace I2.Loc
{
	public class SimpleButton : MonoBehaviour 
	{
		public void OnMouseUp()
		{
			gameObject.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
		}
	}
}