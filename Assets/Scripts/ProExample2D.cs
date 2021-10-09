using UnityEngine;
using System.Collections;

public class ProExample2D : MonoBehaviour 
{
	
	public enum NOISE_MODE { FRACTAL_F0 = 0, FRACTAL_F1_F0 };
	
	public Material m_noiseMat;
	public int m_size = 1024;
	public NOISE_MODE m_noiseMode = NOISE_MODE.FRACTAL_F0;
	public int m_octaves = 8;
	public float m_frequency = 1.0f;
	public float m_lacunarity = 2.0f;
	public float m_gain = 0.5f;
	public float m_jitter = 1.0f;

	RenderTexture m_noiseTex;
	
	void Start () 
	{
		m_jitter = Mathf.Clamp01(m_jitter);
	
		RenderTexture[] temp = new RenderTexture[2];
		
		temp[0] = new RenderTexture(m_size, m_size, 0, RenderTextureFormat.ARGBHalf);
		temp[0].wrapMode = TextureWrapMode.Clamp;
		temp[0].filterMode = FilterMode.Bilinear;
		
		temp[1] = new RenderTexture(m_size, m_size, 0, RenderTextureFormat.ARGBHalf);
		temp[1].wrapMode = TextureWrapMode.Clamp;
		temp[1].filterMode = FilterMode.Bilinear;
		
		ClearColor(temp);
		
		float amp = 0.5f;
		float freq = m_frequency;
		
		const int READ = 0;
		const int WRITE = 1;
		
		m_noiseMat.SetFloat("_Jitter", m_jitter);
		
		for(int i = 0; i < m_octaves; i++)
		{
			m_noiseMat.SetFloat("_Frequency", freq);
			m_noiseMat.SetFloat("_Amp", amp);

			Graphics.Blit(temp[READ], temp[WRITE], m_noiseMat, (int)m_noiseMode);
			Swap(temp);
			
			freq *= m_lacunarity;
			amp *= m_gain;
		}
		
		m_noiseTex = temp[READ];
		RenderTexture.DestroyImmediate(temp[WRITE]);
		
		GetComponent<Renderer>().material.SetTexture("_DispTex", m_noiseTex);
		GetComponent<Renderer>().material.SetVector("Disp Texture Size", new Vector2(m_size, m_size));
	
	}
	
	public void ClearColor(RenderTexture[] texs)
	{
		for(int i = 0; i < texs.Length; i++)
		{
	 		Graphics.SetRenderTarget(texs[i]);
        	GL.Clear(false,true, Color.clear);
		}
	}
	
	public void Swap(RenderTexture[] texs)
	{
		RenderTexture temp = texs[0];	
		texs[0] = texs[1];
		texs[1] = temp;
	}
	
}
