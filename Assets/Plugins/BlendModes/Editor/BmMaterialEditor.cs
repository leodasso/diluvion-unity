using UnityEngine;
using UnityEditor;
using BlendModes;
using System;

public class BmMaterialEditor : MaterialEditor
{
	private BlendMode selectedBlendMode;

	public override void OnEnable ()
	{
		base.OnEnable();

		var currentBlendMode = BlendMode.Normal;

		foreach (var keyword in ((Material)target).shaderKeywords)
		{
			if (keyword.StartsWith("Bm"))
			{
				currentBlendMode = (BlendMode)Enum.Parse(typeof(BlendMode), keyword.Replace("Bm", string.Empty), true);
				break;
			}
		}
		selectedBlendMode = currentBlendMode;
	}

	public override void OnInspectorGUI ()
	{
		base.OnInspectorGUI();
		if (!isVisible) return;

		var targetMaterial = target as Material;

		EditorGUI.BeginChangeCheck();
		selectedBlendMode = (BlendMode)EditorGUILayout.EnumPopup("Blend Mode", selectedBlendMode);
		if (EditorGUI.EndChangeCheck())
		{
			for (int i = 0; i < targetMaterial.shaderKeywords.Length; i++)
				if (targetMaterial.shaderKeywords[i].StartsWith("Bm"))
					targetMaterial.DisableKeyword(targetMaterial.shaderKeywords[i]);
			targetMaterial.EnableKeyword("Bm" + selectedBlendMode);

			EditorUtility.SetDirty(targetMaterial);
		}
	}
}

