using UnityEngine;

namespace BlendModes
{
	public class DemoTiltWindow : MonoBehaviour
	{
		public Vector2 TileRange = new Vector2(5f, 3f);

		Quaternion startRotation;
		Vector2 targetRotation = Vector2.zero;

		void Start ()
		{
			startRotation = transform.localRotation;
		}

		void Update ()
		{
			Vector3 pos = Input.mousePosition;

			float halfWidth = Screen.width * 0.5f;
			float halfHeight = Screen.height * 0.5f;
			float x = Mathf.Clamp((pos.x - halfWidth) / halfWidth, -1f, 1f);
			float y = Mathf.Clamp((pos.y - halfHeight) / halfHeight, -1f, 1f);
			targetRotation = Vector2.Lerp(targetRotation, new Vector2(x, y), Time.deltaTime * 5f);

			transform.localRotation = startRotation * Quaternion.Euler(-targetRotation.y * TileRange.y, targetRotation.x * TileRange.x, 0f);
		}
	}
}
