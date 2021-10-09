using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

/// <summary>
/// A sonar signature. Multiple of these can be in a single sonar stats. Usually would be something like 'metallic', 'organic', 'military'
/// </summary>
namespace Diluvion.Sonar
{
    
    [CreateAssetMenu(fileName = "new signature", menuName = "Diluvion/sonar signature")]
    public class Signature : ScriptableObject {

        [InlineEditor(InlineEditorModes.LargePreview, DrawGUI = false, DrawHeader = false, Expanded = true, PreviewWidth = 40)]
        public Sprite icon;

        public bool faction = false;
        /// <summary>
        /// How strong the sonar signal has to be before this signature is revealed
        /// </summary>
        [Range(0, 1)]
        public float revealStrengh = 1;

        /// <summary>
        /// If greater than zero, will warn the player when this is within the warn distance.
        /// </summary>
        [Range(0, 999)]
        public float warnDistance = 0;

        [Range(0, 10)]
        public float danger = 0;
    }
}
