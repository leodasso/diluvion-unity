using UnityEngine;
using System.Collections;

public class Disable : MonoBehaviour 
{
	public int m_frameToDisable = 1;
	
	int m_frameCount = 0;

	void Update () 
	{
		
		if(m_frameCount > m_frameToDisable)
			gameObject.SetActive(false);
		
		m_frameCount++;
	
	}
}
