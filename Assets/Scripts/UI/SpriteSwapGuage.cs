using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Image))]
public class SpriteSwapGuage : MonoBehaviour
{
	[Range(0, 1), OnValueChanged("UpdateSprites")]
	public float fill;
	
	[Tooltip("In order from empty at index 0 to full."), InlineEditor(InlineEditorModes.LargePreview, DrawGUI = false, PreviewHeight = 60)]
	public List<Sprite> sprites = new List<Sprite>();

	private Image _image;
	void UpdateSprites()
	{
		int index = Mathf.FloorToInt(fill * sprites.Count);

		index = Mathf.Clamp(index, 0, sprites.Count - 1);

		if (!_image) _image = GetComponent<Image>();

		_image.sprite = sprites[index];
	}

	// Use this for initialization
	void Start ()
	{
		UpdateSprites();
	}
	
	// Update is called once per frame
	void Update () 
	{
		UpdateSprites();
	}
}
