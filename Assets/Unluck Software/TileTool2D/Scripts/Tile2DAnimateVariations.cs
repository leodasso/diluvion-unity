using UnityEngine;
using System.Collections;

public class Tile2DAnimateVariations : MonoBehaviour {

	[Tooltip("Animate variations regardless of type [overrides limited]")]
	public bool animateAllVariations;
	[Tooltip("Animate only specific sprite types")]
	public string[] animateLimitedVariations;
	[Tooltip("Speed of animations")]
	public float speed = 0.1f;
	[Tooltip("Randomize start sprite")]
	public bool randomStart = true;

	public bool loop = true;
	public bool autoStart = true;


	int animateVariationsCounter;		// Keeps track of sprite animation frame
	Tile2D tile;						// Reference to the Tile2D component
	Sprite[] tileX;						// Reference to the current sprite type array
	string currentTileSprite;			// Current sprite type
	
	void Start () {
		tile = transform.GetComponent<Tile2D>();
		currentTileSprite = tile.tileSprite;
		if (animateAllVariations || animateLimitedVariations.Length > 0) {
			if (loop && randomStart) {
				animateVariationsCounter = Random.Range(0, 100);
				if (autoStart) Invoke("AnimateVariations", Random.value);
				return;
			}
			if(autoStart) Invoke("AnimateVariations", speed);
		}
	}

	void Animate() {
		if (AbortAnimation()) {
			Invoke("Animate", speed);
			animateVariationsCounter++;
			return;
		}
		if (currentTileSprite == tile.tileSprite) {
			if (!loop && animateVariationsCounter > tileX.Length) return;
			tile.cacheRenderer.sprite = tileX[animateVariationsCounter % tileX.Length];
			animateVariationsCounter++;
			Invoke("Animate", speed);
			return;
		}
		AnimateVariations();
	}

	bool AbortAnimation() {
		// Make sure animations are synced if offscreen at awake.
		//	if (Time.time < 3f) return false;
		// Make sure animation and calculating is not happening if the tile is not visible.
		if(tile.cacheRenderer.isVisible) return false;
		return true;
	}

	void Invoker(int i) {
		Invoke("Animate" + animateLimitedVariations[i], speed);
	}

	void AnimatAllVariations() {
		//Find the tile sprite type array by using string reflection (should only happen at awake or if tile sprite type changes)
		Sprite[] tileXx = (Sprite[])tile.GetType().GetField("tile" + tile.tileSprite).GetValue(tile);
		if (tileXx.Length > 1) {
			tileX = tileXx;
			currentTileSprite = tile.tileSprite;
			Invoke("Animate", speed);
		}
	}

	public void Stop(int stopFrame = -1, bool checkType = false) {
		if(checkType) AnimateVariations();
		CancelInvoke();
		if (tileX.Length <= 1) return;
		if(stopFrame > -1) tile.cacheRenderer.sprite = tileX[stopFrame % tileX.Length];	
	}

	public void Play(int startFrame = -1, bool checkType = false) {
		if(tileX == null || checkType) {
			AnimateVariations();
			return;
		}
		if (tileX.Length <= 1) {
			Debug.LogWarning("Tile2DAnimateVariations: No animation to play in this tile");
			return;
		}
		CancelInvoke();
		if (startFrame > -1) tile.cacheRenderer.sprite = tileX[startFrame % tileX.Length];
		Invoke("Animate", speed);
	}

	void AnimateVariations() {
		if (animateAllVariations) {
			AnimatAllVariations();
			return;
		}
		for (int i = 0; i < animateLimitedVariations.Length; i++) {
			//Find the tile sprite type array by using string reflection (should only happen at awake or if tile sprite type changes)
			Sprite[] tileXx = (Sprite[])tile.GetType().GetField("tile" + animateLimitedVariations[i]).GetValue(tile);
			if (tile.tileSprite == animateLimitedVariations[i] && tileXx.Length > 1) {
				tileX = tileXx;
				currentTileSprite = tile.tileSprite;
				Invoke("Animate", speed);
			}
		}
		// Alternative to using reflection... incomplete
		//for (int i = 0; i < animateVariations.Length; i++) {
		//	if (animateVariations[i] == "C" && tile.tileSprite == "C" && tile.tileC.Length > 1) {
		//		Invoker(i);
		//		break;
		//	}
	}
}
