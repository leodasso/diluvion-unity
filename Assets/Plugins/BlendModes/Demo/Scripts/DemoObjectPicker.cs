using UnityEngine;
using UnityEngine.UI;

namespace BlendModes
{
	public class DemoObjectPicker : MonoBehaviour
	{
		public GameObject GUIObject;
		public GameObject ParticlesObject;
		public GameObject MeshObject;
		public GameObject SpriteObject;

		public Button GUIButton;
		public Button ParticlesButton;
		public Button MeshButton;
		public Button SpriteButton;

		private void OnEnable ()
		{
			GUIButton.onClick.AddListener(() =>
			{
				UnselectAll();
				GUIObject.SetActive(true);
				GUIButton.GetComponent<Image>().color = Color.green;
			});

			ParticlesButton.onClick.AddListener(() =>
			{
				UnselectAll();
				ParticlesObject.SetActive(true);
				ParticlesButton.GetComponent<Image>().color = Color.green;
			});

			MeshButton.onClick.AddListener(() =>
			{
				UnselectAll();
				MeshObject.SetActive(true);
				MeshButton.GetComponent<Image>().color = Color.green;
			});

			SpriteButton.onClick.AddListener(() =>
			{
				UnselectAll();
				SpriteObject.SetActive(true);
				SpriteButton.GetComponent<Image>().color = Color.green;
			});
		}

		private void OnDisable ()
		{
			GUIButton.onClick.RemoveAllListeners();
			ParticlesButton.onClick.RemoveAllListeners();
			MeshButton.onClick.RemoveAllListeners();
			SpriteButton.onClick.RemoveAllListeners();
		}

		private void Start ()
		{
			MeshButton.onClick.Invoke();
		}

		private void UnselectAll ()
		{
			GUIObject.SetActive(false);
			GUIButton.GetComponent<Image>().color = Color.white;

			ParticlesObject.SetActive(false);
			ParticlesButton.GetComponent<Image>().color = Color.white;

			MeshObject.SetActive(false);
			MeshButton.GetComponent<Image>().color = Color.white;

			SpriteObject.SetActive(false);
			SpriteButton.GetComponent<Image>().color = Color.white;
		}
	}
}
