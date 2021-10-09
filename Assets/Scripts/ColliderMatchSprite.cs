using UnityEngine;

namespace Diluvion
{
    public class ColliderMatchSprite : MonoBehaviour
    {

        public BoxCollider2D col;
        public SpriteRenderer spriteRenderer;

        Vector2 worldSpacePivot;
        Sprite sprite;

        // Use this for initialization
        void Start()
        {

            col = GetComponent<BoxCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Update is called once per frame
        void Update()
        {

            if (!spriteRenderer) return;
            if (!spriteRenderer.isVisible) return;
            if (spriteRenderer.sprite != sprite)
            {
                sprite = spriteRenderer.sprite;
                Refresh();
            }
        }

        /// <summary>
        /// Refresh the collider to match the sprite renderer
        /// </summary>
        void Refresh()
        {
            if (!col || !spriteRenderer) return;
            if (!spriteRenderer.sprite) return;
            if (!spriteRenderer.isVisible) return;

            Vector2 extents = spriteRenderer.sprite.bounds.extents;
            col.size = extents * 2;

            worldSpacePivot = spriteRenderer.sprite.pivot / spriteRenderer.sprite.pixelsPerUnit;

            col.offset = extents - worldSpacePivot;
        }
    }
}