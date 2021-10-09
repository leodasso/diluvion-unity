using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "new skin", menuName = "Diluvion/subs/new skin")]
public class SkinHolder : ScriptableObject
{
	[InlineEditor(InlineEditorModes.LargePreview, Expanded = true)]
	public Material normal;
	[InlineEditor(InlineEditorModes.LargePreview,  Expanded = true)]
	public Material destroyed;
}
