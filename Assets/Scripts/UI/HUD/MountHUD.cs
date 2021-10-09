using UnityEngine;
using UnityEngine.UI;

public class MountHUD : MonoBehaviour
{
	public bool ready;
	public bool activeMount;
	public Color activeColor = Color.white;
	public Color readyColor = Color.green;
	public Color blockedColor = Color.red;
	
	Image image;

	// Use this for initialization
	void Start ()
	{
		image = GetComponent<Image>();
	}

	void Update()
	{
		if (!ready) image.color = blockedColor;
		else
		{
			image.color = activeMount ? activeColor : readyColor;	
		}
	}
}
