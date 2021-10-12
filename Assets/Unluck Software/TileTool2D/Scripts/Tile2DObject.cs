using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class Tile2DObject : MonoBehaviour {
	public Tile2D t2;
	public Texture tileTexture;
	public string tileName; //Used to identify sprites when creating tiles in the editor
	public string tileType; //Tile with the same tileType will trigger automatic sprite changes
							//Used in cases where tiles are not using the standard 1 Unity unit scale
	public float tileSize = 1f;
	public bool passive;
	public bool disableColliderOnAwake;

	public ParticleSystem particles;

	//In case a sprite needs custom gameobjects containing for example extra colliders, animations or other components.
	public Tile2DAttachment[] attachments;
	public Tile2DAttachment currentAttachment;

	//Close by tiles
	//These will determine what sprite to use
	public Tile2D nTile;
	public Tile2D eTile;
	public Tile2D sTile;
	public Tile2D wTile;
	public Tile2D nwTile;
	public Tile2D neTile;
	public Tile2D seTile;
	public Tile2D swTile;

	public string tileSprite;



	//Cached components
	public SpriteRenderer cacheRenderer;
	public Collider2D cacheCollider;
	public int sortingOrder;
	[HideInInspector] public Collider2D[] tiles;

	//public string SpriteNumberToName(int n) {
	//	string r = "";
	//	if (n == 0) r = "A";
	//	else if (n == 1) r = "W";
	//	else if (n == 2) r = "NS";
	//	else if (n == 3) r = "E";
	//	else if (n == 4) r = "C";
	//	else if (n == 5) r = "CN";
	//	else if (n == 6) r = "CS";
	//	else if (n == 7) r = "NW";
	//	else if (n == 8) r = "NE";
	//	else if (n == 9) r = "N";
	//	else if (n == 10) r = "CSE";
	//	else if (n == 11) r = "CSWE";
	//	else if (n == 12) r = "CSW";
	//	else if (n == 13) r = "CWE";
	//	else if (n == 14) r = "CW";
	//	else if (n == 15) r = "CE";
	//	else if (n == 16) r = "WE";
	//	else if (n == 17) r = "CENS";
	//	else if (n == 18) r = "CNSWE";
	//	else if (n == 19) r = "CWNS";
	//	else if (n == 20) r = "CEW";
	//	else if (n == 21) r = "SW";
	//	else if (n == 22) r = "SE";
	//	else if (n == 23) r = "S";
	//	else if (n == 24) r = "CNE";
	//	else if (n == 25) r = "CNWE";
	//	else if (n == 26) r = "CNW";
	//	else if (n == 27) r = "CNS";
	//	else if (n == 28) r = "NWSE";
	//	else if (n == 29) r = "NESW";
	//	else if (n == 30) r = "CWSS";
	//	else if (n == 31) r = "CESS";
	//	else if (n == 32) r = "CNEE";
	//	else if (n == 33) r = "CNSS";
	//	else if (n == 34) r = "CSN";
	//	else if (n == 35) r = "SWNE";
	//	else if (n == 36) r = "SENW";
	//	else if (n == 37) r = "CWNN";
	//	else if (n == 38) r = "CENN";
	//	else if (n == 39) r = "CSEE";
	//	else if (n == 40) r = "CSWW";
	//	else if (n == 41) r = "CNWSE";
	//	else if (n == 42) r = "CNESW";
	//	else if (n == 43) r = "CENSW";
	//	else if (n == 44) r = "CNWES";
	//	else if (n == 45) r = "CSWEN";
	//	else if (n == 46) r = "CWNSE";
	//	return r;
	//}

	///
	/// 
	////Function to destroy a tile in run time.
	//public void DestroyTile() {
	//	if (nTile != null) nTile.DestroyedTile(this);
	//	if (eTile != null) eTile.DestroyedTile(this);
	//	if (sTile != null) sTile.DestroyedTile(this);
	//	if (wTile != null) wTile.DestroyedTile(this);
	//	if (nwTile != null) nwTile.DestroyedTile(this);
	//	if (neTile != null) neTile.DestroyedTile(this);
	//	if (seTile != null) seTile.DestroyedTile(this);
	//	if (swTile != null) swTile.DestroyedTile(this);
	//	Destroy(gameObject);
	//	if (!disableColliderOnAwake) {
	//		BeautifyCloseByTiles(false, false);
	//		return;
	//	}
	//	Debug.LogWarning("Destroying objects with disabled collider has no effect on other tiles");
	//}



	public void Awake() {
		CacheComponents();
		//FindTiles();
		DisableColliderOnAwake();
		if (particles) particles.GetComponent<Renderer>().sortingOrder = cacheRenderer.sortingOrder + 1;
	}

	public void CacheComponents() {
		if (cacheCollider == null) cacheCollider = GetComponent<Collider2D>();
		if (cacheRenderer == null) cacheRenderer = GetComponent<SpriteRenderer>();
	}

	public void DisableColliderOnAwake() {
		if (disableColliderOnAwake && cacheCollider) cacheCollider.enabled = false;
	}

	void DestroyedTile(Tile2D t) {
		if (nTile != null && nTile == t) nTile = null;
		if (eTile != null && eTile == t) eTile = null;
		if (sTile != null && sTile == t) sTile = null;
		if (wTile != null && wTile == t) wTile = null;
		if (nwTile != null && nwTile == t) nwTile = null;
		if (neTile != null && neTile == t) neTile = null;
		if (seTile != null && seTile == t) seTile = null;
		if (swTile != null && swTile == t) swTile = null;
	}

	public void BeautifyCloseByTiles(bool enableUndo, bool enablePhysics) {
#if UNITY_EDITOR
		if (!Application.isPlaying) {
			if (nTile != null) {
				if (enableUndo) Undo.RegisterCompleteObjectUndo(nTile.gameObject, "TileTool2D: Magic");
				nTile.FindTiles(enablePhysics);
			}

			if (eTile != null) {
				if (enableUndo) Undo.RegisterCompleteObjectUndo(eTile.gameObject, "TileTool2D: Magic");
				eTile.FindTiles(enablePhysics);
			}

			if (sTile != null) {
				if (enableUndo) Undo.RegisterCompleteObjectUndo(sTile.gameObject, "TileTool2D: Magic");
				sTile.FindTiles(enablePhysics);
			}

			if (wTile != null) {
				if (enableUndo) Undo.RegisterCompleteObjectUndo(wTile.gameObject, "TileTool2D: Magic");
				wTile.FindTiles(enablePhysics);
			}

			if (nwTile != null) {
				if (enableUndo) Undo.RegisterCompleteObjectUndo(nwTile.gameObject, "TileTool2D: Magic");
				nwTile.FindTiles(enablePhysics);
			}

			if (neTile != null) {
				if (enableUndo) Undo.RegisterCompleteObjectUndo(neTile.gameObject, "TileTool2D: Magic");
				neTile.FindTiles(enablePhysics);
			}

			if (seTile != null) {
				if (enableUndo) Undo.RegisterCompleteObjectUndo(seTile.gameObject, "TileTool2D: Magic");
				seTile.FindTiles(enablePhysics);
			}

			if (swTile != null) {
				if (enableUndo) Undo.RegisterCompleteObjectUndo(swTile.gameObject, "TileTool2D: Magic");
				swTile.FindTiles(enablePhysics);
			}
		}
#endif

		//Swap tiles in the nearby tile
		if (nTile != null) nTile.Beautify();
		if (eTile != null) eTile.Beautify();
		if (sTile != null) sTile.Beautify();
		if (wTile != null) wTile.Beautify();
		if (nwTile != null) nwTile.Beautify();
		if (neTile != null) neTile.Beautify();
		if (seTile != null) seTile.Beautify();
		if (swTile != null) swTile.Beautify();
		//Add attachements to nearby tiles
		if (nTile != null) nTile.AddAttachment();
		if (eTile != null) eTile.AddAttachment();
		if (sTile != null) sTile.AddAttachment();
		if (wTile != null) wTile.AddAttachment();
		if (nwTile != null) nwTile.AddAttachment();
		if (neTile != null) neTile.AddAttachment();
		if (seTile != null) seTile.AddAttachment();
		if (swTile != null) swTile.AddAttachment();
	}

	public void CheckIt(bool enableUndo, bool enablePhysics) {
		//BeautifyCloseByTiles();
		// Find new close by tiles
		FindTiles(enablePhysics);
		// Swap sprites based on close by tiles
		Beautify();
		// Find and add attachment
		AddAttachment();
		//Fix new close by tiles
		BeautifyCloseByTiles(enableUndo, enablePhysics);
	}

	public void FindTiles(bool enablePhysics) {
		nwTile = neTile = seTile = swTile = nTile = eTile = sTile = wTile = null;
		float overLapSize = 2f * tileSize;
		int layer = cacheRenderer.sortingLayerID;
		if (enablePhysics) {
			if (layer != 0)
				tiles = Physics2D.OverlapAreaAll(new Vector2(transform.position.x - overLapSize, transform.position.y - overLapSize), new Vector2(transform.position.x + overLapSize, transform.position.y + overLapSize), cacheRenderer.sortingLayerID);
			else
				tiles = Physics2D.OverlapAreaAll(new Vector2(transform.position.x - overLapSize, transform.position.y - overLapSize), new Vector2(transform.position.x + overLapSize, transform.position.y + overLapSize));

			for (int i = 0; i < tiles.Length; i++) {
				Tile2D t = tiles[i].transform.GetComponent<Tile2D>();
				if (t != null && t != this) FindTile(t);
			}
		} else {
			for (int i = 0; i < transform.parent.childCount; i++) {
				Tile2D t = transform.parent.GetChild(i).GetComponent<Tile2D>();
				if (t != null && t != this) FindTile(t);
			}
		}
	}

	public void CheckOverlap(bool enableUndo, bool enablePhysics) {
		//Only useable in Editor
#if UNITY_EDITOR
		float overLapSize = 2f * tileSize;
		int layer = cacheRenderer.sortingLayerID;
		if (enablePhysics) {
			if (layer != 0)
				tiles = Physics2D.OverlapAreaAll(new Vector2(transform.position.x - overLapSize, transform.position.y - overLapSize), new Vector2(transform.position.x + overLapSize, transform.position.y + overLapSize), cacheRenderer.sortingLayerID);
			else
				tiles = Physics2D.OverlapAreaAll(new Vector2(transform.position.x - overLapSize, transform.position.y - overLapSize), new Vector2(transform.position.x + overLapSize, transform.position.y + overLapSize));

			for (int i = 0; i < tiles.Length; i++) {
				Tile2D t = tiles[i].GetComponent<Tile2D>();
				if (t != null) {
					Transform tt = t.transform;
					if (t != this && gameObject != null && tt.parent == transform.parent && tt.position.x == transform.position.x && tt.position.y == transform.position.y) {
						tt.position = new Vector3(9999999.999999f, 9999999.999999f, 0);
						if (enableUndo) Undo.DestroyObjectImmediate(t.gameObject);
						else DestroyImmediate(t.gameObject);
					}
				}
			}
		} else {
			for (int i = 0; i < transform.parent.childCount; i++) {
				Tile2D t = transform.parent.GetChild(i).GetComponent<Tile2D>();
				if (t != null) {
					Transform tt = t.transform;
					if (t != this && gameObject != null && tt.position.x == transform.position.x && tt.position.y == transform.position.y) {
						tt.position = new Vector3(9999999.999999f, 9999999.999999f, 0);
						if (enableUndo) Undo.DestroyObjectImmediate(t.gameObject);
						else DestroyImmediate(t.gameObject);
					}
				}
			}
		}

#endif
	}

	public void FindTile(Tile2D t) {
		Transform tt = t.transform;
		if (tt.parent != transform.parent)
			return;
		// Check if tile is directly above
		if (tt.position.y == Round(transform.position.y + tileSize, tileSize) && tt.position.x == transform.position.x) {

			if (!t.passive) nTile = t;
#if UNITY_EDITOR
			if (nTile != null) EditorUtility.SetDirty(nTile);
#endif
			return;
		}

		// Check if tile is directly below
		if (tt.position.y == Round(transform.position.y - tileSize, tileSize) && tt.position.x == transform.position.x) {
			if (!t.passive) sTile = t;
#if UNITY_EDITOR
			if (sTile != null) EditorUtility.SetDirty(sTile);
#endif
			return;
		}

		// Check if tile is directly to the right
		if (tt.position.x == Round(transform.position.x + tileSize, tileSize) && tt.position.y == transform.position.y) {
			if (!t.passive) eTile = t;
#if UNITY_EDITOR
			if (eTile != null) EditorUtility.SetDirty(eTile);
#endif
			return;
		}

		// Check if tile is directly to the left
		if (tt.position.x == Round(transform.position.x - tileSize, tileSize) && tt.position.y == transform.position.y) {
			if (!t.passive) wTile = t;
#if UNITY_EDITOR
			if (wTile != null) EditorUtility.SetDirty(wTile);
#endif
			return;
		}

		// Check if tile is north west
		if (tt.position.x == Round(transform.position.x - tileSize, tileSize) && tt.position.y == Round(transform.position.y + tileSize, tileSize)) {
			if (!t.passive) nwTile = t;
#if UNITY_EDITOR
			if (nwTile != null) EditorUtility.SetDirty(nwTile);
#endif
			return;
		}

		// Check if tile is north east
		if (tt.position.x == Round(transform.position.x + tileSize, tileSize) && tt.position.y == Round(transform.position.y + tileSize, tileSize)) {
			if (!t.passive) neTile = t;
#if UNITY_EDITOR
			if (neTile != null) EditorUtility.SetDirty(neTile);
#endif
			return;
		}

		// Check if tile is south east
		if (tt.position.x == Round(transform.position.x + tileSize, tileSize) && tt.position.y == Round(transform.position.y - tileSize, tileSize)) {
			if (!t.passive) seTile = t;
#if UNITY_EDITOR
			if (seTile != null) EditorUtility.SetDirty(seTile);
#endif
			return;
		}

		// Check if tile is south west
		if (tt.position.x == Round(transform.position.x - tileSize, tileSize) && tt.position.y == Round(transform.position.y - tileSize, tileSize)) {
			if (!t.passive) swTile = t;
#if UNITY_EDITOR
			if (swTile != null) EditorUtility.SetDirty(swTile);
#endif
			return;
		}

		// Check if tile is overlapping (EDITOR ONLY)
		//#if UNITY_EDITOR
		//		if (gameObject != null && tt.position.x == transform.position.x && tt.position.y == transform.position.y) {
		//			tt.position = new Vector3(9999999.999999f, 9999999.999999f, 0);
		//			//Undo.DestroyObjectImmediate(t.gameObject);
		//			//DestroyImmediate(t.gameObject);
		//			return;
		//		}
		//#endif
		//		// Check if tile is overlapping
		//		if (gameObject != null && tt.position.x == transform.position.x && tt.position.y == transform.position.y) {
		//			tt.position = new Vector3(9999999.999999f, 9999999.999999f, 0);
		//			Destroy(t.gameObject);
		//			return;
		//		}
	}

	public void FindCloseByHighestSortOrder() {
		int s = 0;
		if (nTile != null) s = nTile.sortingOrder;
		if ((eTile != null) && eTile.sortingOrder > s) s = eTile.sortingOrder;
		if ((sTile != null) && sTile.sortingOrder > s) s = sTile.sortingOrder;
		if ((wTile != null) && wTile.sortingOrder > s) s = wTile.sortingOrder;
		sortingOrder = s + 1;
	}

	public void FixSortingLayer(int layerIndex) {
		GetComponent<SpriteRenderer>().sortingOrder = layerIndex + Mathf.Clamp(sortingOrder, -1999, 1999);
		if (currentAttachment != null)
			SortAttachment();
		//#if UNITY_EDITOR
		//	EditorUtility.SetDirty(this);
		//#endif
	}

	public void FindCloseByLowestSortOrder() {
		int s = 1000000;
		if (nTile != null) s = nTile.sortingOrder;
		if ((eTile != null) && eTile.sortingOrder < s) {
			s = eTile.sortingOrder;
		}

		if ((sTile != null) && sTile.sortingOrder < s) {
			s = sTile.sortingOrder;
		}
		if ((wTile != null) && wTile.sortingOrder < s) {
			s = wTile.sortingOrder;
		}
		if (s == 1000000) s = 0;
		sortingOrder = s - 1;
	}

	public void AddAttachment() {
		GameObject g = FindAttachment();
		if (g == null) {
			if (currentAttachment) {
				cacheRenderer.enabled = true;
				DestroyImmediate(currentAttachment.gameObject);
			}
			return;
		}
		if (currentAttachment) DestroyImmediate(currentAttachment.gameObject);
		GameObject ng = null;

#if UNITY_EDITOR
		ng = (GameObject)PrefabUtility.InstantiatePrefab(g);
#endif
		if (Application.isPlaying)
			ng = (GameObject)Instantiate(g, transform.position, transform.rotation);

		ng.transform.parent = transform;
		ng.transform.localPosition = Vector3.zero;
		currentAttachment = ng.GetComponent<Tile2DAttachment>();
		if (currentAttachment.replaceSprite) cacheRenderer.enabled = false;
		SortAttachment();
	}

	public void SortAttachment() {
		SpriteRenderer[] s = (SpriteRenderer[])currentAttachment.transform.GetComponents<SpriteRenderer>();
		for (int i = 0; i < s.Length; i++) {
			s[i].sortingOrder = cacheRenderer.sortingOrder;
			s[i].sortingLayerName = cacheRenderer.sortingLayerName;
			s[i].sortingLayerID = cacheRenderer.sortingLayerID;
		}
	}

	public GameObject FindAttachment() {
		GameObject g = null;
		for (int i = 0; i < attachments.Length; i++) {
			if (attachments[i].replaceTile == tileSprite) {
				g = attachments[i].gameObject;
				break;
			}
		}
		return g;
	}

	public float Round(float input, float size) {
		float snappedValue = 0.0f;
		snappedValue = size * Mathf.Round((input / size));
		return (snappedValue);
	}

	public void Beautify() {
		Tile2D nTileXXX = null;
		Tile2D eTileXXX = null;
		Tile2D sTileXXX = null;
		Tile2D wTileXXX = null;
		Tile2D neTileX = null;
		Tile2D nwTileX = null;
		Tile2D seTileX = null;
		Tile2D swTileX = null;
		if ((nTile != null) && nTile.tileType == tileType) nTileXXX = nTile;
		if ((eTile != null) && eTile.tileType == tileType) eTileXXX = eTile;
		if ((sTile != null) && sTile.tileType == tileType) sTileXXX = sTile;
		if ((wTile != null) && wTile.tileType == tileType) wTileXXX = wTile;
		if ((nwTile != null) && nwTile.tileType == tileType) nwTileX = nwTile;
		if ((neTile != null) && neTile.tileType == tileType) neTileX = neTile;
		if ((seTile != null) && seTile.tileType == tileType) seTileX = seTile;
		if ((swTile != null) && swTile.tileType == tileType) swTileX = swTile;

		if (passive || (nTileXXX == null) && (eTileXXX == null) && (sTileXXX == null) && (wTileXXX == null) && t2.tileA.Length > 0) cacheRenderer.sprite = t2.tileA[Random.Range(0, t2.tileA.Length)];

		//Has a tile at all NESW sides
		if ((nTileXXX != null) && (eTileXXX != null) && (sTileXXX != null) && (wTileXXX != null)) {

			if (swTileX == null && seTileX == null && nwTileX == null && neTileX == null && t2.tileCNSWE.Length > 0) {
				cacheRenderer.sprite = t2.tileCNSWE[Random.Range(0, t2.tileCNSWE.Length)];
				tileSprite = "CNSWE";
				return;
			}

			if (nwTileX == null && neTileX == null && seTileX == null && t2.tileCENSW.Length > 0) {
				cacheRenderer.sprite =t2.tileCENSW[Random.Range(0,t2.tileCENSW.Length)];
				tileSprite = "CENSW";
				return;
			}

			if (nwTileX == null && neTileX == null && swTileX == null &&t2.tileCNWES.Length > 0) {
				cacheRenderer.sprite =t2.tileCNWES[Random.Range(0,t2.tileCNWES.Length)];
				tileSprite = "CNWES";
				return;
			}

			if (neTileX == null && seTileX == null && swTileX == null &&t2.tileCSWEN.Length > 0) {
				cacheRenderer.sprite =t2.tileCSWEN[Random.Range(0,t2.tileCSWEN.Length)];
				tileSprite = "CSWEN";
				return;
			}

			if (nwTileX == null && seTileX == null && swTileX == null &&t2.tileCWNSE.Length > 0) {
				cacheRenderer.sprite =t2.tileCWNSE[Random.Range(0,t2.tileCWNSE.Length)];
				tileSprite = "CWNSE";
				return;
			}

			if (nwTileX == null && seTileX == null &&t2.tileCNWSE.Length > 0) {
				cacheRenderer.sprite =t2.tileCNWSE[Random.Range(0,t2.tileCNWSE.Length)];
				tileSprite = "CNWSE";
				return;
			}

			if (neTileX == null && swTileX == null &&t2.tileCNESW.Length > 0) {
				cacheRenderer.sprite =t2.tileCNESW[Random.Range(0,t2.tileCNESW.Length)];
				tileSprite = "CNESW";
				return;
			}

			if (nwTileX == null && neTileX == null &&t2.tileCNWE.Length > 0) {
				cacheRenderer.sprite =t2.tileCNWE[Random.Range(0,t2.tileCNWE.Length)];
				tileSprite = "CNWE";
				return;
			}

			if (seTileX == null && neTileX == null &&t2.tileCENS.Length > 0) {
				cacheRenderer.sprite =t2.tileCENS[Random.Range(0,t2.tileCENS.Length)];
				tileSprite = "CENS";
				return;
			}

			if (seTileX == null && swTileX == null &&t2.tileCSWE.Length > 0) {
				cacheRenderer.sprite =t2.tileCSWE[Random.Range(0,t2.tileCSWE.Length)];
				tileSprite = "CSWE";
				return;
			}

			if (nwTileX == null && swTileX == null &&t2.tileCWNS.Length > 0) {
				cacheRenderer.sprite =t2.tileCWNS[Random.Range(0,t2.tileCWNS.Length)];
				tileSprite = "CWNS";
				return;
			}

			if (nwTileX == null &&t2.tileCNW.Length > 0) {
				cacheRenderer.sprite =t2.tileCNW[Random.Range(0,t2.tileCNW.Length)];
				tileSprite = "CNW";
				return;
			}

			if (neTileX == null &&t2.tileCNE.Length > 0) {
				cacheRenderer.sprite =t2.tileCNE[Random.Range(0,t2.tileCNE.Length)];
				tileSprite = "CNE";
				return;
			}

			if (seTileX == null &&t2.tileCSE.Length > 0) {
				cacheRenderer.sprite =t2.tileCSE[Random.Range(0,t2.tileCSE.Length)];
				tileSprite = "CSE";
				return;
			}

			if (swTileX == null &&t2.tileCSW.Length > 0) {
				cacheRenderer.sprite =t2.tileCSW[Random.Range(0,t2.tileCSW.Length)];
				tileSprite = "CSW";
				return;
			}

			if (t2.tileC.Length > 0) {
				cacheRenderer.sprite =t2.tileC[Random.Range(0,t2.tileC.Length)];
				tileSprite = "C";
				return;
			}
		}

		if ((nTileXXX == null) && (eTileXXX != null) && (sTileXXX != null) && (wTileXXX != null)) {

			if (seTileX == null && swTileX == null &&t2.tileCNS.Length > 0) {
				cacheRenderer.sprite =t2.tileCNS[Random.Range(0,t2.tileCNS.Length)];
				tileSprite = "CNS";
				return;
			}

			if (seTileX == null &&t2.tileCNEE.Length > 0) {
				cacheRenderer.sprite =t2.tileCNEE[Random.Range(0,t2.tileCNEE.Length)];
				tileSprite = "CNEE";
				return;
			}

			if (swTileX == null &&t2.tileCNSS.Length > 0) {
				cacheRenderer.sprite =t2.tileCNSS[Random.Range(0,t2.tileCNSS.Length)];
				tileSprite = "CNSS";
				return;
			}

			if (t2.tileCN.Length > 0) {
				cacheRenderer.sprite =t2.tileCN[Random.Range(0,t2.tileCN.Length)];
				tileSprite = "CN";
				return;
			}
		}

		if ((nTileXXX != null) && (eTileXXX != null) && (sTileXXX == null) && (wTileXXX != null)) {

			if (neTileX == null && nwTileX == null &&t2.tileCSN.Length > 0) {
				cacheRenderer.sprite =t2.tileCSN[Random.Range(0,t2.tileCSN.Length)];
				tileSprite = "CSN";
				return;
			}

			if (nwTileX == null &&t2.tileCSWW.Length > 0) {
				cacheRenderer.sprite =t2.tileCSWW[Random.Range(0,t2.tileCSWW.Length)];
				tileSprite = "CSWW";
				return;
			}

			if (neTileX == null &&t2.tileCSEE.Length > 0) {
				cacheRenderer.sprite =t2.tileCSEE[Random.Range(0,t2.tileCSEE.Length)];
				tileSprite = "CSEE";
				return;
			}

			if (t2.tileCS.Length > 0) {
				cacheRenderer.sprite =t2.tileCS[Random.Range(0,t2.tileCS.Length)];
				tileSprite = "CS";
				return;
			}
		}

		if ((nTileXXX != null) && (eTileXXX != null) && (sTileXXX != null) && (wTileXXX == null)) {

			if (neTileX == null && seTileX == null &&t2.tileCWE.Length > 0) {
				cacheRenderer.sprite =t2.tileCWE[Random.Range(0,t2.tileCWE.Length)];
				tileSprite = "CWE";
				return;
			}

			if (neTileX == null &&t2.tileCWNN.Length > 0) {
				cacheRenderer.sprite =t2.tileCWNN[Random.Range(0,t2.tileCWNN.Length)];
				tileSprite = "CWNN";
				return;
			}

			if (seTileX == null &&t2.tileCWSS.Length > 0) {
				cacheRenderer.sprite =t2.tileCWSS[Random.Range(0,t2.tileCWSS.Length)];
				tileSprite = "CWSS";
				return;
			}

			if (t2.tileCW.Length > 0) {
				cacheRenderer.sprite =t2.tileCW[Random.Range(0,t2.tileCW.Length)];
				tileSprite = "CW";
				return;
			}
		}

		if ((nTileXXX != null) && (eTileXXX == null) && (sTileXXX != null) && (wTileXXX != null)) {

			if (nwTileX == null && swTileX == null &&t2.tileCEW.Length > 0) {
				cacheRenderer.sprite =t2.tileCEW[Random.Range(0,t2.tileCEW.Length)];
				tileSprite = "CEW";
				return;
			}

			if (swTileX == null &&t2.tileCESS.Length > 0) {
				cacheRenderer.sprite =t2.tileCESS[Random.Range(0,t2.tileCESS.Length)];
				tileSprite = "CESS";
				return;
			}

			if (nwTileX == null &&t2.tileCENN.Length > 0) {
				cacheRenderer.sprite =t2.tileCENN[Random.Range(0,t2.tileCENN.Length)];
				tileSprite = "CENN";
				return;
			}

			if (t2.tileCE.Length > 0) {
				cacheRenderer.sprite =t2.tileCE[Random.Range(0,t2.tileCE.Length)];
				tileSprite = "CE";
				return;
			}
		}

		// Check if this is NWt2.tile
		if ((nTileXXX == null) && (eTileXXX != null) && (sTileXXX != null) && (wTileXXX == null)) {

			if (seTileX == null &&t2.tileNWSE.Length > 0) {
				cacheRenderer.sprite =t2.tileNWSE[Random.Range(0,t2.tileNWSE.Length)];
				tileSprite = "NWSE";
				return;
			}

			if (t2.tileNW.Length > 0) {
				cacheRenderer.sprite =t2.tileNW[Random.Range(0,t2.tileNW.Length)];
				tileSprite = "NW";
				return;
			}
		}

		// Check if this is NEt2.tile
		if ((nTileXXX == null) && (eTileXXX == null) && (sTileXXX != null) && (wTileXXX != null)) {

			if (swTileX == null &&t2.tileNESW.Length > 0) {
				cacheRenderer.sprite =t2.tileNESW[Random.Range(0,t2.tileNESW.Length)];
				tileSprite = "NESW";
				return;
			}

			if (t2.tileNE.Length > 0) {
				cacheRenderer.sprite =t2.tileNE[Random.Range(0,t2.tileNE.Length)];
				tileSprite = "NE";
				return;
			}
		}

		// Check if this is SWt2.tile
		if ((nTileXXX != null) && (eTileXXX != null) && (sTileXXX == null) && (wTileXXX == null)) {

			if (neTileX == null &&t2.tileSWNE.Length > 0) {
				cacheRenderer.sprite =t2.tileSWNE[Random.Range(0,t2.tileSWNE.Length)];
				tileSprite = "SWNE";
				return;
			}

			if (t2.tileSW.Length > 0) {
				cacheRenderer.sprite =t2.tileSW[Random.Range(0,t2.tileSW.Length)];
				tileSprite = "SW";
				return;
			}
		}

		// Check if this is SEt2.tile
		if ((nTileXXX != null) && (eTileXXX == null) && (sTileXXX == null) && (wTileXXX != null)) {

			if (nwTileX == null &&t2.tileSENW.Length > 0) {
				cacheRenderer.sprite =t2.tileSENW[Random.Range(0,t2.tileSENW.Length)];
				tileSprite = "SENW";
				return;
			}

			if (t2.tileSE.Length > 0) {
				cacheRenderer.sprite =t2.tileSE[Random.Range(0,t2.tileSE.Length)];
				tileSprite = "SE";
				return;
			}
		}

		// Check if this is Nt2.tile
		if ((nTileXXX == null) && (eTileXXX == null) && (sTileXXX != null) && (wTileXXX == null)) {
			if (t2.tileN.Length > 0) {
				cacheRenderer.sprite =t2.tileN[Random.Range(0,t2.tileN.Length)];
				tileSprite = "N";
				return;
			}
		}
		// Check if this is Et2.tile
		if ((nTileXXX == null) && (eTileXXX == null) && (sTileXXX == null) && (wTileXXX != null)) {
			if (t2.tileE.Length > 0) {
				cacheRenderer.sprite =t2.tileE[Random.Range(0,t2.tileE.Length)];
				tileSprite = "E";
				return;
			}
		}
		// Check if this is St2.tile
		if ((nTileXXX != null) && (eTileXXX == null) && (sTileXXX == null) && (wTileXXX == null)) {
			if (t2.tileS.Length > 0) {
				cacheRenderer.sprite =t2.tileS[Random.Range(0,t2.tileS.Length)];
				tileSprite = "S";
				return;
			}

		}
		// Check if this is Wt2.tile
		if ((nTileXXX == null) && (eTileXXX != null) && (sTileXXX == null) && (wTileXXX == null)) {
			if (t2.tileW.Length > 0) {
				cacheRenderer.sprite =t2.tileW[Random.Range(0,t2.tileW.Length)];
				tileSprite = "W";
				return;
			}
		}

		// Check if this is NSt2.tile
		if ((nTileXXX == null) && (eTileXXX != null) && (sTileXXX == null) && (wTileXXX != null)) {
			if (t2.tileNS.Length > 0) {
				cacheRenderer.sprite =t2.tileNS[Random.Range(0,t2.tileNS.Length)];
				tileSprite = "NS";
				return;
			}
		}
		// Check if this is WEt2.tile
		if ((nTileXXX != null) && (eTileXXX == null) && (sTileXXX != null) && (wTileXXX == null)) {
			if (t2.tileWE.Length > 0) {
				cacheRenderer.sprite =t2.tileWE[Random.Range(0,t2.tileWE.Length)];
				tileSprite = "WE";
				return;
			}
		}

		// Check if this is A tile
		if (t2.tileA.Length > 0) {
			cacheRenderer.sprite = t2.tileA[Random.Range(0, t2.tileA.Length)];
			tileSprite = "A";
			return;
		}
	}
}
