using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "new appearance", menuName = "Diluvion/Characters/appearances")]
public class Appearance : ScriptableObject {

    [InlineEditor(InlineEditorModes.LargePreview)]
    public Sprite portrait;

    [InlineEditor(InlineEditorModes.LargePreview, DrawGUI = false)]
    public Sprite chatterPortrait;

    public RuntimeAnimatorController animController;
    public Gender gender;
}
