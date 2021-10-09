using UnityEngine;

namespace BlendModes
{
	public class DemoRotator : MonoBehaviour
	{
		public float Speed;
		public Vector3 Axis = new Vector3(1, 1, 1);

		private void Update ()
		{
			transform.Rotate(Axis, Speed * Time.deltaTime);
		}
	}
}