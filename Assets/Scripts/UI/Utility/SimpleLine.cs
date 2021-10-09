using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLine : MonoBehaviour
{

	static SimpleLine _instance;

	LineRenderer _lineRenderer;

	void GetRenderer()
	{
		if (_lineRenderer) return;
		_lineRenderer = GetComponent<LineRenderer>();
	}

	static SimpleLine Get()
	{
		if (_instance) return _instance;
		_instance = Instantiate(Resources.Load<SimpleLine>("Aim Line"));
		return _instance;
	}

	public static void DrawLine(Vector3 from, Vector3 to)
	{
		Get().gameObject.SetActive(true);
		Get().GetRenderer();
		Get()._lineRenderer.SetPosition(0, from);
		Get()._lineRenderer.SetPosition(1, to);
	}

	public static void HideLine()
	{
		Get().gameObject.SetActive(false);
	}
}
