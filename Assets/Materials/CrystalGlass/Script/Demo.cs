using UnityEngine;
using System.Collections;

public class Demo : MonoBehaviour
{
	public Texture m_Rainbow;
	private bool m_ToggleBump = false;

	private void OnGUI()
	{
		GameObject obj = GameObject.Find ("Teapot");
		Renderer rd = obj.GetComponent<Renderer>();
		
		int btnW = 180;
		int btnH = 30;
		GUI.Label (new Rect (10, 10, 250, 30), "Crystal Glass Demo");
		if (GUI.Button (new Rect (10, btnH * 0 + 40, btnW, btnH), "Opacity"))
		{
			Shader sdr = Shader.Find ("Crystal Glass/Opacity");
			rd.material.shader = sdr;
		}
		if (GUI.Button (new Rect (10, btnH * 1 + 40, btnW, btnH), "Single Pass Transparency"))
		{
			Shader sdr = Shader.Find ("Crystal Glass/Single Pass Transparency");
			rd.material.shader = sdr;
		}
		if (GUI.Button (new Rect (10, btnH * 2 + 40, btnW, btnH), "Double Pass Transparency"))
		{
			Shader sdr = Shader.Find ("Crystal Glass/Double Pass Transparency");
			rd.material.shader = sdr;
		}
		
		m_ToggleBump = GUI.Toggle (new Rect(10, btnH * 3 + 50, 120, btnH), m_ToggleBump, "Bump Mapping");
		if (m_ToggleBump)
			Shader.EnableKeyword ("CRYSTAL_GLASS_BUMP");
		else
			Shader.DisableKeyword ("CRYSTAL_GLASS_BUMP");
			
		if (GUI.Button (new Rect (10, btnH * 4 + 50, btnW, btnH), "Rainbow Crystal Glass"))
		{
			Shader sdr = Shader.Find ("Crystal Glass/Rainbow");
			rd.material.shader = sdr;
			rd.material.SetTexture ("_RainbowTex", m_Rainbow);
		}	
	}
}