using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VersionNumberText : MonoBehaviour
{

	TextMeshProUGUI _textBox;

	// Use this for initialization
	void Start ()
	{
		_textBox = GetComponent<TextMeshProUGUI>();
		_textBox.text = string.Format(_textBox.text, GameVersion.VersionNumber());
	}
}
