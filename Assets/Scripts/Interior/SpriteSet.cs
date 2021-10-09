using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Sprite Set", menuName ="Diluvion/Sprite set")]
public class SpriteSet : ScriptableObject {

    [InlineEditor(inlineEditorMode: InlineEditorModes.LargePreview)]
    public Sprite normal;

    [InlineEditor(inlineEditorMode: InlineEditorModes.LargePreview)]
    public Sprite hover;

    [InlineEditor(inlineEditorMode: InlineEditorModes.LargePreview)]
    public Sprite empty;
}
