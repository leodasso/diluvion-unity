using UnityEngine;
using System.Collections;

public class ExampleGPU_4D : MonoBehaviour 
{
	ImprovedPerlinNoise m_perlin;
	
	public int m_seed = 0;
	public float m_frequency = 10.0f;
	public float m_lacunarity = 2.0f;
	public float m_gain = 0.5f;
	
	void Start () 
	{
		m_perlin = new ImprovedPerlinNoise(m_seed);
		
		m_perlin.LoadResourcesFor4DNoise();
		
		GetComponent<Renderer>().material.SetTexture("_PermTable1D", m_perlin.GetPermutationTable1D());
		GetComponent<Renderer>().material.SetTexture("_PermTable2D", m_perlin.GetPermutationTable2D());
		GetComponent<Renderer>().material.SetTexture("_Gradient4D", m_perlin.GetGradient4D());
	}
	
	void Update()
	{
		GetComponent<Renderer>().material.SetFloat("_Frequency", m_frequency);
		GetComponent<Renderer>().material.SetFloat("_Lacunarity", m_lacunarity);
		GetComponent<Renderer>().material.SetFloat("_Gain", m_gain);
	}
	
}
