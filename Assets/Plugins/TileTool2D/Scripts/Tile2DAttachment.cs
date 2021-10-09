//Script is used on tile attachments when the default box colliders or sprites are not enough.
//Can also be used to add more content to a specific tile, like animated sprites.

using UnityEngine;
public class Tile2DAttachment : MonoBehaviour {
	
	//Name of the tile to add this attachment 
	//Example "NW" to add this gameobject to all North West tiles
	public string replaceTile;
	[HideInInspector]
	public int aIndex = 0;
	//Disables the default collider of the tile
	public bool replaceCollider;

	//Disables default sprite
	public bool replaceSprite;

	void Start() {
		Replace();
	}

	void Replace() {
		if (transform.parent == null) {
			Invoke("Replace", Time.deltaTime);
			return;
		}
		if (replaceCollider) {
			BoxCollider2D c = transform.parent.GetComponent<BoxCollider2D>();
			if (c != null) c.enabled = false;
		}
	}
}
